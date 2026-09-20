using UnityEditor;
using UnityEngine;

public static class SetupSettingWindow
{
    const string PrefabPath = "Assets/UI/Prefab/Hall/PreGameUI/Meau/SettingWindow.prefab";
    const string SettingsPath = "Assets/UI/Prefab/Hall/UISetting.asset";

    public static string Run()
    {
        string localized = BindLocalizedTexts();
        AddressableCatalogMenu.AddPrefab(PrefabPath);
        string registered = RegisterSettings();
        PublishedConfigBuilder.Prepare();
        AssetDatabase.SaveAssets();
        return localized + ";" + registered + ";bytes-ok";
    }

    static string BindLocalizedTexts()
    {
        GameObject root = PrefabUtility.LoadPrefabContents(PrefabPath);
        try
        {
            Bind(root.transform, "Btn_Exit/Text (TMP)", "ui.settings.logout", false);
            Bind(root.transform, "Sld_BGM/BGM_txt", "ui.settings.bgm", false);
            Bind(root.transform, "Sld_SFX/BGM_txt", "ui.settings.sfx", false);
            Bind(root.transform, "Btn_ChangeLanguage/Text (TMP)", "", true);
            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            return "localized-ok";
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    static void Bind(Transform root, string path, string key, bool dynamicContent)
    {
        Transform target = root.Find(path);
        if (target == null)
            throw new System.Exception("找不到文本: " + path);

        LocalizedText localized = target.GetComponent<LocalizedText>();
        if (localized == null)
            localized = target.gameObject.AddComponent<LocalizedText>();

        SerializedObject so = new SerializedObject(localized);
        so.FindProperty("key").stringValue = key;
        so.FindProperty("dynamicContent").boolValue = dynamicContent;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static string RegisterSettings()
    {
        UISettings settings = AssetDatabase.LoadAssetAtPath<UISettings>(SettingsPath);
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (settings == null || prefab == null)
            throw new System.Exception("找不到 UISetting 或 SettingWindow 预制体");

        SerializedObject so = new SerializedObject(settings);
        SerializedProperty prop = so.FindProperty("screensToRegister");
        for (int i = 0; i < prop.arraySize; i++)
        {
            if (prop.GetArrayElementAtIndex(i).objectReferenceValue == prefab)
                return "uisetting-exists";
        }

        int index = prop.arraySize;
        prop.arraySize = index + 1;
        prop.GetArrayElementAtIndex(index).objectReferenceValue = prefab;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(settings);
        return "uisetting-added";
    }
}
