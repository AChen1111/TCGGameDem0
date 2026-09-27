using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class AuthorDeckLimits
{
    const string Sprites = "Assets/UI/Sprite/DeckEditor/";
    const string Prefabs = "Assets/UI/Prefab/Hall/Deck/";
    static T[] All<T>(GameObject root) where T : Component => Resources.FindObjectsOfTypeAll<T>()
        .Where(x => x.transform == root.transform || x.transform.IsChildOf(root.transform)).ToArray();
    public static string Main()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play before authoring.");
        var sprites = new Sprite[3];
        for (int i = 0; i < 3; i++)
        {
            string path = Sprites + "GUI_T_Icon1_Limit0" + i + ".png";
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
            sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
        foreach (string name in new[] { "DeckCardCell", "DeckPlacedCardCell" })
        {
            var root = PrefabUtility.LoadPrefabContents(Prefabs + name + ".prefab");
            var cell = All<DeckCardCell>(root).Single();
            foreach (Transform old in root.transform.Cast<Transform>().Where(x => x.name == "LimitIcon").ToArray())
                UnityEngine.Object.DestroyImmediate(old.gameObject);
            var icon = new GameObject("LimitIcon", typeof(RectTransform), typeof(CanvasRenderer));
            icon.layer = root.layer;
            var rect = (RectTransform)icon.transform;
            rect.SetParent(root.transform, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.one * (name == "DeckCardCell" ? 20 : 24);
            var image = icon.AddComponent<Image>();
            image.sprite = sprites[0]; image.preserveAspect = true; image.raycastTarget = false;
            icon.SetActive(false);
            var so = new SerializedObject(cell);
            so.FindProperty("m_LimitIcon").objectReferenceValue = image;
            var array = so.FindProperty("m_LimitSprites"); array.arraySize = 3;
            for (int i = 0; i < 3; i++) array.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];
            so.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(root, Prefabs + name + ".prefab");
            PrefabUtility.UnloadPrefabContents(root);
        }
        PublishedConfigBuilder.Prepare();
        for (int i = 0; i < 3; i++) AddressableCatalogMenu.AddSprite(Sprites + "GUI_T_Icon1_Limit0" + i + ".png");
        AssetDatabase.SaveAssets();
        return "Authored serialized limit icons on pool and placed card prefabs; imported three sprites and generated configuration.";
    }
}
