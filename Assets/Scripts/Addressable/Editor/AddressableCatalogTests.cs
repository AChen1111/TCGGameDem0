using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class AddressableCatalogTests
{
    SpriteAddressableCatalog m_catalog;

    [SetUp]
    public void SetUp()
    {
        m_catalog = ScriptableObject.CreateInstance<SpriteAddressableCatalog>();
    }

    [TearDown]
    public void TearDown()
    {
        if (m_catalog != null)
        {
            UnityEngine.Object.DestroyImmediate(m_catalog);
        }
    }

    [Test]
    public void BuildMap_Get_ReturnsReference()
    {
        var reference = new AssetReferenceSprite("");
        m_catalog.EditorSetEntries(new List<AddressableEntry<AssetReferenceSprite>>
        {
            new AddressableEntry<AssetReferenceSprite> { assetName = "hero", reference = reference },
        });
        m_catalog.BuildMap();

        Assert.AreSame(reference, m_catalog.Get("hero"));
    }

    [Test]
    public void BuildMap_DuplicateName_Throws()
    {
        m_catalog.EditorSetEntries(new List<AddressableEntry<AssetReferenceSprite>>
        {
            new AddressableEntry<AssetReferenceSprite> { assetName = "hero", reference = new AssetReferenceSprite("") },
            new AddressableEntry<AssetReferenceSprite> { assetName = "hero", reference = new AssetReferenceSprite("") },
        });

        Assert.Throws<ArgumentException>(() => m_catalog.BuildMap());
    }

    [Test]
    public void Get_MissingName_Throws()
    {
        m_catalog.EditorSetEntries(new List<AddressableEntry<AssetReferenceSprite>>());
        m_catalog.BuildMap();

        Assert.Throws<KeyNotFoundException>(() => m_catalog.Get("missing"));
    }

    [Test]
    public void EditorAdd_ThenGet_ReturnsReference()
    {
        var reference = new AssetReferenceSprite("");
        m_catalog.EditorAdd("hero", reference);
        m_catalog.BuildMap();

        Assert.AreSame(reference, m_catalog.Get("hero"));
    }

    [Test]
    public void EditorAdd_SameName_ReplacesReference()
    {
        var first = new AssetReferenceSprite("a");
        var second = new AssetReferenceSprite("b");
        m_catalog.EditorAdd("hero", first);
        m_catalog.EditorAdd("hero", second);
        m_catalog.BuildMap();

        Assert.AreSame(second, m_catalog.Get("hero"));
    }

    [Test]
    public void ConcreteCatalogs_CreateInstance()
    {
        var prefab = ScriptableObject.CreateInstance<PrefabAddressableCatalog>();
        var scene = ScriptableObject.CreateInstance<SceneAddressableCatalog>();
        var uiSettings = ScriptableObject.CreateInstance<UISettingsAddressableCatalog>();
        try
        {
            prefab.BuildMap();
            scene.BuildMap();
            uiSettings.BuildMap();
            Assert.IsNotNull(prefab);
            Assert.IsNotNull(scene);
            Assert.IsNotNull(uiSettings);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(prefab);
            UnityEngine.Object.DestroyImmediate(scene);
            UnityEngine.Object.DestroyImmediate(uiSettings);
        }
    }

    [Test]
    public void UiGroupForPath_RoutesFolders()
    {
        Assert.AreEqual(AddressableCatalogSetup.RemoteUiEventGroup,
            AddressableCatalogSetup.UiGroupForPath("Assets/UI/Prefab/Event/LoginBonus/Foo.asset"));
        Assert.AreEqual(AddressableCatalogSetup.RemoteCardGroup,
            AddressableCatalogSetup.UiGroupForPath("Assets/Art/Card01/sprite.png"));
        Assert.AreEqual(AddressableCatalogSetup.RemoteSharedGroup,
            AddressableCatalogSetup.UiGroupForPath("Assets/UI/Prefab/BaseUI/UIFrame.prefab"));
        Assert.AreEqual(AddressableCatalogSetup.RemoteUiHallGroup,
            AddressableCatalogSetup.UiGroupForPath("Assets/UI/Prefab/Hall/PreGameUI/PreGameSceneUI.asset"));
    }

    [Test]
    public void Groups_ExistWithExpectedPacking()
    {
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.GetSettings(false);

        AddressableAssetGroup boot = settings.FindGroup(AddressableCatalogSetup.LocalBootGroup);
        AddressableAssetGroup catalog = settings.FindGroup(AddressableCatalogSetup.RemoteCatalogGroup);
        AddressableAssetGroup shared = settings.FindGroup(AddressableCatalogSetup.RemoteSharedGroup);
        AddressableAssetGroup hall = settings.FindGroup(AddressableCatalogSetup.RemoteUiHallGroup);
        AddressableAssetGroup eventUi = settings.FindGroup(AddressableCatalogSetup.RemoteUiEventGroup);
        AddressableAssetGroup card = settings.FindGroup(AddressableCatalogSetup.RemoteCardGroup);

        Assert.IsNotNull(boot);
        Assert.IsNotNull(catalog);
        Assert.IsNotNull(shared);
        Assert.IsNotNull(hall);
        Assert.IsNotNull(eventUi);
        Assert.IsNotNull(card);
        Assert.AreEqual(BundledAssetGroupSchema.BundlePackingMode.PackTogether,
            boot.GetSchema<BundledAssetGroupSchema>().BundleMode);
        Assert.AreEqual(BundledAssetGroupSchema.BundlePackingMode.PackTogether,
            catalog.GetSchema<BundledAssetGroupSchema>().BundleMode);
        Assert.AreEqual(BundledAssetGroupSchema.BundlePackingMode.PackTogether,
            shared.GetSchema<BundledAssetGroupSchema>().BundleMode);
        Assert.AreEqual(BundledAssetGroupSchema.BundlePackingMode.PackSeparately,
            hall.GetSchema<BundledAssetGroupSchema>().BundleMode);
        Assert.AreEqual(AddressableAssetSettings.kLocalBuildPath, boot.GetSchema<BundledAssetGroupSchema>().BuildPath.GetName(settings));
        Assert.AreEqual(AddressableAssetSettings.kRemoteBuildPath, catalog.GetSchema<BundledAssetGroupSchema>().BuildPath.GetName(settings));
        Assert.AreEqual(AddressableAssetSettings.kRemoteLoadPath, hall.GetSchema<BundledAssetGroupSchema>().LoadPath.GetName(settings));
    }

    [Test]
    public void CatalogFolder_IsRemoteFolderEntry()
    {
        Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<UISettingsAddressableCatalog>(AddressableCatalogSetup.UISettingsPath));

        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.GetSettings(false);
        AddressableAssetGroup catalog = settings.FindGroup(AddressableCatalogSetup.RemoteCatalogGroup);
        AddressableAssetEntry folder = catalog.GetAssetEntry(AssetDatabase.AssetPathToGUID(AddressableCatalogSetup.CatalogsFolder));
        Assert.IsNotNull(folder);
        Assert.IsTrue(folder.IsFolder);
        Assert.IsFalse(AddressableCatalogSetup.IsDirectEntry(catalog, AssetDatabase.AssetPathToGUID(AddressableCatalogSetup.PrefabPath)));
        Assert.IsNull(settings.FindGroup("Catalogs"));
    }

    [Test]
    public void RemoteCatalog_IsEnabled()
    {
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.GetSettings(false);
        Assert.IsTrue(settings.BuildRemoteCatalog);
        Assert.AreEqual(AddressableAssetSettings.kRemoteBuildPath, settings.RemoteCatalogBuildPath.GetName(settings));
        Assert.AreEqual(AddressableAssetSettings.kRemoteLoadPath, settings.RemoteCatalogLoadPath.GetName(settings));
        string load = settings.profileSettings.GetValueByName(settings.activeProfileId, AddressableAssetSettings.kRemoteLoadPath);
        Assert.AreNotEqual(AddressableAssetProfileSettings.undefinedEntryValue, load);
        Assert.IsFalse(string.IsNullOrEmpty(load));
    }

    [Test]
    public void SharedAndHallFolders_AreMarked()
    {
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.GetSettings(false);
        AddressableAssetGroup shared = settings.FindGroup(AddressableCatalogSetup.RemoteSharedGroup);
        AddressableAssetEntry baseUi = shared.GetAssetEntry(AssetDatabase.AssetPathToGUID(AddressableCatalogSetup.BaseUiFolder));
        AddressableAssetEntry fonts = shared.GetAssetEntry(AssetDatabase.AssetPathToGUID(AddressableCatalogSetup.FontsFolder));
        Assert.IsNotNull(baseUi);
        Assert.IsTrue(baseUi.IsFolder);
        Assert.IsNotNull(fonts);
        Assert.IsTrue(fonts.IsFolder);

        AddressableAssetGroup hall = settings.FindGroup(AddressableCatalogSetup.RemoteUiHallGroup);
        AddressableAssetEntry preGame = hall.GetAssetEntry(AssetDatabase.AssetPathToGUID(AddressableCatalogSetup.PreGameUiFolder));
        Assert.IsNotNull(preGame);
        Assert.IsTrue(preGame.IsFolder);
    }

    [Test]
    public void AddToUISettingsCatalog_WritesKeyAndMarksFolder()
    {
        string path = AddressableCatalogSetup.PreGameUiSettingsPath;
        AddressableCatalogMenu.AddToUISettingsCatalog(path);

        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.GetSettings(false);
        AddressableAssetGroup hall = settings.FindGroup(AddressableCatalogSetup.RemoteUiHallGroup);
        AddressableAssetEntry folder = hall.GetAssetEntry(AssetDatabase.AssetPathToGUID(AddressableCatalogSetup.PreGameUiFolder));
        Assert.IsNotNull(folder);
        Assert.AreEqual("PreGameUI", folder.address);
        Assert.IsFalse(AddressableCatalogSetup.IsDirectEntry(hall, AssetDatabase.AssetPathToGUID(path)));

        var catalog = AssetDatabase.LoadAssetAtPath<UISettingsAddressableCatalog>(AddressableCatalogSetup.UISettingsPath);
        catalog.BuildMap();
        Assert.AreEqual(AssetDatabase.AssetPathToGUID(path), catalog.Get("PreGameSceneUI").AssetGUID);
        Assert.AreEqual("PreGameSceneUI", AddressKeys.UISettings.PreGameSceneUI);
        StringAssert.Contains("PreGameSceneUI", File.ReadAllText(AddressableCatalogSetup.AddressKeysPath));
    }

    [Test]
    public void AddPrefab_WritesKeyAndPointsToHallPrefab()
    {
        AddressableCatalogMenu.AddPrefab(AddressableCatalogSetup.PreGameUiPanelPath);
        var catalog = AssetDatabase.LoadAssetAtPath<PrefabAddressableCatalog>(AddressableCatalogSetup.PrefabPath);
        catalog.BuildMap();
        Assert.AreEqual(
            AssetDatabase.AssetPathToGUID(AddressableCatalogSetup.PreGameUiPanelPath),
            catalog.Get("PreGameUIPanel").AssetGUID);
        Assert.AreEqual("PreGameUIPanel", AddressKeys.Prefab.PreGameUIPanel);
        StringAssert.Contains("PreGameUIPanel", File.ReadAllText(AddressableCatalogSetup.AddressKeysPath));
    }
}
