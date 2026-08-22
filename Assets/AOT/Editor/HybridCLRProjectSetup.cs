using System.IO;
using HybridCLR.Editor;
using HybridCLR.Editor.Commands;
using HybridCLR.Editor.Installer;
using HybridCLR.Editor.Settings;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class HybridCLRProjectSetup
{
    public const string HotUpdateAsmdefPath = "Assets/Scripts/HotUpdate.asmdef";
    public const string BootstrapScenePath = "Assets/Scenes/Bootstrap.unity";
    const string StreamingDir = "Assets/StreamingAssets/" + LoadDll.DllDir;

    [MenuItem("Tools/HybridCLR/Configure")]
    public static void Configure()
    {
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone, ScriptingImplementation.IL2CPP);
        PlayerSettings.SetApiCompatibilityLevel(NamedBuildTarget.Standalone, ApiCompatibilityLevel.NET_Unity_4_8);

        AssemblyDefinitionAsset hotUpdate = AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>(HotUpdateAsmdefPath);
        HybridCLRSettings settings = HybridCLRSettings.Instance;
        settings.enable = true;
        settings.hotUpdateAssemblyDefinitions = new[] { hotUpdate };
        settings.hotUpdateAssemblies = new string[0];
        settings.patchAOTAssemblies = LoadDll.AotDllNames;
        HybridCLRSettings.Save();

        CreateBootstrapScene();
        AddressableCatalogSetup.EnsureSceneAddressables();
        InsertBootstrapInBuildSettings();
        AssetDatabase.SaveAssets();
        Debug.Log("[HybridCLR] Configure done");
    }

    [MenuItem("Tools/HybridCLR/Install Runtime")]
    public static void InstallRuntime()
    {
        var installer = new InstallerController();
        bool same = installer.HasInstalledHybridCLR()
            && !string.IsNullOrEmpty(installer.PackageVersion)
            && installer.InstalledLibil2cppVersion == installer.PackageVersion;
        if (same)
        {
            Debug.Log("[HybridCLR] already installed");
            EnsureZlibHeaders();
            return;
        }
        installer.InstallDefaultHybridCLR();
        EnsureZlibHeaders();
    }

    public static void EnsureZlibHeaders()
    {
        string helper = Path.Combine(SettingsUtil.LocalIl2CppDir, "libil2cpp/mono/MonoPosixHelper.cpp");
        string zlibUnity = Path.Combine(SettingsUtil.LocalIl2CppDir, "external/zlib-unity/zlib.h");
        if (!File.Exists(helper) || !File.Exists(zlibUnity))
        {
            return;
        }

        string text = File.ReadAllText(helper);
        const string oldInc = "#include \"../external/zlib/zlib.h\"";
        const string newInc = "#include \"../external/zlib-unity/zlib.h\"";
        if (text.Contains(oldInc))
        {
            File.WriteAllText(helper, text.Replace(oldInc, newInc));
        }

        string stale = Path.Combine(SettingsUtil.LocalIl2CppDir, "external/zlib");
        if (Directory.Exists(stale))
        {
            Directory.Delete(stale, true);
        }
    }

    [MenuItem("Tools/HybridCLR/Copy Dlls To StreamingAssets")]
    public static void CopyDlls()
    {
        CompileDllCommand.CompileDll(EditorUserBuildSettings.activeBuildTarget);
        CopyCompiledDlls();
    }

    public static void CopyCompiledDlls()
    {
        BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
        Directory.CreateDirectory(StreamingDir);
        string srcDir = Path.Combine(SettingsUtil.ProjectDir, SettingsUtil.GetHotUpdateDllsOutputDirByTarget(target));
        File.Copy(Path.Combine(srcDir, "HotUpdate.dll"), Path.Combine(StreamingDir, LoadDll.HotUpdateFile), true);

        string aotDir = Path.Combine(SettingsUtil.ProjectDir, SettingsUtil.GetAssembliesPostIl2CppStripDir(target));
        foreach (string dll in LoadDll.AotDllNames)
        {
            string src = Path.Combine(aotDir, dll);
            if (File.Exists(src))
            {
                File.Copy(src, Path.Combine(StreamingDir, dll + ".bytes"), true);
            }
        }
        AssetDatabase.Refresh();
        Debug.Log($"[HybridCLR] copied dlls to {StreamingDir}");
    }

    static void CreateBootstrapScene()
    {
        if (File.Exists(BootstrapScenePath))
        {
            return;
        }

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        var go = new GameObject("LoadDll");
        SceneManager.MoveGameObjectToScene(go, scene);
        go.AddComponent<LoadDll>();
        EditorSceneManager.SaveScene(scene, BootstrapScenePath);
        EditorSceneManager.CloseScene(scene, true);
    }

    static void InsertBootstrapInBuildSettings()
    {
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(BootstrapScenePath, true) };
    }
}

class HybridCLRCopyDllsOnBuild : IPreprocessBuildWithReport
{
    public int callbackOrder => -100;
    static bool s_Generating;

    public void OnPreprocessBuild(BuildReport report)
    {
        HybridCLRProjectSetup.EnsureZlibHeaders();
        if (s_Generating || EditorUserBuildSettings.buildScriptsOnly)
        {
            return;
        }

        string unityVersionH = $"{SettingsUtil.LocalIl2CppDir}/libil2cpp/hybridclr/generated/UnityVersion.h";
        if (!File.Exists(unityVersionH) || !File.ReadAllText(unityVersionH).Contains("HYBRIDCLR_UNITY_VERSION"))
        {
            s_Generating = true;
            try
            {
                PrebuildCommand.GenerateAll();
            }
            finally
            {
                s_Generating = false;
            }

            HybridCLRProjectSetup.CopyCompiledDlls();
            return;
        }

        HybridCLRProjectSetup.CopyDlls();
    }
}
