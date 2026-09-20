using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace AChen.Configuration
{
    public static class ConfigArtifacts
    {
        public static void Validate(ConfigArtifact[] artifacts, string prefix)
        {
            if (artifacts == null) throw new FormatException("发布清单缺少 configs");
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in artifacts)
                if (item == null || !GameConfigTables.ValidName(item.category) || !names.Add(item.category)
                    || item.format != GameConfigTables.Format(item.category)
                    || item.address != GameConfigTables.Address(item.category) || item.path != prefix + GameConfigTables.PackagePath(item.category)
                    || item.size < 1 || item.sha256 == null || !Regex.IsMatch(item.sha256, "^[0-9a-fA-F]{64}$"))
                    throw new FormatException("配置清单条目无效或重复: " + item?.category);
            foreach (string name in GameConfigTables.Required)
                if (!artifacts.Any(a => a.category == name)) throw new FormatException("发布清单缺少配置: " + name);
        }

        public static void Verify(IReadOnlyDictionary<string, byte[]> files, ConfigArtifact[] artifacts)
        {
            if (artifacts == null || artifacts.Length != files.Count) throw new FormatException("配置类别与发布清单不一致");
            foreach (var item in artifacts)
            {
                if (!files.TryGetValue(item.category, out var bytes) || bytes.LongLength != item.size) throw new FormatException("配置缺失或大小不符: " + item.category);
                using (var hash = SHA256.Create())
                    if (!string.Equals(BitConverter.ToString(hash.ComputeHash(bytes)).Replace("-", ""), item.sha256, StringComparison.OrdinalIgnoreCase))
                        throw new FormatException("配置哈希不符: " + item.category);
            }
        }
    }
}
