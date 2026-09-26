using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using AChen.Configuration;
using UnityEngine;

public static class DevelopmentPackage
{
    public static string HashFile(string path)
    {
        using (var hash = SHA256.Create())
        using (var stream = File.OpenRead(path))
            return BitConverter.ToString(hash.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
    }
    public static DevelopmentManifest Describe(string target, string compatibility, Dictionary<string, string> sources)
    {
        var configs = sources.Where(x => x.Key.StartsWith("GameConfig/", StringComparison.Ordinal)).Select(x =>
        {
            string name = Path.GetFileNameWithoutExtension(x.Key);
            return new ConfigArtifact { category = name, address = GameConfigTables.Address(name), format = GameConfigTables.Format(name),
                path = x.Key, size = new FileInfo(x.Value).Length, sha256 = HashFile(x.Value) };
        }).OrderBy(x => x.category, StringComparer.Ordinal).ToArray();
        ConfigArtifacts.Validate(configs, "");
        GameConfigTables.Assemble(configs.ToDictionary(x => x.category, x => File.ReadAllBytes(sources[x.path])));
        return new DevelopmentManifest
        {
            platform = target, apkCompatibility = compatibility, configs = configs,
            configHash = DevelopmentProtocol.ConfigHash(configs),
            hotUpdatePath = target == "Editor" ? null : "HybridCLR/HotUpdate.dll.bytes",
            catalogPath = sources.Keys.SingleOrDefault(x => x.StartsWith("Addressables/") && x.EndsWith(".bin")),
            catalogHashPath = sources.Keys.SingleOrDefault(x => x.StartsWith("Addressables/") && x.EndsWith(".hash")),
            files = sources.OrderBy(x => x.Key, StringComparer.Ordinal).Select(x => new DevelopmentFile
                { path = x.Key, size = new FileInfo(x.Value).Length, sha256 = HashFile(x.Value) }).ToArray()
        };
    }
    public static Dictionary<string, string> ConfigSources() => Directory.GetFiles(PublishedConfigBuilder.Root, "*.bytes")
        .ToDictionary(x => "GameConfig/" + Path.GetFileName(x), x => x);
    public static void SaveResourceSnapshot(string target, string outputRoot, IEnumerable<string> builtFiles)
    {
        string remote = Path.GetFullPath("ServerData/" + target) + Path.DirectorySeparatorChar;
        var sources = ConfigSources();
        foreach (string file in builtFiles.Select(Path.GetFullPath).Where(x => x.StartsWith(remote, StringComparison.OrdinalIgnoreCase) &&
            (x.EndsWith(".bundle") || x.EndsWith(".bin") || x.EndsWith(".hash"))))
            sources.Add("Addressables/" + file.Substring(remote.Length).Replace('\\', '/'), file);
        var manifest = Describe(target, "", sources);
        if (manifest.catalogPath == null || manifest.catalogHashPath == null || !sources.Keys.Any(x => x.EndsWith(".bundle")))
            throw new InvalidOperationException("本次构建未生成远程 catalog/资源, 请检查 Addressables Remote.BuildPath 是否为 ServerData/[BuildTarget]");
        string root = outputRoot + "/Resources";
        string receipt = outputRoot + "/resources.json";
        // 快照写入失败时不允许上传半成品; 服务器上一次成功内容不受影响.
        if (File.Exists(receipt)) File.Delete(receipt);
        foreach (var pair in sources)
        {
            string destination = SnapshotFile(root, pair.Key);
            Directory.CreateDirectory(Path.GetDirectoryName(destination));
            File.Copy(pair.Value, destination, true);
        }
        File.WriteAllText(receipt, JsonUtility.ToJson(manifest, true));
    }
    static string SnapshotFile(string root, string relative)
    {
        if (string.IsNullOrEmpty(relative) || relative.Contains(":") || relative.Contains("\\") ||
            relative.Split('/').Any(x => x == ".." || x == "." || x.Length == 0)) throw new FormatException("本地产物路径无效");
        string path = Path.GetFullPath(Path.Combine(root, relative));
        if (!path.StartsWith(Path.GetFullPath(root) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            throw new FormatException("本地产物路径越界");
        return path;
    }
    public static bool HasResourceSnapshot(string root, string target)
    {
        try
        {
            var manifest = JsonUtility.FromJson<DevelopmentManifest>(File.ReadAllText(root + "/resources.json"));
            return manifest.platform == target && manifest.files != null && manifest.files.Length > 0 && manifest.files.All(file =>
            {
                string path = SnapshotFile(root + "/Resources", file.path);
                return File.Exists(path) && new FileInfo(path).Length == file.size && HashFile(path) == file.sha256;
            });
        }
        catch (Exception) { return false; }
    }
    public static string WriteSnapshot(string target, string root, string compatibility, out DevelopmentManifest manifest)
    {
        if (!HasResourceSnapshot(root, target)) throw new InvalidOperationException("资源快照不完整, 请重新打包资源");
        var resources = JsonUtility.FromJson<DevelopmentManifest>(File.ReadAllText(root + "/resources.json"));
        var sources = resources.files.ToDictionary(x => x.path, x => SnapshotFile(root + "/Resources", x.path));
        sources.Add("HybridCLR/HotUpdate.dll.bytes", root + "/Code/HotUpdate.dll.bytes");
        manifest = Describe(target, compatibility, sources);
        return WriteArchive(root + "/Content.zip", manifest, sources);
    }
    public static string WriteEditor(out DevelopmentManifest manifest)
    {
        var sources = ConfigSources();
        manifest = Describe("Editor", "", sources);
        Directory.CreateDirectory("Library/Development");
        return WriteArchive("Library/Development/Editor.zip", manifest, sources);
    }
    static string WriteArchive(string output, DevelopmentManifest manifest, Dictionary<string, string> sources)
    {
        string signature = JsonUtility.ToJson(manifest);
        if (File.Exists(output) && File.Exists(output + ".manifest") && File.ReadAllText(output + ".manifest") == signature) return output;
        if (File.Exists(output + ".manifest")) File.Delete(output + ".manifest");
        if (File.Exists(output)) File.Delete(output);
        using (var zip = ZipFile.Open(output, ZipArchiveMode.Create))
        {
            foreach (var pair in sources) zip.CreateEntryFromFile(pair.Value, pair.Key, System.IO.Compression.CompressionLevel.Fastest);
            using (var writer = new StreamWriter(zip.CreateEntry("manifest.json").Open()))
                writer.Write(JsonUtility.ToJson(manifest));
        }
        File.WriteAllText(output + ".manifest", signature);
        return output;
    }
}
