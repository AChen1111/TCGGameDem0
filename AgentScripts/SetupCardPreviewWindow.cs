using System;
using System.Collections.Generic;
using System.Reflection;
using SuperScrollView;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class SetupCardPreviewWindow
{
    const string WindowPath = "Assets/UI/Prefab/BaseUI/CardPreviewWindow.prefab";
    const string CardInUiPath = "Assets/UI/Prefab/Hall/Shop/CardInUI.prefab";
    const string RowPath = "Assets/UI/Prefab/Hall/Shop/CardPreviewRowPrefab.prefab";
    const string SettingsPath = "Assets/UI/Prefab/Hall/UISetting.asset";
    const int UiLayer = 5;
    const int CardsPerRow = 5;
    const float ViewportWidth = 700f;
    const float CardWidth = 131f;
    const float CardHeight = 191f;
    const float RowHeight = 199f;

    public static string Run()
    {
        Type cardInUiType = RequireType("CardInUI");
        Type rowType = RequireType("CardPreviewRowItem");
        Type windowType = FindType("CardPreviewWindow");

        GameObject cardPrefab = CreateCardInUiPrefab(cardInUiType);
        CreateRowPrefab(cardPrefab, cardInUiType, rowType);
        AdaptWindow(windowType);
        RegisterPrefabs();
        return windowType != null ? "card-preview-setup-ok" : "card-preview-setup-ok;window-script-injected";
    }

    static GameObject CreateCardInUiPrefab(Type cardInUiType)
    {
        var root = new GameObject("CardInUI", typeof(RectTransform));
        root.layer = UiLayer;
        var rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0.5f, 0.5f);
        rootRect.anchorMax = new Vector2(0.5f, 0.5f);
        rootRect.pivot = new Vector2(0.5f, 0.5f);
        rootRect.sizeDelta = new Vector2(CardWidth, CardHeight);

        GameObject rawGo = CreateUiChild(root.transform, "Raw_Card", typeof(CanvasRenderer), typeof(RawImage));
        var rawRect = rawGo.GetComponent<RectTransform>();
        Stretch(rawRect);
        RawImage raw = rawGo.GetComponent<RawImage>();
        raw.color = Color.white;
        raw.raycastTarget = false;

        GameObject btnGo = CreateUiChild(root.transform, "Btn_All", typeof(CanvasRenderer), typeof(Image), typeof(Button));
        Stretch(btnGo.GetComponent<RectTransform>());
        Image btnImage = btnGo.GetComponent<Image>();
        btnImage.color = new Color(1f, 1f, 1f, 0f);
        btnImage.raycastTarget = true;
        Button button = btnGo.GetComponent<Button>();
        button.targetGraphic = btnImage;

        Component view = root.AddComponent(cardInUiType);
        var so = new SerializedObject(view);
        so.FindProperty("m_RawCard").objectReferenceValue = raw;
        so.FindProperty("m_BtnAll").objectReferenceValue = button;
        so.ApplyModifiedPropertiesWithoutUndo();

        EnsureFolder("Assets/UI/Prefab/Hall/Shop");
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, CardInUiPath);
        UnityEngine.Object.DestroyImmediate(root);
        if (prefab == null)
        {
            throw new Exception("保存 CardInUI 失败");
        }

        return prefab;
    }

    static void CreateRowPrefab(GameObject cardPrefab, Type cardInUiType, Type rowType)
    {
        var root = new GameObject("CardPreviewRowPrefab", typeof(RectTransform));
        root.layer = UiLayer;
        var rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0.5f, 1f);
        rootRect.anchorMax = new Vector2(0.5f, 1f);
        rootRect.pivot = new Vector2(0.5f, 1f);
        rootRect.sizeDelta = new Vector2(ViewportWidth, RowHeight);

        var layout = root.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(10, 10, 4, 4);
        layout.spacing = 6f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        root.AddComponent<LoopListViewItem2>();
        Component row = root.AddComponent(rowType);
        var cards = new List<UnityEngine.Object>();
        for (int i = 0; i < CardsPerRow; i++)
        {
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(cardPrefab);
            instance.name = "CardInUI";
            instance.layer = UiLayer;
            instance.transform.SetParent(root.transform, false);
            var rect = instance.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(CardWidth, CardHeight);
            Component item = instance.GetComponent(cardInUiType);

            cards.Add(item);
        }

        var so = new SerializedObject(row);
        SerializedProperty array = so.FindProperty("m_Cards");
        array.arraySize = cards.Count;
        for (int i = 0; i < cards.Count; i++)
        {
            array.GetArrayElementAtIndex(i).objectReferenceValue = cards[i];
        }

        so.ApplyModifiedPropertiesWithoutUndo();
        PrefabUtility.SaveAsPrefabAsset(root, RowPath);
        UnityEngine.Object.DestroyImmediate(root);
    }

    static void AdaptWindow(Type windowType)
    {
        GameObject contents = PrefabUtility.LoadPrefabContents(WindowPath);
        try
        {
            Transform root = contents.transform;
            ChooseWindow choose = contents.GetComponent<ChooseWindow>();
            TextMeshProUGUI message = null;
            Button btnNo = null;
            Button btnOk = null;
            if (choose != null)
            {
                var chooseSo = new SerializedObject(choose);
                message = chooseSo.FindProperty("m_TxtMessage").objectReferenceValue as TextMeshProUGUI;
                btnNo = chooseSo.FindProperty("m_BtnNo").objectReferenceValue as Button;
                btnOk = chooseSo.FindProperty("m_BtnOk").objectReferenceValue as Button;
                UnityEngine.Object.DestroyImmediate(choose);
            }

            if (message == null)
            {
                message = FindComponent<TextMeshProUGUI>(root, "Txt_Message");
            }

            if (btnNo == null)
            {
                btnNo = FindComponent<Button>(root, "Btn_No");
            }

            if (btnOk == null)
            {
                btnOk = FindComponent<Button>(root, "Btn_Ok");
            }

            Transform scroll = root.Find("Scroll View") ?? root.Find("Scr_Cards");
            if (scroll == null)
            {
                throw new Exception("找不到 Scroll View");
            }

            scroll.name = "Scr_Cards";
            scroll.gameObject.layer = UiLayer;
            ScrollRect scrollRect = scroll.GetComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.horizontalScrollbar = null;
            Transform horizontal = scroll.Find("Scrollbar Horizontal");
            if (horizontal != null)
            {
                horizontal.gameObject.SetActive(false);
            }

            LoopListView2 listView = scroll.GetComponent<LoopListView2>();
            if (listView == null)
            {
                listView = scroll.gameObject.AddComponent<LoopListView2>();
            }

            var listSo = new SerializedObject(listView);
            listSo.FindProperty("mArrangeType").enumValueIndex = (int)ListItemArrangeType.TopToBottom;
            listSo.FindProperty("mSupportScrollBar").boolValue = true;
            listSo.ApplyModifiedPropertiesWithoutUndo();

            GridListController listController = scroll.GetComponent<GridListController>();
            if (listController == null)
            {
                listController = scroll.gameObject.AddComponent<GridListController>();
            }

            var gridSo = new SerializedObject(listController);
            gridSo.FindProperty("loopListView").objectReferenceValue = listView;
            gridSo.ApplyModifiedPropertiesWithoutUndo();

            if (windowType != null)
            {
                Component window = contents.GetComponent(windowType) ?? contents.AddComponent(windowType);
                BindWindow(window, message, btnNo, btnOk, listController);
            }

            UiScreenGenerator generator = contents.GetComponent<UiScreenGenerator>();
            if (generator != null)
            {
                var generatorSo = new SerializedObject(generator);
                generatorSo.FindProperty("m_className").stringValue = "CardPreviewWindow";
                generatorSo.FindProperty("m_folderPath").stringValue = "Assets/Scripts/UI/PreGameUI/Shop";
                UiPrefixCollector.WriteBinds(generatorSo, "m_uiBinds", UiPrefixCollector.Collect(root));
                generatorSo.ApplyModifiedPropertiesWithoutUndo();
            }

            PrefabUtility.SaveAsPrefabAsset(contents, WindowPath);
            AssetDatabase.SaveAssets();
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(contents);
        }

        if (windowType == null)
        {
            InjectWindowScript();
        }
    }

    static void BindWindow(Component window, TextMeshProUGUI message, Button btnNo, Button btnOk, GridListController listController)
    {
        var windowSo = new SerializedObject(window);
        windowSo.FindProperty("m_destroyOnClose").boolValue = false;
        windowSo.FindProperty("hideOnForegroundLost").boolValue = true;
        windowSo.FindProperty("windowPriority").enumValueIndex = 0;
        windowSo.FindProperty("isPopup").boolValue = true;
        windowSo.FindProperty("m_TxtMessage").objectReferenceValue = message;
        windowSo.FindProperty("m_BtnNo").objectReferenceValue = btnNo;
        windowSo.FindProperty("m_BtnOk").objectReferenceValue = btnOk;
        windowSo.FindProperty("m_ListController").objectReferenceValue = listController;
        windowSo.ApplyModifiedPropertiesWithoutUndo();
    }

    static void InjectWindowScript()
    {
        string yaml = System.IO.File.ReadAllText(WindowPath);
        if (yaml.Contains("HotUpdate::CardPreviewWindow"))
        {
            return;
        }

        long rootId = FindYamlFileId(yaml, "m_Name: CardPreviewWindow");
        long listId = FindYamlFileId(yaml, "HotUpdate::GridListController");
        long messageId = FindNamedScript(yaml, "Txt_Message", "TMPro.TextMeshProUGUI");
        long noId = FindNamedScript(yaml, "Btn_No", "UnityEngine.UI.Button");
        long okId = FindNamedScript(yaml, "Btn_Ok", "UnityEngine.UI.Button");
        if (rootId == 0 || listId == 0 || messageId == 0 || noId == 0 || okId == 0)
        {
            throw new Exception(
                $"注入窗口脚本失败. root={rootId}; list={listId}; message={messageId}; no={noId}; ok={okId}");
        }

        long componentId = 7413544022705800948;
        yaml = AddComponentToGameObject(yaml, rootId, componentId);
        yaml +=
            "\n--- !u!114 &" + componentId + "\n" +
            "MonoBehaviour:\n" +
            "  m_ObjectHideFlags: 0\n" +
            "  m_CorrespondingSourceObject: {fileID: 0}\n" +
            "  m_PrefabInstance: {fileID: 0}\n" +
            "  m_PrefabAsset: {fileID: 0}\n" +
            "  m_GameObject: {fileID: " + rootId + "}\n" +
            "  m_Enabled: 1\n" +
            "  m_EditorHideFlags: 0\n" +
            "  m_Script: {fileID: 11500000, guid: 7e4c2b91a6d84f2e9c8a15b3d0f24680, type: 3}\n" +
            "  m_Name: \n" +
            "  m_EditorClassIdentifier: HotUpdate::CardPreviewWindow\n" +
            "  m_destroyOnClose: 0\n" +
            "  hideOnForegroundLost: 1\n" +
            "  windowPriority: 0\n" +
            "  isPopup: 1\n" +
            "  m_TxtMessage: {fileID: " + messageId + "}\n" +
            "  m_BtnNo: {fileID: " + noId + "}\n" +
            "  m_BtnOk: {fileID: " + okId + "}\n" +
            "  m_ListController: {fileID: " + listId + "}\n";
        System.IO.File.WriteAllText(WindowPath, yaml);
        AssetDatabase.ImportAsset(WindowPath);
    }

    static long FindNamedScript(string yaml, string objectName, string classIdentifier)
    {
        int nameIndex = yaml.IndexOf("m_Name: " + objectName, StringComparison.Ordinal);
        if (nameIndex < 0)
        {
            return 0;
        }

        int start = yaml.LastIndexOf("--- !u!1 &", nameIndex, StringComparison.Ordinal);
        int nextObject = yaml.IndexOf("\n--- !u!1 &", nameIndex, StringComparison.Ordinal);
        string block = nextObject > start ? yaml.Substring(start, nextObject - start) : yaml.Substring(start);
        int classIndex = yaml.IndexOf(classIdentifier, nameIndex, StringComparison.Ordinal);
        if (classIndex < 0 || (nextObject > 0 && classIndex > nextObject + 4000))
        {
            classIndex = yaml.IndexOf(classIdentifier, start, StringComparison.Ordinal);
        }

        if (classIndex < 0)
        {
            return 0;
        }

        int header = yaml.LastIndexOf("--- !u!114 &", classIndex, StringComparison.Ordinal);
        if (header < 0)
        {
            return 0;
        }

        int amp = yaml.IndexOf('&', header);
        int end = yaml.IndexOf('\n', amp);
        return long.Parse(yaml.Substring(amp + 1, end - amp - 1).Trim());
    }

    static long FindYamlFileId(string yaml, string marker)
    {
        int markerIndex = yaml.IndexOf(marker, StringComparison.Ordinal);
        if (markerIndex < 0)
        {
            return 0;
        }

        int start = yaml.LastIndexOf("--- !u!", markerIndex, StringComparison.Ordinal);
        if (start < 0)
        {
            return 0;
        }

        int amp = yaml.IndexOf('&', start);
        int end = yaml.IndexOf('\n', amp);
        if (amp < 0 || end < 0)
        {
            return 0;
        }

        return long.Parse(yaml.Substring(amp + 1, end - amp - 1).Trim());
    }

    static string AddComponentToGameObject(string yaml, long rootId, long componentId)
    {
        string idLine = "&" + rootId;
        int start = yaml.IndexOf(idLine, StringComparison.Ordinal);
        if (start < 0)
        {
            throw new Exception("找不到窗口根物体");
        }

        int components = yaml.IndexOf("m_Component:", start, StringComparison.Ordinal);
        int layer = yaml.IndexOf("m_Layer:", components, StringComparison.Ordinal);
        if (components < 0 || layer < 0)
        {
            throw new Exception("找不到窗口组件列表");
        }

        string insertion = "  - component: {fileID: " + componentId + "}\n";
        return yaml.Insert(layer, insertion);
    }

    static void RegisterPrefabs()
    {
        AddressableCatalogMenu.AddPrefab(WindowPath);
        AddressableCatalogMenu.AddPrefab(RowPath);

        UISettings settings = AssetDatabase.LoadAssetAtPath<UISettings>(SettingsPath);
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(WindowPath);
        if (settings == null || prefab == null)
        {
            throw new Exception("找不到 UISetting 或 CardPreviewWindow 预制体");
        }

        SerializedObject so = new SerializedObject(settings);
        SerializedProperty prop = so.FindProperty("screensToRegister");
        for (int i = 0; i < prop.arraySize; i++)
        {
            if (prop.GetArrayElementAtIndex(i).objectReferenceValue == prefab)
            {
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(settings);
                AssetDatabase.SaveAssets();
                return;
            }
        }

        int index = prop.arraySize;
        prop.arraySize = index + 1;
        prop.GetArrayElementAtIndex(index).objectReferenceValue = prefab;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(settings);
        AssetDatabase.SaveAssets();
    }

    static Type RequireType(string name)
    {
        Type type = FindType(name);
        if (type == null)
        {
            throw new Exception("类型尚未编译: " + name);
        }

        return type;
    }

    static Type FindType(string name)
    {
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        for (int i = 0; i < assemblies.Length; i++)
        {
            Type type = assemblies[i].GetType(name);
            if (type != null)
            {
                return type;
            }
        }

        return null;
    }

    static GameObject CreateUiChild(Transform parent, string name, params Type[] components)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.layer = UiLayer;
        go.transform.SetParent(parent, false);
        for (int i = 0; i < components.Length; i++)
        {
            if (go.GetComponent(components[i]) == null)
            {
                go.AddComponent(components[i]);
            }
        }

        return go;
    }

    static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.pivot = new Vector2(0.5f, 0.5f);
    }

    static T FindComponent<T>(Transform root, string name) where T : Component
    {
        Transform child = root.Find(name);
        if (child == null)
        {
            Transform[] all = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i].name == name)
                {
                    child = all[i];
                    break;
                }
            }
        }

        return child != null ? child.GetComponent<T>() : null;
    }

    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
        {
            return;
        }

        string parent = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
        string name = System.IO.Path.GetFileName(path);
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, name);
    }
}
