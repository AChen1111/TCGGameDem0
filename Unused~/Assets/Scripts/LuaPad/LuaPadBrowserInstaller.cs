using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;

public static class LuaPadBrowserInstaller
{
    public static string HelperPath => Path.Combine(LuaPadEmmyLuaInstaller.CacheDir, "LuaPadBrowser.exe");

    public static string EnsureHelper()
    {
#if UNITY_EDITOR
        string src = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Tools", "LuaPadBrowser", "Program.cs"));
        if (File.Exists(HelperPath) && File.Exists(src) && File.GetLastWriteTimeUtc(src) <= File.GetLastWriteTimeUtc(HelperPath))
        {
            return HelperPath;
        }
#else
        if (File.Exists(HelperPath))
        {
            return HelperPath;
        }
#endif
#if UNITY_EDITOR
        string project = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Tools", "LuaPadBrowser", "LuaPadBrowser.csproj"));
        Directory.CreateDirectory(LuaPadEmmyLuaInstaller.CacheDir);
        string outDir = Path.Combine(LuaPadEmmyLuaInstaller.CacheDir, "browser-build");
        var psi = new ProcessStartInfo
        {
            FileName = File.Exists(@"C:\Program Files\dotnet\dotnet.exe")
                ? @"C:\Program Files\dotnet\dotnet.exe"
                : "dotnet",
            Arguments = "publish \"" + project + "\" -c Release -r win-x64 --self-contained true -o \"" + outDir + "\" /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
        };
        Process p = Process.Start(psi);
        string stdout = p.StandardOutput.ReadToEnd();
        string stderr = p.StandardError.ReadToEnd();
        p.WaitForExit();
        if (p.ExitCode != 0)
        {
            UnityEngine.Debug.LogError("[LuaPad] dotnet publish 失败: " + stderr + stdout);
            return null;
        }
        string built = Path.Combine(outDir, "LuaPadBrowser.exe");
        File.Copy(built, HelperPath, true);
        return HelperPath;
#else
        string streamed = Path.Combine(Application.streamingAssetsPath, "LuaPadBrowser.exe");
        if (File.Exists(streamed))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(HelperPath));
            File.Copy(streamed, HelperPath, true);
            return HelperPath;
        }
        UnityEngine.Debug.LogError("[LuaPad] 缺少 LuaPadBrowser.exe，请在 Editor 里先打开一次 Pad 或执行 Tools/Lua/Publish LuaPadBrowser");
        return null;
#endif
    }
}
