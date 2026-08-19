using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using UnityEngine;

public static class LuaPadEmmyLuaInstaller
{
    public const string Version = "0.25.1";
    const string ZipName = "emmylua_ls-win32-x64.zip";
    const string Url =
        "https://github.com/EmmyLuaLs/emmylua-analyzer-rust/releases/download/0.25.1/emmylua_ls-win32-x64.zip";

    public static string CacheDir
    {
        get
        {
#if UNITY_EDITOR
            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Library", "LuaPad"));
#else
            return Path.GetFullPath(Path.Combine(Application.persistentDataPath, "LuaPad"));
#endif
        }
    }

    public static string BinaryPath => Path.Combine(CacheDir, "emmylua_ls.exe");

    public static string EnsureBinary()
    {
        if (File.Exists(BinaryPath))
        {
            return BinaryPath;
        }
        Directory.CreateDirectory(CacheDir);
        string zipPath = Path.Combine(CacheDir, ZipName);
        using (var client = new HttpClient())
        {
            client.Timeout = TimeSpan.FromMinutes(3);
            byte[] data = client.GetByteArrayAsync(Url).GetAwaiter().GetResult();
            File.WriteAllBytes(zipPath, data);
        }
        string extractDir = Path.Combine(CacheDir, "extract");
        if (Directory.Exists(extractDir))
        {
            Directory.Delete(extractDir, true);
        }
        ZipFile.ExtractToDirectory(zipPath, extractDir);
        if (!File.Exists(BinaryPath))
        {
            foreach (string file in Directory.GetFiles(extractDir, "emmylua_ls.exe", SearchOption.AllDirectories))
            {
                File.Copy(file, BinaryPath, true);
                break;
            }
        }
        if (!File.Exists(BinaryPath))
        {
            throw new FileNotFoundException("emmylua_ls.exe not found after download", BinaryPath);
        }
        return BinaryPath;
    }
}
