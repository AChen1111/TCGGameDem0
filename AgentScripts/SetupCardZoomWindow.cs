using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class SetupCardZoomWindow
{
    const string PrefabPath = "Assets/UI/Prefab/Hall/Shop/CardZoomWindow.prefab";
    const string OverlayPath = "Assets/UI/Prefab/Hall/Shop/CardDetailOverlay.prefab";
    const string SettingsPath = "Assets/UI/Prefab/Hall/UISetting.asset";
    const string ScriptGuid = "c9d8e4b21a7f4e6d8b0c1a2e3f405174";
    const int UiLayer = 5;
    const long InjectedComponentId = 8123456789012345681;

    public static string Run()
    {
        string created = CreatePrefab();
        string injected = InjectController();
        string catalog = RegisterCatalog();
        string settings = RegisterSettings();
        string detail = AddDetailCardButton();
        AssetDatabase.SaveAssets();
        return created + ";" + injected + ";" + catalog + ";" + settings + ";" + detail;
    }

    static string CreatePrefab()
    {
        var root = new GameObject("CardZoomWindow", typeof(RectTransform));
        root.layer = UiLayer;
        Stretch(root.GetComponent<RectTransform>());
        root.SetActive(false);

        GameObject dim = Child(root.transform, "Img_Dim", typeof(CanvasRenderer), typeof(Image));
        Stretch(dim.GetComponent<RectTransform>());
        Image dimImage = dim.GetComponent<Image>();
        dimImage.color = new Color(0f, 0f, 0f, 0.4f);
        dimImage.raycastTarget = true;

        GameObject card = Child(root.transform, "Raw_Card", typeof(CanvasRenderer), typeof(RawImage));
        Stretch(card.GetComponent<RectTransform>());
        RawImage raw = card.GetComponent<RawImage>();
        raw.color = Color.white;
        raw.raycastTarget = true;

        GameObject stagePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Card/CardPickStage.prefab");
        Type zoomType = FindType("CardZoomWindow");
        if (zoomType != null && root.GetComponent(zoomType) == null)
        {
            Component controller = root.AddComponent(zoomType);
            SerializedObject so = new SerializedObject(controller);
            so.FindProperty("m_destroyOnClose").boolValue = false;
            so.FindProperty("hideOnForegroundLost").boolValue = true;
            so.FindProperty("windowPriority").enumValueIndex = 0;
            so.FindProperty("isPopup").boolValue = true;
            so.FindProperty("m_ImgDim").objectReferenceValue = dimImage;
            so.FindProperty("m_RawCard").objectReferenceValue = raw;
            SerializedProperty stage = so.FindProperty("m_StagePrefab");
            if (stage != null)
            {
                stage.objectReferenceValue = stagePrefab;
            }

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        Directory.CreateDirectory(Path.GetDirectoryName(PrefabPath));
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        UnityEngine.Object.DestroyImmediate(root);
        if (prefab == null)
        {
            throw new Exception("保存 CardZoomWindow 失败");
        }

        return zoomType != null ? "prefab-ok" : "prefab-ok;script-pending";
    }

    static string InjectController()
    {
        string yaml = File.ReadAllText(PrefabPath);
        if (yaml.Contains("HotUpdate::CardZoomWindow"))
        {
            return "inject-exists";
        }

        long rootId = FindYamlFileId(yaml, "m_Name: CardZoomWindow");
        if (rootId == 0)
        {
            throw new Exception("注入放大窗脚本失败: 找不到根物体");
        }

        yaml = AddComponentToGameObject(yaml, rootId, InjectedComponentId);
        yaml +=
            "\n--- !u!114 &" + InjectedComponentId + "\n" +
            "MonoBehaviour:\n" +
            "  m_ObjectHideFlags: 0\n" +
            "  m_CorrespondingSourceObject: {fileID: 0}\n" +
            "  m_PrefabInstance: {fileID: 0}\n" +
            "  m_PrefabAsset: {fileID: 0}\n" +
            "  m_GameObject: {fileID: " + rootId + "}\n" +
            "  m_Enabled: 1\n" +
            "  m_EditorHideFlags: 0\n" +
            "  m_Script: {fileID: 11500000, guid: " + ScriptGuid + ", type: 3}\n" +
            "  m_Name: \n" +
            "  m_EditorClassIdentifier: HotUpdate::CardZoomWindow\n" +
            "  m_destroyOnClose: 0\n" +
            "  hideOnForegroundLost: 1\n" +
            "  windowPriority: 0\n" +
            "  isPopup: 1\n" +
            "  m_StagePrefab: {fileID: 3403963794378004767, guid: 1fd3162e16e25d344b06f9c52f75f84a, type: 3}\n";
        File.WriteAllText(PrefabPath, yaml);
        AssetDatabase.ImportAsset(PrefabPath);
        return "inject-ok";
    }

    static string RegisterCatalog()
    {
        AddressableCatalogMenu.AddPrefab(PrefabPath);
        return "catalog-ok";
    }

    static string RegisterSettings()
    {
        UISettings settings = AssetDatabase.LoadAssetAtPath<UISettings>(SettingsPath);
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (settings == null || prefab == null)
        {
            throw new Exception("找不到 UISetting 或 CardZoomWindow 预制体");
        }

        if (prefab.GetComponent<IUIScreenController>() != null)
        {
            SerializedObject so = new SerializedObject(settings);
            SerializedProperty prop = so.FindProperty("screensToRegister");
            for (int i = 0; i < prop.arraySize; i++)
            {
                if (prop.GetArrayElementAtIndex(i).objectReferenceValue == prefab)
                {
                    return "uisetting-exists";
                }
            }

            int index = prop.arraySize;
            prop.arraySize = index + 1;
            prop.GetArrayElementAtIndex(index).objectReferenceValue = prefab;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(settings);
            return "uisetting-added";
        }

        string guid = AssetDatabase.AssetPathToGUID(PrefabPath);
        string yaml = File.ReadAllText(SettingsPath);
        if (yaml.Contains(guid))
        {
            return "uisetting-yaml-exists";
        }

        string prefabYaml = File.ReadAllText(PrefabPath);
        long rootId = FindYamlFileId(prefabYaml, "m_Name: CardZoomWindow");
        if (rootId == 0)
        {
            return "uisetting-skipped-no-root";
        }

        const string Marker = "  deactivateScreenGOs:";
        int marker = yaml.IndexOf(Marker, StringComparison.Ordinal);
        if (marker < 0)
        {
            return "uisetting-skipped-no-marker";
        }

        yaml = yaml.Insert(marker, "  - {fileID: " + rootId + ", guid: " + guid + ", type: 3}\n");
        File.WriteAllText(SettingsPath, yaml);
        AssetDatabase.ImportAsset(SettingsPath);
        return "uisetting-yaml-added";
    }

    static string AddDetailCardButton()
    {
        GameObject contents = PrefabUtility.LoadPrefabContents(OverlayPath);
        try
        {
            Transform card = FindDeep(contents.transform, "Raw_Card");
            if (card == null)
            {
                return "detail-no-raw";
            }

            RawImage raw = card.GetComponent<RawImage>();
            if (raw != null)
            {
                raw.raycastTarget = true;
            }

            Button button = card.GetComponent<Button>();
            if (button == null)
            {
                button = card.gameObject.AddComponent<Button>();
            }

            button.targetGraphic = raw;
            button.transition = Selectable.Transition.None;
            Navigation navigation = button.navigation;
            navigation.mode = Navigation.Mode.None;
            button.navigation = navigation;
            PrefabUtility.SaveAsPrefabAsset(contents, OverlayPath);
            return "detail-button-ok";
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(contents);
        }
    }

    static GameObject Child(Transform parent, string name, params Type[] components)
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
        rect.localScale = Vector3.one;
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

    static Transform FindDeep(Transform root, string name)
    {
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

    static long FindYamlFileId(string yaml, string marker)
    {
        int markerIndex = yaml.IndexOf(marker, StringComparison.Ordinal);
        if (markerIndex < 0)
        {
            return 0;
        }

        int start = yaml.LastIndexOf("--- !u!", markerIndex, StringComparison.Ordinal);
        int amp = yaml.IndexOf('&', start);
        int end = yaml.IndexOf('\n', amp);
        return long.Parse(yaml.Substring(amp + 1, end - amp - 1).Trim());
    }

    static string AddComponentToGameObject(string yaml, long rootId, long componentId)
    {
        string idLine = "&" + rootId;
        int start = yaml.IndexOf(idLine, StringComparison.Ordinal);
        int components = yaml.IndexOf("m_Component:", start, StringComparison.Ordinal);
        int layer = yaml.IndexOf("\n  m_Layer:", components, StringComparison.Ordinal);
        if (layer >= 0)
        {
            return yaml.Insert(layer + 1, "  - component: {fileID: " + componentId + "}\n");
        }

        layer = yaml.IndexOf("m_Layer:", components, StringComparison.Ordinal);
        return yaml.Insert(layer, "  - component: {fileID: " + componentId + "}\n  ");
    }
}
