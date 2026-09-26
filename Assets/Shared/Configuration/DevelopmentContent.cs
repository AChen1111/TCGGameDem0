#nullable disable
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace AChen.Configuration
{
    public static class DevelopmentProtocol
    {
        public const int Version = 4;
        public const string Project = "TCGCardDem0";
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
        public string configHash;
        public string apkCompatibility;
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
