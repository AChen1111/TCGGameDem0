using System;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

public static class SetupPublishedConfig
{
    public static void Run()
    {
        const string root = "Assets/GameConfiguration";
        AssetDatabase.ImportAsset(root);
        if (!AssetDatabase.IsValidFolder(root)) AssetDatabase.CreateFolder("Assets", "GameConfiguration");
        const string oldSettings = "Assets/Resources/Localization/Settings.asset";
        const string newSettings = root + "/LocalizationSettings.asset";
        if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(oldSettings) != null)
        {
            string error = AssetDatabase.MoveAsset(oldSettings, newSettings);
            if (!string.IsNullOrEmpty(error)) throw new InvalidOperationException(error);
        }
        AssetDatabase.ImportAsset(root + "/config.json");
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        var group = settings.FindGroup("Remote_GameConfig") ?? settings.CreateGroup("Remote_GameConfig", false, false, false, null,
            typeof(BundledAssetGroupSchema), typeof(ContentUpdateGroupSchema));
        var schema = group.GetSchema<BundledAssetGroupSchema>();
        schema.BuildPath.SetVariableByName(settings, "Remote.BuildPath");
        schema.LoadPath.SetVariableByName(settings, "Remote.LoadPath");
        schema.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackTogether;
        group.GetSchema<ContentUpdateGroupSchema>().StaticContent = false;
        settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(root + "/config.json"), group).SetAddress("GameConfig/Data");
        settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(newSettings), group).SetAddress("GameConfig/LocalizationSettings");
        foreach (var path in new[] { "Assets/Resources/Card/Cards.bytes", "Assets/Resources/Localization/Translations.bytes" })
            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path) != null) AssetDatabase.DeleteAsset(path);
        var prefabPath = "Assets/UI/Prefab/Hall/PreGameUI/PreGameUIPanel.prefab";
        var prefab = PrefabUtility.LoadPrefabContents(prefabPath);
        try
        {
            foreach (var component in prefab.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (component == null) continue;
                var serialized = new SerializedObject(component);
                var field = serialized.FindProperty("m_WallpaperDisplayConfig");
                if (field == null) continue;
                field.objectReferenceValue = null;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
            PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);
        }
        finally { PrefabUtility.UnloadPrefabContents(prefab); }
        EditorUtility.SetDirty(group); EditorUtility.SetDirty(settings); AssetDatabase.SaveAssets();
        Debug.Log("Remote_GameConfig created; legacy Resources tables removed; wallpaper reference detached.");
    }
}
