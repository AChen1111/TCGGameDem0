using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

/// <summary>Python 运营终端入口及内容构建桥接, 复用现有配置和 Release 构建器.</summary>
public static class PythonOperationsBridge
{
    static bool s_Building;

    [MenuItem(EditorMenus.Window + "Python 终端")]
    [MenuItem(EditorMenus.Ops + "Python 终端")]
    static void OpenTerminal()
    {
        try
        {
            string launcher = EditorPaths.FromProjectRoot("运营工具.cmd");
            if (!File.Exists(launcher)) throw new FileNotFoundException("找不到 Python 运营终端入口", launcher);
            // 使用编码命令保留中文路径, 并将路径作为 PowerShell 字面量传入.
            string command = "Set-Location -LiteralPath '" + EditorPaths.ProjectRoot.Replace("'", "''")
                + "'; & '" + launcher.Replace("'", "''") + "'";
            var info = new ProcessStartInfo
            {
                FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System),
                    "WindowsPowerShell", "v1.0", "powershell.exe"),
                Arguments = "-NoLogo -NoProfile -NoExit -EncodedCommand "
                    + Convert.ToBase64String(Encoding.Unicode.GetBytes(command)),
                WorkingDirectory = EditorPaths.ProjectRoot,
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Normal,
                CreateNoWindow = false
            };
            using (Process process = Process.Start(info))
            {
                if (process == null) throw new InvalidOperationException("系统未创建运营终端进程");
                ALog.Log($"打开 PowerShell Python 运营终端. PID={process.Id}; Result=Success", ALogCategories.Default);
            }
        }
        catch (Exception exception)
        {
            ALog.LogError("打开 Python 运营终端失败. Error=" + exception.Message, ALogCategories.Default);
            EditorUtility.DisplayDialog("打开运营终端失败", exception.Message, "关闭");
        }
    }

    public static string BeginBuild(string jobId, string contentVersion)
    {
        throw new InvalidOperationException("旧内容发布流程已退出, 请使用 Window/TCG/开发工作台");
    }
}
