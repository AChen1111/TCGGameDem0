using System.IO;
using HybridCLR.Editor;
using HybridCLR.Editor.Installer;
using UnityEngine;

public static class HybridCLRProjectSetup
{
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
}
