using UnityEditor;
using UnityEngine;

public static class RegisterCardDetailWindow
{
    const string OverlayPath = "Assets/UI/Prefab/Hall/Shop/CardDetailOverlay.prefab";
    const string SettingsPath = "Assets/UI/Prefab/Hall/UISetting.asset";
    const string PreviewPath = "Assets/UI/Prefab/BaseUI/CardPreviewWindow.prefab";
    const string PickPath = "Assets/UI/Prefab/Hall/Shop/CardPickWindow.prefab";

    public static string Run()
    {
        AddressableCatalogMenu.AddPrefab(OverlayPath);
        string settings = RegisterSettings();
        string preview = StripNested(PreviewPath);
        string pick = StripNested(PickPath);
        AssetDatabase.SaveAssets();
        return "catalog-ok;" + settings + ";" + preview + ";" + pick;
    }

    static string RegisterSettings()
    {
        UISettings settings = AssetDatabase.LoadAssetAtPath<UISettings>(SettingsPath);
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(OverlayPath);
        if (settings == null || prefab == null)
        {
            throw new System.Exception("找不到 UISetting 或 CardDetailOverlay 预制体");
        }

        if (prefab.GetComponent<IUIScreenController>() == null)
        {
            return "uisetting-skipped-no-controller";
        }

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

    static string StripNested(string prefabPath)
    {
        GameObject contents = PrefabUtility.LoadPrefabContents(prefabPath);
        try
        {
            Transform overlay = FindNamed(contents.transform, "CardDetailOverlay");
            if (overlay == null)
            {
                return System.IO.Path.GetFileNameWithoutExtension(prefabPath) + "-no-nested";
            }

            Object.DestroyImmediate(overlay.gameObject);
            PrefabUtility.SaveAsPrefabAsset(contents, prefabPath);
            return System.IO.Path.GetFileNameWithoutExtension(prefabPath) + "-stripped";
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(contents);
        }
    }

    static Transform FindNamed(Transform root, string objectName)
    {
        Transform[] all = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i] != root && all[i].name == objectName)
            {
                return all[i];
            }
        }

        return null;
    }
}
