using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>原 UnityExcel2BytesCs 的 CSV 无编译导表入口.</summary>
public static class Excel2CsBytesTool
{
    [MenuItem("SDGSupporter/Excel/Export Localization CSV")]
    public static void ExportLocalization()
    {
        string root = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        string script = Path.Combine(root, "Tools/UnityExcel2BytesCs/Excel2CsBytesTool.ps1");
        var start = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = "-NoProfile -NonInteractive -ExecutionPolicy Bypass -File \"" + script + "\"",
            WorkingDirectory = root,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };
        try
        {
            using (Process process = Process.Start(start))
            {
                var output = process.StandardOutput.ReadToEndAsync();
                var error = process.StandardError.ReadToEndAsync();
                process.WaitForExit();
                if (process.ExitCode != 0)
                {
                    ALog.LogError("语言表导出失败. Source=TableData/Localization/Translations.csv; Error=" + error.GetAwaiter().GetResult(), ALogCategories.Localization);
                    return;
                }
                ALog.Log(output.GetAwaiter().GetResult().Trim(), ALogCategories.Localization);
            }
        }
        catch (Exception exception)
        {
            ALog.LogError("语言表导出失败. Tool=" + script + "; Error=" + exception.Message, ALogCategories.Localization);
        }
    }
}
