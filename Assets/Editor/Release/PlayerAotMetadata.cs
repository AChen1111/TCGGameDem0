using System;
using System.IO;
using System.Reflection;
using HybridCLR.Editor;
using HybridCLR.Editor.Settings;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>将当前平台的裁剪后 AOT 元数据同步到主包，避免沿用旧的手工复制文件。</summary>
public sealed class PlayerAotMetadata : IPreprocessBuildWithReport
{
    // HybridCLR 在 order=0 的预处理里清空裁剪输出，必须先取出上次 Generate/All 的产物。
    public int callbackOrder => -10000;

    public void OnPreprocessBuild(BuildReport report)
    {
        if (!SettingsUtil.Enable || IsMetadataGeneration(report.summary.outputPath)) return;
        BuildTarget target = report.summary.platform;
        if (target != BuildTarget.Android && target != BuildTarget.StandaloneWindows64) return;
        Synchronize(target);
    }

    static bool IsMetadataGeneration(string outputPath)
    {
        // 官方 Generate/AOTDlls 自身也调用 BuildPlayer；第一次生成时还没有源文件。
        string temporaryRoot = Path.GetFullPath(Path.Combine(SettingsUtil.HybridCLRDataDir,
            "StrippedAOTDllsTempProj")) + Path.DirectorySeparatorChar;
        return Path.GetFullPath(outputPath).StartsWith(temporaryRoot, StringComparison.OrdinalIgnoreCase);
    }

    [MenuItem("HybridCLR/Copy AOT Metadata to StreamingAssets", priority = 250)]
    public static void SynchronizeCurrentTarget()
    {
        Synchronize(EditorUserBuildSettings.activeBuildTarget);
        AssetDatabase.Refresh();
    }

    public static void Synchronize(BuildTarget target)
    {
        string source = SettingsUtil.GetAssembliesPostIl2CppStripDir(target);
        string destination = Path.Combine(Application.streamingAssetsPath, LoadDll.DllDir);
        string[] names = LoadDll.AotDllNames;
        // 先检查整组文件，缺失时阻止构建，不能悄悄混用另一个平台或旧版本。
        foreach (string name in names)
        {
            string file = Path.Combine(source, name);
            if (!File.Exists(file) || new FileInfo(file).Length == 0)
                throw new BuildFailedException($"AOT 元数据缺失: {file}。请保持主包的 Development 设置，先执行 HybridCLR/Generate/All，再构建主包。");
            if (AssemblyName.GetAssemblyName(file).Name != Path.GetFileNameWithoutExtension(name))
                throw new BuildFailedException($"AOT 程序集名称不匹配: {file}");
        }

        Directory.CreateDirectory(destination);
        // 热更 DLL 由 Manifest 下载；清理旧工具留下的主包副本。
        string legacyHotUpdate = Path.Combine(destination, LoadDll.HotUpdateFile + ".bytes");
        if (File.Exists(legacyHotUpdate)) File.Delete(legacyHotUpdate);
        if (File.Exists(legacyHotUpdate + ".meta")) File.Delete(legacyHotUpdate + ".meta");
        foreach (string name in names)
            File.Copy(Path.Combine(source, name), Path.Combine(destination, name + ".bytes"), true);

        HybridCLRSettings.Instance.patchAOTAssemblies = names;
        HybridCLRSettings.Save();

        Debug.Log($"[HybridCLR] 已同步主包 AOT 元数据. Target={target}; Count={names.Length}; Source={source}");
    }
}
