using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

public static class AddressableCatalogSetup
{
    public const string CatalogsFolder = "Assets/AddressableCatalogs";
    public const string SpritePath = CatalogsFolder + "/SpriteCatalog.asset";
    public const string PrefabPath = CatalogsFolder + "/PrefabCatalog.asset";
    public const string ScenePath = CatalogsFolder + "/SceneCatalog.asset";
    public const string UISettingsPath = CatalogsFolder + "/UISettingsCatalog.asset";
    public const string InitScenePath = "Assets/Scenes/Init.unity";

    public const string CatalogsGroup = "Catalogs";
    public const string LocalBootGroup = "Local_Boot";
    public const string RemoteUiGroup = "Remote_UI";

    [MenuItem("Tools/Addressable/Ensure Groups")]
    public static void EnsureCatalogs()
    {
        if (!AssetDatabase.IsValidFolder(CatalogsFolder))
        {
            AssetDatabase.CreateFolder("Assets", "AddressableCatalogs");
        }

        CreateIfMissing<SpriteAddressableCatalog>(SpritePath);
        CreateIfMissing<PrefabAddressableCatalog>(PrefabPath);
        CreateIfMissing<SceneAddressableCatalog>(ScenePath);
        CreateIfMissing<UISettingsAddressableCatalog>(UISettingsPath);

        EnsureGroups();
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.GetSettings(true);
        AddressableAssetGroup catalogs = settings.FindGroup(CatalogsGroup);
        MarkAddressable(settings, catalogs, SpritePath);
        MarkAddressable(settings, catalogs, PrefabPath);
        MarkAddressable(settings, catalogs, ScenePath);
        MarkAddressable(settings, catalogs, UISettingsPath);
        MarkInGroup(LocalBootGroup, InitScenePath, "Init");
        AssetDatabase.SaveAssets();
    }

    public static void EnsureGroups()
    {
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.GetSettings(true);
        EnsureGroup(settings, CatalogsGroup, BundledAssetGroupSchema.BundlePackingMode.PackTogether);
        EnsureGroup(settings, LocalBootGroup, BundledAssetGroupSchema.BundlePackingMode.PackTogether);
        EnsureGroup(settings, RemoteUiGroup, BundledAssetGroupSchema.BundlePackingMode.PackTogetherByLabel);
        AssetDatabase.SaveAssets();
    }

    public static void WireLoader(AddressableLoader loader)
    {
        SerializedObject so = new SerializedObject(loader);
        SetAssetRef(so, "m_spriteCatalogRef", SpritePath);
        SetAssetRef(so, "m_prefabCatalogRef", PrefabPath);
        SetAssetRef(so, "m_sceneCatalogRef", ScenePath);
        SetAssetRef(so, "m_uiSettingsCatalogRef", UISettingsPath);
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    public static void WireSingletonManager(AddressableLoader loader)
    {
        SingletonManager manager = Object.FindFirstObjectByType<SingletonManager>();
        SerializedObject so = new SerializedObject(manager);
        SerializedProperty list = so.FindProperty("m_singletons");
        for (int i = 0; i < list.arraySize; i++)
        {
            if (list.GetArrayElementAtIndex(i).objectReferenceValue == loader)
            {
                return;
            }
        }

        list.InsertArrayElementAtIndex(0);
        list.GetArrayElementAtIndex(0).objectReferenceValue = loader;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static T CreateIfMissing<T>(string path) where T : ScriptableObject
    {
        T existing = AssetDatabase.LoadAssetAtPath<T>(path);
        if (existing != null)
        {
            BindScript(existing);
            return existing;
        }

        T asset = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(asset, path);
        BindScript(asset);
        return asset;
    }

    static void BindScript<T>(T asset) where T : ScriptableObject
    {
        T temp = ScriptableObject.CreateInstance<T>();
        MonoScript script = MonoScript.FromScriptableObject(temp);
        Object.DestroyImmediate(temp);
        SerializedObject so = new SerializedObject(asset);
        so.FindProperty("m_Script").objectReferenceValue = script;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    public static void MarkInDefaultGroup(string path, string address)
    {
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.GetSettings(true);
        MarkAddressable(settings, settings.DefaultGroup, path, address);
    }

    public static void MarkInGroup(string groupName, string path, string address, string label = null)
    {
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.GetSettings(true);
        AddressableAssetGroup group = settings.FindGroup(groupName);
        AddressableAssetEntry entry = MarkAddressable(settings, group, path, address);
        if (!string.IsNullOrEmpty(label))
        {
            entry.SetLabel(label, true, true, false);
        }
    }

    static AddressableAssetGroup EnsureGroup(
        AddressableAssetSettings settings,
        string name,
        BundledAssetGroupSchema.BundlePackingMode packing)
    {
        AddressableAssetGroup group = settings.FindGroup(name);
        if (group == null)
        {
            group = settings.CreateGroup(name, false, false, true, null, typeof(BundledAssetGroupSchema), typeof(ContentUpdateGroupSchema));
        }

        BundledAssetGroupSchema bundled = group.GetSchema<BundledAssetGroupSchema>();
        bundled.BundleMode = packing;
        bundled.BuildPath.SetVariableByName(settings, AddressableAssetSettings.kLocalBuildPath);
        bundled.LoadPath.SetVariableByName(settings, AddressableAssetSettings.kLocalLoadPath);
        EditorUtility.SetDirty(bundled);
        EditorUtility.SetDirty(group);
        return group;
    }

    static void MarkAddressable(AddressableAssetSettings settings, AddressableAssetGroup group, string path)
    {
        MarkAddressable(settings, group, path, Path.GetFileNameWithoutExtension(path));
    }

    static AddressableAssetEntry MarkAddressable(AddressableAssetSettings settings, AddressableAssetGroup group, string path, string address)
    {
        string guid = AssetDatabase.AssetPathToGUID(path);
        AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, group, false, false);
        entry.SetAddress(address, false);
        return entry;
    }

    static void SetAssetRef(SerializedObject so, string field, string assetPath)
    {
        so.FindProperty(field).FindPropertyRelative("m_AssetGUID").stringValue = AssetDatabase.AssetPathToGUID(assetPath);
    }
}
