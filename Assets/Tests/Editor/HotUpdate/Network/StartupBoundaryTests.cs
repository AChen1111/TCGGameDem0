using System;
using System.Linq;
using AChen.Configuration;
using NUnit.Framework;

public sealed class StartupBoundaryTests
{
    [Test]
    public void Main_package_resources_do_not_depend_on_hot_update_scripts()
    {
        foreach (string guid in UnityEditor.AssetDatabase.FindAssets("", new[] { "Assets/Resources" }))
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            foreach (string dependency in UnityEditor.AssetDatabase.GetDependencies(path))
            {
                var script = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEditor.MonoScript>(dependency);
                var type = script == null ? null : script.GetClass();
                if (type != null) Assert.AreNotEqual("HotUpdate", type.Assembly.GetName().Name, path + " -> " + dependency);
            }
        }
        var settings = UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject.Settings;
        string overlayGuid = UnityEditor.AssetDatabase.AssetPathToGUID("Assets/UI/Prefab/Common/LoadIN.prefab");
        Assert.IsNotEmpty(overlayGuid);
        Assert.AreEqual(SceneTransitionOverlay.Address, settings.FindAssetEntry(overlayGuid).address);
    }

    [Test]
    public void Business_and_vendor_types_are_loaded_from_hot_update()
    {
        foreach (var type in new[] { typeof(CardCatalog), typeof(CardAttribute), typeof(LocalizationService),
            typeof(LocalizedText), typeof(LocalizationSettings), typeof(BinaryTable), typeof(GameConfigTables),
            typeof(PublishedGameConfig), typeof(ConfigArtifacts), typeof(ContentSession), typeof(ALog),
            typeof(ALogSettings), typeof(Table.CardRow), typeof(Table.TranslationRow), typeof(ScreenFitter),
            typeof(Spine.Unity.SkeletonGraphic), typeof(SuperScrollView.LoopListView2), typeof(SafeAreaAdapter) })
            Assert.AreEqual("HotUpdate", type.Assembly.GetName().Name, type.FullName);
    }

    [Test]
    public void Bootstrap_and_contract_assemblies_do_not_reference_business()
    {
        foreach (var assembly in new[] { typeof(LoadDll).Assembly, typeof(StartupContext).Assembly })
        {
            var references = assembly.GetReferencedAssemblies().Select(x => x.Name).ToArray();
            foreach (string name in new[] { "HotUpdate", "spine-unity", "spine-csharp", "SuperScrollView", "UIAdapter", "UniTask", "LitMotion" })
                CollectionAssert.DoesNotContain(references, name, assembly.GetName().Name);
        }
        Assert.AreEqual("TCG.Bootstrap", typeof(LoadDll).Assembly.GetName().Name);
    }

    [Test]
    public void Startup_message_can_be_formatted_without_translation_configuration()
    {
        var message = new LocalizedMessage("err.test", new System.Collections.Generic.Dictionary<string, object> { ["file"] = "HotUpdate.dll" });
        StringAssert.Contains("err.test", message.ToString());
        StringAssert.Contains("HotUpdate.dll", message.ToString());
    }

    [Test]
    public void Hot_update_entry_uses_the_minimal_context_contract()
    {
        Assert.NotNull(typeof(HotUpdateEntry).GetMethod("Boot", new[] { typeof(Action<float>), typeof(StartupContext), typeof(Action<LocalizedMessage>) }));
    }
}
