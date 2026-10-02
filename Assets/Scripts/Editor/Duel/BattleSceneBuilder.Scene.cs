using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AChen.Duel.Presentation;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public static partial class BattleSceneBuilder
{
    static BattleZoneView Zone(Transform parent,DuelZone kind,int player,int slot,Vector3 position)
    {
        var root=Obj($"{player}_{kind}_{slot}",parent,position);var view=root.gameObject.AddComponent<BattleZoneView>();
        var anchor=Obj("CardAnchor",root,new Vector3(0,.2f,0));
        var collider=root.gameObject.AddComponent<BoxCollider>();collider.center=new Vector3(0,.15f,0);
        collider.size=kind is DuelZone.Graveyard or DuelZone.Banished?new Vector3(6,.3f,5):new Vector3(7.5f,.3f,9.8f);
        var surface=Surface("Selection",root,new Vector3(0,.3f,0),7.6f,10.2f,Sprite("GUI_CommonSelectCursor_CornerAll"),true);
        var label=WorldText("ZoneLabel",root,new Vector3(0,.65f,0),"",1);
        label.rectTransform.sizeDelta=new Vector2(10,3);label.gameObject.SetActive(false);
        Ref(view,"m_anchor",anchor);Ref(view,"m_hitbox",collider);Ref(view,"m_surface",surface);Ref(view,"m_label",label);
        Int(view,"m_kind",(int)kind);Int(view,"m_player",player);Int(view,"m_slot",slot);return view;
    }
    static GameObject Original(string path,Transform parent)
        =>(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(MDPro3BattleImport.Folder+"/"+path),parent);
    static Transform Named(Transform root,string name)=>SceneComponents<Transform>(root).First(x=>x.name==name);
    static TextMeshPro NativeLabel(Transform node,string text,float size)
    {
        foreach(var old in SceneComponents<TMP_Text>(node).ToArray())UnityEngine.Object.DestroyImmediate(old);
        node.gameObject.SetActive(true);
        var label=node.gameObject.AddComponent<TextMeshPro>();label.font=s_font;label.fontSharedMaterial=s_worldText;label.text=text;label.fontSize=size;
        label.color=Color.white;label.alignment=TextAlignmentOptions.Center;label.enableWordWrapping=false;
        if(node.name=="TextMain"||node.name=="TextAbove"||node.name=="TextBelow")
        {node.position+=Vector3.up*.55f;PrefabUtility.RecordPrefabInstancePropertyModifications(node);}
        return label;
    }
    public static void PositionPhaseLabels(BattlePhaseDial dial)
    {
        var references=new SerializedObject(dial);
        var main=(TMP_Text)references.FindProperty("m_main").objectReferenceValue;
        foreach(string field in new[]{"m_main","m_above","m_below"})
        {
            var label=(TMP_Text)references.FindProperty(field).objectReferenceValue;
            var position=label.transform.position;position.y=2.13f;
            if(field=="m_above")position.z=main.transform.position.z+1.7f;
            label.transform.position=position;
            PrefabUtility.RecordPrefabInstancePropertyModifications(label.transform);
        }
    }
    static void BindBattleInput(InputSystemUIInputModule module)
    {
        var asset=ScriptableObject.CreateInstance<InputActionAsset>();var map=asset.AddActionMap("UI");
        var point=map.AddAction("Point",InputActionType.PassThrough,expectedControlLayout:"Vector2");
        point.AddBinding("<Mouse>/position");point.AddBinding("<Touchscreen>/primaryTouch/position");
        var click=map.AddAction("Click",InputActionType.PassThrough,expectedControlLayout:"Button");
        click.AddBinding("<Mouse>/leftButton");click.AddBinding("<Touchscreen>/primaryTouch/press");
        map.AddAction("Scroll",InputActionType.PassThrough,"<Mouse>/scroll",expectedControlLayout:"Vector2");
        map.AddAction("RightClick",InputActionType.PassThrough,"<Mouse>/rightButton",expectedControlLayout:"Button");
        map.AddAction("MiddleClick",InputActionType.PassThrough,"<Mouse>/middleButton",expectedControlLayout:"Button");
        map.AddAction("Submit",InputActionType.Button,"<Keyboard>/enter");map.AddAction("Cancel",InputActionType.Button,"<Keyboard>/escape");
        var move=map.AddAction("Move",InputActionType.Value,expectedControlLayout:"Vector2");
        move.AddCompositeBinding("2DVector").With("Up","<Keyboard>/upArrow").With("Down","<Keyboard>/downArrow").With("Left","<Keyboard>/leftArrow").With("Right","<Keyboard>/rightArrow");
        string json=asset.ToJson();
        asset=Store(asset,Art+"BattleInputActions.asset");
        asset.LoadFromJson(json);EditorUtility.SetDirty(asset);
        InputActionReference Reference(string name)
        {
            var action=asset.FindAction("UI/"+name);
            var reference=Store(InputActionReference.Create(action),Art+"BattleInput_"+name+".asset");
            reference.Set(action);EditorUtility.SetDirty(reference);return reference;
        }
        module.actionsAsset=asset;module.point=Reference("Point");module.leftClick=Reference("Click");module.scrollWheel=Reference("Scroll");
        module.rightClick=Reference("RightClick");module.middleClick=Reference("MiddleClick");module.move=Reference("Move");module.submit=Reference("Submit");module.cancel=Reference("Cancel");
    }
    public static void RebindBattleInput(InputSystemUIInputModule module)=>BindBattleInput(module);
    static BattlePhaseEffects PhaseEffects(Transform parent,Camera camera)
    {
        var container=Obj("OriginalPhaseEffects",parent,Vector3.zero);var effect=container.gameObject.AddComponent<BattlePhaseEffects>();
        var directors=new List<PlayableDirector>();var near=new List<GameObject>();var far=new List<GameObject>();
        var spriteMaterial=Store(new Material(Shader.Find("TCG/Battle/OriginalSprite")),Art+"OriginalSprite.mat");
        string[] phases={"DuelDrawPhase","DuelStandbyPhase","DuelMainPhase1","DuelBattlePhase","DuelMainPhase2","DuelEndPhase"};
        string[] captions={"DRAW PHASE","STANDBY PHASE","MAIN PHASE 1","BATTLE PHASE","MAIN PHASE 2","END PHASE"};
        GameObject Load(string name,string caption,bool turning)
        {
            var root=Original("GameObject/"+name+".prefab",container);
            foreach(var renderer in SceneComponents<SpriteRenderer>(root.transform))
            {
                string sprite=turning?(name.EndsWith("00")?"img_TunChange_bg01":"img_TunChange_bg02"):renderer.name;
                renderer.sprite=Sprite(sprite);
                var persistentSprite=new Material(spriteMaterial);persistentSprite.SetTexture("_BaseMap",renderer.sprite.texture);
                renderer.sharedMaterial=Store(persistentSprite,Art+sprite+".mat");
            }
            foreach(var node in SceneComponents<Transform>(root.transform).Where(x=>x.name=="Text"||x.name=="TextFace"||x.name=="TextLine"||x.name.StartsWith("Text (TMP)_")).ToArray())
            {
                string value=turning?node.name.Substring(node.name.LastIndexOf('_')+1):caption;
                var text=NativeLabel(node,value,turning?10:7);text.fontStyle=FontStyles.Italic;
                text.alignment=turning?TextAlignmentOptions.TopLeft:TextAlignmentOptions.Baseline;
            }
            var director=SceneComponents<PlayableDirector>(root.transform).Single();
            director.timeUpdateMode=DirectorUpdateMode.UnscaledGameTime;
            MDPro3BattleImport.PrepareOriginalTimeline(root,name,director);directors.Add(director);
            foreach(var node in SceneComponents<Transform>(root.transform))node.gameObject.layer=18;
            root.SetActive(false);return root;
        }
        for(int i=0;i<phases.Length;i++)
        {near.Add(Load(phases[i]+"_near",captions[i],false));far.Add(Load(phases[i]+"_far",captions[i],false));}
        var turns=new[]{Load("DuelTurnChange00","TURN CHANGE",true),Load("DuelTurnChange01","TURN CHANGE",true)};
        Refs(effect,"m_near",near);Refs(effect,"m_far",far);Refs(effect,"m_turns",turns);
        Refs(effect,"m_directors",directors);Refs(effect,"m_particles",SceneComponents<ParticleSystem>(container));
        var overlay=Obj("Original2DEffectCamera",null,new Vector3(0,0,-10)).gameObject.AddComponent<Camera>();
        overlay.orthographic=true;overlay.orthographicSize=5;overlay.nearClipPlane=.1f;overlay.farClipPlane=100;
        overlay.cullingMask=1<<18;overlay.clearFlags=CameraClearFlags.Depth;
        var overlayData=overlay.gameObject.AddComponent<UniversalAdditionalCameraData>();overlayData.renderType=CameraRenderType.Overlay;
        Root<UniversalAdditionalCameraData>(camera.gameObject).cameraStack.Add(overlay);
        return effect;
    }
    static BattlePileView Pile(BattleZoneView zone,Transform parent)
    {
        var pile=Original("OriginalAdditions/Deck/resources/duel/timeline/duel/universal/dueldeckappearance/DuelDeckAppearance.prefab",parent);
        pile.name="OriginalPile_"+zone.Zone;pile.transform.position=zone.transform.position;
        float yaw=(zone.Zone.Player==0?0:180)+(zone.Zone.Kind==DuelZone.MainDeck?-19.5f:19.5f);
        pile.transform.rotation=Quaternion.Euler(0,yaw,0);
        foreach(var director in SceneComponents<PlayableDirector>(pile.transform)){director.playOnAwake=false;director.enabled=false;director.playableAsset=null;}
        foreach(var white in SceneComponents<Transform>(pile.transform).Where(t=>t.name=="DeckWhite"))white.gameObject.SetActive(false);
        foreach(var particle in SceneComponents<ParticleSystem>(pile.transform))particle.gameObject.SetActive(false);
        var cardBacks=SceneComponents<MeshRenderer>(pile.transform).Where(x=>x.name.EndsWith("_back")).ToArray();
        var highlights=new List<Renderer>();
        foreach(var renderer in cardBacks)
        {
            renderer.sharedMaterial=s_back;
            var glow=UnityEngine.Object.Instantiate(renderer.gameObject,renderer.transform.parent);glow.name="PileOuterGlow";
            var glowRenderer=Root<MeshRenderer>(glow);glowRenderer.sharedMaterial=s_glow;highlights.Add(renderer);highlights.Add(glowRenderer);
        }
        var view=pile.AddComponent<BattlePileView>();Ref(view,"m_shuffleTop",Named(pile.transform,"card_shuffle"));
        Refs(view,"m_renderers",highlights);Ref(view,"m_count",Root<TextMeshPro>(Named(zone.transform,"ZoneLabel").gameObject));
        Int(view,"m_kind",(int)zone.Zone.Kind);Int(view,"m_player",zone.Zone.Player);return view;
    }
    static void BuildScene(GameObject cardPrefab,DuelDemoPreset preset)
    {
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        var stage=Obj("OriginalMDPro3Field",null,Vector3.zero);
        var near=Original("Resources/duel/bg/mat/mat_002/Mat_002_near.prefab",stage);
        var far=Original("Resources/duel/bg/mat/mat_002/Mat_002_far.prefab",stage);
        var graves=new[]{Original("Resources/duel/bg/grave/grave_002/Grave_002_near.prefab",Named(near.transform,"POS_Grave_near")),
            Original("Resources/duel/bg/grave/grave_002/Grave_002_far.prefab",Named(far.transform,"POS_Grave_far"))};
        Original("resourcesassetbundle/duel/bg/avatarstand/avatarstand_002/sd/AvatarStand_002_near.prefab",Named(near.transform,"POS_AvatarStand_near"));
        Original("resourcesassetbundle/duel/bg/avatarstand/avatarstand_002/sd/AvatarStand_002_far.prefab",Named(far.transform,"POS_AvatarStand_far"));
        foreach(var particle in SceneComponents<ParticleSystem>(stage))particle.gameObject.SetActive(false);
        RenderSettings.fog=false;RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat;RenderSettings.ambientLight=Color.white;
        var light=Obj("BattleLight",null,Vector3.zero).gameObject.AddComponent<Light>();light.type=LightType.Directional;light.intensity=1;light.transform.rotation=Quaternion.Euler(55,-30,0);
        var camera=Obj("BattleCamera",null,new Vector3(0,95,-37)).gameObject.AddComponent<Camera>();camera.tag="MainCamera";
        camera.transform.rotation=Quaternion.Euler(70,0,0);camera.fieldOfView=30;camera.nearClipPlane=.1f;camera.farClipPlane=400;
        camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.035f,.065f,.047f);camera.rect=new Rect(0,0,1,1);camera.cullingMask=~((1<<18)|(1<<5));
        camera.gameObject.AddComponent<UniversalAdditionalCameraData>();camera.gameObject.AddComponent<AudioListener>();
        var environment=stage.gameObject.AddComponent<BattleEnvironmentView>();
        var animators=SceneComponents<Animator>(stage).Where(a=>a.gameObject.activeInHierarchy&&a.runtimeAnimatorController is UnityEditor.Animations.AnimatorController).ToArray();
        Refs(environment,"m_animators",animators);Ref(environment,"m_camera",camera);
        var data=new SerializedObject(environment);var states=data.FindProperty("m_states");states.arraySize=animators.Length;
        for(int i=0;i<animators.Length;i++)states.GetArrayElementAtIndex(i).stringValue=((UnityEditor.Animations.AnimatorController)animators[i].runtimeAnimatorController).layers[0].stateMachine.defaultState.name;
        data.ApplyModifiedPropertiesWithoutUndo();
        var events=Obj("EventSystem",null,Vector3.zero).gameObject.AddComponent<EventSystem>();BindBattleInput(events.gameObject.AddComponent<InputSystemUIInputModule>());
        var root=Obj("BattlePresentation",null,Vector3.zero);var controller=root.gameObject.AddComponent<BattleSceneController>();var zones=new List<BattleZoneView>();
        for(int player=0;player<2;player++)
        {
            var half=Obj(player==0?"NearField":"FarField",root,Vector3.zero);float sign=player==0?-1:1;
            for(int i=0;i<5;i++)
            {float x=(i-2)*8.6f*(player==0?1:-1);zones.Add(Zone(half,DuelZone.Monster,player,i,new Vector3(x,0,sign*9.48f)));zones.Add(Zone(half,DuelZone.SpellTrap,player,i,new Vector3(x,0,sign*18)));}
            zones.Add(Zone(half,DuelZone.Field,player,0,new Vector3(25*sign,0,sign*10)));
            zones.Add(Zone(half,DuelZone.MainDeck,player,0,new Vector3(-26.86f*sign,1.5f,sign*23.93f)));
            zones.Add(Zone(half,DuelZone.ExtraDeck,player,0,new Vector3(26.86f*sign,1.5f,sign*23.93f)));
            zones.Add(Zone(half,DuelZone.Graveyard,player,0,new Vector3(-25.74f*sign,4.8f,sign*14.26f)));
            zones.Add(Zone(half,DuelZone.Banished,player,0,new Vector3(-27.583f*sign,4.8f,sign*8.024f)));
        }
        for(int i=0;i<2;i++)zones.Add(Zone(root,DuelZone.ExtraMonster,-1,i,new Vector3((i==0?-1:1)*8.6f,0,0)));
        var piles=zones.Where(x=>x.Zone.Kind is DuelZone.MainDeck or DuelZone.ExtraDeck).Select(x=>Pile(x,root)).ToArray();
        var graveViews=new List<BattleGraveView>();
        foreach(var zone in zones.Where(x=>x.Zone.Kind is DuelZone.Graveyard or DuelZone.Banished))
        {
            var view=zone.gameObject.AddComponent<BattleGraveView>();Refs(view,"m_renderers",SceneComponents<Renderer>(graves[zone.Zone.Player].transform));
            Ref(view,"m_count",Root<TextMeshPro>(Named(zone.transform,"ZoneLabel").gameObject));Int(view,"m_kind",(int)zone.Zone.Kind);Int(view,"m_player",zone.Zone.Player);graveViews.Add(view);
        }
        var phase=Original("Resources/duel/bg/timer/timer_c001/PhaseButton_c001.prefab",root);phase.transform.localPosition=Vector3.zero;
        var dial=phase.AddComponent<BattlePhaseDial>();var common=Named(phase.transform,"CommonPart");
        Ref(dial,"m_hitbox",Root<BoxCollider>(common.gameObject));
        Ref(dial,"m_main",NativeLabel(Named(common,"TextMain"),"Main1",21));Ref(dial,"m_above",NativeLabel(Named(common,"TextAbove"),"TURN 1",10));Ref(dial,"m_below",NativeLabel(Named(common,"TextBelow"),"",7));
        PositionPhaseLabels(dial);
        Ref(dial,"m_playerPart",Named(phase.transform,"PlayerPart").gameObject);Ref(dial,"m_opponentPart",Named(phase.transform,"OpponentPart").gameObject);Refs(dial,"m_renderers",SceneComponents<Renderer>(phase.transform));
        var timerObject=Original("Resources/duel/bg/timer/timer_c001/Timer_c001.prefab",root);timerObject.transform.localPosition=Vector3.zero;
        var timer=timerObject.AddComponent<BattleTimerView>();Refs(timer,"m_renderers",SceneComponents<Renderer>(timerObject.transform));
        Ref(timer,"m_seconds",NativeLabel(Named(timerObject.transform,"TextMain"),"180",30));
        var guides=new[]{Original("OriginalAdditions/Guide/resourcesassetbundle/duel/bg/timer/playableguide_c001/PlayableGuide_c001_near_mat13.prefab",root),
            Original("OriginalAdditions/Guide/resourcesassetbundle/duel/bg/timer/playableguide_c001/PlayableGuide_c001_far_mat13.prefab",root)};
        foreach(var guide in guides)
        {
            foreach(var animator in SceneComponents<Animator>(guide.transform))animator.enabled=false;
            foreach(var node in SceneComponents<Transform>(guide.transform))
            {
                if(node.name.EndsWith("_change"))node.gameObject.SetActive(false);
                if(node.name.EndsWith("_play")||node.name.EndsWith("_Luminous"))node.localScale=Vector3.one;
            }
        }
        Ref(controller,"m_camera",camera);Ref(controller,"m_events",events);Ref(controller,"m_cardPrefab",Root<BattleCardView>(cardPrefab));Ref(controller,"m_cardRoot",Obj("Cards",root,Vector3.zero));
        Ref(controller,"m_mainCameraData",Root<UniversalAdditionalCameraData>(camera.gameObject));
        Refs(controller,"m_zones",zones);Ref(controller,"m_dial",dial);Ref(controller,"m_timer",timer);Refs(controller,"m_piles",piles);Refs(controller,"m_graves",graveViews);Refs(controller,"m_playableGuides",guides);
        Ref(controller,"m_phaseEffects",PhaseEffects(root,camera));Ref(controller,"m_preset",preset);
        var entry=root.gameObject.AddComponent<SceneEntry>();Int(entry,"m_kind",2);Ref(entry,"m_battle",controller);
        EditorSceneManager.SaveScene(scene,"Assets/Scenes/BattleScene.unity");
        Ref(controller,"m_preset",AssetDatabase.LoadAssetAtPath<DuelDemoPreset>(Art+"BattleDemoPreset.asset"));
        EditorSceneManager.SaveScene(scene,"Assets/Scenes/BattleScene.unity");
        EditorSceneManager.playModeStartScene=AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Scenes/PreInit.unity");
    }
    internal static IEnumerable<T> SceneComponents<T>(Transform transform) where T:Component
    {
        var components=new SerializedObject(transform.gameObject).FindProperty("m_Component");
        for(int i=0;i<components.arraySize;i++)if(components.GetArrayElementAtIndex(i).FindPropertyRelative("component").objectReferenceValue is T component)yield return component;
        foreach(Transform child in transform)foreach(var component in SceneComponents<T>(child))yield return component;
    }
    [MenuItem("Tools/Battle/按指定素材制作战斗界面")]
    public static void Build()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("制作资源前需退出PlayMode");
        for(int i=0;i<SceneManager.sceneCount;i++)if(SceneManager.GetSceneAt(i).isDirty)throw new InvalidOperationException("当前场景有未保存内容");
        Directory.CreateDirectory(Art);Directory.CreateDirectory(Prefabs);AssetDatabase.Refresh();ImportSprites();DisplayAssets();MDPro3BattleImport.PrepareMaterials();
        var preset=BuildPreset();var card=BuildCard();BuildRow();BuildHud();BuildChoices();BuildZoneWindow();BuildSettings();AssetDatabase.SaveAssets();BuildScene(card,preset);
        foreach(string name in new[]{"BattleCardView","BattleCardRow","BattleHudPanel","BattleChoiceWindow","BattleZoneWindow"})AddressableCatalogMenu.AddPrefab(Prefabs+name+".prefab");
        AddressableCatalogMenu.AddToUISettingsCatalog(Prefabs+"BattleUISetting.asset");AddressableCatalogMenu.AddScene("Assets/Scenes/BattleScene.unity");
        AddressableCatalogSetup.MarkFolderInGroup(AddressableCatalogSetup.RemoteUiHallGroup,Sprites.TrimEnd('/'));
        var init=EditorSceneManager.OpenScene("Assets/Scenes/Init.unity",OpenSceneMode.Additive);
        var manager=init.GetRootGameObjects().SelectMany(go=>SceneComponents<SingletonManager>(go.transform)).Single();var so=new SerializedObject(manager);so.FindProperty("m_sceneName").stringValue="BattleScene";so.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.SaveScene(init);EditorSceneManager.CloseScene(init,true);AssetDatabase.SaveAssets();
    }
}
