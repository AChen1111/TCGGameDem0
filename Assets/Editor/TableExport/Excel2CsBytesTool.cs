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
        ExportSchema("Tools/UnityExcel2BytesCs/Localization.schema.json", ALogCategories.Localization);
    }

    [MenuItem("SDGSupporter/Excel/Export Card CSV")]
    public static void ExportCard()
    {
        ExportSchema("Tools/UnityExcel2BytesCs/Card.schema.json", ALogCategories.Default);
    }

    static void ExportSchema(string schemaRelativePath, string category)
    {
        string root = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        string script = Path.Combine(root, "Tools/UnityExcel2BytesCs/Excel2CsBytesTool.ps1");
        string schema = Path.Combine(root, schemaRelativePath.Replace('/', Path.DirectorySeparatorChar));
        var start = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = "-NoProfile -NonInteractive -ExecutionPolicy Bypass -File \"" + script + "\" -SchemaPath \"" + schema + "\"",
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
                    ALog.LogError("导表失败. Schema=" + schemaRelativePath + "; Error=" + error.GetAwaiter().GetResult(), category);
                    return;
                }

                ALog.Log(output.GetAwaiter().GetResult().Trim(), category);
            }
        }
        catch (Exception exception)
        {
            ALog.LogError("导表失败. Schema=" + schemaRelativePath + "; Error=" + exception.Message, category);
        }
    }
}
