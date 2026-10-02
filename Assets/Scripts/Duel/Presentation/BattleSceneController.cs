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
        readonly IDuelVisibilityPolicy m_visibility=new DemoVisibilityPolicy();
        DuelDemoSession m_session;
        UIFrame m_frame;
        MotionHandle m_motion;
        Action m_finish=delegate { };
        bool m_ready,m_pointerHeld,m_touchCaptured;
        int m_pressedCard,m_hovered,m_selected,m_animationGeneration,m_width,m_height;
        TouchControl m_touch;
        Vector2 m_pointerStart,m_selectedAnchor;
        public IDuelPresentationSource Source=>m_session;
        public int SelectedCardId=>m_selected;
        public Camera BattleCamera=>m_camera;
        public Camera BattleUICamera=>m_frame.UICamera;
        public Camera CameraForCard(int id)=>m_camera;
        public BattleCardView CardObject(int id)=>m_cards[id];
        public bool CanInspect(CardView card)=>m_visibility.CanInspect(0,card);
        public event Action<int> SelectionChanged=delegate { };
        public event Action<string> Notice=delegate { };
        public event Action<int,Vector2> CardActionsRequested=delegate { };
        public Vector2 CardScreenAnchor(int id)
        {
            var card=m_session.Current.Card(id);
            return card.Zone.IsSlot||card.Zone.Kind==DuelZone.Hand
                ? (Vector2)m_camera.WorldToScreenPoint(m_cards[id].ActionWorldAnchor):m_selectedAnchor;
        }
        public async UniTask InitializeAsync(UIFrame frame)
        {
            m_frame=frame;m_mainCameraData.cameraStack.Add(frame.UICamera);m_session=m_preset.CreateSession();
            var textures=new Dictionary<string,Texture>();
            foreach(var definition in m_session.Current.Cards.Select(x=>x.Definition).GroupBy(x=>x.CardId).Select(g=>g.First()))
            {
                var texture=await CardPoolAddress.LoadCardTextureAsync(definition.SourcePool,definition.CardId)
                    .AttachExternalCancellation(this.GetCancellationTokenOnDestroy());
                textures.Add(definition.CardId,texture);
            }
            foreach(var card in m_session.Current.Cards)
            {
                var obj=Instantiate(m_cardPrefab,m_cardRoot); obj.name="Card_"+card.InstanceId;
                obj.Bind(card.InstanceId,textures[card.Definition.CardId]);
                m_cards.Add(card.InstanceId,obj); m_cardHits.Add(obj.Hitbox,card.InstanceId);
            }
            foreach(var zone in m_zones)m_zoneHits.Add(zone.Hitbox,zone);
            m_session.Changed+=OnChanged; m_ready=true; m_selected=m_session.Current.Cards[0].InstanceId;
            m_width=Screen.width; m_height=Screen.height;
            Rebuild(false,0); RefreshWorld(m_session.Current);
            m_phaseEffects.Stop();
            frame.ShowPanel(AddressKeys.Prefab.BattleHudPanel,new BattleHudProperties(this));
            SceneTransitionOverlay.Hide();
        }
        void Update()
        {
            if(!m_ready)return;
            m_session.Tick(Time.unscaledDeltaTime);
            foreach(var device in InputSystem.devices)
                if(device is Keyboard keyboard&&keyboard.escapeKey.wasPressedThisFrame)
                {if(m_frame.IsWindowBusy)m_frame.CloseCurrentWindow();else CancelAction();}
            if((m_width!=Screen.width||m_height!=Screen.height)&&!m_session.Current.Animating)
            {m_width=Screen.width;m_height=Screen.height;Rebuild(false,0);}
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
            bool blocked=m_frame.IsWindowBusy||state.Animating||OverUI(point);
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
                if(!state.CanInteract)return;
                if(pressed!=0&&HitCard(point)==pressed){ShowCardActions(pressed,point);return;}
                if(Physics.Raycast(m_camera.ScreenPointToRay(point),out var hit,300))
                {
                    if(hit.collider==m_dial.Hitbox){OpenPhases();return;}
                    if(m_zoneHits.TryGetValue(hit.collider,out var zone)&&!zone.Zone.IsSlot){OpenZone(zone.Zone);return;}
                }
                CancelAction();CloseZone();
            }
            if(!held&&!m_pointerHeld)
            {
                int hovered=blocked||!state.CanInteract?0:HitCard(point);
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
            if(!m_session.Current.CanInteract)return;
            if(m_selected!=0){m_cards[m_selected].StopMotion();m_cards[m_selected].SetSelected(false);}
            m_selected=id;m_cards[id].SetSelected(true);SelectionChanged(id);
        }
        public void ShowCardActions(int id)=>ShowCardActions(id,m_camera.WorldToScreenPoint(m_cards[id].ActionWorldAnchor));
        public void ShowCardActions(int id,Vector2 anchor)
        {if(!m_session.Current.CanInteract)return;m_selectedAnchor=anchor;SelectCard(id);CardActionsRequested(id,anchor);}
        public void OpenDetail(int id)=>SelectCard(id);
        public void OpenZone(ZoneRef zone)
        {if(m_session.Current.CanInteract){ClearPointer();m_frame.ShowPanel(AddressKeys.Prefab.BattleZoneWindow,new BattleZoneProperties(this,zone));}}
        public void CloseZone()=>m_frame.HidePanel(AddressKeys.Prefab.BattleZoneWindow);
        public void BeginAction(int id,string actionId)
        {
            CloseZone();ClearPointer();m_session.Submit(new BeginCardAction(id,actionId));
            if(m_session.Current.HasPendingAction&&m_session.Current.PendingAction.NeedsPosition)OpenPlacement();
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
            if(change.Kind==DuelChangeKind.Reset)
            {CancelAnimations();ClearPointer();CloseZone();foreach(var card in m_cards.Values)card.SetSelected(false);
             m_hovered=0;m_selected=change.View.Cards[0].InstanceId;Rebuild(false,0);SelectionChanged(m_selected);}
            else if(change.Kind is DuelChangeKind.Move or DuelChangeKind.Position)
            {ClearPointer();foreach(var card in m_cards.Values)card.SetSelected(false);Rebuild(true,change.InstanceId);}
            else if(change.Kind==DuelChangeKind.Effect)PlayEffect(change.InstanceId);
            else if(change.Kind is DuelChangeKind.Phase or DuelChangeKind.Turn)
            {
                CancelAnimations();float duration=m_phaseEffects.Begin(change);
                StartAnimation(duration,_=>{},m_phaseEffects.Stop);
            }
            else if(change.Kind==DuelChangeKind.Highlight)
                foreach(var card in change.View.Cards)m_cards[card.InstanceId].Apply(card,m_visibility.IsFaceVisible(0,card));
            RefreshWorld(change.View);
        }
        BattleCardPose PoseFor(CardView card,DuelView view)
        {
            float yaw=card.Owner==0?0:180;
            bool hand=card.Zone.Kind==DuelZone.Hand;
            Vector3 position,pivot=Vector3.zero;Quaternion plane=Quaternion.identity,offset=Quaternion.identity;
            if(hand)
            {
                var cards=view.InZone(card.Zone);int index=cards.ToList().FindIndex(x=>x.InstanceId==card.InstanceId);
                float spacing=Mathf.Min(4,42f/Mathf.Max(1,cards.Count));float x=(index-(cards.Count-1)*.5f)*spacing;
                float z=card.Owner==0?-28+(30-m_camera.fieldOfView)*.7f:23-(30-m_camera.fieldOfView)*.7f;
                position=new Vector3(card.Owner==0?x:-x,card.Owner==0?15:5,z);
                plane=Quaternion.Euler(card.Owner==0?-20:20,0,0);
                float abs=Mathf.Abs(x);pivot=new Vector3(0,0,-abs*(abs*.0055f+.08f));
                offset=Quaternion.Euler(0,x*(1.2f-.006f*abs)*(card.Owner==0?1:-1),-10);
            }
            else
            {
                position=m_zones.First(x=>x.Zone.Equals(card.Zone)).Anchor.position;
                if(card.Zone.Kind is DuelZone.MainDeck or DuelZone.ExtraDeck)
                {position+=Vector3.up*Mathf.Max(0,view.InZone(card.Zone).Count-1)*.1f;yaw+=card.Zone.Kind==DuelZone.MainDeck?-19.5f:19.5f;}
                if(card.Position is CardPosition.FaceUpDefense or CardPosition.FaceDownDefense)yaw+=90;
            }
            bool face=m_visibility.IsFaceVisible(0,card);
            float scale=(card.Zone.Kind is DuelZone.SpellTrap or DuelZone.Field) ? .8f : 1;
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
        void StartAnimation(float duration,Action<float> sample,Action finish)
        {
            int generation=++m_animationGeneration;m_finish=finish;
            m_motion=LMotion.Create(0f,1f,duration).WithEase(Ease.InOutCubic).WithOnComplete(()=>
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
        void OnDestroy()
        {if(!m_ready)return;CancelAnimations();m_session.Changed-=OnChanged;}
    }
}
