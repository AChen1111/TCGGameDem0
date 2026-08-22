using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

public static class AddressableCatalogMenu
{
    [MenuItem("Assets/AddToSpriteSO", false, 2000)]
    static void AddToSpriteSO()
    {
        foreach (string path in SelectedPaths("t:Sprite"))
        {
            AddSprite(path);
        }
    }

    [MenuItem("Assets/AddToSpriteSO", true)]
    static bool ValidateAddToSpriteSO()
    {
        return SelectedPaths("t:Sprite").Count > 0;
    }

    [MenuItem("Assets/AddToPrefabSO", false, 2001)]
    static void AddToPrefabSO()
    {
        foreach (string path in SelectedPaths("t:Prefab"))
        {
            AddPrefab(path);
        }
    }

    [MenuItem("Assets/AddToPrefabSO", true)]
    static bool ValidateAddToPrefabSO()
    {
        return SelectedPaths("t:Prefab").Count > 0;
    }

    [MenuItem("Assets/AddToSceneSO", false, 2002)]
    static void AddToSceneSO()
    {
        foreach (string path in SelectedPaths("t:Scene"))
        {
            AddScene(path);
        }
    }

    [MenuItem("Assets/AddToSceneSO", true)]
    static bool ValidateAddToSceneSO()
    {
        return SelectedPaths("t:Scene").Count > 0;
    }

    [MenuItem("Assets/AddToUISettingsSO", false, 2003)]
    static void AddToUISettingsSO()
    {
        foreach (string path in SelectedPaths("t:UISettings"))
        {
            AddToUISettingsCatalog(path);
        }
    }

    [MenuItem("Assets/AddToUISettingsSO", true)]
    static bool ValidateAddToUISettingsSO()
    {
        return SelectedPaths("t:UISettings").Count > 0;
    }

    public static void AddSprite(string path)
    {
        var catalog = AssetDatabase.LoadAssetAtPath<SpriteAddressableCatalog>(AddressableCatalogSetup.SpritePath);
        string folder = Path.GetDirectoryName(path).Replace('\\', '/');
        AddressableCatalogSetup.MarkFolderInGroup(AddressableCatalogSetup.RemoteCardGroup, folder);
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
        for (int i = 0; i < assets.Length; i++)
        {
            if (assets[i] is Sprite sprite)
            {
                catalog.EditorAdd(sprite.name, new AssetReferenceSprite(AssetDatabase.AssetPathToGUID(path))
                {
                    SubObjectName = sprite.name
                });
            }
        }

        AddressableCatalogSetup.SyncAddressKeys();
        Save(catalog);
    }

    public static void AddPrefab(string path)
    {
        var catalog = AssetDatabase.LoadAssetAtPath<PrefabAddressableCatalog>(AddressableCatalogSetup.PrefabPath);
        string name = Path.GetFileNameWithoutExtension(path);
        string folder = Path.GetDirectoryName(path).Replace('\\', '/');
        AddressableCatalogSetup.MarkFolderInGroup(AddressableCatalogSetup.UiGroupForPath(folder), folder);
        catalog.EditorAdd(name, new AssetReferenceGameObject(AssetDatabase.AssetPathToGUID(path)));
        AddressableCatalogSetup.SyncAddressKeys();
        Save(catalog);
    }

    public static void AddScene(string path)
    {
        string name = Path.GetFileNameWithoutExtension(path);
        if (path == AddressableCatalogSetup.InitScenePath)
        {
            AddressableCatalogSetup.MarkInGroup(AddressableCatalogSetup.LocalBootGroup, path, name);
            return;
        }

        AddressableCatalogSetup.EnsureRemoteSceneGroup();
        AddressableCatalogSetup.MarkInGroup(AddressableCatalogSetup.RemoteSceneGroup, path, name);
        var catalog = AssetDatabase.LoadAssetAtPath<SceneAddressableCatalog>(AddressableCatalogSetup.ScenePath);
        catalog.EditorAdd(name, new AssetReferenceScene(AssetDatabase.AssetPathToGUID(path)));
        AddressableCatalogSetup.SyncAddressKeys();
        Save(catalog);
    }

    public static void AddToUISettingsCatalog(string path)
    {
        var catalog = AssetDatabase.LoadAssetAtPath<UISettingsAddressableCatalog>(AddressableCatalogSetup.UISettingsPath);
        string folder = Path.GetDirectoryName(path).Replace('\\', '/');
        AddressableCatalogSetup.MarkFolderInGroup(AddressableCatalogSetup.UiGroupForPath(folder), folder);
        catalog.EditorAdd(Path.GetFileNameWithoutExtension(path), new AssetReferenceUISettings(AssetDatabase.AssetPathToGUID(path)));
        AddressableCatalogSetup.SyncAddressKeys();
        Save(catalog);
    }

    static List<string> SelectedPaths(string filter)
    {
        var paths = new List<string>();
        foreach (string guid in Selection.assetGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (AssetDatabase.IsValidFolder(path))
            {
                string[] found = AssetDatabase.FindAssets(filter, new[] { path });
                for (int i = 0; i < found.Length; i++)
                {
                    paths.Add(AssetDatabase.GUIDToAssetPath(found[i]));
                }
            }
            else if (Matches(path, filter))
            {
                paths.Add(path);
            }
        }

        return paths;
    }

    static bool Matches(string path, string filter)
    {
        System.Type main = AssetDatabase.GetMainAssetTypeAtPath(path);
        if (filter == "t:Sprite")
        {
            return main == typeof(Texture2D) || main == typeof(Sprite);
        }

        if (filter == "t:Prefab")
        {
            return path.EndsWith(".prefab", System.StringComparison.OrdinalIgnoreCase);
        }

        if (filter == "t:UISettings")
        {
            return typeof(UISettings).IsAssignableFrom(main);
        }

        return path.EndsWith(".unity", System.StringComparison.OrdinalIgnoreCase);
    }

    static void Save(Object catalog)
    {
        EditorUtility.SetDirty(catalog);
        AssetDatabase.SaveAssets();
    }
}
