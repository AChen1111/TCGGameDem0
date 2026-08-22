using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;

public static class LocalCdnMenu
{
    public const string CdnRoot = "ServerData";
    public const string CodeDir = CdnRoot + "/HybridCLR";

    [MenuItem("Tools/HotUpdate/Build Addressables")]
    public static void BuildAddressables()
    {
        AddressableAssetSettings.BuildPlayerContent();
    }

    [MenuItem("Tools/HotUpdate/Publish Code To Local CDN")]
    public static void PublishCode()
    {
        HybridCLRProjectSetup.CopyDlls();
        Directory.CreateDirectory(CodeDir);
        string src = Path.Combine("Assets/StreamingAssets", LoadDll.DllDir, LoadDll.HotUpdateFile);
        string dest = Path.Combine(CodeDir, LoadDll.HotUpdateFile);
        File.Copy(src, dest, true);
        File.WriteAllText(dest + ".hash", CodeUpdate.HashOf(File.ReadAllBytes(src)));
        UnityEngine.Debug.Log("[CDN] published " + dest);
    }

    [MenuItem("Tools/HotUpdate/Start Local CDN")]
    public static void StartLocalCdn()
    {
        Directory.CreateDirectory(CdnRoot);
        var info = new ProcessStartInfo
        {
            FileName = "py",
            Arguments = "-3 \"" + Path.GetFullPath("tools/serve_cdn.py") + "\"",
            UseShellExecute = true,
        };
        Process.Start(info);
    }
}
