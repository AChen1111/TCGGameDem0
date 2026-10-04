// [UNITY-SKILL:SPRITEATLAS]
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.U2D;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.U2D;

/// <summary>主包与 mBulid 内容共用 UI、头像、列表缩略图及中英文卡图规则。</summary>
public sealed class UiAtlasPrebuild : IPreprocessBuildWithReport
{
    public const string Folder = "Assets/UI/Atlases";
    public const string CatalogPath = "Assets/AddressableCatalogs/AtlasCatalog.asset";
    public int callbackOrder => -100;
    public void OnPreprocessBuild(BuildReport report) => Prepare();

    public static void Prepare()
    {
        EditorSettings.spritePackerMode = SpritePackerMode.SpriteAtlasV2;
        Directory.CreateDirectory(Folder); AssetDatabase.Refresh();
        var paths = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/UI" })
            .Select(AssetDatabase.GUIDToAssetPath).Where(p => !p.StartsWith(Folder + "/", StringComparison.Ordinal)).ToArray();
        var cards = paths.Where(p => p.Contains("/CardBag")).ToArray();
        foreach (string path in cards) ConfigureCard(path);
        var sprites = paths.Where(p => !p.Contains("/CardBag")).SelectMany(AssetDatabase.LoadAllAssetsAtPath)
            .OfType<Sprite>().ToArray();
        var avatar = sprites.Where(s => AssetDatabase.GetAssetPath(s).Contains("/ProfileCustomization/")
            || s.name.StartsWith("a_", StringComparison.Ordinal) || s.name.StartsWith("af_", StringComparison.Ordinal)).ToArray();
        var ui = sprites.Except(avatar).ToArray();
        var entries = new List<AddressableEntry<AssetReferenceT<SpriteAtlas>>>();
        Generate("Avatars", avatar, false, entries);
        Generate("UI", ui, false, entries);
        Generate("PortraitThumbnails", PortraitThumbnailBuilder.Build(), false, entries);
        Generate("Cards_CN", cards.Where(p => !p.Contains("/en/")).SelectMany(AssetDatabase.LoadAllAssetsAtPath).OfType<Sprite>().ToArray(), true, entries);
        Generate("Cards_EN", cards.Where(p => p.Contains("/en/")).SelectMany(AssetDatabase.LoadAllAssetsAtPath).OfType<Sprite>().ToArray(), true, entries);
        var catalog = AssetDatabase.LoadAssetAtPath<AtlasAddressableCatalog>(CatalogPath);
        if (!File.Exists(CatalogPath))
        {
            catalog = ScriptableObject.CreateInstance<AtlasAddressableCatalog>();
            AssetDatabase.CreateAsset(catalog, CatalogPath);
        }
        catalog.EditorSetEntries(entries); EditorUtility.SetDirty(catalog);
        AddressableCatalogSetup.MarkInGroup(AddressableCatalogSetup.RemoteCatalogGroup, CatalogPath, "AtlasCatalog");
        NormalizeSources(cards, avatar, ui);
        AssetDatabase.SaveAssets();
    }

    static void ConfigureCard(string path)
    {
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        string pool = path.Contains("CardBag01_") ? "Card01" : path.Contains("CardBag02_") ? "Card02" : "Card03";
        string name = pool + "_" + Path.GetFileNameWithoutExtension(path);
        var existing = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();
        if (existing.Length == 1 && existing[0].name == name && importer.spriteImportMode == SpriteImportMode.Multiple) return;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.mipmapEnabled = false;
        importer.npotScale = TextureImporterNPOTScale.None;
        importer.SaveAndReimport();
        var factory = new SpriteDataProviderFactories(); factory.Init();
        var provider = factory.GetSpriteEditorDataProviderFromObject(importer);
        provider.InitSpriteEditorDataProvider();
        var capability = provider.GetDataProvider<ISpriteFrameEditCapability>();
        if (capability == null || !capability.GetEditCapability().HasCapability(EEditCapability.CreateAndDeleteSprite)
            || !capability.GetEditCapability().HasCapability(EEditCapability.EditSpriteName))
            throw new InvalidOperationException("卡图导入器不支持创建与命名 Sprite: " + path);
        importer.GetSourceTextureWidthAndHeight(out int width, out int height);
        var id = GUID.Generate();
        provider.SetSpriteRects(new[] { new SpriteRect { name = name, spriteID = id,
            rect = new Rect(0, 0, width, height), alignment = SpriteAlignment.Center, pivot = new Vector2(.5f, .5f) } });
        provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(new[] { new SpriteNameFileIdPair(name, id) });
        provider.Apply(); importer.SaveAndReimport();
    }

    public static void PreparePortraitThumbnails()
    {
        EditorSettings.spritePackerMode=SpritePackerMode.SpriteAtlasV2;
        Directory.CreateDirectory(Folder);
        var catalog=AssetDatabase.LoadAssetAtPath<AtlasAddressableCatalog>(CatalogPath);
        var entries=catalog.Entries.Where(e=>e.assetName!="PortraitThumbnails").ToList();
        Generate("PortraitThumbnails",PortraitThumbnailBuilder.Build(),false,entries);
        catalog.EditorSetEntries(entries);EditorUtility.SetDirty(catalog);AssetDatabase.SaveAssets();
    }

    static void Generate(string name, Sprite[] sprites, bool opaque, List<AddressableEntry<AssetReferenceT<SpriteAtlas>>> entries)
    {
        string path = Folder + "/" + name + ".spriteatlasv2";
        var asset = new SpriteAtlasAsset(); asset.Add(sprites.Cast<UnityEngine.Object>().ToArray());
        SpriteAtlasAsset.Save(asset, path); AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        var importer = (SpriteAtlasImporter)AssetImporter.GetAtPath(path);
        importer.includeInBuild = false;
        var packing = importer.packingSettings;
        packing.enableRotation = false; packing.enableTightPacking = false; packing.padding = 4;
        importer.packingSettings = packing;
        var texture = importer.textureSettings; texture.generateMipMaps = false; texture.filterMode = FilterMode.Bilinear;
        importer.textureSettings = texture;
        importer.SetPlatformSettings(new TextureImporterPlatformSettings { name = "Standalone", overridden = true,
            maxTextureSize = 4096, format = opaque ? TextureImporterFormat.DXT1 : TextureImporterFormat.DXT5 });
        importer.SetPlatformSettings(new TextureImporterPlatformSettings { name = "Android", overridden = true,
            maxTextureSize = 4096, format = TextureImporterFormat.ASTC_4x4 });
        importer.SaveAndReimport();
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        var group = Group(settings, "Remote_Atlas_" + name);
        string guid = AssetDatabase.AssetPathToGUID(path);
        settings.CreateOrMoveEntry(guid, group).address = "Atlas/" + name;
        entries.Add(new AddressableEntry<AssetReferenceT<SpriteAtlas>> { assetName = name,
            reference = new AssetReferenceT<SpriteAtlas>(guid) });
    }

    static AddressableAssetGroup Group(AddressableAssetSettings settings, string name)
    {
        var group = settings.FindGroup(name);
        if (group == null) group = settings.CreateGroup(name, false, false, true, null,
            typeof(BundledAssetGroupSchema), typeof(ContentUpdateGroupSchema));
        var bundle = group.GetSchema<BundledAssetGroupSchema>();
        bundle.BuildPath.SetVariableByName(settings, AddressableAssetSettings.kRemoteBuildPath);
        bundle.LoadPath.SetVariableByName(settings, AddressableAssetSettings.kRemoteLoadPath);
        bundle.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackTogether;
        group.GetSchema<ContentUpdateGroupSchema>().StaticContent = false;
        EditorUtility.SetDirty(bundle); EditorUtility.SetDirty(group);
        return group;
    }

    static void NormalizeSources(string[] cards, Sprite[] avatars, Sprite[] ui)
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        foreach (var group in settings.groups.Where(g => g != null).ToArray())
            foreach (var entry in group.entries.ToArray())
                if (entry.AssetPath.StartsWith("Assets/UI/Sprite/", StringComparison.Ordinal)
                    || entry.AssetPath.StartsWith("Assets/UI/Card/", StringComparison.Ordinal)) settings.RemoveAssetEntry(entry.guid);
        foreach (var pair in new[] { ("Avatars", avatars), ("UI", ui) })
        {
            var group = Group(settings, "Remote_Atlas_" + pair.Item1);
            foreach (string path in pair.Item2.Select(AssetDatabase.GetAssetPath).Distinct())
                settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(path), group).address = path;
        }
        // 卡图只通过图集读取，不将原 jpg 同时列为独立地址导致重复纹理打包。
        foreach (var group in settings.groups.Where(g => g != null
            && (g.Name == "Remote_Card" || g.Name == "Remote_UI_Event") && !g.entries.Any()).ToArray())
            settings.RemoveGroup(group);
        EditorUtility.SetDirty(settings);
    }
}
