using System;
using System.IO;
using System.Linq;
using AChen.Duel.Presentation;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static partial class BattleSceneBuilder
{
    const string DesktopUI = "C:/Users/ldc20/OneDrive/Desktop/UI/Sprites/CommonUI/";
    static void ImportOfflineSprite(string name, bool frame = false)
    {
        string path = Sprites + name + ".png";
        if (!File.Exists(path)) File.Copy(DesktopUI + name + ".png", path);
        AssetDatabase.ImportAsset(path);
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
        importer.mipmapEnabled = false; importer.alphaIsTransparency = true;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.spritePixelsPerUnit = 100;
        if (frame) importer.spriteBorder = name == "GUI_Global_Window_Frame_NoCorner" ? new Vector4(2,2,2,2) : new Vector4(16,16,16,16);
        importer.SaveAndReimport();
    }
    static BattleSelectionItem OfflineSelectionItem()
    {
        var root = Rect("BattleSelectionItem", null, 0, 0, 160, 240);
        var item = root.gameObject.AddComponent<BattleSelectionItem>();
        var background = Image("Background", root, 0, 0, 160, 230, new Color(.025f, .045f, .08f, .85f));
        background.raycastTarget = true;
        var button = root.gameObject.AddComponent<UnityEngine.UI.Button>(); button.targetGraphic = background;
        var art = Rect("CardArt", root, 10, 8, 140, 205).gameObject.AddComponent<UnityEngine.UI.RawImage>(); art.raycastTarget = false;
        var selected = Image("Selected", root, 1, 1, 158, 228, new Color(.45f, .7f, 1), "GUI_CommonSelectCursor_CornerAll");
        selected.type = UnityEngine.UI.Image.Type.Simple;
        var label = Text("Label", root, 6, 228, 148, 60, "", 20); label.alignment = TextAlignmentOptions.Center;
        label.enableWordWrapping = true;
        var order = Text("Order", root, 114, 8, 38, 36, "", 25); order.alignment = TextAlignmentOptions.Center;
        Ref(item, "m_button", button); Ref(item, "m_art", art); Ref(item, "m_label", label);
        Ref(item, "m_back", Sprite("DefaultProtector").texture);
        Ref(item, "m_order", order); Ref(item, "m_selected", selected.gameObject);
        return Root<BattleSelectionItem>(Save(root.gameObject, "BattleSelectionItem"));
    }
    static GameObject OfflineSelectionPanel(BattleSelectionItem item)
    {
        var root = Rect("BattleSelectionPanel", null, 0, 0, 1706, 960); Stretch(root);
        var screen = root.gameObject.AddComponent<BattleSelectionPanel>(); Int(screen, "priority", (int)PanelPriority.Prioritary);
        var safe = SafeArea(root);
        var body=Image("ResponseFrame",safe,0,0,1060,366,new Color(.008f,.012f,.018f,.9f),"GUI_CommonWindowS");
        body.rectTransform.anchorMin=new Vector2(.23f,0);body.rectTransform.anchorMax=new Vector2(.77f,0);
        body.rectTransform.pivot=new Vector2(.5f,0);body.rectTransform.sizeDelta=new Vector2(0,366);
        body.rectTransform.anchoredPosition=Vector2.zero;body.raycastTarget=true;
        var border=Image("Frame",body.transform,0,0,1060,366,new Color(.5f,.52f,.55f,.85f),"GUI_Global_Window_Frame_NoCorner");Stretch(border.rectTransform);
        var title=Text("Txt_Title",body.transform,20,8,1020,40,"",25);title.alignment=TextAlignmentOptions.Center;
        title.rectTransform.anchorMin=new Vector2(.02f,1);title.rectTransform.anchorMax=new Vector2(.98f,1);
        title.rectTransform.sizeDelta=new Vector2(0,40);title.rectTransform.anchoredPosition=new Vector2(0,-8);title.enableWordWrapping=true;
        var content=Rect("CardChoices",body.transform,0,50,1020,220);content.anchorMin=new Vector2(0,1);content.anchorMax=new Vector2(1,1);
        content.pivot=new Vector2(.5f,1);content.sizeDelta=new Vector2(-40,220);content.anchoredPosition=new Vector2(0,-50);
        var count=Text("Txt_Count",body.transform,0,265,740,24,"",18);count.gameObject.SetActive(false);
        var page=Text("Txt_Page",body.transform,0,265,126,24,"",17);page.alignment=TextAlignmentOptions.Center;
        page.rectTransform.anchorMin=page.rectTransform.anchorMax=page.rectTransform.pivot=new Vector2(.5f,1);page.rectTransform.anchoredPosition=new Vector2(0,-265);
        var previous=Button("Previous",body.transform,0,265,52,24,"‹");var next=Button("Next",body.transform,0,265,52,24,"›");
        foreach(var pair in new[]{(previous,-100f),(next,100f)}){var r=(RectTransform)pair.Item1.transform;r.anchorMin=r.anchorMax=r.pivot=new Vector2(.5f,1);r.anchoredPosition=new Vector2(pair.Item2,-265);}
        var cancel=Button("Cancel",body.transform,0,297,360,55,"取消");var confirm=Button("Confirm",body.transform,0,297,360,55,"选择");
        var cr=(RectTransform)cancel.transform;cr.anchorMin=new Vector2(.13f,1);cr.anchorMax=new Vector2(.44f,1);cr.sizeDelta=new Vector2(0,55);cr.anchoredPosition=new Vector2(0,-297);
        var rr=(RectTransform)confirm.transform;rr.anchorMin=new Vector2(.56f,1);rr.anchorMax=new Vector2(.87f,1);rr.sizeDelta=new Vector2(0,55);rr.anchoredPosition=new Vector2(0,-297);
        foreach(var button in new[]{cancel,confirm})
        {
            var graphic=(UnityEngine.UI.Image)button.targetGraphic;graphic.color=new Color(.01f,.012f,.015f);
            var outline=button.gameObject.AddComponent<UnityEngine.UI.Outline>();outline.effectColor=new Color(.55f,.55f,.55f);outline.effectDistance=new Vector2(2,-2);
            var label=Root<TextMeshProUGUI>(button.transform.GetChild(0).gameObject);label.fontSize=24;label.color=new Color(.8f,1,0);Stretch(label.rectTransform);
        }
        Ref(screen,"m_confirmLabel",Root<TextMeshProUGUI>(confirm.transform.GetChild(0).gameObject));
        var searchRoot = Rect("Search", body.transform, 0, 40, 640, 30);
        searchRoot.anchorMin=searchRoot.anchorMax=searchRoot.pivot=new Vector2(.5f,1);searchRoot.anchoredPosition=new Vector2(0,-42);
        var searchBg = searchRoot.gameObject.AddComponent<UnityEngine.UI.Image>(); searchBg.color = new Color(.04f, .07f, .1f);
        var searchText = Text("SearchText", searchRoot, 10, 0, 620, 38, "", 22);
        var search = searchRoot.gameObject.AddComponent<TMP_InputField>(); search.textViewport = searchRoot; search.textComponent = searchText;
        search.targetGraphic = searchBg;
        var reset = Button("Reset", safe, 0, 0, 166, 46, "整局重置");
        var resetRect = (RectTransform)reset.transform; resetRect.anchorMin = resetRect.anchorMax = resetRect.pivot = Vector2.one;
        resetRect.anchoredPosition = new Vector2(-24, -125);
        Ref(screen, "m_title", title); Ref(screen, "m_count", count); Ref(screen, "m_page", page);
        Ref(screen, "m_cancel", cancel); Ref(screen, "m_confirm", confirm); Ref(screen, "m_previous", previous);
        Ref(screen, "m_next", next); Ref(screen, "m_search", search); Ref(screen, "m_reset", reset);
        Ref(screen, "m_itemPrefab", item); Ref(screen, "m_content", content);
        Generate(root.gameObject, nameof(BattleSelectionPanel), false);
        return Save(root.gameObject, nameof(BattleSelectionPanel));
    }
    static BattleChainBadge OfflineChainBadge()
    {
        var root = Obj("BattleChainBadge", null, Vector3.zero); var badge = root.gameObject.AddComponent<BattleChainBadge>();
        var material = Store(new Material(Shader.Find("TCG/Battle/OriginalSprite")), Art + "BattleChainSprite.mat");
        var ring = Obj("Ring", root, Vector3.zero); var renderer = ring.gameObject.AddComponent<SpriteRenderer>();
        renderer.sprite = Sprite("_images_chain_01"); material.SetTexture("_BaseMap", renderer.sprite.texture); EditorUtility.SetDirty(material); renderer.sharedMaterial = material;
        ring.localScale = Vector3.one * 5f / renderer.sprite.bounds.size.y;
        var properties = new MaterialPropertyBlock(); properties.SetTexture("_BaseMap", renderer.sprite.texture); renderer.SetPropertyBlock(properties);
        var digit = Obj("DigitTemplate", root, Vector3.zero).gameObject.AddComponent<SpriteRenderer>();
        digit.sharedMaterial = material; digit.sortingOrder = 2; digit.gameObject.SetActive(false);
        Ref(badge, "m_digitPrefab", digit); Ref(badge, "m_ring", ring);
        Refs(badge, "m_digits", Enumerable.Range(0, 10).Select(i => Sprite("ChainNumSet_" + i)));
        return Root<BattleChainBadge>(Save(root.gameObject, "BattleChainBadge"));
    }
    public static string UpgradeOfflineBattle()
    {
        foreach (string name in new[] { "_images_chain_01", "GUI_Attack_Arrow", "GUI_Global_Window_Frame_NoCorner", "GUI_CommonWindowS" })
            ImportOfflineSprite(name, name.StartsWith("GUI_Global"));
        foreach (int i in Enumerable.Range(0, 10)) ImportOfflineSprite("ChainNumSet_" + i);
        foreach (int i in Enumerable.Range(1, 4)) ImportOfflineSprite("GUI_T_DuelButtonActIcon01_" + i);
        DisplayAssets();
        var panel = OfflineSelectionPanel(OfflineSelectionItem()); var badge = OfflineChainBadge();
        var hud = PrefabUtility.LoadPrefabContents(Prefabs + "BattleHudPanel.prefab");
        var hudScreen = Root<BattleHudPanel>(hud); var hs = new SerializedObject(hudScreen);
        var reset = (UnityEngine.UI.Button)hs.FindProperty("m_BtnReset").objectReferenceValue;
        reset.transform.SetParent(hud.transform, false);
        hud.transform.Find("SafeArea/DebugToolbar").gameObject.SetActive(false);
        var resetRect = (RectTransform)reset.transform; resetRect.anchorMin = resetRect.anchorMax = resetRect.pivot = Vector2.one;
        resetRect.anchoredPosition = new Vector2(-24, -125); resetRect.sizeDelta = new Vector2(166, 46);
        ((TMP_Text)hs.FindProperty("m_TxtReset").objectReferenceValue).text = "整局重置";
        var actions = (BattleCardActionsView)hs.FindProperty("m_actions").objectReferenceValue;
        var icons = new SerializedObject(actions).FindProperty("m_kindSprites");
        var originalIcons = Enumerable.Range(0, icons.arraySize).Select(i => icons.GetArrayElementAtIndex(i).objectReferenceValue).ToArray();
        Refs(actions, "m_kindSprites", originalIcons.Take(32).Concat(Enumerable.Range(1, 4).Select(i => Sprite("GUI_T_DuelButtonActIcon01_" + i))));
        PrefabUtility.SaveAsPrefabAsset(hud, Prefabs + "BattleHudPanel.prefab"); PrefabUtility.UnloadPrefabContents(hud);
        var choices = PrefabUtility.LoadPrefabContents(Prefabs + "BattleChoiceWindow.prefab");
        var choiceScreen = Root<BattleChoiceWindow>(choices);
        foreach(var old in SceneComponents<Transform>(choices.transform).Where(t=>t.name=="Btn_Surrender").ToArray())UnityEngine.Object.DestroyImmediate(old.gameObject);
        var choicePanel = choices.transform.Find("SafeArea/ChoicePanel");
        var surrender = Button("Surrender", choicePanel, 24, 310, 230, 46, "投降");
        Ref(choiceScreen, "m_surrender", surrender);
        var choiceRefs = new SerializedObject(choiceScreen);
        ((TMP_Text)choiceRefs.FindProperty("m_TxtReset").objectReferenceValue).text = "整局重置";
        Root<UiScreenGenerator>(choices).RebuildUiBinds();
        PrefabUtility.SaveAsPrefabAsset(choices, Prefabs + "BattleChoiceWindow.prefab"); PrefabUtility.UnloadPrefabContents(choices);
        var settings = AssetDatabase.LoadAssetAtPath<UISettings>(Prefabs + "BattleUISetting.asset");
        var ss = new SerializedObject(settings); var screens = ss.FindProperty("screensToRegister");
        if (!Enumerable.Range(0, screens.arraySize).Any(i => screens.GetArrayElementAtIndex(i).objectReferenceValue == panel))
        { screens.arraySize++; screens.GetArrayElementAtIndex(screens.arraySize - 1).objectReferenceValue = panel; }
        ss.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(settings);
        var preset = AssetDatabase.LoadAssetAtPath<DuelDemoPreset>(Art + "BattleDemoPreset.asset");
        var ps = new SerializedObject(preset);
        foreach (var pair in new[] { ("m_main", "m_opponentMain"), ("m_extra", "m_opponentExtra") })
        {
            var from = ps.FindProperty(pair.Item1); var to = ps.FindProperty(pair.Item2); to.arraySize = from.arraySize;
            for (int i = 0; i < from.arraySize; i++)
            { var a = from.GetArrayElementAtIndex(i); var b = to.GetArrayElementAtIndex(i);
              b.FindPropertyRelative("CardId").stringValue = a.FindPropertyRelative("CardId").stringValue;
              b.FindPropertyRelative("SourcePool").stringValue = a.FindPropertyRelative("SourcePool").stringValue;
              b.FindPropertyRelative("Count").intValue = a.FindPropertyRelative("Count").intValue; }
        }
        ps.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(preset);
        var scene = SceneManager.GetSceneByPath("Assets/Scenes/BattleScene.unity");
        if (!scene.isLoaded) scene = EditorSceneManager.OpenScene("Assets/Scenes/BattleScene.unity", OpenSceneMode.Additive);
        var controller = scene.GetRootGameObjects().SelectMany(go => SceneComponents<BattleSceneController>(go.transform)).Single();
        var camera = (Camera)new SerializedObject(controller).FindProperty("m_camera").objectReferenceValue;
        foreach(var old in SceneComponents<Transform>(controller.transform).Where(t=>t.name=="OfflineBattleEffects").ToArray())UnityEngine.Object.DestroyImmediate(old.gameObject);
        var effects = Obj("OfflineBattleEffects", controller.transform, Vector3.zero);
        var arrow = Obj("AttackArrow", effects, Vector3.zero); var filter = arrow.gameObject.AddComponent<MeshFilter>();
        var arrowRenderer = arrow.gameObject.AddComponent<MeshRenderer>();
        var arrowMaterial = Store(new Material(Shader.Find("TCG/Battle/OriginalSprite")), Art + "BattleAttackArrow.mat");
        arrowRenderer.sharedMaterial = arrowMaterial;
        var arrowView = arrow.gameObject.AddComponent<BattleAttackArrow>();
        Ref(arrowView, "m_filter", filter); Ref(arrowView, "m_renderer", arrowRenderer); Ref(arrowView, "m_texture", Sprite("GUI_Attack_Arrow").texture);
        var chain = Obj("ChainBadges", effects, Vector3.zero).gameObject.AddComponent<BattleChainView>();
        Ref(chain, "m_prefab", badge); Ref(chain, "m_camera", camera);
        var near = Obj("NearLPAnchor", effects, new Vector3(-28, 0, -30));
        var far = Obj("FarLPAnchor", effects, new Vector3(28, 0, 30));
        Ref(controller, "m_attackArrow", arrowView); Ref(controller, "m_chainView", chain); Refs(controller, "m_directAttackAnchors", new[] { near, far });
        EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
        AddressableCatalogMenu.AddPrefab(Prefabs + "BattleSelectionPanel.prefab");
        AddressableCatalogMenu.AddPrefab(Prefabs + "BattleSelectionItem.prefab");
        AddressableCatalogMenu.AddPrefab(Prefabs + "BattleChainBadge.prefab");
        AddressableCatalogSetup.SyncAddressKeys(); AssetDatabase.SaveAssets();
        return "离线决斗界面、引用和预设已装配";
    }
}
