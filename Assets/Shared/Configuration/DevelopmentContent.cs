#nullable disable
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace AChen.Configuration
{
    public static class DevelopmentProtocol
    {
        public const int Version = 5;
        public const string Project = "TCGCardDem0";
        public const string HotUpdatePath = "HybridCLR/HotUpdate.dll";
        public static bool ValidContentVersion(string version) => version != null &&
            Regex.IsMatch(version, "^[A-Za-z0-9][A-Za-z0-9._-]{0,63}\\z") && !version.EndsWith(".", StringComparison.Ordinal);
        public static string VersionFolder(string version, string platform)
        {
            if (!ValidContentVersion(version)) throw new FormatException("版本号必须是 1–64 位字母、数字、点、短横线或下划线, 且不能以点结尾");
            if (platform == "Android") return version + "_安卓";
            if (platform == "StandaloneWindows64") return version + "_win";
            throw new FormatException("mBulid 仅支持 Android 和 Windows x64");
        }
        public static string ConfigHash(ConfigArtifact[] configs)
        {
            var text = string.Join("\n", configs.OrderBy(x => x.category, StringComparer.Ordinal)
                .Select(x => x.category + ":" + x.sha256.ToLowerInvariant()));
            using (var hash = SHA256.Create())
                return BitConverter.ToString(hash.ComputeHash(Encoding.UTF8.GetBytes(text))).Replace("-", "").ToLowerInvariant();
        }
    }

    [Serializable]
    public sealed class DevelopmentManifest
    {
        public int schemaVersion = DevelopmentProtocol.Version;
        public string platform;
        public string contentId;
        public string contentVersion;
        public string configHash;
        public string serverTime;
        public string hotUpdatePath;
        public string catalogPath;
        public string catalogHashPath;
        public ConfigArtifact[] configs;
        public DevelopmentFile[] files;
    }

    [Serializable]
    public sealed class DevelopmentFile
    {
        public string path;
        public long size;
        public string sha256;
    }
}
