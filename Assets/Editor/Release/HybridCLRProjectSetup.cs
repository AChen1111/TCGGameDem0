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
    const string StreamingDir = "Assets/StreamingAssets/" + LoadDll.DllDir;

    public static bool WorkbenchOwnsBuild;

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

    public static void CopyDlls()
    {
        CompileDllCommand.CompileDll(EditorUserBuildSettings.activeBuildTarget);
        CopyCompiledDlls();
    }

    public static void CopyCompiledDlls(bool requireAot = false)
    {
        BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
        Directory.CreateDirectory(StreamingDir);
        string srcDir = Path.Combine(SettingsUtil.ProjectDir, SettingsUtil.GetHotUpdateDllsOutputDirByTarget(target));
        File.Copy(Path.Combine(srcDir, "HotUpdate.dll"), Path.Combine(StreamingDir, LoadDll.HotUpdateFile), true);

        string aotDir = Path.Combine(SettingsUtil.ProjectDir, SettingsUtil.GetAssembliesPostIl2CppStripDir(target));
        foreach (string dll in LoadDll.AotDllNames)
        {
            string src = Path.Combine(aotDir, dll);
            if (requireAot && !File.Exists(src)) throw new FileNotFoundException("缺少当前平台 AOT 元数据, 请重新生成完整包: " + target, src);
            if (File.Exists(src))
            {
                File.Copy(src, Path.Combine(StreamingDir, dll + ".bytes"), true);
            }
        }
        AssetDatabase.Refresh();
        Debug.Log($"[HybridCLR] copied dlls to {StreamingDir}");
    }


}

class HybridCLRCopyDllsOnBuild : IPreprocessBuildWithReport
{
    public int callbackOrder => -100;
    public void OnPreprocessBuild(BuildReport report)
    {
        if ((report.summary.platform == BuildTarget.Android || report.summary.platform == BuildTarget.StandaloneWindows64) &&
            !HybridCLRProjectSetup.WorkbenchOwnsBuild)
            throw new BuildFailedException("请通过 Window/TCG/开发工作台 → 生成完整包 (当前平台), 保证游戏包与热更内容一致.");
    }
}
