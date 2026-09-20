using SuperScrollView;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class SetupFriendGiftWindows
{
    const string FriendWindowPath = "Assets/UI/Prefab/Hall/PreGameUI/Meau/FriendWindow/FriendWindow.prefab";
    const string GiftWindowPath = "Assets/UI/Prefab/Hall/PreGameUI/Meau/GiftWindow/GiftWindow.prefab";
    const string FriendRowPath = "Assets/UI/Prefab/Hall/PreGameUI/Meau/FriendWindow/FriendRowPrefab.prefab";
    const string RequestRowPath = "Assets/UI/Prefab/Hall/PreGameUI/Meau/GiftWindow/FriendApplyRowPrefab.prefab";
    const string GiftRowPath = "Assets/UI/Prefab/Hall/PreGameUI/Meau/GiftWindow/GiftApplyRowPrefab.prefab";
    const string SettingsPath = "Assets/UI/Prefab/Hall/UISetting.asset";
    const string CloseSpriteGuid = "5fc8596035b03b94faa0081cf34a2484";

    public static string Run()
    {
        string friendRow = SetupFriendRow();
        string requestRow = SetupRequestRow();
        string giftRow = SetupGiftRow();
        string friendWin = SetupFriendWindow();
        string giftWin = SetupGiftWindow();

        AddressableCatalogMenu.AddPrefab(FriendWindowPath);
        AddressableCatalogMenu.AddPrefab(GiftWindowPath);
        AddressableCatalogMenu.AddPrefab(FriendRowPath);
        AddressableCatalogMenu.AddPrefab(RequestRowPath);
        AddressableCatalogMenu.AddPrefab(GiftRowPath);
        string registered = RegisterWindows();
        PublishedConfigBuilder.Prepare();
        AssetDatabase.SaveAssets();
        return friendRow + ";" + requestRow + ";" + giftRow + ";" + friendWin + ";" + giftWin + ";" + registered + ";bytes-ok";
    }

    static string SetupFriendWindow()
    {
        GameObject root = PrefabUtility.LoadPrefabContents(FriendWindowPath);
        try
        {
            Transform scroll = Require(root.transform, "Scroll View") ?? Require(root.transform, "Scr_List");
            scroll.name = "Scr_List";
            SetupList(scroll.gameObject, true);
            BindFindButton(root.transform.Find("Btn_Find"));
            TMP_Text empty = EnsureEmptyText(root.transform, scroll as RectTransform);
            BindLocalized(root.transform, "Inp_Name/Text Area/Placeholder", "ui.friends.placeholder", false);
            BindLocalized(empty.transform, "", "ui.friends.empty", true);

            var generator = root.GetComponent<UiScreenGenerator>();
            generator.CollectUiBinds();

            var window = root.GetComponent<FriendWindow>();
            var list = scroll.GetComponent<GridListController>();
            SerializedObject so = new SerializedObject(window);
            so.FindProperty("m_ListController").objectReferenceValue = list;
            so.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(root, FriendWindowPath);
            return "friend-window-ok";
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    static string SetupGiftWindow()
    {
        GameObject root = PrefabUtility.LoadPrefabContents(GiftWindowPath);
        try
        {
            Transform scroll = Require(root.transform, "Scroll View") ?? Require(root.transform, "Scr_List");
            scroll.name = "Scr_List";
            SetupList(scroll.gameObject, false);
            EnsureCloseButton(root.transform);
            TMP_Text empty = EnsureEmptyText(root.transform, scroll as RectTransform);
            BindLocalized(empty.transform, "", "ui.gifts.empty", true);

            var generator = root.GetComponent<UiScreenGenerator>();
            generator.CollectUiBinds();

            var window = root.GetComponent<GiftWindow>();
            var list = scroll.GetComponent<InboxListController>();
            SerializedObject so = new SerializedObject(window);
            so.FindProperty("m_ListController").objectReferenceValue = list;
            so.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(root, GiftWindowPath);
            return "gift-window-ok";
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    static string SetupFriendRow()
    {
        GameObject root = PrefabUtility.LoadPrefabContents(FriendRowPath);
        try
        {
            Transform button = Require(root.transform, "Button") ?? Require(root.transform, "Btn_Action");
            button.name = "Btn_Action";
            BindLocalized(button, "Text (TMP)", "ui.friends.duel", true);
            EnsureLoopItem(root);
            var row = EnsureComponent<FriendRowItem>(root);
            SerializedObject so = new SerializedObject(row);
            so.FindProperty("m_ImgIcon").objectReferenceValue = Require(root.transform, "Img_icon").GetComponent<Image>();
            so.FindProperty("m_TxtName").objectReferenceValue = Require(root.transform, "Txt_name").GetComponent<TMP_Text>();
            so.FindProperty("m_BtnAction").objectReferenceValue = button.GetComponent<Button>();
            so.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(root, FriendRowPath);
            return "friend-row-ok";
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    static string SetupRequestRow()
    {
        GameObject root = PrefabUtility.LoadPrefabContents(RequestRowPath);
        try
        {
            EnsureLoopItem(root);
            var row = EnsureComponent<GiftRequestRowItem>(root);
            SerializedObject so = new SerializedObject(row);
            so.FindProperty("m_ImgIcon").objectReferenceValue = Require(root.transform, "Img_icon").GetComponent<Image>();
            so.FindProperty("m_TxtName").objectReferenceValue = Require(root.transform, "Txt_name").GetComponent<TMP_Text>();
            so.FindProperty("m_BtnYes").objectReferenceValue = Require(root.transform, "Btn_Yes").GetComponent<Button>();
            so.FindProperty("m_BtnNo").objectReferenceValue = Require(root.transform, "Btn_No").GetComponent<Button>();
            so.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(root, RequestRowPath);
            return "request-row-ok";
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    static string SetupGiftRow()
    {
        GameObject root = PrefabUtility.LoadPrefabContents(GiftRowPath);
        try
        {
            EnsureLoopItem(root);
            var row = EnsureComponent<GiftRewardRowItem>(root);
            SerializedObject so = new SerializedObject(row);
            Transform reward = root.transform.Find("Reward");
            so.FindProperty("m_Reward").objectReferenceValue = reward != null ? reward.GetComponent("GiftRewardItem") : null;
            so.FindProperty("m_TxtName").objectReferenceValue = Require(root.transform, "Txt_name").GetComponent<TMP_Text>();
            so.FindProperty("m_BtnYes").objectReferenceValue = Require(root.transform, "Btn_Yes").GetComponent<Button>();
            so.ApplyModifiedPropertiesWithoutUndo();
            BindLocalized(root.transform, "Txt_name", "", true);
            PrefabUtility.SaveAsPrefabAsset(root, GiftRowPath);
            return "gift-row-ok";
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    static void SetupList(GameObject scroll, bool friend)
    {
        if (scroll.GetComponent<LoopListView2>() == null)
        {
            scroll.AddComponent<LoopListView2>();
        }

        if (friend)
        {
            var grid = EnsureComponent<GridListController>(scroll);
            SerializedObject so = new SerializedObject(grid);
            so.FindProperty("loopListView").objectReferenceValue = scroll.GetComponent<LoopListView2>();
            so.ApplyModifiedPropertiesWithoutUndo();
            return;
        }

        var inbox = EnsureComponent<InboxListController>(scroll);
        SerializedObject inboxSo = new SerializedObject(inbox);
        inboxSo.FindProperty("loopListView").objectReferenceValue = scroll.GetComponent<LoopListView2>();
        inboxSo.ApplyModifiedPropertiesWithoutUndo();
    }

    static void BindFindButton(Transform find)
    {
        if (find == null) return;
        var button = find.GetComponent<Button>();
        var image = find.GetComponent<Image>();
        if (button != null && image != null && button.targetGraphic == null)
        {
            button.targetGraphic = image;
            EditorUtility.SetDirty(button);
        }
    }

    static void EnsureCloseButton(Transform root)
    {
        if (root.Find("Btn_Close") != null) return;

        var go = CreateUiChild(root, "Btn_Close", typeof(Image), typeof(Button));
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(-68f, -52f);
        rt.sizeDelta = new Vector2(76f, 76f);
        var image = go.GetComponent<Image>();
        image.sprite = LoadSprite(CloseSpriteGuid);
        go.GetComponent<Button>().targetGraphic = image;
    }

    static TMP_Text EnsureEmptyText(Transform root, RectTransform list)
    {
        Transform existing = root.Find("Txt_Empty");
        if (existing != null)
        {
            return existing.GetComponent<TMP_Text>();
        }

        TMP_Text tmp = CreateTmp(root, "Txt_Empty", string.Empty);
        RectTransform rt = tmp.rectTransform;
        if (list != null)
        {
            rt.anchorMin = list.anchorMin;
            rt.anchorMax = list.anchorMax;
            rt.anchoredPosition = list.anchoredPosition;
            rt.sizeDelta = list.sizeDelta;
        }
        else
        {
            Stretch(rt, Vector2.zero, new Vector2(800f, 80f));
        }

        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = 42f;
        tmp.color = Color.white;
        return tmp;
    }

    static void EnsureLoopItem(GameObject root)
    {
        if (root.GetComponent<LoopListViewItem2>() == null)
        {
            root.AddComponent<LoopListViewItem2>();
        }
    }

    static void BindLocalized(Transform root, string path, string key, bool dynamicContent)
    {
        Transform target = string.IsNullOrEmpty(path) ? root : root.Find(path);
        if (target == null)
        {
            throw new System.Exception("找不到文本: " + (root.name + "/" + path));
        }

        LocalizedText localized = target.GetComponent<LocalizedText>();
        if (localized == null)
        {
            localized = target.gameObject.AddComponent<LocalizedText>();
        }

        SerializedObject so = new SerializedObject(localized);
        so.FindProperty("key").stringValue = key;
        so.FindProperty("dynamicContent").boolValue = dynamicContent;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static string RegisterWindows()
    {
        UISettings settings = AssetDatabase.LoadAssetAtPath<UISettings>(SettingsPath);
        GameObject friend = AssetDatabase.LoadAssetAtPath<GameObject>(FriendWindowPath);
        GameObject gift = AssetDatabase.LoadAssetAtPath<GameObject>(GiftWindowPath);
        if (settings == null || friend == null || gift == null)
        {
            throw new System.Exception("找不到 UISetting 或窗口预制体");
        }

        SerializedObject so = new SerializedObject(settings);
        SerializedProperty prop = so.FindProperty("screensToRegister");
        bool added = AddScreen(prop, friend) | AddScreen(prop, gift);
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(settings);
        return added ? "uisetting-added" : "uisetting-exists";
    }

    static bool AddScreen(SerializedProperty prop, GameObject prefab)
    {
        for (int i = 0; i < prop.arraySize; i++)
        {
            if (prop.GetArrayElementAtIndex(i).objectReferenceValue == prefab)
            {
                return false;
            }
        }

        int index = prop.arraySize;
        prop.arraySize = index + 1;
        prop.GetArrayElementAtIndex(index).objectReferenceValue = prefab;
        return true;
    }

    static GameObject CreateUiChild(Transform parent, string name, params System.Type[] types)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        go.AddComponent<CanvasRenderer>();
        for (int i = 0; i < types.Length; i++)
        {
            if (go.GetComponent(types[i]) == null)
            {
                go.AddComponent(types[i]);
            }
        }

        return go;
    }

    static TMP_Text CreateTmp(Transform parent, string name, string text)
    {
        var go = CreateUiChild(parent, name, typeof(TextMeshProUGUI));
        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 28f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        TMP_Text sample = parent.GetComponentInChildren<TMP_Text>(true);
        if (sample != null && sample != tmp)
        {
            tmp.font = sample.font;
        }

        return tmp;
    }

    static void Stretch(RectTransform rt, Vector2 anchored, Vector2 size)
    {
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchored;
        rt.sizeDelta = size;
    }

    static Sprite LoadSprite(string guid)
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(guid));
    }

    static T EnsureComponent<T>(GameObject go) where T : Component
    {
        T component = go.GetComponent<T>();
        return component != null ? component : go.AddComponent<T>();
    }

    static Transform Require(Transform root, string name)
    {
        Transform child = root.Find(name);
        if (child != null) return child;
        Transform[] all = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i].name == name)
            {
                return all[i];
            }
        }

        return null;
    }
}
