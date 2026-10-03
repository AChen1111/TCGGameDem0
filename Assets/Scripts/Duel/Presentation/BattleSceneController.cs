using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using LitMotion;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Rendering.Universal;

namespace AChen.Duel.Presentation
{
    public sealed class BattleSceneController : MonoBehaviour
    {
        [SerializeField] Camera m_camera;
        [SerializeField] UniversalAdditionalCameraData m_mainCameraData;
        [SerializeField] EventSystem m_events;
        [SerializeField] BattleCardView m_cardPrefab;
        [SerializeField] Transform m_cardRoot;
        [SerializeField] BattleZoneView[] m_zones;
        [SerializeField] BattlePhaseDial m_dial;
        [SerializeField] BattleTimerView m_timer;
        [SerializeField] BattlePileView[] m_piles;
        [SerializeField] BattleGraveView[] m_graves;
        [SerializeField] GameObject[] m_playableGuides;
        [SerializeField] BattlePhaseEffects m_phaseEffects;
        [SerializeField] DuelDemoPreset m_preset;
        readonly Dictionary<int,BattleCardView> m_cards=new Dictionary<int,BattleCardView>();
        readonly Dictionary<Collider,int> m_cardHits=new Dictionary<Collider,int>();
        readonly Dictionary<Collider,BattleZoneView> m_zoneHits=new Dictionary<Collider,BattleZoneView>();
        readonly List<RaycastResult> m_uiHits=new List<RaycastResult>();
        readonly IDuelVisibilityPolicy m_visibility=new BattleVisibilityPolicy();
        readonly Dictionary<string,Texture> m_textures = new Dictionary<string,Texture>();
        [SerializeField] BattleAttackArrow m_attackArrow;
        [SerializeField] BattleChainView m_chainView;
        [SerializeField] Transform[] m_directAttackAnchors;
        [SerializeField] float m_retreatDuration=.15f, m_chargeDuration=.18f, m_returnDuration=.22f;
        [SerializeField] float m_retreatDistance=2.5f, m_targetClearance=5f;
        [SerializeField] Ease m_retreatEase=Ease.OutQuad, m_chargeEase=Ease.InQuad, m_returnEase=Ease.OutCubic;
        LocalDuelSession m_session;
        UIFrame m_frame;
        MotionHandle m_motion;
        Action m_finish=delegate { };
        bool m_ready,m_pointerHeld,m_touchCaptured;
        int m_pressedCard,m_hovered,m_selected,m_animationGeneration,m_width,m_height;
        float m_layoutFieldOfView;
        TouchControl m_touch;
        Vector2 m_pointerStart,m_selectedAnchor;
        public IDuelPresentationSource Source=>m_session;
        public int SelectedCardId=>m_selected;
        public Camera BattleCamera=>m_camera;
        public Camera BattleUICamera=>m_frame.UICamera;
        public Camera CameraForCard(int id)=>m_camera;
        public BattleCardView CardObject(int id)=>m_cards[id];
        public bool CanInspect(CardView card)=>m_visibility.CanInspect(0,card);
        public Texture TextureForDefinition(string id)=>m_textures[id];
        public Vector3 RegionAnchor(ZoneRef zone)=>zone.Kind==DuelZone.Hand?new Vector3(0,8,zone.Player==0?-28:23):zone.Kind==DuelZone.Material?Vector3.zero:m_zones.First(z=>z.Zone.Equals(zone)).Anchor.position;
        public event Action<int> SelectionChanged=delegate { };
        public event Action<string> Notice=delegate { };
        public event Action<int,Vector2> CardActionsRequested=delegate { };
        public event Action<string> ChoiceInspectionRequested=delegate { };
        public void InspectChoice(string definitionId)=>ChoiceInspectionRequested(definitionId);
        public DuelCardSpec ChoiceDefinition(string id)=>m_session.Definitions.First(d=>d.CardId==id);
        public Vector2 CardScreenAnchor(int id)
        {
            var card=m_session.Current.Card(id);
            return card.Zone.IsSlot||card.Zone.Kind==DuelZone.Hand
                ? (Vector2)m_camera.WorldToScreenPoint(m_cards[id].ActionWorldAnchor):m_selectedAnchor;
        }
        public async UniTask InitializeAsync(UIFrame frame)
        {
            m_frame=frame;m_mainCameraData.cameraStack.Add(frame.UICamera);m_session=m_preset.CreateLocalSession();
            var textures=new Dictionary<string,Texture>();
            foreach(var definition in m_session.Definitions)
            {
                var texture=await CardPoolAddress.LoadCardTextureAsync(definition.SourcePool,definition.CardId)
                    .AttachExternalCancellation(this.GetCancellationTokenOnDestroy());
                textures.Add(definition.CardId,texture);m_textures.Add(definition.CardId,texture);
            }
            foreach(var card in m_session.Current.Cards)
            {
                var obj=Instantiate(m_cardPrefab,m_cardRoot); obj.name="Card_"+card.InstanceId;
                obj.Bind(card.InstanceId,textures[m_session.DefinitionForInstance(card.InstanceId).CardId]);
                m_cards.Add(card.InstanceId,obj); m_cardHits.Add(obj.Hitbox,card.InstanceId);
            }
            foreach(var zone in m_zones)m_zoneHits.Add(zone.Hitbox,zone);
            m_session.Changed+=OnChanged; m_ready=true; m_selected=m_session.Current.Cards[0].InstanceId;
            m_width=Screen.width; m_height=Screen.height;
            m_layoutFieldOfView=m_camera.fieldOfView;
            Rebuild(false,0); RefreshWorld(m_session.Current);
            m_phaseEffects.Stop();
            frame.ShowPanel(AddressKeys.Prefab.BattleHudPanel,new BattleHudProperties(this));
            SceneTransitionOverlay.Hide();m_session.Start();
        }
        void Update()
        {
            if(!m_ready)return;
            m_session.Tick(Time.unscaledDeltaTime);
            foreach(var device in InputSystem.devices)
                if(device is Keyboard keyboard&&keyboard.escapeKey.wasPressedThisFrame)
                {if(m_session.Current.Choice.Active)CancelAction();else if(m_frame.IsWindowBusy)m_frame.CloseCurrentWindow();else CancelAction();}
            if((m_width!=Screen.width||m_height!=Screen.height||!Mathf.Approximately(m_layoutFieldOfView,m_camera.fieldOfView))&&!m_session.Current.Animating)
            {m_width=Screen.width;m_height=Screen.height;m_layoutFieldOfView=m_camera.fieldOfView;Rebuild(false,0);}
            foreach(var device in InputSystem.devices)
            {
                if(device is not Touchscreen screen)continue;
                if(!m_touchCaptured)
                    if(screen.primaryTouch.press.wasPressedThisFrame){m_touch=screen.primaryTouch;m_touchCaptured=true;}
                if(!m_touchCaptured)continue;
                InputPointer(m_touch.position.ReadValue(),m_touch.press.wasPressedThisFrame,m_touch.press.isPressed,m_touch.press.wasReleasedThisFrame);
                if(m_touch.press.wasReleasedThisFrame)m_touchCaptured=false;
                return;
            }
            foreach(var device in InputSystem.devices)
                if(device is Mouse mouse)
                {InputPointer(mouse.position.ReadValue(),mouse.leftButton.wasPressedThisFrame,mouse.leftButton.isPressed,mouse.leftButton.wasReleasedThisFrame);break;}
        }
        public void InputPointer(Vector2 point,bool down,bool held,bool up)
        {
            var state=m_session.Current;
            bool blocked=m_frame.IsWindowBusy||state.Animating||(state.Choice.Active&&!state.Choice.IsResponse)||OverUI(point);
            if(down)
            {
                if(blocked){ClearPointer();return;}
                m_pointerStart=point;m_pointerHeld=true;m_pressedCard=HitCard(point);
                SetRegionPointer(point,true);
            }
            if(up&&m_pointerHeld)
            {
                bool click=Vector2.Distance(point,m_pointerStart)<=Mathf.Max(8,Screen.height*.012f);
                int pressed=m_pressedCard;ClearPointer();SetRegionPointer(point,false);
                if(blocked||!click)return;
                if(state.HasPendingAction)
                {
                    if(Physics.Raycast(m_camera.ScreenPointToRay(point),out var targetHit,300)&&m_zoneHits.TryGetValue(targetHit.collider,out var target))
                    {
                        if(state.PendingAction.Targets.Contains(target.Zone))m_session.Submit(new ConfirmActionTarget(target.Zone));
                        else Notice("请选择黄色标记的可用区域");
                    }
                    else if(HitCard(point)!=0)Notice("请完成当前区域选择或取消");
                    else CancelAction();
                    return;
                }
                if(!state.CanBrowseCards)return;
                if(pressed!=0&&HitCard(point)==pressed){ShowCardActions(pressed,point);return;}
                if(Physics.Raycast(m_camera.ScreenPointToRay(point),out var hit,300))
                {
                    if(hit.collider==m_dial.Hitbox){OpenPhases();return;}
                    if(m_zoneHits.TryGetValue(hit.collider,out var zone)&&!zone.Zone.IsSlot){OpenZone(zone.Zone);return;}
                }
                if(!state.Choice.Active)CancelAction();CloseZone();
            }
            if(!held&&!m_pointerHeld)
            {
                int hovered=blocked||!state.CanBrowseCards?0:HitCard(point);
                if(hovered!=m_hovered)
                {
                    if(m_hovered!=0)m_cards[m_hovered].Hover(false);
                    m_hovered=hovered;
                    if(hovered!=0&&state.Card(hovered).Zone.Kind==DuelZone.Hand&&state.Card(hovered).Owner==0)m_cards[hovered].Hover(true);
                }
                SetRegionPointer(point,false,blocked);
            }
        }
        bool OverUI(Vector2 point)
        {m_uiHits.Clear();m_events.RaycastAll(new PointerEventData(m_events){position=point},m_uiHits);return m_uiHits.Count>0;}
        int HitCard(Vector2 point)=>Physics.Raycast(m_camera.ScreenPointToRay(point),out var hit,300)&&m_cardHits.TryGetValue(hit.collider,out int id)?id:0;
        void SetRegionPointer(Vector2 point,bool pressed,bool blocked=false)
        {
            ZoneRef hovered=default;bool found=!blocked&&Physics.Raycast(m_camera.ScreenPointToRay(point),out var hit,300)&&m_zoneHits.TryGetValue(hit.collider,out var zone);
            if(found){Physics.Raycast(m_camera.ScreenPointToRay(point),out var regionHit,300);hovered=m_zoneHits[regionHit.collider].Zone;}
            for(int i=0;i<m_piles.Length;i++)
            {
                var target=m_zones.Where(x=>x.Zone.Kind is DuelZone.MainDeck or DuelZone.ExtraDeck).ElementAt(i).Zone;
                bool active=found&&target.Equals(hovered);m_piles[i].SetHovered(active);m_piles[i].SetPressed(active&&pressed);
            }
            for(int i=0;i<m_graves.Length;i++)
            {
                var target=m_zones.Where(x=>x.Zone.Kind is DuelZone.Graveyard or DuelZone.Banished).ElementAt(i).Zone;
                bool active=found&&target.Equals(hovered);m_graves[i].SetHovered(active);m_graves[i].SetPressed(active&&pressed);
            }
        }
        void ClearPointer(){m_pointerHeld=false;m_pressedCard=0;}
        public void SelectCard(int id)
        {
            if(!m_session.Current.CanBrowseCards)return;
            if(m_selected!=0){m_cards[m_selected].StopMotion();m_cards[m_selected].SetSelected(false);}
            m_selected=id;m_cards[id].SetSelected(true);SelectionChanged(id);
        }
        public void ShowCardActions(int id)=>ShowCardActions(id,m_camera.WorldToScreenPoint(m_cards[id].ActionWorldAnchor));
        public void ShowCardActions(int id,Vector2 anchor)
        {if(!m_session.Current.CanBrowseCards||!CanInspect(m_session.Current.Card(id)))return;m_selectedAnchor=anchor;SelectCard(id);CardActionsRequested(id,anchor);}
        public void OpenDetail(int id)=>SelectCard(id);
        public void OpenZone(ZoneRef zone)
        {if(m_session.Current.CanBrowseCards){ClearPointer();m_frame.ShowPanel(AddressKeys.Prefab.BattleZoneWindow,new BattleZoneProperties(this,zone));}}
        public void CloseZone()=>m_frame.HidePanel(AddressKeys.Prefab.BattleZoneWindow);
        public void BeginAction(int id,string actionId)
        {
            CloseZone();ClearPointer();m_session.Submit(new BeginCardAction(id,actionId));

        }
        public void OpenPlacement()=>m_frame.OpenWindow(AddressKeys.Prefab.BattleChoiceWindow,new BattleChoiceProperties(this,BattleChoiceKind.ActionPosition));
        public void OpenPhases()
        {if(m_session.Current.CanInteract){CloseZone();m_frame.OpenWindow(AddressKeys.Prefab.BattleChoiceWindow,new BattleChoiceProperties(this,BattleChoiceKind.Phase));}}
        public void OpenPosition()
        {if(m_session.Current.CanInteract){CloseZone();m_frame.OpenWindow(AddressKeys.Prefab.BattleChoiceWindow,new BattleChoiceProperties(this,BattleChoiceKind.Position));}}
        public void PrepareSelectedPlacement()
        {
            CloseZone();m_session.Submit(new BeginDebugPlacement(m_selected));
            if(m_session.Current.HasPendingAction&&m_session.Current.PendingAction.NeedsPosition)OpenPlacement();
        }
        public void CancelAction()
        {
            if(m_session.Current.Animating)return;
            m_session.Submit(new CancelCardAction());ClearPointer();
            foreach(var card in m_cards.Values)card.SetSelected(false);
        }
        void OnChanged(DuelViewChange change)
        {
            if(change.Kind==DuelChangeKind.Rejected){Notice(change.Message);return;}
            if(change.Kind==DuelChangeKind.Timer){RefreshWorld(change.View);return;}
            ClearPointer();
            if(change.Kind==DuelChangeKind.Reset)
            {CancelAnimations();m_chainView.Clear();m_attackArrow.Hide();CloseZone();m_hovered=0;foreach(var card in m_cards.Values)card.SetSelected(false);}
            if(!change.View.Choice.Active && m_frame.IsPanelOpen(AddressKeys.Prefab.BattleSelectionPanel))m_frame.HidePanel(AddressKeys.Prefab.BattleSelectionPanel);
            if(change.Kind==DuelChangeKind.Effect && change.LinkNumber>0)m_chainView.Add(change);
            if(change.Kind==DuelChangeKind.ChainResolved && change.LinkNumber>0)
            {
                m_chainView.Emphasize(change);Rebuild(false,0);RefreshWorld(change.View);
                StartAnimation(.4f,_=>{},()=>m_chainView.Remove(change));return;
            }
            if(change.Kind is DuelChangeKind.Move or DuelChangeKind.Position)
            {Rebuild(true,change.InstanceId);RefreshWorld(change.View);return;}
            if(change.Kind==DuelChangeKind.Attack){PlayAttack(change);return;}
            if(change.Kind==DuelChangeKind.Effect){Rebuild(false,0);RefreshWorld(change.View);PlayEffect(change.InstanceId);return;}
            if(change.Kind is DuelChangeKind.Phase or DuelChangeKind.Turn)
            {Rebuild(false,0);RefreshWorld(change.View);float duration=m_phaseEffects.Begin(change);StartAnimation(duration,_=>{},m_phaseEffects.Stop);return;}
            Rebuild(false,0);RefreshWorld(change.View);
            if(change.View.Choice.Active && !m_frame.IsPanelOpen(AddressKeys.Prefab.BattleSelectionPanel))
                m_frame.ShowPanel(AddressKeys.Prefab.BattleSelectionPanel,new BattleSelectionProperties(this));
            if(change.View.Animating){m_session.Submit(new AnimationCompleted());return;}
            int attacker=m_session.HasAttackPreview?m_session.AttackPreviewSource:m_session.DeclaredAttacker;
            int target=m_session.HasAttackPreview?m_session.AttackPreviewTarget:m_session.DeclaredTarget;
            if(attacker!=0 && change.View.Card(attacker).Zone.IsSlot && change.View.Card(attacker).Position==CardPosition.FaceUpAttack && change.View.Card(attacker).Owner==change.View.ActivePlayer)
                m_attackArrow.Show(m_cards[attacker].ActionWorldAnchor,AttackDestination(target,attacker));else m_attackArrow.Hide();
            if(change.View.Finished)Notice(change.View.Outcome);
        }
        Vector3 AttackDestination(int target,int attacker)=>target==0?m_directAttackAnchors[1-m_session.Current.Card(attacker).Owner].position:m_cards[target].ActionWorldAnchor;
        void PlayAttack(DuelViewChange change)
        {
            CancelAnimations();var obj=m_cards[change.AttackerId];var pose=obj.CapturePose();
            Vector3 destination=AttackDestination(change.TargetId,change.AttackerId);Vector3 direction=(destination-pose.Position).normalized;
            Vector3 retreat=pose.Position-direction*m_retreatDistance;
            Vector3 impact=destination-direction*m_targetClearance;
            float total=m_retreatDuration+m_chargeDuration+m_returnDuration;
            bool impacted=false;
            m_attackArrow.Show(pose.Position,destination);
            StartAnimation(total,t=>
            {
                float elapsed=t*total;Vector3 position;
                if(!impacted && elapsed>=m_retreatDuration+m_chargeDuration)
                {impacted=true;m_session.PresentImpact(change.ImpactLifePoints);}
                if(elapsed<m_retreatDuration)position=Vector3.Lerp(pose.Position,retreat,EaseUtility.Evaluate(elapsed/m_retreatDuration,m_retreatEase));
                else if(elapsed<m_retreatDuration+m_chargeDuration)position=Vector3.Lerp(retreat,impact,EaseUtility.Evaluate((elapsed-m_retreatDuration)/m_chargeDuration,m_chargeEase));
                else position=Vector3.Lerp(impact,pose.Position,EaseUtility.Evaluate((elapsed-m_retreatDuration-m_chargeDuration)/m_returnDuration,m_returnEase));
                obj.SetPose(new BattleCardPose(position,pose.Rotation,pose.PlaneRotation,pose.PivotPosition,pose.Scale,pose.OffsetPosition,pose.OffsetRotation,pose.TurnRotation,pose.Hand));
            },()=>obj.SetPose(pose),Ease.Linear);
        }
        BattleCardPose PoseFor(CardView card,DuelView view)
        {
            float yaw=card.Owner==0?0:180;
            bool hand=card.Zone.Kind==DuelZone.Hand;
            Vector3 position,pivot=Vector3.zero;Quaternion plane=Quaternion.identity,offset=Quaternion.identity;
            if(hand)
            {
                var cards=view.InZone(card.Zone);int index=cards.ToList().FindIndex(x=>x.InstanceId==card.InstanceId);
                float spacing=card.Owner==0?Mathf.Min(4,42f/Mathf.Max(1,cards.Count)):Mathf.Min(2.2f,26f/Mathf.Max(1,cards.Count));float x=(index-(cards.Count-1)*.5f)*spacing;
                float z=card.Owner==0?-28+(30-m_camera.fieldOfView)*.7f:23-(30-m_camera.fieldOfView)*.7f;
                position=new Vector3(card.Owner==0?x:-x,card.Owner==0?15:5,z);
                plane=Quaternion.Euler(card.Owner==0?-20:20,0,0);
                float abs=Mathf.Abs(x);pivot=new Vector3(0,0,-abs*(abs*.0055f+.08f));
                offset=Quaternion.Euler(0,x*(1.2f-.006f*abs)*(card.Owner==0?1:-1),-10);
                if(card.Owner==1)
                {
                    var ray=m_camera.ViewportPointToRay(new Vector3(.5f,.92f,0));
                    float distance=(5-ray.origin.y)/ray.direction.y;
                    position=new Vector3(-x,5,ray.GetPoint(distance).z);
                    plane=Quaternion.identity;pivot=Vector3.zero;offset=Quaternion.identity;
                }
            }
            else
            {
                position=card.Zone.Kind==DuelZone.Material?RegionAnchor(view.Card(card.HostInstanceId).Zone):m_zones.First(x=>x.Zone.Equals(card.Zone)).Anchor.position;
                if(card.Zone.Kind is DuelZone.MainDeck or DuelZone.ExtraDeck)
                {position+=Vector3.up*Mathf.Max(0,view.InZone(card.Zone).Count-1)*.1f;yaw+=card.Zone.Kind==DuelZone.MainDeck?-19.5f:19.5f;}
                if(card.Position is CardPosition.FaceUpDefense or CardPosition.FaceDownDefense)yaw+=90;
            }
            bool face=m_visibility.IsFaceVisible(0,card);
            float scale=hand&&card.Owner==1?.58f:(card.Zone.Kind is DuelZone.SpellTrap or DuelZone.Field) ? .8f : 1;
            return new BattleCardPose(position,Quaternion.Euler(0,yaw,0),plane,pivot,Vector3.one*scale,
                Vector3.zero,offset,Quaternion.Euler(0,0,face?0:180),hand);
        }
        void Rebuild(bool animate,int changingCard)
        {
            CancelAnimations();var view=m_session.Current;
            var starts=new Dictionary<int,BattleCardPose>();var ends=new Dictionary<int,BattleCardPose>();
            foreach(var card in view.Cards)
            {
                var obj=m_cards[card.InstanceId];obj.StopMotion();starts.Add(card.InstanceId,obj.CapturePose());
                var end=PoseFor(card,view);ends.Add(card.InstanceId,end);obj.SetRestPose(end);
                bool visible=card.Zone.IsSlot||card.Zone.Kind==DuelZone.Hand;
                obj.gameObject.SetActive(visible||(animate&&card.InstanceId==changingCard));obj.Hitbox.enabled=visible&&!animate;
                obj.Apply(card,m_visibility.IsFaceVisible(0,card));
            }
            FitFarHand(view,ends);
            foreach(var card in view.Cards)m_cards[card.InstanceId].SetRestPose(ends[card.InstanceId]);
            void Sample(float t)
            {foreach(var card in view.Cards)m_cards[card.InstanceId].SetPose(BattleCardPose.Lerp(starts[card.InstanceId],ends[card.InstanceId],t,animate&&card.InstanceId==changingCard?5:0));}
            void Finish()
            {
                Sample(1);
                foreach(var card in view.Cards)
                {
                    var obj=m_cards[card.InstanceId];bool visible=card.Zone.IsSlot||card.Zone.Kind==DuelZone.Hand;
                    obj.gameObject.SetActive(visible);obj.Hitbox.enabled=visible;obj.Apply(card,m_visibility.IsFaceVisible(0,card));
                }
            }
            if(animate)StartAnimation(.55f,Sample,Finish);else Finish();
        }
        void FitFarHand(DuelView view,Dictionary<int,BattleCardPose> poses)
        {
            var hand=view.Cards.Where(card=>card.Zone.Kind==DuelZone.Hand && card.Owner==1).ToArray();
            if(hand.Length==0)return;
            var previous=new Dictionary<int,BattleCardPose>();
            float top=float.NegativeInfinity;Vector3 topPoint=Vector3.zero;
            foreach(var card in hand)
            {
                var obj=m_cards[card.InstanceId];previous.Add(card.InstanceId,obj.CapturePose());obj.SetPose(poses[card.InstanceId]);
                var bounds=obj.WorldBounds;
                for(int i=0;i<8;i++)
                {
                    var point=bounds.center+Vector3.Scale(bounds.extents,new Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1));
                    float y=m_camera.WorldToViewportPoint(point).y;
                    if(y>top){top=y;topPoint=point;}
                }
            }
            if(top>.97f)
            {
                var viewport=m_camera.WorldToViewportPoint(topPoint);viewport.y=.97f;
                var ray=m_camera.ViewportPointToRay(viewport);
                float distance=(topPoint.y-ray.origin.y)/ray.direction.y;
                float shift=ray.GetPoint(distance).z-topPoint.z;
                foreach(var card in hand)poses[card.InstanceId]=poses[card.InstanceId].WithPosition(poses[card.InstanceId].Position+Vector3.forward*shift);
            }
            foreach(var card in hand)m_cards[card.InstanceId].SetPose(previous[card.InstanceId]);
        }
        void PlayEffect(int id)
        {
            CancelAnimations();var obj=m_cards[id];var card=m_session.Current.Card(id);var pose=obj.CapturePose();
            bool visible=card.Zone.IsSlot||card.Zone.Kind==DuelZone.Hand;
            obj.gameObject.SetActive(true);obj.SetEffectAvailable(true);
            Notice("发动效果 · "+LocalizationService.GetText("card."+card.Definition.CardId+".name"));
            StartAnimation(.6f,t=>obj.SetPose(new BattleCardPose(pose.Position+Vector3.up*Mathf.Sin(t*Mathf.PI)*3,pose.Rotation,
                pose.PlaneRotation,pose.PivotPosition,pose.Scale,pose.OffsetPosition,pose.OffsetRotation,pose.TurnRotation,pose.Hand)),
                ()=>{obj.SetPose(pose);obj.Apply(card,m_visibility.IsFaceVisible(0,card));obj.gameObject.SetActive(visible);});
        }
        void StartAnimation(float duration,Action<float> sample,Action finish,Ease ease=Ease.InOutCubic)
        {
            int generation=++m_animationGeneration;m_finish=finish;
            m_motion=LMotion.Create(0f,1f,duration).WithEase(ease).WithOnComplete(()=>
            {if(generation!=m_animationGeneration)return;m_finish();m_finish=delegate{};m_session.Submit(new AnimationCompleted());})
                .Bind(sample).AddTo(this);
        }
        public void SkipAnimation()
        {
            if(!m_session.Current.Animating)return;
            m_animationGeneration++;m_motion.TryCancel();m_finish();m_finish=delegate{};
            foreach(var card in m_cards.Values)card.StopMotion();m_phaseEffects.Stop();m_session.Submit(new AnimationCompleted());
        }
        void CancelAnimations()
        {m_animationGeneration++;m_motion.TryCancel();m_finish=delegate{};foreach(var card in m_cards.Values)card.StopMotion();m_phaseEffects.Stop();}
        void RefreshWorld(DuelView view)
        {
            foreach(var zone in m_zones)
            {zone.Refresh(view);zone.SetHint(view.HasPendingAction&&!view.PendingAction.NeedsPosition&&view.PendingAction.Targets.Contains(zone.Zone));}
            m_dial.Refresh(view,false);m_timer.Refresh(view);
            foreach(var pile in m_piles)pile.Refresh(view);
            foreach(var grave in m_graves)grave.Refresh(view);
            for(int i=0;i<2;i++)m_playableGuides[i].SetActive(i==view.ActivePlayer);
        }
        void LateUpdate(){if(m_ready)m_chainView.Refresh(this,m_session.Current);}
        void OnDestroy()
        {if(!m_ready)return;CancelAnimations();m_chainView.Clear();m_attackArrow.Hide();m_session.Changed-=OnChanged;}
    }
}
