using System;
using System.Collections.Generic;
using System.Linq;
using AChen.Duel.Presentation;
using SuperScrollView;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public static partial class BattleSceneBuilder
{
    static RectTransform Rect(string name,Transform parent,float x,float y,float width,float height)
    {
        var rect=(RectTransform)new GameObject(name,typeof(RectTransform)).transform;rect.SetParent(parent,false);
        rect.gameObject.layer=5;
        rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(0,1);rect.anchoredPosition=new Vector2(x,-y);rect.sizeDelta=new Vector2(width,height);return rect;
    }
    static void Stretch(RectTransform rect) {rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;}
    static UnityEngine.UI.Image Image(string name,Transform parent,float x,float y,float width,float height,Color color,string sprite="")
    {
        var image=Rect(name,parent,x,y,width,height).gameObject.AddComponent<UnityEngine.UI.Image>();image.color=color;image.raycastTarget=false;
        if(sprite.Length>0){image.sprite=Sprite(sprite);image.type=UnityEngine.UI.Image.Type.Sliced;}return image;
    }
    static TextMeshProUGUI Text(string name,Transform parent,float x,float y,float width,float height,string value,float size)
    {
        var text=Rect(name,parent,x,y,width,height).gameObject.AddComponent<TextMeshProUGUI>();text.font=s_font;text.text=value;
        text.fontSize=size;text.color=new Color(.93f,.94f,.85f);text.alignment=TextAlignmentOptions.MidlineLeft;
        text.overflowMode=TextOverflowModes.Ellipsis;text.raycastTarget=false;return text;
    }
    static UnityEngine.UI.Button Button(string name,Transform parent,float x,float y,float width,float height,string label)
    {
        var image=Image("Btn_"+name,parent,x,y,width,height,new Color(.09f,.16f,.21f),"GUI_CommonButtonL");image.raycastTarget=true;
        var button=image.gameObject.AddComponent<UnityEngine.UI.Button>();button.targetGraphic=image;
        var colors=button.colors;colors.highlightedColor=new Color(1.2f,1.2f,1.1f);colors.pressedColor=new Color(.8f,.85f,.6f);
        colors.disabledColor=new Color(.45f,.48f,.4f,.65f);button.colors=colors;
        var text=Text("Txt_"+name,image.transform,8,0,width-16,height,label,20);text.alignment=TextAlignmentOptions.Center;
        return button;
    }
    static UiScreenGenerator Generate(GameObject root,string name,bool window)
    {
        var generator=root.AddComponent<UiScreenGenerator>();var so=new SerializedObject(generator);
        so.FindProperty("m_className").stringValue=name;so.FindProperty("m_folderPath").stringValue=Scripts;
        so.FindProperty("m_kind").enumValueIndex=window?1:0;so.ApplyModifiedPropertiesWithoutUndo();
        generator.CollectUiBinds();generator.RebuildUiBinds();return generator;
    }
    static RectTransform SafeArea(Transform parent)
    {
        var area=Rect("SafeArea",parent,0,0,1706,960);Stretch(area);
        var safe=area.gameObject.AddComponent<BattleSafeArea>();Ref(safe,"m_root",area);return area;
    }
    static void Place(RectTransform rect, float x, float y, float width, float height)
    {
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = new Vector2(x, -y); rect.sizeDelta = new Vector2(width, height);
    }
    static void BuildHud()
    {
        var root = Rect("BattleHudPanel", null, 0, 0, 1706, 960); Stretch(root);
        var screen = root.gameObject.AddComponent<BattleHudPanel>(); var safe = SafeArea(root);
        var names = new List<TextMeshProUGUI>(); var life = new List<TextMeshProUGUI>(); var portraits = new List<AvatarPortraitView>();
        for (int i = 0; i < 2; i++)
        {
            bool near = i == 0;
            var player = Image(near ? "Player" : "Opponent", safe, 0, 0, 380, 108, Color.white, near ? "GUI_LP_Blue" : "GUI_LP_Red");
            player.type = UnityEngine.UI.Image.Type.Simple;
            player.rectTransform.anchorMin = player.rectTransform.anchorMax = player.rectTransform.pivot = near ? Vector2.zero : Vector2.one;
            player.rectTransform.anchoredPosition = near ? new Vector2(22, 90) : new Vector2(-22, -16);
            Image("NameBase", player.transform, near ? 105 : 0, 0, 275, 32, new Color(0, 0, 0, .95f));
            names.Add(Text("PlayerName", player.transform, near ? 118 : 12, 0, 250, 32, "", 22));
            Text("LifeLabel", player.transform, near ? 112 : 16, 44, 55, 55, "LP", 31);
            life.Add(Text("PlayerLife", player.transform, near ? 174 : 80, 40, 176, 58, "8000", 48));
            var portraitObject = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/UI/Prefab/Hall/Profile/AvatarPortrait.prefab"), player.transform);
            portraitObject.name = "Portrait"; Place((RectTransform)portraitObject.transform, near ? 0 : 280, 0, 106, 106);
            foreach (var graphic in SceneComponents<UnityEngine.UI.Graphic>(portraitObject.transform)) graphic.raycastTarget = false;
            portraits.Add(Root<AvatarPortraitView>(portraitObject));
        }
        Refs(screen, "m_playerNames", names); Refs(screen, "m_playerLPs", life); Refs(screen, "m_portraits", portraits);
        var turn = Text("Txt_Turn", safe, 0, 0, 560, 30, "", 18);
        turn.rectTransform.anchorMin = turn.rectTransform.anchorMax = new Vector2(.5f, 1);
        turn.rectTransform.pivot = new Vector2(.5f, 1); turn.rectTransform.anchoredPosition = new Vector2(0, -12); turn.alignment = TextAlignmentOptions.Center;
        var hintPanel = Image("TargetHint", safe, 0, 0, 940, 70, new Color(.04f, .13f, .18f, .98f), "GUI_CardInfo_NameBase");
        hintPanel.rectTransform.anchorMin = hintPanel.rectTransform.anchorMax = hintPanel.rectTransform.pivot = new Vector2(.5f, .76f);
        hintPanel.rectTransform.anchoredPosition = Vector2.zero;
        var hint = Text("Txt_Hint", safe, 0, 0, 820, 66, "", 27);
        hint.rectTransform.anchorMin = hint.rectTransform.anchorMax = hint.rectTransform.pivot = new Vector2(.5f, .76f);
        hint.rectTransform.anchoredPosition = new Vector2(-42, 0); hint.alignment = TextAlignmentOptions.Center;
        var cancel = Button("ActionCancel", hintPanel.transform, 830, 12, 94, 46, "取消"); cancel.name = "CancelActionButton";
        Ref(screen, "m_targetHint", hintPanel.gameObject); Ref(screen, "m_cancelAction", cancel);
        var toolbar = Rect("DebugToolbar", safe, 0, 0, 1666, 62); toolbar.anchorMin = new Vector2(0, 0); toolbar.anchorMax = new Vector2(1, 0);
        toolbar.pivot = Vector2.zero; toolbar.offsetMin = new Vector2(14, 10); toolbar.offsetMax = new Vector2(-14, 72);
        var scroll = toolbar.gameObject.AddComponent<UnityEngine.UI.ScrollRect>(); scroll.horizontal = true; scroll.vertical = false;
        scroll.movementType = UnityEngine.UI.ScrollRect.MovementType.Clamped;
        var viewport = Rect("Viewport", toolbar, 0, 0, 1666, 62); Stretch(viewport); viewport.gameObject.AddComponent<UnityEngine.UI.RectMask2D>();
        var bg = viewport.gameObject.AddComponent<UnityEngine.UI.Image>(); bg.color = new Color(.02f, .045f, .065f, .96f);
        var content = Rect("Content", viewport, 8, 0, 2500, 62); scroll.viewport = viewport; scroll.content = content;
        var buttons = new[] { ("Player", "操作我方", 100f), ("Source", "手牌", 100f), ("Card", "选择下一张", 220f), ("View", "查看", 80f), ("Draw", "抽牌", 80f),
            ("Place", "入场", 80f), ("Grave", "入墓", 80f), ("Banished", "除外", 80f), ("Hand", "回手", 80f), ("Deck", "回卡组", 100f), ("Extra", "回额外", 100f),
            ("Position", "表示", 80f), ("Highlight", "卡牌高亮", 110f), ("Pile", "区域高亮", 110f), ("Phase", "阶段", 80f), ("Turn", "结束回合", 110f),
            ("Pause", "暂停计时", 110f), ("Clock", "重置计时", 110f), ("SkipAnimation", "跳过动画", 110f), ("Reset", "重置演示", 110f) };
        float x = 0;
        foreach (var entry in buttons)
        {
            var button = Button(entry.Item1, content, x, 7, entry.Item3, 48, entry.Item2);
            if (entry.Item1 == "Card") Root<TextMeshProUGUI>(button.transform.GetChild(0).gameObject).name = "Txt_Selection";
            if (entry.Item1 == "Turn") Root<TextMeshProUGUI>(button.transform.GetChild(0).gameObject).name = "TurnCaption";
            if (entry.Item1 == "SkipAnimation") { button.name = "SkipAnimationButton"; Ref(screen, "m_skipAnimation", button); }
            x += entry.Item3 + 6;
        }
        content.sizeDelta = new Vector2(x + 8, 62);
        BuildDetailSidebar(safe, screen); BuildCardActions(root);
        Generate(root.gameObject, "BattleHudPanel", false); Save(root.gameObject, "BattleHudPanel");
    }
    static void BuildDetailSidebar(Transform parent, BattleHudPanel screen)
    {
        var detail = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/UI/Prefab/Hall/Shop/CardDetailOverlay.prefab"), parent);
        detail.name = "BattleDetailSidebar";
        UnityEngine.Object.DestroyImmediate(Root<CardDetailWindow>(detail));
        var detailRect = (RectTransform)detail.transform;
        detailRect.anchorMin = new Vector2(0, .24f); detailRect.anchorMax = new Vector2(0, .94f); detailRect.pivot = new Vector2(0, 1);
        detailRect.offsetMin = new Vector2(20, 0); detailRect.offsetMax = new Vector2(408, 0);
        var view = Root<CardDetailView>(detail);
        var safe = detail.transform.Find("SafeArea"); UnityEngine.Object.DestroyImmediate(Root<SafeAreaAdapter>(safe.gameObject));
        Stretch((RectTransform)safe);
        var background = detail.AddComponent<UnityEngine.UI.Image>(); background.sprite = Sprite("GUI_CardInfo_NameBase");
        background.type = UnityEngine.UI.Image.Type.Sliced; background.color = new Color(.035f, .075f, .095f, .99f); background.raycastTarget = true;
        var opaque = Image("SidebarBackdrop", detail.transform, 0, 0, 388, 674, new Color(.015f, .025f, .04f, 1));
        Stretch(opaque.rectTransform); opaque.rectTransform.SetAsFirstSibling(); opaque.raycastTarget = true;
        var info = (RectTransform)safe.Find("Go_Info"); Stretch(info); info.offsetMin = new Vector2(12, 12); info.offsetMax = new Vector2(-12, -8);
        Place((RectTransform)safe.Find("Raw_Card"), 16, 62, 145, 211);
        Ref(view, "m_BtnCard", Root<UnityEngine.UI.Button>(safe.Find("Raw_Card").gameObject));
        Place((RectTransform)info.Find("Img_NameBase"), 0, 0, 322, 44);
        Root<TextMeshProUGUI>(info.Find("Img_NameBase/Txt_Name").gameObject).fontSize = 26;
        var level = (RectTransform)info.Find("Go_LevelRow"); Place(level, 163, 56, 187, 40);
        var levelLayout = Root<UnityEngine.UI.HorizontalLayoutGroup>(level.gameObject); levelLayout.spacing = 3; levelLayout.padding = new RectOffset();
        Root<UnityEngine.UI.Image>(level.gameObject).color = Color.clear;
        foreach (Transform child in level) ((RectTransform)child).sizeDelta = new Vector2(child.name == "Txt_Level" ? 40 : 27, 30);
        var stats = (RectTransform)info.Find("Go_StatRow"); Place(stats, 163, 105, 187, 92);
        Root<UnityEngine.UI.HorizontalLayoutGroup>(stats.gameObject).enabled = false; Root<UnityEngine.UI.Image>(stats.gameObject).color = Color.clear;
        Place((RectTransform)stats.Find("Go_Atk"), 0, 0, 185, 40); Place((RectTransform)stats.Find("Go_Def"), 0, 44, 185, 40);
        Place((RectTransform)info.Find("Img_TypeBar"), 0, 284, 364, 40);
        var desc = (RectTransform)info.Find("Scr_Desc"); Stretch(desc); desc.offsetMin = new Vector2(0, 0); desc.offsetMax = new Vector2(0, -338);
        Root<TextMeshProUGUI>(info.Find("Scr_Desc/Viewport/Txt_Desc").gameObject).fontSize = 23;
        var navigation = Rect("NavigationHidden", safe, 0, 0, 1, 1);
        safe.Find("Btn_Prev").SetParent(navigation, false); safe.Find("Btn_Next").SetParent(navigation, false); navigation.gameObject.SetActive(false);
        var close = Button("SidebarClose", safe, 337, 5, 43, 37, "×"); close.name = "SidebarCloseButton";
        Ref(view, "m_BtnDim", close); UnityEngine.Object.DestroyImmediate(detail.transform.Find("Img_Dim").gameObject);
        Ref(screen, "m_detail", view);
        string[] detailPrefixes = { "Btn_", "Txt_", "Img_", "Raw_", "Go_", "Tog_", "Sld_", "Inp_", "Scr_", "Drop_" };
        foreach (var child in SceneComponents<Transform>(detail.transform))
            if (detailPrefixes.Any(prefix => child.name.StartsWith(prefix, StringComparison.Ordinal))) child.name = "Detail" + child.name;
        detail.SetActive(false);
    }
    static void BuildCardActions(RectTransform hud)
    {
        var host = Rect("CardActionsHost", hud, 0, 0, 1706, 960); Stretch(host); var actions = host.gameObject.AddComponent<BattleCardActionsView>();
        var menu = Rect("CardActions", host, 0, 0, 768, 145); menu.anchorMin = menu.anchorMax = menu.pivot = new Vector2(.5f, .5f);
        var buttons = new List<UnityEngine.UI.Button>(); var icons = new List<UnityEngine.UI.Image>(); var labels = new List<TextMeshProUGUI>();
        for (int i = 0; i < 7; i++)
        {
            var image = Image("Action" + i, menu, i * 128, 0, 112, 112, Color.white);
            image.type = UnityEngine.UI.Image.Type.Simple; image.raycastTarget = true;
            var button = image.gameObject.AddComponent<UnityEngine.UI.Button>(); button.targetGraphic = image; button.transition = UnityEngine.UI.Selectable.Transition.SpriteSwap;
            Image("CaptionBase", image.transform, -8, 111, 128, 31, new Color(0, 0, 0, .76f));
            var caption = Text("Caption", image.transform, -8, 112, 128, 30, "", 20); caption.alignment = TextAlignmentOptions.Center;
            buttons.Add(button); icons.Add(image); labels.Add(caption);
        }
        var kindIcons = new[] { "05", "11", "06", "04", "07", "13", "08", "06" }
            .SelectMany(id => Enumerable.Range(1, 4).Select(state => Sprite("GUI_T_DuelButtonActIcon" + id + "_" + state))).ToArray();
        Ref(actions, "m_menu", menu); Refs(actions, "m_buttons", buttons); Refs(actions, "m_icons", icons); Refs(actions, "m_labels", labels); Refs(actions, "m_kindSprites", kindIcons);
        Ref(Root<BattleHudPanel>(hud.gameObject), "m_actions", actions);
    }
    static void BuildChoices()
    {
        var root = Rect("BattleChoiceWindow", null, 0, 0, 1706, 960); Stretch(root); var screen = root.gameObject.AddComponent<BattleChoiceWindow>();
        var safe = SafeArea(root); var panel = Image("ChoicePanel", safe, 0, 0, 1090, 370, new Color(.035f, .075f, .11f, .99f), "GUI_CardInfo_NameBase");
        panel.rectTransform.anchorMin = panel.rectTransform.anchorMax = panel.rectTransform.pivot = new Vector2(.5f, .5f);
        panel.rectTransform.anchoredPosition = Vector2.zero; panel.raycastTarget = true;
        Text("Txt_Title", panel.transform, 38, 22, 1014, 48, "", 29).alignment = TextAlignmentOptions.Center;
        var positionGroup = Rect("PositionChoices", panel.transform, 0, 80, 1090, 166);
        var options = new List<UnityEngine.UI.Button>(); var labels = new List<TextMeshProUGUI>();
        for (int i = 0; i < 3; i++)
        {
            var button = Button("Option" + i, positionGroup, 55 + i * 330, 18, 300, 104, "");
            options.Add(button); labels.Add(Root<TextMeshProUGUI>(button.transform.GetChild(0).gameObject));
        }
        var phaseGroup = Rect("PhaseChoices", panel.transform, 0, 80, 1090, 230);
        var phases = new List<UnityEngine.UI.Button>(); var cursors = new List<UnityEngine.UI.Image>();
        string[] phaseImages = { "GUI_PhaseDP", "GUI_PhaseSP", "GUI_PhaseMP1", "GUI_PhaseBP", "GUI_PhaseMP2", "GUI_PhaseEP" };
        for (int i = 0; i < 6; i++)
        {
            var image = Image("Phase" + i, phaseGroup, 62 + i * 162, 0, 144, 144, new Color(.12f, .31f, .41f), "GUI_PhaseButtonBase2");
            image.type = UnityEngine.UI.Image.Type.Simple; image.raycastTarget = true;
            var button = image.gameObject.AddComponent<UnityEngine.UI.Button>(); button.targetGraphic = image;
            var cursor = Image("SelectedPhase", image.transform, 0, 0, 144, 144, new Color(.55f, .9f, 1f), "GUI_PhaseButtonCursor2"); cursor.type = UnityEngine.UI.Image.Type.Simple;
            var icon = Image("PhaseIcon", image.transform, 28, 29, 88, 75, Color.white, phaseImages[i]); icon.type = UnityEngine.UI.Image.Type.Simple;
            Text("PhaseCaption", image.transform, -6, 150, 156, 30, BattleLabels.Phase((DuelPhase)i), 21).alignment = TextAlignmentOptions.Center;
            phases.Add(button); cursors.Add(cursor);
        }
        phases.Add(Button("EndTurn", phaseGroup, 429, 190, 230, 46, "结束回合"));
        Button("Cancel", panel.transform, 309, 310, 230, 46, "取消"); Button("Reset", panel.transform, 560, 310, 230, 46, "重置演示");
        Refs(screen, "m_options", options); Refs(screen, "m_labels", labels); Refs(screen, "m_phaseOptions", phases); Refs(screen, "m_phaseCursors", cursors);
        Ref(screen, "m_positionGroup", positionGroup.gameObject); Ref(screen, "m_phaseGroup", phaseGroup.gameObject);
        Generate(root.gameObject, "BattleChoiceWindow", true); Save(root.gameObject, "BattleChoiceWindow");
    }
    static void BuildRow()
    {
        var root = Rect("BattleCardRow", null, 0, 0, 370, 118); var row = root.gameObject.AddComponent<BattleCardRow>(); root.gameObject.AddComponent<LoopListViewItem2>();
        var background = root.gameObject.AddComponent<UnityEngine.UI.Image>(); background.color = new Color(.035f, .065f, .085f, .98f);
        var button = root.gameObject.AddComponent<UnityEngine.UI.Button>(); button.targetGraphic = background;
        var art = Rect("Art", root, 10, 6, 71, 104).gameObject.AddComponent<UnityEngine.UI.RawImage>(); art.raycastTarget = false;
        Ref(row, "m_root", root); Ref(row, "m_button", button); Ref(row, "m_art", art); Ref(row, "m_background", background);
        Ref(row, "m_name", Text("Name", root, 94, 14, 260, 57, "", 22)); Ref(row, "m_info", Text("Info", root, 94, 82, 260, 24, "", 16));
        Save(root.gameObject, "BattleCardRow");
    }
    static void BuildZoneWindow()
    {
        var root = Rect("BattleZoneWindow", null, 0, 0, 1706, 960); Stretch(root); var screen = root.gameObject.AddComponent<BattleZoneWindow>();
        var safe = SafeArea(root); var panel = Image("ZonePanel", safe, 0, 0, 410, 670, new Color(.035f, .075f, .11f, .99f), "GUI_CardInfo_NameBase");
        panel.rectTransform.anchorMin = panel.rectTransform.anchorMax = new Vector2(1, .5f); panel.rectTransform.pivot = new Vector2(1, .5f);
        panel.rectTransform.anchoredPosition = new Vector2(-18, 24); panel.raycastTarget = true;
        Text("Txt_Title", panel.transform, 18, 16, 310, 42, "", 24);
        Button("Close", panel.transform, 339, 14, 58, 42, "关闭");
        Text("Txt_Empty", panel.transform, 20, 272, 370, 66, "该区域没有卡牌", 23).alignment = TextAlignmentOptions.Center;
        var listRoot = Rect("CardList", panel.transform, 18, 76, 374, 576); var scroll = listRoot.gameObject.AddComponent<UnityEngine.UI.ScrollRect>();
        scroll.horizontal = false; scroll.vertical = true; scroll.movementType = UnityEngine.UI.ScrollRect.MovementType.Clamped;
        var viewport = Rect("Viewport", listRoot, 0, 0, 374, 576); Stretch(viewport); viewport.gameObject.AddComponent<UnityEngine.UI.RectMask2D>();
        var bg = viewport.gameObject.AddComponent<UnityEngine.UI.Image>(); bg.color = new Color(0, 0, 0, .01f);
        var content = Rect("Content", viewport, 0, 0, 374, 576); scroll.viewport = viewport; scroll.content = content;
        var loop = listRoot.gameObject.AddComponent<LoopListView2>(); Int(loop, "mArrangeType", 0);
        var list = listRoot.gameObject.AddComponent<GridListController>(); Ref(list, "loopListView", loop); Ref(screen, "m_list", list);
        Generate(root.gameObject, "BattleZoneWindow", false); Save(root.gameObject, "BattleZoneWindow");
    }
    static UIFrame BuildBattleUIFrame()
    {
        var frameRoot = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/UI/Prefab/BaseUI/UIFrame.prefab"));
        frameRoot.name = "BattleUIFrame";
        UnityEngine.Object.DestroyImmediate(frameRoot.transform.Find("EventSystem").gameObject);
        var camera = Root<Camera>(frameRoot.transform.Find("UICamera").gameObject);
        camera.cullingMask = 1 << 5;
        Root<UniversalAdditionalCameraData>(camera.gameObject).renderType = CameraRenderType.Overlay;
        var canvas = Root<Canvas>(frameRoot);
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = camera;
        canvas.planeDistance = 10f;
        frameRoot.transform.Find("PanelLayer/bg").gameObject.SetActive(false);
        return Root<UIFrame>(Save(frameRoot, "BattleUIFrame"));
    }
    public static void RebuildBattleUIFrame()
    {
        BuildBattleUIFrame();
        AssetDatabase.SaveAssets();
    }
    static UISettings BuildSettings()
    {
        var frame = BuildBattleUIFrame();
        var settings=ScriptableObject.CreateInstance<UISettings>();var so=new SerializedObject(settings);
        so.FindProperty("templateUIPrefab").objectReferenceValue=frame;
        var screens=new[]{"BattleHudPanel","BattleChoiceWindow","BattleZoneWindow"}.Select(name=>AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs+name+".prefab"))
            .Concat(new[]{AssetDatabase.LoadAssetAtPath<GameObject>("Assets/UI/Prefab/Hall/Shop/CardZoomWindow.prefab")}).ToArray();
        var array=so.FindProperty("screensToRegister");array.arraySize=screens.Length;
        for(int i=0;i<screens.Length;i++)array.GetArrayElementAtIndex(i).objectReferenceValue=screens[i];so.ApplyModifiedPropertiesWithoutUndo();
        return Store(settings,Prefabs+"BattleUISetting.asset");
    }
    public static void RebindScreens()
    {
        foreach(string name in new[]{"BattleHudPanel","BattleChoiceWindow","BattleZoneWindow"})
        {var root=PrefabUtility.LoadPrefabContents(Prefabs+name+".prefab");Root<UiScreenGenerator>(root).CollectUiBinds();
         PrefabUtility.SaveAsPrefabAsset(root,Prefabs+name+".prefab");PrefabUtility.UnloadPrefabContents(root);}
        AssetDatabase.SaveAssets();
    }
}
