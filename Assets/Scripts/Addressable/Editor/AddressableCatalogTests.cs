using System;
using System.Collections.Generic;
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
    public void EnsureGroups_CreatesLocalPacking()
    {
        AddressableCatalogSetup.EnsureGroups();
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.GetSettings(false);

        AddressableAssetGroup catalogs = settings.FindGroup(AddressableCatalogSetup.CatalogsGroup);
        AddressableAssetGroup boot = settings.FindGroup(AddressableCatalogSetup.LocalBootGroup);
        AddressableAssetGroup ui = settings.FindGroup(AddressableCatalogSetup.RemoteUiGroup);

        Assert.IsNotNull(catalogs);
        Assert.IsNotNull(boot);
        Assert.IsNotNull(ui);
        Assert.AreEqual(BundledAssetGroupSchema.BundlePackingMode.PackTogether,
            catalogs.GetSchema<BundledAssetGroupSchema>().BundleMode);
        Assert.AreEqual(BundledAssetGroupSchema.BundlePackingMode.PackTogether,
            boot.GetSchema<BundledAssetGroupSchema>().BundleMode);
        Assert.AreEqual(BundledAssetGroupSchema.BundlePackingMode.PackTogetherByLabel,
            ui.GetSchema<BundledAssetGroupSchema>().BundleMode);
    }

    [Test]
    public void EnsureCatalogs_CreatesUISettingsCatalog()
    {
        AddressableCatalogSetup.EnsureCatalogs();
        Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<UISettingsAddressableCatalog>(AddressableCatalogSetup.UISettingsPath));

        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.GetSettings(false);
        string guid = AssetDatabase.AssetPathToGUID(AddressableCatalogSetup.UISettingsPath);
        Assert.IsNotNull(settings.FindGroup(AddressableCatalogSetup.CatalogsGroup).GetAssetEntry(guid));
    }

    [Test]
    public void AddToUISettingsCatalog_MarksRemoteUi()
    {
        const string path = "Assets/UI/Prefab/PreGameUI/PreGameSceneUI.asset";
        AddressableCatalogMenu.AddToUISettingsCatalog(path);

        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.GetSettings(false);
        string guid = AssetDatabase.AssetPathToGUID(path);
        AddressableAssetEntry entry = settings.FindGroup(AddressableCatalogSetup.RemoteUiGroup).GetAssetEntry(guid);
        Assert.IsNotNull(entry);
        Assert.AreEqual("PreGameSceneUI", entry.address);
        Assert.IsTrue(entry.labels.Contains("PreGameUI"));

        var catalog = AssetDatabase.LoadAssetAtPath<UISettingsAddressableCatalog>(AddressableCatalogSetup.UISettingsPath);
        catalog.BuildMap();
        Assert.AreEqual(guid, catalog.Get("PreGameSceneUI").AssetGUID);
    }
}
