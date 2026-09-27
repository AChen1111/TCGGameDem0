using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;

/// <summary>从 Tools 菜单启动工程根目录的同步编辑器配置脚本.</summary>
public static class EditorConfigSyncMenu
{
    [MenuItem(EditorMenus.SyncEditorConfig)]
    static void Sync()
    {
        try
        {
            string launcher = EditorPaths.FromProjectRoot("同步编辑器配置.cmd");
            if (!File.Exists(launcher)) throw new FileNotFoundException("找不到同步编辑器配置入口", launcher);
            var info = new ProcessStartInfo
            {
                FileName = launcher,
                WorkingDirectory = EditorPaths.ProjectRoot,
                UseShellExecute = true
            };
            using (Process process = Process.Start(info))
            {
                if (process == null) throw new InvalidOperationException("系统未创建同步进程");
                ALog.Log($"已启动同步编辑器配置. PID={process.Id}; Result=Success", ALogCategories.Default);
            }
        }
        catch (Exception exception)
        {
            ALog.LogError("同步编辑器配置启动失败. Error=" + exception.Message, ALogCategories.Default);
            EditorUtility.DisplayDialog("同步编辑器配置失败", exception.Message, "关闭");
        }
    }
}
