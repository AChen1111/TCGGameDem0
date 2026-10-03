using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;

/// <summary>在 Editor 内创建 C 版大厅窗口，保存真实序列化引用；注册由集成步骤完成。</summary>
public static class DuelLobbyPrefabBuilder
{
    const string Folder = "Assets/UI/Prefab/Hall/Duel/";
    // C 版原稿为 1920×1080，项目 UIFrame 的参考高度为 960。
    const float Scale = 960f / 1080f;
    static TMP_FontAsset s_font;
    static readonly Color Ink = new Color(.035f, .055f, .09f, 1);
    static readonly Color Panel = new Color(.085f, .12f, .18f, .98f);
    static readonly Color Accent = new Color(.8f, 1f, .02f, 1f);
    static Sprite S(string name) => AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Sprite/Common/" + name + ".png");
    static T ComponentIn<T>(GameObject root) where T : Component => Resources.FindObjectsOfTypeAll<T>()
        .Single(x => x.transform == root.transform || x.transform.IsChildOf(root.transform));
    static void Ref(Object host, string field, Object value)
    {
        var so = new SerializedObject(host);
        so.FindProperty(field).objectReferenceValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }
    static void ArrayRef(Object host, string field, Object[] values)
    {
        var so = new SerializedObject(host); var array = so.FindProperty(field); array.arraySize = values.Length;
        for (var i = 0; i < values.Length; i++) array.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        so.ApplyModifiedPropertiesWithoutUndo();
    }
    static RectTransform Rect(string name, Transform parent, float x, float y, float w, float h)
    {
        var r = (RectTransform)new GameObject(name, typeof(RectTransform)).transform;
        r.SetParent(parent, false); r.anchorMin = r.anchorMax = r.pivot = new Vector2(0, 1);
        r.anchoredPosition = new Vector2(x, -y) * Scale; r.sizeDelta = new Vector2(w, h) * Scale; return r;
    }
    static void Stretch(RectTransform r)
    { r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = r.offsetMax = Vector2.zero; }
    static UnityEngine.UI.Image Image(string name, Transform parent, float x, float y, float w, float h, Color color, Sprite sprite = null)
    {
        var image = Rect(name, parent, x, y, w, h).gameObject.AddComponent<UnityEngine.UI.Image>();
        image.color = color; image.sprite = sprite; image.type = UnityEngine.UI.Image.Type.Sliced; image.raycastTarget = false;
        return image;
    }
    static TextMeshProUGUI Text(string name, Transform parent, float x, float y, float w, float h, string label, float size = 26)
    {
        var text = Rect(name, parent, x, y, w, h).gameObject.AddComponent<TextMeshProUGUI>();
        text.font = s_font; text.fontSize = size * Scale; text.text = label; text.color = Color.white;
        text.raycastTarget = false; text.alignment = TextAlignmentOptions.MidlineLeft; text.overflowMode = TextOverflowModes.Ellipsis;
        return text;
    }
    static UnityEngine.UI.Button Button(string name, Transform parent, float x, float y, float w, float h, string label, bool accent = false)
    {
        var image = Image(name, parent, x, y, w, h, Color.white, S(accent ? "GUI_CommonButtonM_On" : "GUI_CommonButtonM"));
        image.raycastTarget = true;
        var button = image.gameObject.AddComponent<UnityEngine.UI.Button>(); button.targetGraphic = image;
        var text = Text("Label", image.transform, 12, 0, w - 24, h, label, 24); text.alignment = TextAlignmentOptions.Center;
        if (accent) text.color = Accent;
        return button;
    }
    static AvatarPortraitView Portrait(Transform parent, float x, float y, float size)
    {
        var root = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/UI/Prefab/Hall/Profile/AvatarPortrait.prefab"), parent);
        var rect = (RectTransform)root.transform; rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = new Vector2(x, -y) * Scale; rect.sizeDelta = new Vector2(size, size) * Scale;
        return ComponentIn<AvatarPortraitView>(root);
    }
    static RectTransform Scroll(Transform parent, float x, float y, float w, float h)
    {
        var root = Rect("Scr_List", parent, x, y, w, h);
        var scroll = root.gameObject.AddComponent<UnityEngine.UI.ScrollRect>();
        scroll.horizontal = false; scroll.vertical = true; scroll.movementType = UnityEngine.UI.ScrollRect.MovementType.Clamped;
        var viewport = Image("Viewport", root, 0, 0, w, h, new Color(0, 0, 0, .01f)); viewport.raycastTarget = true;
        viewport.gameObject.AddComponent<UnityEngine.UI.Mask>().showMaskGraphic = false;
        var content = Rect("Content", viewport.transform, 0, 0, w, 0);
        content.anchorMax = new Vector2(1, 1); content.sizeDelta = Vector2.zero;
        var layout = content.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>(); layout.spacing = 16;
        layout.childControlWidth = layout.childControlHeight = true;
        layout.childForceExpandWidth = layout.childForceExpandHeight = false;
        var fitter = content.gameObject.AddComponent<UnityEngine.UI.ContentSizeFitter>(); fitter.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
        scroll.viewport = viewport.rectTransform; scroll.content = content;
        return content;
    }
    static GameObject Save(RectTransform root, string name)
    {
        var prefab = PrefabUtility.SaveAsPrefabAsset(root.gameObject, Folder + name + ".prefab");
        Object.DestroyImmediate(root.gameObject); return prefab;
    }
    [MenuItem("Tools/UI/创建 Duel 大厅窗口")]
    public static void Build()
    {
        Directory.CreateDirectory(Folder); AssetDatabase.Refresh();
        s_font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/UI/Fonts/FZZYJW SDF.asset");
        BuildRow(); BuildRoom(); BuildReplay(); AssetDatabase.SaveAssets();
    }
    static void BuildRow()
    {
        var root = Rect("DuelLobbyRow", null, 0, 0, 830, 122);
        var layout = root.gameObject.AddComponent<UnityEngine.UI.LayoutElement>(); layout.preferredWidth = 830 * Scale; layout.preferredHeight = 122 * Scale;
        var view = root.gameObject.AddComponent<DuelLobbyRow>();
        var bg = Image("Row", root, 0, 0, 830, 122, Panel, S("GUI_CommonButtonList")); bg.raycastTarget = true;
        var button = bg.gameObject.AddComponent<UnityEngine.UI.Button>(); button.targetGraphic = bg;
        var selected = Image("Selected", root, 0, 0, 4, 122, Accent); selected.gameObject.SetActive(false);
        Ref(view, "m_Select", button); Ref(view, "m_Selected", selected.gameObject); Ref(view, "m_Portrait", Portrait(root, 20, 26, 70));
        Ref(view, "m_Title", Text("Title", root, 112, 17, 560, 42, "", 28));
        Ref(view, "m_Subtitle", Text("Subtitle", root, 112, 65, 560, 36, "", 21));
        var state = Text("State", root, 670, 24, 145, 74, "", 24); state.color = Accent; state.alignment = TextAlignmentOptions.Center;
        Ref(view, "m_State", state); Save(root, "DuelLobbyRow");
    }
    static RectTransform Window<T>(string name, string title, out T window) where T : AWindowController
    {
        var root = Rect(name, null, 0, 0, 1920, 1080); Stretch(root); root.gameObject.SetActive(false);
        root.gameObject.AddComponent<CanvasGroup>(); window = root.gameObject.AddComponent<T>();
        var backdrop = Image("Backdrop", root, 0, 0, 1920, 1080, Ink); Stretch(backdrop.rectTransform); backdrop.raycastTarget = true;
        Image("SideBar", root, 0, 0, 284, 1080, new Color(.025f, .04f, .07f, 1));
        Text("Brand", root, 36, 60, 232, 88, "DUEL", 62);
        Text("Title", root, 332, 85, 690, 72, title, 46);
        Image("Rule", root, 332, 176, 1540, 2, new Color(.2f, .29f, .39f, 1));
        Ref(window, "m_BtnClose", Button("Btn_Close", root, 28, 910, 225, 62, "返回大厅"));
        Ref(window, "m_BtnRefresh", Button("Btn_Refresh", root, 950, 94, 210, 56, "刷新"));
        Ref(window, "m_TxtStatus", Text("Txt_Status", root, 332, 997, 1536, 55, "", 23));
        Ref(window, "m_ListContent", Scroll(root, 332, 258, 830, 690));
        Ref(window, "m_RowPrefab", ComponentIn<DuelLobbyRow>(AssetDatabase.LoadAssetAtPath<GameObject>(Folder + "DuelLobbyRow.prefab")));
        Ref(window, "m_TxtEmpty", Text("Txt_Empty", root, 352, 298, 760, 80, "", 26));
        var generator = root.gameObject.AddComponent<UiScreenGenerator>();
        var so = new SerializedObject(generator); so.FindProperty("m_kind").enumValueIndex = (int)UiScreenGenerator.Kind.Window;
        so.FindProperty("m_className").stringValue = name; so.FindProperty("m_folderPath").stringValue = "Assets/Scripts/UI/PreGameUI/Duel";
        so.ApplyModifiedPropertiesWithoutUndo();
        return root;
    }
    [MenuItem("Tools/UI/移除 Duel 大厅侧栏场地展示")]
    public static void RemoveSidebarField()
    {
        foreach (var name in new[] { "DuelRoomWindow", "DuelReplayWindow" })
        {
            var path = Folder + name + ".prefab";
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var field = root.transform.Find("Field");
                if (field == null) continue; // 允许重复执行该一次性资源修正。
                Object.DestroyImmediate(field.gameObject);
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
    }
    static TMP_InputField Input(Transform parent, float x, float y, float w, float h)
    {
        var bg = Image("Inp_Code", parent, x, y, w, h, Color.white, S("GUI_CommonInputField_Base")); bg.raycastTarget = true;
        var input = bg.gameObject.AddComponent<TMP_InputField>(); input.targetGraphic = bg;
        var area = Rect("TextArea", bg.transform, 16, 4, w - 32, h - 8); area.gameObject.AddComponent<UnityEngine.UI.RectMask2D>();
        var value = Text("Text", area, 0, 0, w - 32, h - 8, "", 24); Stretch(value.rectTransform);
        var placeholder = Text("Placeholder", area, 0, 0, w - 32, h - 8, "输入房间码", 24); Stretch(placeholder.rectTransform); placeholder.color = new Color(.55f, .64f, .76f);
        input.textViewport = area; input.textComponent = value; input.placeholder = placeholder; input.characterLimit = 16;
        return input;
    }
    static TMP_Dropdown Dropdown(Transform parent)
    {
        var bg = Image("Drop_Deck", parent, 24, 440, 575, 60, Color.white, S("GUI_CommonInputField_Base")); bg.raycastTarget = true;
        var drop = bg.gameObject.AddComponent<TMP_Dropdown>(); drop.targetGraphic = bg;
        drop.captionText = Text("Caption", bg.transform, 18, 0, 530, 60, "选择已保存牌组", 24);
        var template = Rect("Template", bg.transform, 0, 60, 575, 240); Image("Base", template, 0, 0, 575, 240, Panel);
        var scroll = template.gameObject.AddComponent<UnityEngine.UI.ScrollRect>(); scroll.horizontal = false; scroll.vertical = true;
        var viewport = Rect("Viewport", template, 0, 0, 575, 240); viewport.gameObject.AddComponent<UnityEngine.UI.RectMask2D>();
        var content = Rect("Content", viewport, 0, 0, 575, 48);
        var item = Rect("Item", content, 0, 0, 575, 48);
        var itemBg = Image("Background", item, 0, 0, 575, 48, Panel); itemBg.raycastTarget = true;
        var check = Image("Checkmark", item, 6, 12, 5, 24, Accent);
        var toggle = item.gameObject.AddComponent<UnityEngine.UI.Toggle>(); toggle.targetGraphic = itemBg; toggle.graphic = check;
        drop.itemText = Text("ItemLabel", item, 20, 0, 540, 48, "牌组", 23);
        scroll.viewport = viewport; scroll.content = content; drop.template = template;
        template.gameObject.SetActive(false); drop.ClearOptions(); return drop;
    }
    static void BuildRoom()
    {
        var root = Window<DuelRoomWindow>("DuelRoomWindow", "房间选择", out var window);
        Button("RoomTab", root, 28, 295, 225, 72, "房间选择", true);
        Ref(window, "m_BtnReplay", Button("Btn_Replay", root, 28, 384, 225, 72, "回放查看"));
        Ref(window, "m_BtnCreate", Button("Btn_Create", root, 1650, 94, 222, 56, "创建房间", true));
        Ref(window, "m_InpCode", Input(root, 332, 193, 578, 50));
        Ref(window, "m_BtnJoin", Button("Btn_Join", root, 926, 193, 236, 50, "加入房间"));
        var detail = Image("Go_Room", root, 1208, 258, 664, 690, Panel);
        Ref(window, "m_GoRoom", detail.gameObject);
        Ref(window, "m_TxtRoom", Text("Txt_Room", detail.transform, 24, 18, 616, 88, "", 32));
        Image("Rule", detail.transform, 24, 122, 616, 2, new Color(.2f, .29f, .39f, 1));
        var portraits = new Object[2]; var names = new Object[2]; var ready = new Object[2];
        for (var i = 0; i < 2; i++)
        {
            var seat = Rect("Seat" + i, detail.transform, 46 + i * 300, 145, 256, 224);
            portraits[i] = Portrait(seat, 64, 0, 108);
            var name = Text("Name", seat, 0, 118, 256, 44, "", 26); name.alignment = TextAlignmentOptions.Center; names[i] = name;
            var state = Text("Ready", seat, 0, 168, 256, 36, "", 22); state.alignment = TextAlignmentOptions.Center; state.color = Accent; ready[i] = state;
        }
        ArrayRef(window, "m_Portraits", portraits); ArrayRef(window, "m_Names", names); ArrayRef(window, "m_Readies", ready);
        Text("DeckLabel", detail.transform, 24, 395, 575, 40, "我的牌组", 24);
        Ref(window, "m_DropDeck", Dropdown(detail.transform));
        var readyButton = Button("Btn_Ready", detail.transform, 24, 532, 616, 60, "准备", true);
        Ref(window, "m_BtnReady", readyButton); Ref(window, "m_TxtReady", ComponentIn<TextMeshProUGUI>(readyButton.gameObject));
        Ref(window, "m_BtnLeave", Button("Btn_Leave", detail.transform, 24, 614, 616, 52, "退出房间"));
        Ref(window, "m_TxtNoRoom", Text("Txt_NoRoom", root, 1240, 455, 600, 200, "选择或创建房间\n\n双方准备后进入决斗", 29));
        ComponentIn<UiScreenGenerator>(root.gameObject).CollectUiBinds(); Save(root, "DuelRoomWindow");
    }
    static void BuildReplay()
    {
        var root = Window<DuelReplayWindow>("DuelReplayWindow", "回放查看", out var window);
        Ref(window, "m_BtnRooms", Button("Btn_Rooms", root, 28, 295, 225, 72, "房间选择"));
        Button("ReplayTab", root, 28, 384, 225, 72, "回放查看", true);
        var detail = Image("Go_Detail", root, 1208, 258, 664, 690, Panel); Ref(window, "m_GoDetail", detail.gameObject);
        Ref(window, "m_Portrait", Portrait(detail.transform, 266, 70, 128));
        var title = Text("Txt_Opponent", detail.transform, 24, 225, 616, 66, "", 36); title.alignment = TextAlignmentOptions.Center;
        Ref(window, "m_TxtOpponent", title);
        var info = Text("Txt_Info", detail.transform, 24, 328, 616, 180, "", 27); info.alignment = TextAlignmentOptions.Center; Ref(window, "m_TxtInfo", info);
        Ref(window, "m_BtnWatch", Button("Btn_Watch", detail.transform, 24, 584, 616, 72, "观看回放", true));
        ComponentIn<UiScreenGenerator>(root.gameObject).CollectUiBinds(); Save(root, "DuelReplayWindow");
    }
}
