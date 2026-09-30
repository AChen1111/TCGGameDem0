using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>活动界面沿用 UIFrame 的窗口、引用生成器和 Addressable 目录。</summary>
public static class ActivityPrefabBuilder
{
    const string Folder = "Assets/UI/Prefab/Hall/Activities/";
    const string Sprites = "Assets/UI/Sprite/Activities/";
    static TMP_FontAsset s_font;
    static Sprite S(string name) => AssetDatabase.LoadAssetAtPath<Sprite>(name == "gold"
        ? "Assets/UI/Sprite/Shop/GUI_GemShopIcon03.png"
        : Sprites + "activity_" + name + ".png");
    static T[] All<T>(GameObject root) where T : Component => Resources.FindObjectsOfTypeAll<T>()
        .Where(x => x.transform == root.transform || x.transform.IsChildOf(root.transform)).ToArray();
    static void Ref(UnityEngine.Object host, string name, UnityEngine.Object value)
    { var so = new SerializedObject(host); so.FindProperty(name).objectReferenceValue = value; so.ApplyModifiedPropertiesWithoutUndo(); }
    static void Refs(UnityEngine.Object host, string name, UnityEngine.Object[] values)
    {
        var so = new SerializedObject(host); var p = so.FindProperty(name); p.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++) p.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        so.ApplyModifiedPropertiesWithoutUndo();
    }
    static RectTransform Rect(string name, Transform parent, float x, float y, float w, float h)
    {
        var r = (RectTransform)new GameObject(name, typeof(RectTransform)).transform;
        r.SetParent(parent, false); r.anchorMin = r.anchorMax = r.pivot = new Vector2(0, 1);
        r.anchoredPosition = new Vector2(x, -y); r.sizeDelta = new Vector2(w, h); return r;
    }
    static void Stretch(RectTransform r) { r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = r.offsetMax = Vector2.zero; }
    static Image Img(string name, Transform parent, float x, float y, float w, float h, Sprite sprite, Color color, bool sliced = true)
    {
        var image = Rect(name, parent, x, y, w, h).gameObject.AddComponent<Image>();
        image.sprite = sprite; image.color = color; image.raycastTarget = false; image.type = sliced ? Image.Type.Sliced : Image.Type.Simple;
        return image;
    }
    static TextMeshProUGUI Text(string name, Transform parent, float x, float y, float w, float h, string value, float size)
    {
        var t = Rect(name, parent, x, y, w, h).gameObject.AddComponent<TextMeshProUGUI>();
        t.font = s_font; t.text = value; t.fontSize = size; t.color = Color.white; t.raycastTarget = false;
        t.alignment = TextAlignmentOptions.MidlineLeft; t.overflowMode = TextOverflowModes.Ellipsis; return t;
    }
    static LocalizedText Local(string name, Transform parent, float x, float y, float w, float h, string key, float size = 25, bool dynamic = true, TextAlignmentOptions alignment = TextAlignmentOptions.MidlineLeft)
    {
        var text = Text(name, parent, x, y, w, h, key, size);
        text.alignment = alignment;
        var local = text.gameObject.AddComponent<LocalizedText>(); var so = new SerializedObject(local);
        so.FindProperty("key").stringValue = key; so.FindProperty("dynamicContent").boolValue = dynamic; so.ApplyModifiedPropertiesWithoutUndo(); return local;
    }
    static Button Btn(string name, Transform parent, float x, float y, float w, float h, bool close = false)
    {
        var image = Img(name, parent, x, y, w, h, S(close ? "close" : "button"), Color.white, !close); image.raycastTarget = true;
        var button = image.gameObject.AddComponent<Button>(); button.targetGraphic = image;
        button.transition = close ? Selectable.Transition.None : Selectable.Transition.SpriteSwap;
        button.spriteState = new SpriteState { highlightedSprite = S("button_hover"), pressedSprite = S("button_hover"), selectedSprite = S("button_hover") };
        return button;
    }
    static RectTransform Scroll(Transform parent, string name, float x, float y, float w, float h, bool horizontal)
    {
        var root = Rect(name, parent, x, y, w, h); var scroll = root.gameObject.AddComponent<ScrollRect>();
        scroll.horizontal = horizontal; scroll.vertical = !horizontal; scroll.movementType = ScrollRect.MovementType.Clamped;
        var view = Rect("Viewport", root, 0, 0, w, h); view.gameObject.AddComponent<RectMask2D>();
        var bg = view.gameObject.AddComponent<Image>(); bg.color = new Color(0, 0, 0, .01f);
        var content = Rect("Content", view, 0, 0, w, h); scroll.viewport = view; scroll.content = content;
        var layout = horizontal ? (HorizontalOrVerticalLayoutGroup)content.gameObject.AddComponent<HorizontalLayoutGroup>() : content.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 14; layout.childControlHeight = layout.childControlWidth = true;
        layout.childForceExpandHeight = layout.childForceExpandWidth = false;
        var fitter = content.gameObject.AddComponent<ContentSizeFitter>();
        if (horizontal) { fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize; content.anchorMax = new Vector2(0, 1); }
        else { fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize; content.anchorMax = new Vector2(1, 1); content.sizeDelta = new Vector2(0, h); }
        return content;
    }
    static GameObject Save(GameObject root, string name)
    {
        var prefab = PrefabUtility.SaveAsPrefabAsset(root, Folder + name + ".prefab");
        UnityEngine.Object.DestroyImmediate(root); return prefab;
    }
    static void Layout(GameObject root, float w, float h)
    { var l = root.AddComponent<LayoutElement>(); l.preferredWidth = w; l.preferredHeight = h; l.minWidth = w; l.minHeight = h; }
    [MenuItem("Tools/Activities/Build Prefabs")]
    public static void Build()
    {
        Directory.CreateDirectory(Folder); AssetDatabase.Refresh();
        ImportSprites(); s_font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/UI/Fonts/FZZYJW SDF.asset");
        BuildListItem(); BuildRewardItem(); BuildDetail(); BuildWindow(false); BuildWindow(true); BindLobby();
        foreach (var path in Directory.GetFiles(Sprites, "*.png")) AddressableCatalogMenu.AddSprite(path.Replace('\\', '/'));
        foreach (var path in Directory.GetFiles(Folder, "*.prefab")) AddressableCatalogMenu.AddPrefab(path.Replace('\\', '/'));
        RegisterWindows(); AssetDatabase.SaveAssets();
        Debug.Log("Activity Prefabs created and bound to UIFrame");
    }
    static void ImportSprites()
    {
        var entries = JsonConvert.DeserializeObject<ImportEntry[]>(File.ReadAllText(Sprites + "import-metadata.json"));
        foreach (var entry in entries)
        {
            string path = Sprites + entry.name + ".png";
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = entry.pixelsPerUnit; importer.spritePivot = entry.pivot;
            importer.spriteBorder = entry.border; importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true; importer.maxTextureSize = 2048; importer.SaveAndReimport();
        }
    }
    static Sprite[] Icons() => new[] { S("notice"), S("gift"), S("checked"), S("gold"), S("card") };
    static void BuildListItem()
    {
        var root = Rect("ActivityListItem", null, 0, 0, 350, 126); Layout(root.gameObject, 350, 126);
        var row = root.gameObject.AddComponent<ActivityListItem>();
        var bg = Img("Base", root, 0, 0, 350, 126, S("topic_base"), new Color(.1f, .15f, .2f)); bg.raycastTarget = true;
        var select = bg.gameObject.AddComponent<Button>(); select.targetGraphic = bg; select.transition = Selectable.Transition.None;
        Img("Frame", root, 0, 0, 350, 126, S("topic_frame"), new Color(.55f, .65f, .72f));
        var selected = Img("Selected", root, -2, -2, 354, 130, S("topic_hover"), Color.white); selected.gameObject.SetActive(false);
        var icon = Img("TypeIcon", root, 18, 35, 54, 54, S("gift"), Color.white, false); icon.preserveAspect = true;
        Ref(row, "m_Select", select); Ref(row, "m_Label", Local("Name", root, 90, 16, 238, 62, "ui.activities.title", 26));
        Ref(row, "m_Status", Local("Status", root, 90, 77, 238, 30, "ui.activities.view", 20));
        Ref(row, "m_RedDot", Img("RedDot", root, 325, 12, 14, 14, S("window"), new Color(1, .15f, .2f), false).gameObject);
        Ref(row, "m_Selected", selected.gameObject); Ref(row, "m_Icon", icon); Refs(row, "m_TypeIcons", Icons());
        Save(root.gameObject, "ActivityListItem");
    }
    static void BuildRewardItem()
    {
        var root = Rect("ActivityRewardItem", null, 0, 0, 236, 310); Layout(root.gameObject, 236, 310);
        var item = root.gameObject.AddComponent<ActivityRewardItem>();
        Img("Base", root, 0, 0, 236, 310, S("item_base"), new Color(.08f, .14f, .19f));
        Img("Frame", root, 0, 0, 236, 310, S("item_frame"), new Color(.55f, .65f, .72f));
        Ref(item, "m_Label", Local("RewardName", root, 16, 12, 204, 38, "ui.activities.reward", 24));
        var icon = Img("RewardIcon", root, 76, 58, 84, 84, S("gold"), Color.white, false); icon.preserveAspect = true;
        Ref(item, "m_Icon", icon); Ref(item, "m_GoldIcon", S("gold")); Ref(item, "m_CardIcon", S("card"));
        Ref(item, "m_Amount", Local("Amount", root, 16, 151, 204, 62, "ui.activities.reward", 21));
        Ref(item, "m_Description", Text("Description", root, 16, 215, 204, 26, "", 16));
        Ref(item, "m_Cost", Local("Cost", root, 16, 238, 204, 26, "ui.activities.cost", 19));
        var button = Btn("ClaimButton", root, 16, 269, 204, 36); Ref(item, "m_Button", button);
        Ref(item, "m_Action", Local("ActionLabel", button.transform, 24, 0, 156, 36, "ui.activities.claim", 20, true, TextAlignmentOptions.Center));
        Ref(item, "m_Claimed", Img("Claimed", root, 181, 57, 36, 36, S("checked"), Color.white, false).gameObject);
        Save(root.gameObject, "ActivityRewardItem");
    }
    static void BuildDetail()
    {
        var root = Rect("ActivityDetailView", null, 0, 0, 890, 680); var view = root.gameObject.AddComponent<ActivityDetailView>();
        Ref(view, "m_Title", Local("ActivityTitle", root, 0, 8, 660, 60, "ui.activities.title", 36));
        Ref(view, "m_Description", Text("Description", root, 0, 78, 660, 92, "", 24));
        var banner = Img("Banner", root, 710, 20, 150, 150, S("gift"), Color.white, false); banner.preserveAspect = true;
        Ref(view, "m_Banner", banner); Refs(view, "m_TypeBanners", Icons());
        Ref(view, "m_Time", Local("Time", root, 0, 176, 890, 34, "ui.activities.permanent", 23));
        Ref(view, "m_Reset", Local("DailyReset", root, 0, 214, 890, 32, "ui.activities.next_reset", 20));
        Ref(view, "m_Progress", Local("Progress", root, 0, 250, 890, 35, "ui.activities.sign_in.progress", 24));
        Ref(view, "m_State", Local("ActivityState", root, 0, 288, 890, 32, "ui.activities.running", 21));
        Ref(view, "m_Notice", Text("NoticeContent", root, 0, 250, 890, 295, "", 28));
        Ref(view, "m_Content", Scroll(root, "Rewards", 0, 344, 890, 328, true));
        Ref(view, "m_RewardPrefab", All<ActivityRewardItem>(AssetDatabase.LoadAssetAtPath<GameObject>(Folder + "ActivityRewardItem.prefab")).Single());
        var go = Btn("GoToActivity", root, 620, 612, 260, 56); Local("GoToLabel", go.transform, 24, 0, 212, 56, "ui.activities.goto", 25, false, TextAlignmentOptions.Center); Ref(view, "m_GoTo", go);
        Save(root.gameObject, "ActivityDetailView");
    }
    static void BuildWindow(bool popup)
    {
        string name = popup ? "ActivityPopupWindow" : "ActivityWindow";
        float w = popup ? 1010 : 1400;
        var root = Rect(name, null, 0, 0, 1706, 960); Stretch(root); root.gameObject.SetActive(false);
        root.gameObject.AddComponent<CanvasGroup>();
        var screen = popup ? (AWindowController)root.gameObject.AddComponent<ActivityPopupWindow>() : root.gameObject.AddComponent<ActivityWindow>();
        var shade = Img("Shade", root, 0, 0, 1706, 960, null, new Color(0, 0, 0, .72f)); Stretch(shade.rectTransform); shade.raycastTarget = true;
        var panel = Img("Window", root, 0, 0, w, 840, S("window"), new Color(.025f, .05f, .085f, .99f)); panel.raycastTarget = true;
        panel.rectTransform.anchorMin = panel.rectTransform.anchorMax = panel.rectTransform.pivot = new Vector2(.5f, .5f); panel.rectTransform.anchoredPosition = Vector2.zero;
        Local("WindowTitle", panel.transform, 36, 18, w - 150, 54, "ui.activities.title", 32, false);
        Img("Accent", panel.transform, 36, 88, w - 72, 2, null, new Color(.72f, 1, .05f));
        Ref(screen, "m_BtnClose", Btn("Btn_Close", panel.transform, w - 82, 24, 44, 44, true));
        var detailPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(Folder + "ActivityDetailView.prefab");
        var detailRoot = (GameObject)PrefabUtility.InstantiatePrefab(detailPrefab, panel.transform);
        var detailRect = (RectTransform)detailRoot.transform; detailRect.anchoredPosition = new Vector2(popup ? 60 : 454, -118);
        Ref(screen, "m_Detail", All<ActivityDetailView>(detailRoot).Single());
        if (!popup)
        {
            var refresh = Btn("Btn_Refresh", panel.transform, w - 298, 22, 190, 48);
            Local("RefreshLabel", refresh.transform, 25, 0, 140, 48, "ui.activities.refresh", 22, false, TextAlignmentOptions.Center); Ref(screen, "m_BtnRefresh", refresh);
            Ref(screen, "m_ListContent", Scroll(panel.transform, "ActivityList", 36, 118, 366, 680, false));
            Ref(screen, "m_ItemPrefab", All<ActivityListItem>(AssetDatabase.LoadAssetAtPath<GameObject>(Folder + "ActivityListItem.prefab")).Single());
            Ref(screen, "m_Empty", Local("EmptyState", panel.transform, 454, 320, 890, 140, "ui.activities.loading", 29));
        }
        var so = new SerializedObject(screen); so.FindProperty("windowPriority").enumValueIndex = (int)WindowPriority.Enqueue;
        so.FindProperty("isPopup").boolValue = popup; so.FindProperty("m_destroyOnClose").boolValue = true; so.ApplyModifiedPropertiesWithoutUndo();
        var generator = root.gameObject.AddComponent<UiScreenGenerator>(); var gs = new SerializedObject(generator);
        gs.FindProperty("m_kind").enumValueIndex = (int)UiScreenGenerator.Kind.Window;
        gs.FindProperty("m_className").stringValue = name; gs.FindProperty("m_folderPath").stringValue = "Assets/Scripts/UI/PreGameUI/Activities"; gs.ApplyModifiedPropertiesWithoutUndo();
        generator.CollectUiBinds(); generator.RebuildUiBinds(); Save(root.gameObject, name);
    }
    static void BindLobby()
    {
        const string path = "Assets/UI/Prefab/Hall/PreGameUI/PreGameUIPanel.prefab";
        var root = PrefabUtility.LoadPrefabContents(path); var lobby = All<PreGameUIPanel>(root).Single(); var so = new SerializedObject(lobby);
        var gift = (Button)so.FindProperty("m_BtnGift").objectReferenceValue; var mail = (Button)so.FindProperty("m_BtnMail").objectReferenceValue;
        var existing = All<Image>(gift.gameObject).Where(x => x.name == "ActivityRedDot").ToArray();
        var dot = existing.Length == 0 ? Img("ActivityRedDot", gift.transform, 0, 0, 14, 14, S("window"), new Color(1, .15f, .2f), false) : existing[0];
        dot.rectTransform.anchorMin = dot.rectTransform.anchorMax = new Vector2(1, 1); dot.rectTransform.pivot = new Vector2(.5f, .5f); dot.rectTransform.anchoredPosition = new Vector2(-8, -8);
        dot.gameObject.SetActive(false); Ref(lobby, "m_ActivityRedDot", dot.gameObject);
        foreach (var pair in new[] { (gift, "ui.lobby.activities"), (mail, "ui.lobby.mail") })
            foreach (var text in All<LocalizedText>(pair.Item1.gameObject))
            { var ts = new SerializedObject(text); ts.FindProperty("key").stringValue = pair.Item2; ts.ApplyModifiedPropertiesWithoutUndo(); }
        PrefabUtility.SaveAsPrefabAsset(root, path); PrefabUtility.UnloadPrefabContents(root);
    }
    static void RegisterWindows()
    {
        var settings = AssetDatabase.LoadAssetAtPath<UISettings>("Assets/UI/Prefab/Hall/UISetting.asset"); var so = new SerializedObject(settings);
        var screens = so.FindProperty("screensToRegister");
        foreach (string name in new[] { "ActivityWindow", "ActivityPopupWindow" })
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Folder + name + ".prefab");
            if (Enumerable.Range(0, screens.arraySize).Any(i => screens.GetArrayElementAtIndex(i).objectReferenceValue == prefab)) continue;
            int i = screens.arraySize++; screens.GetArrayElementAtIndex(i).objectReferenceValue = prefab;
        }
        so.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(settings);
    }
    [MenuItem("Tools/Activities/Rebind Generated References")]
    public static void Rebind()
    {
        foreach (string name in new[] { "ActivityWindow", "ActivityPopupWindow" })
        {
            string path = Folder + name + ".prefab"; var root = PrefabUtility.LoadPrefabContents(path);
            All<UiScreenGenerator>(root).Single().CollectUiBinds();
            PrefabUtility.SaveAsPrefabAsset(root, path); PrefabUtility.UnloadPrefabContents(root);
        }
        AssetDatabase.SaveAssets();
    }
    sealed class ImportEntry { public string name; public Vector2 pivot; public Vector4 border; public float pixelsPerUnit; }
}
