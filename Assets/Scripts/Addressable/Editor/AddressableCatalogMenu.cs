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
        AddressableCatalogSetup.EnsureCatalogs();
        var catalog = AssetDatabase.LoadAssetAtPath<SpriteAddressableCatalog>(AddressableCatalogSetup.SpritePath);
        foreach (string path in SelectedPaths("t:Sprite"))
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
            for (int i = 0; i < assets.Length; i++)
            {
                if (assets[i] is Sprite sprite)
                {
                    string guid = AssetDatabase.AssetPathToGUID(path);
                    AddressableCatalogSetup.MarkInDefaultGroup(path, sprite.name);
                    catalog.EditorAdd(sprite.name, new AssetReferenceSprite(guid) { SubObjectName = sprite.name });
                }
            }
        }
        Save(catalog);
    }

    [MenuItem("Assets/AddToSpriteSO", true)]
    static bool ValidateAddToSpriteSO()
    {
        return SelectedPaths("t:Sprite").Count > 0;
    }

    [MenuItem("Assets/AddToPrefabSO", false, 2001)]
    static void AddToPrefabSO()
    {
        AddressableCatalogSetup.EnsureCatalogs();
        var catalog = AssetDatabase.LoadAssetAtPath<PrefabAddressableCatalog>(AddressableCatalogSetup.PrefabPath);
        foreach (string path in SelectedPaths("t:Prefab"))
        {
            string name = Path.GetFileNameWithoutExtension(path);
            string label = Path.GetFileName(Path.GetDirectoryName(path));
            AddressableCatalogSetup.MarkInGroup(AddressableCatalogSetup.RemoteUiGroup, path, name, label);
            catalog.EditorAdd(name, new AssetReferenceGameObject(AssetDatabase.AssetPathToGUID(path)));
        }
        Save(catalog);
    }

    [MenuItem("Assets/AddToPrefabSO", true)]
    static bool ValidateAddToPrefabSO()
    {
        return SelectedPaths("t:Prefab").Count > 0;
    }

    [MenuItem("Assets/AddToSceneSO", false, 2002)]
    static void AddToSceneSO()
    {
        AddressableCatalogSetup.EnsureCatalogs();
        var catalog = AssetDatabase.LoadAssetAtPath<SceneAddressableCatalog>(AddressableCatalogSetup.ScenePath);
        foreach (string path in SelectedPaths("t:Scene"))
        {
            string name = Path.GetFileNameWithoutExtension(path);
            if (path == AddressableCatalogSetup.InitScenePath)
            {
                AddressableCatalogSetup.MarkInGroup(AddressableCatalogSetup.LocalBootGroup, path, name);
            }
            else
            {
                AddressableCatalogSetup.MarkInDefaultGroup(path, name);
            }
            catalog.EditorAdd(name, new AssetReferenceScene(AssetDatabase.AssetPathToGUID(path)));
        }
        Save(catalog);
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

    public static void AddToUISettingsCatalog(string path)
    {
        AddressableCatalogSetup.EnsureCatalogs();
        var catalog = AssetDatabase.LoadAssetAtPath<UISettingsAddressableCatalog>(AddressableCatalogSetup.UISettingsPath);
        string name = Path.GetFileNameWithoutExtension(path);
        string label = Path.GetFileName(Path.GetDirectoryName(path));
        AddressableCatalogSetup.MarkInGroup(AddressableCatalogSetup.RemoteUiGroup, path, name, label);
        catalog.EditorAdd(name, new AssetReferenceUISettings(AssetDatabase.AssetPathToGUID(path)));
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
