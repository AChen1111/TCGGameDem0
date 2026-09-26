using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using AChen.Configuration;
using UnityEngine;

public sealed class MBulidPackage
{
    [Serializable]
    public sealed class BuildInfo
    {
        public string platform;
        public string contentVersion;
        public bool codeComplete;
        public bool resourcesComplete;
        public DevelopmentFile dll;
        public DevelopmentFile[] resources;
        public string catalogPath;
        public string catalogHashPath;
    }

    readonly string projectRoot;
    public MBulidPackage(string projectRoot) => this.projectRoot = Path.GetFullPath(projectRoot);
    public string VersionRoot => Path.Combine(projectRoot, "Version");
    public string DirectoryFor(string platform, string version) => SafePath(VersionRoot, DevelopmentProtocol.VersionFolder(version, platform));
    public static string HashFile(string path)
    {
        using (var hash = SHA256.Create())
        using (var input = File.OpenRead(path))
            return BitConverter.ToString(hash.ComputeHash(input)).Replace("-", "").ToLowerInvariant();
    }
    public static string SafePath(string root, string relative)
    {
        if (string.IsNullOrWhiteSpace(relative) || relative.Contains(":") || relative.Contains("\\") ||
            relative.Split('/').Any(x => x == "" || x == "." || x == "..")) throw new FormatException("产物路径无效");
        root = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar);
        var path = Path.GetFullPath(Path.Combine(root, relative));
        if (!path.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) throw new FormatException("产物路径越界");
        for (string directory = Path.GetDirectoryName(path); directory != null && directory.Length >= root.Length; directory = Path.GetDirectoryName(directory))
            if (Directory.Exists(directory) && (File.GetAttributes(directory) & FileAttributes.ReparsePoint) != 0)
                throw new IOException("产物目录包含链接: " + directory);
        if ((File.Exists(path) || Directory.Exists(path)) && (File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0)
            throw new IOException("产物路径包含链接: " + path);
        return path;
    }
    public void ResetVersion(string platform, string version)
    {
        string directory = DirectoryFor(platform, version);
        DeleteDirectory(VersionRoot, DevelopmentProtocol.VersionFolder(version, platform));
        Directory.CreateDirectory(directory);
        Save(directory, new BuildInfo { platform = platform, contentVersion = version });
    }
    public BuildInfo Read(string platform, string version)
    {
        var info = JsonUtility.FromJson<BuildInfo>(File.ReadAllText(SafePath(DirectoryFor(platform, version), "build-info.json")));
        if (info == null || info.platform != platform || info.contentVersion != version) throw new FormatException("版本目录记录不匹配, 请重新设置版本号");
        // JsonUtility 会将 JSON null 读成空对象/数组, 完成标记必须独立保存.
        if (!info.codeComplete) info.dll = null;
        if (!info.resourcesComplete) info.resources = null;
        return info;
    }
    static void Save(string directory, BuildInfo info) => File.WriteAllText(SafePath(directory, "build-info.json"), JsonUtility.ToJson(info, true));
    static DevelopmentFile Describe(string directory, string relative)
    {
        string path = SafePath(directory, relative);
        return new DevelopmentFile { path = relative, size = new FileInfo(path).Length, sha256 = HashFile(path) };
    }
    static void RequireFile(string directory, DevelopmentFile file)
    {
        if (file == null) throw new InvalidOperationException("产物尚未完成构建");
        string path = SafePath(directory, file.path);
        if (!File.Exists(path) || new FileInfo(path).Length != file.size || HashFile(path) != file.sha256)
            throw new IOException("产物缺失或已变化, 请重新构建: " + file.path);
    }
    public static void DeleteDirectory(string root, string relative)
    {
        string path = SafePath(root, relative);
        if (!Directory.Exists(path)) return;
        if (Directory.EnumerateFileSystemEntries(path, "*", SearchOption.AllDirectories)
            .Any(x => (File.GetAttributes(x) & FileAttributes.ReparsePoint) != 0)) throw new IOException("产物目录包含链接, 拒绝删除");
        Directory.Delete(path, true);
    }
    public void BeginCode(string platform, string version)
    {
        var info = Read(platform, version);
        info.dll = null; info.codeComplete = false;
        string directory = DirectoryFor(platform, version);
        Save(directory, info);
        DeleteDirectory(directory, "HybridCLR");
        File.Delete(SafePath(directory, "manifest.json"));
    }
    public void CompleteCode(string platform, string version, string compiledDll)
    {
        var info = Read(platform, version);
        if (!File.Exists(compiledDll) || new FileInfo(compiledDll).Length == 0 || AssemblyName.GetAssemblyName(compiledDll).Name != "HotUpdate")
            throw new InvalidOperationException("本次构建没有生成有效的 HotUpdate.dll");
        string directory = DirectoryFor(platform, version);
        string destination = SafePath(directory, DevelopmentProtocol.HotUpdatePath);
        Directory.CreateDirectory(Path.GetDirectoryName(destination));
        File.Copy(compiledDll, destination, true);
        info.dll = Describe(directory, DevelopmentProtocol.HotUpdatePath);
        File.WriteAllText(destination + ".sha256", info.dll.sha256 + "\n");
        info.codeComplete = true;
        Save(directory, info);
    }
    public void BeginResources(string platform, string version)
    {
        var info = Read(platform, version);
        info.resources = null; info.resourcesComplete = false; info.catalogPath = info.catalogHashPath = null;
        string directory = DirectoryFor(platform, version);
        Save(directory, info);
        DeleteDirectory(directory, "Addressables");
        DeleteDirectory(directory, "GameConfig");
        File.Delete(SafePath(directory, "manifest.json"));
    }
    public void CompleteResources(string platform, string version, IEnumerable<string> builtFiles, string configRoot)
    {
        string directory = DirectoryFor(platform, version);
        string remote = SafePath(directory, "Addressables");
        string prefix = remote + Path.DirectorySeparatorChar;
        var paths = builtFiles.Select(Path.GetFullPath).Where(x => x.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) &&
            (x.EndsWith(".bundle", StringComparison.Ordinal) || x.EndsWith(".bin", StringComparison.Ordinal) || x.EndsWith(".hash", StringComparison.Ordinal)))
            .Select(x => "Addressables/" + x.Substring(prefix.Length).Replace('\\', '/')).Distinct().ToList();
        foreach (string source in Directory.GetFiles(configRoot, "*.bytes"))
        {
            string relative = "GameConfig/" + Path.GetFileName(source);
            string destination = SafePath(directory, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(destination)); File.Copy(source, destination, true);
            paths.Add(relative);
        }
        var info = Read(platform, version);
        info.catalogPath = paths.SingleOrDefault(x => x.EndsWith(".bin", StringComparison.Ordinal));
        info.catalogHashPath = paths.SingleOrDefault(x => x.StartsWith("Addressables/", StringComparison.Ordinal) && x.EndsWith(".hash", StringComparison.Ordinal));
        if (info.catalogPath == null || info.catalogHashPath == null || !paths.Any(x => x.EndsWith(".bundle", StringComparison.Ordinal)))
            throw new InvalidOperationException("本次构建缺少远程 catalog、hash 或 bundle");
        info.resources = paths.OrderBy(x => x, StringComparer.Ordinal).Select(x => Describe(directory, x)).ToArray();
        ConfigArtifacts.Validate(Configs(info.resources), "");
        GameConfigTables.Assemble(info.resources.Where(x => x.path.StartsWith("GameConfig/", StringComparison.Ordinal))
            .ToDictionary(x => Path.GetFileNameWithoutExtension(x.path), x => File.ReadAllBytes(SafePath(directory, x.path))));
        info.resourcesComplete = true;
        Save(directory, info);
    }
    static ConfigArtifact[] Configs(IEnumerable<DevelopmentFile> files) => files.Where(x => x.path.StartsWith("GameConfig/", StringComparison.Ordinal))
        .Select(x => new ConfigArtifact { category = Path.GetFileNameWithoutExtension(x.path), address = GameConfigTables.Address(Path.GetFileNameWithoutExtension(x.path)),
            format = GameConfigTables.Format(Path.GetFileNameWithoutExtension(x.path)), path = x.path, size = x.size, sha256 = x.sha256 })
        .OrderBy(x => x.category, StringComparer.Ordinal).ToArray();
    public DevelopmentManifest Validate(string platform, string version)
    {
        string directory = DirectoryFor(platform, version);
        var info = Read(platform, version);
        RequireFile(directory, info.dll);
        if (info.dll.path != DevelopmentProtocol.HotUpdatePath ||
            File.ReadAllText(SafePath(directory, DevelopmentProtocol.HotUpdatePath + ".sha256")).Trim() != info.dll.sha256)
            throw new IOException("DLL 哈希记录不匹配, 请重新构建 DLL");
        if (info.resources == null || info.resources.Length == 0) throw new InvalidOperationException("请先构建 Addressables");
        foreach (var file in info.resources) RequireFile(directory, file);
        if (info.catalogPath == null || info.catalogHashPath == null || !info.resources.Any(x => x.path == info.catalogPath) ||
            !info.resources.Any(x => x.path == info.catalogHashPath)) throw new FormatException("资源目录不完整");
        var expected = info.resources.Select(x => x.path).Concat(new[] { DevelopmentProtocol.HotUpdatePath, DevelopmentProtocol.HotUpdatePath + ".sha256", "build-info.json" })
            .OrderBy(x => x, StringComparer.Ordinal).ToArray();
        var actual = Directory.GetFiles(directory, "*", SearchOption.AllDirectories)
            .Select(x => x.Substring(directory.Length + 1).Replace('\\', '/')).Where(x => x != "manifest.json").OrderBy(x => x, StringComparer.Ordinal).ToArray();
        if (!expected.SequenceEqual(actual)) throw new IOException("版本目录包含未记录或缺失的文件, 请重新构建");
        var files = expected.Select(x => Describe(directory, x)).ToArray();
        var configs = Configs(info.resources);
        ConfigArtifacts.Validate(configs, "");
        GameConfigTables.Assemble(configs.ToDictionary(x => x.category, x => File.ReadAllBytes(SafePath(directory, x.path))));
        return new DevelopmentManifest { platform = platform, contentVersion = version, hotUpdatePath = DevelopmentProtocol.HotUpdatePath,
            catalogPath = info.catalogPath, catalogHashPath = info.catalogHashPath, configs = configs,
            configHash = DevelopmentProtocol.ConfigHash(configs), files = files };
    }
    public string CreateArchive(string platform, string version, out DevelopmentManifest manifest)
    {
        manifest = Validate(platform, version);
        string directory = DirectoryFor(platform, version);
        File.WriteAllText(SafePath(directory, "manifest.json"), JsonUtility.ToJson(manifest, true));
        string temporary = Path.Combine(projectRoot, "Temp", "mBulid", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(temporary);
        string zipPath = Path.Combine(temporary, "Content.zip");
        using (var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
        {
            zip.CreateEntryFromFile(SafePath(directory, "manifest.json"), "manifest.json");
            foreach (var file in manifest.files) zip.CreateEntryFromFile(SafePath(directory, file.path), file.path, System.IO.Compression.CompressionLevel.Fastest);
        }
        return zipPath;
    }
}
