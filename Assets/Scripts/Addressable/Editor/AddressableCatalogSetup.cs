using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine.AddressableAssets;

public static class AddressableCatalogSetup
{
    public const string CatalogsFolder = "Assets/AddressableCatalogs";
    public const string AddressKeysPath = "Assets/Scripts/Addressable/AddressKeys.cs";
    public const string SpritePath = CatalogsFolder + "/SpriteCatalog.asset";
    public const string PrefabPath = CatalogsFolder + "/PrefabCatalog.asset";
    public const string ScenePath = CatalogsFolder + "/SceneCatalog.asset";
    public const string UISettingsPath = CatalogsFolder + "/UISettingsCatalog.asset";
    public const string InitScenePath = "Assets/Scenes/Init.unity";
    public const string GameScenePath = "Assets/Scenes/GameScene.unity";
    public const string BaseUiFolder = "Assets/UI/Prefab/BaseUI";
    public const string HallFolder = "Assets/UI/Prefab/Hall";
    public const string FontsFolder = "Assets/UI/Fonts";
    public const string SpriteFolder = "Assets/UI/Sprite";
    public const string CardFolder = "Assets/UI/Card";
    public const string ShaderFolder = "Assets/UI/Shader";
    public const string PreGameUiFolder = HallFolder + "/PreGameUI";
    public const string PreGameUiSettingsPath = PreGameUiFolder + "/PreGameSceneUI.asset";
    public const string PreGameUiPanelPath = PreGameUiFolder + "/PreGameUIPanel.prefab";

    public const string LocalBootGroup = "Local_Boot";
    public const string RemoteCatalogGroup = "Remote_Catalog";
    public const string RemoteUiHallGroup = "Remote_UI_Hall";
    public const string RemoteUiEventGroup = "Remote_UI_Event";
    public const string RemoteSharedGroup = "Remote_Shared";
    public const string RemoteCardGroup = "Remote_Card";
    public const string RemoteSceneGroup = "Remote_Scene";

    public static string UiGroupForPath(string path)
    {
        path = path.Replace('\\', '/');
        if (path.Contains("/Event/") || path.Contains("/LiveOps/"))
        {
            return RemoteUiEventGroup;
        }

        if (path.Contains("/BaseUI") || path.Contains("/Fonts/") || path.Contains(FontsFolder)
            || path.Contains("/Shader/") || path.Contains(ShaderFolder))
        {
            return RemoteSharedGroup;
        }

        if (path.Contains("/Card"))
        {
            return RemoteCardGroup;
        }

        return RemoteUiHallGroup;
    }

    public static void MarkUiArtFolders()
    {
        MarkFolderInGroup(RemoteSharedGroup, FontsFolder);
        if (AssetDatabase.IsValidFolder(ShaderFolder))
        {
            MarkFolderInGroup(RemoteSharedGroup, ShaderFolder);
        }

        if (AssetDatabase.IsValidFolder(CardFolder + "/CardShopItem"))
        {
            MarkFolderInGroup(RemoteCardGroup, CardFolder + "/CardShopItem");
        }

        if (AssetDatabase.IsValidFolder(CardFolder + "/Wallpaper"))
        {
            MarkFolderInGroup(RemoteCardGroup, CardFolder + "/Wallpaper");
        }

        if (!AssetDatabase.IsValidFolder(SpriteFolder))
        {
            return;
        }

        string[] children = AssetDatabase.GetSubFolders(SpriteFolder);
        for (int i = 0; i < children.Length; i++)
        {
            string child = children[i].Replace('\\', '/');
            MarkFolderInGroup(UiGroupForPath(child), child, "Sprite/" + Path.GetFileName(child));
        }
    }

    public static void MarkFolderInGroup(string groupName, string folderPath)
    {
        MarkFolderInGroup(groupName, folderPath, null);
    }

    public static void MarkFolderInGroup(string groupName, string folderPath, string address)
    {
        folderPath = folderPath.Replace('\\', '/').TrimEnd('/');
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.GetSettings(true);
        AddressableAssetGroup group = settings.FindGroup(groupName);
        string[] childGuids = AssetDatabase.FindAssets("", new[] { folderPath });
        for (int i = 0; i < childGuids.Length; i++)
        {
            string childPath = AssetDatabase.GUIDToAssetPath(childGuids[i]);
            if (childPath.Replace('\\', '/') == folderPath)
            {
                continue;
            }

            settings.RemoveAssetEntry(childGuids[i]);
        }

        AddressableAssetEntry entry = MarkAddressable(settings, group, folderPath,
            string.IsNullOrEmpty(address) ? Path.GetFileName(folderPath) : address);
        entry.IsFolder = true;
    }

    public static void SyncAddressKeys()
    {
        var sb = new StringBuilder();
        sb.AppendLine("public static class AddressKeys");
        sb.AppendLine("{");
        WriteNested(sb, "Prefab", AssetDatabase.LoadAssetAtPath<PrefabAddressableCatalog>(PrefabPath));
        WriteNested(sb, "Sprite", AssetDatabase.LoadAssetAtPath<SpriteAddressableCatalog>(SpritePath));
        WriteNested(sb, "Scene", AssetDatabase.LoadAssetAtPath<SceneAddressableCatalog>(ScenePath));
        WriteNested(sb, "UISettings", AssetDatabase.LoadAssetAtPath<UISettingsAddressableCatalog>(UISettingsPath));
        sb.AppendLine();
        sb.AppendLine("    public static string GetAvatarAddress(int avatarId)");
        sb.AppendLine("    {");
        sb.AppendLine("        return $\"a_{avatarId:D2}\";");
        sb.AppendLine("    }");
        sb.AppendLine("    public static string GetBackgroundDownAddress(int backgroundId)");
        sb.AppendLine("    {");
        sb.AppendLine("        return $\"w_{backgroundId:D2}_Down\";");
        sb.AppendLine("    }");
        sb.AppendLine("    public static string GetBackgroundSpriteAddress(int backgroundId)");
        sb.AppendLine("    {");
        sb.AppendLine("        return $\"w_{backgroundId:D2}_Sprite\";");
        sb.AppendLine("    }");
        sb.AppendLine("}");
        string text = sb.ToString();
        if (File.ReadAllText(AddressKeysPath).Replace("\r\n", "\n") == text.Replace("\r\n", "\n"))
        {
            return;
        }

        File.WriteAllText(AddressKeysPath, text);
        AssetDatabase.ImportAsset(AddressKeysPath);
    }

    public static void MarkInGroup(string groupName, string path, string address)
    {
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.GetSettings(true);
        MarkAddressable(settings, settings.FindGroup(groupName), path, address);
    }

    public static void EnsureRemoteSceneGroup()
    {
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.GetSettings(true);
        AddressableAssetGroup group = settings.FindGroup(RemoteSceneGroup);
        if (group == null)
        {
            group = settings.CreateGroup(RemoteSceneGroup, false, false, true, null,
                typeof(BundledAssetGroupSchema), typeof(ContentUpdateGroupSchema));
        }

        BundledAssetGroupSchema schema = group.GetSchema<BundledAssetGroupSchema>();
        schema.BuildPath.SetVariableByName(settings, AddressableAssetSettings.kRemoteBuildPath);
        schema.LoadPath.SetVariableByName(settings, AddressableAssetSettings.kRemoteLoadPath);
        schema.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackSeparately;
        EditorUtility.SetDirty(schema);
        EditorUtility.SetDirty(group);
        EditorUtility.SetDirty(settings);
    }

    public static void EnsureSceneAddressables()
    {
        EnsureRemoteSceneGroup();
        MarkInGroup(LocalBootGroup, InitScenePath, "Init");
        AddressableCatalogMenu.AddScene(GameScenePath);
    }

    public static bool IsDirectEntry(AddressableAssetGroup group, string guid)
    {
        foreach (AddressableAssetEntry entry in group.entries)
        {
            if (entry.guid == guid)
            {
                return true;
            }
        }

        return false;
    }

    static void WriteNested<TRef>(StringBuilder sb, string className, AddressableCatalog<TRef> catalog)
        where TRef : AssetReference
    {
        sb.AppendLine("    public static class " + className);
        sb.AppendLine("    {");
        var names = new List<string>();
        List<AddressableEntry<TRef>> entries = catalog.Entries;
        for (int i = 0; i < entries.Count; i++)
        {
            names.Add(entries[i].assetName);
        }

        names.Sort(System.StringComparer.Ordinal);
        for (int i = 0; i < names.Count; i++)
        {
            sb.AppendLine("        public static readonly string " + names[i] + " = \"" + names[i] + "\";");
        }

        sb.AppendLine("    }");
    }

    static AddressableAssetEntry MarkAddressable(AddressableAssetSettings settings, AddressableAssetGroup group, string path, string address)
    {
        AddressableAssetEntry entry = settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(path), group, false, false);
        entry.SetAddress(address, false);
        return entry;
    }
}
