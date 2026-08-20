using System.Collections.Generic;
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
    public const string InitScenePath = "Assets/Scenes/Init.unity";
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
        MarkInitAddressable();
        InsertBootstrapInBuildSettings();
        AssetDatabase.SaveAssets();
        Debug.Log("[HybridCLR] Configure done");
    }

    [MenuItem("Tools/HybridCLR/Install Runtime")]
    public static void InstallRuntime()
    {
        var installer = new InstallerController();
        if (installer.HasInstalledHybridCLR())
        {
            Debug.Log("[HybridCLR] already installed");
            return;
        }
        installer.InstallDefaultHybridCLR();
    }

    [MenuItem("Tools/HybridCLR/Copy Dlls To StreamingAssets")]
    public static void CopyDlls()
    {
        BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
        CompileDllCommand.CompileDll(target);

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

    static void MarkInitAddressable()
    {
        AddressableCatalogSetup.MarkInGroup(AddressableCatalogSetup.LocalBootGroup, InitScenePath, "Init");
    }

    static void InsertBootstrapInBuildSettings()
    {
        var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        scenes.RemoveAll(s => s.path == BootstrapScenePath);
        scenes.Insert(0, new EditorBuildSettingsScene(BootstrapScenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}

class HybridCLRCopyDllsOnBuild : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        HybridCLRProjectSetup.CopyDlls();
    }
}
