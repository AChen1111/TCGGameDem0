using System;
using System.IO;
using System.Linq;
using System.Reflection;
using HybridCLR.Editor.Settings;
using NUnit.Framework;
using UnityEditor;
using UnityEditorInternal;

public class HybridCLRSetupTests
{
    [Test]
    public void PlayerBuild_ContainsOnlyBootstrap()
    {
        EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
        Assert.AreEqual(1, scenes.Length);
        Assert.AreEqual(HybridCLRProjectSetup.BootstrapScenePath, scenes[0].path);
        Assert.IsTrue(scenes[0].enabled);
    }

    [Test]
    public void HotUpdate_Asmdef_DisablesAutoReferenced()
    {
        string json = File.ReadAllText("Assets/Scripts/HotUpdate.asmdef");
        StringAssert.Contains("\"name\": \"HotUpdate\"", json);
        StringAssert.Contains("\"autoReferenced\": false", json);
    }

    [Test]
    public void LoadDll_LivesInAotAssembly()
    {
        Assert.AreEqual("Assembly-CSharp", typeof(LoadDll).Assembly.GetName().Name);
    }

    [Test]
    public void AotDllNames_MatchPatchedAotList()
    {
        CollectionAssert.AreEquivalent(AOTGenericReferences.PatchedAOTAssemblyList, LoadDll.AotDllNames);
        CollectionAssert.Contains(LoadDll.AotDllNames, "UniTask.dll");
        CollectionAssert.AreEquivalent(AOTGenericReferences.PatchedAOTAssemblyList, HybridCLRSettings.Instance.patchAOTAssemblies);
    }

    [Test]
    public void XLuaAndLua_AreArchivedOutsideUnityImport()
    {
        Assert.IsFalse(Directory.Exists("Assets/XLua"));
        Assert.IsFalse(Directory.Exists("Assets/Scripts/LuaRaw"));
        Assert.IsFalse(Directory.Exists("Assets/Scripts/LuaComponet"));
        Assert.IsFalse(Directory.Exists("Assets/Scripts/LuaPad"));
        Assert.IsFalse(Directory.Exists("Assets/StreamingAssets/LuaWorkspace"));
        Assert.IsFalse(Directory.Exists("Assets/StreamingAssets/LuaPad"));
        Assert.IsFalse(File.Exists("Assets/Plugins/x86_64/xlua.dll"));
        Assert.IsFalse(File.Exists("Assets/Resources/LuaBundle.bytes"));
        Assert.IsTrue(File.Exists("Unused~/Assets/XLua/XLua.Runtime.asmdef"));
        Assert.IsTrue(File.Exists("Unused~/Assets/Scripts/LuaRaw/Main.lua"));
        Assert.IsTrue(File.Exists("Unused~/Assets/Resources/LuaBundle.bytes"));
        Assert.IsTrue(File.Exists("Assets/Scripts/UI/ImageAspectLayoutElement.cs"));
    }

    [Test]
    public void HotUpdate_Asmdef_DoesNotReferenceXLua()
    {
        StringAssert.DoesNotContain("XLua", File.ReadAllText("Assets/Scripts/HotUpdate.asmdef"));
        StringAssert.DoesNotContain("XLua", File.ReadAllText("Assets/Scripts/Editor/HotUpdate.Editor.asmdef"));
        StringAssert.DoesNotContain("LuaPad", File.ReadAllText("Assets/Scripts/HotUpdateEntry.cs"));
    }

    [Test]
    public void HotUpdateEntry_LivesInHotUpdateAssembly()
    {
        Assembly hotUpdate = AppDomain.CurrentDomain.GetAssemblies()
            .First(a => a.GetName().Name == "HotUpdate");
        Assert.IsNotNull(hotUpdate.GetType("HotUpdateEntry"));
        Assert.IsNull(hotUpdate.GetType("LoadDll"));
    }

    [Test]
    public void HybridCLRSettings_ContainsHotUpdateDefinition()
    {
        AssemblyDefinitionAsset[] defs = HybridCLRSettings.Instance.hotUpdateAssemblyDefinitions;
        Assert.IsNotNull(defs);
        Assert.IsTrue(defs.Any(d => d != null && d.name == "HotUpdate"));
    }

    [Test]
    public void HybridClrPackage_PinsV8141()
    {
        StringAssert.Contains("hybridclr_unity.git#v8.14.1", File.ReadAllText("Packages/manifest.json"));
    }

    [Test]
    public void InstalledLibil2cpp_MatchesPackage8141()
    {
        var installer = new HybridCLR.Editor.Installer.InstallerController();
        Assert.AreEqual("8.14.1", installer.PackageVersion);
        Assert.AreEqual(installer.PackageVersion, installer.InstalledLibil2cppVersion);
    }

    [Test]
    public void UnityVersionH_DefinesHybridClrUnityVersion()
    {
        string path = Path.Combine(HybridCLR.Editor.SettingsUtil.LocalIl2CppDir,
            "libil2cpp/hybridclr/generated/UnityVersion.h");
        StringAssert.Contains("HYBRIDCLR_UNITY_VERSION", File.ReadAllText(path));
    }

    [Test]
    public void ZlibHeaders_ExistForIl2Cpp()
    {
        HybridCLRProjectSetup.EnsureZlibHeaders();
        string helper = Path.Combine(HybridCLR.Editor.SettingsUtil.LocalIl2CppDir, "libil2cpp/mono/MonoPosixHelper.cpp");
        StringAssert.Contains("zlib-unity/zlib.h", File.ReadAllText(helper));
        Assert.IsFalse(Directory.Exists(Path.Combine(HybridCLR.Editor.SettingsUtil.LocalIl2CppDir, "external/zlib")));
    }
}
