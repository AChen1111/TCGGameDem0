using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using AChen.Configuration;
using UnityEngine;
using UnityEngine.Networking;

public static class CodeUpdate
{
    public const string DefaultBackendUrl = "http://39.97.56.180";
    public const string DefaultChannel = "development";
    public const string EditorLocalReleaseId = "editor-local";
    public static bool IsComplete { get; private set; }
    public static LocalizedMessage LastErrorMessage { get; private set; }
    public static string LastError => LastErrorMessage?.ToString();
    public static DevelopmentManifest CurrentManifest { get; private set; }
    public static StartupContext Context { get; private set; } = new StartupContext();
    public static string AddressablesBaseUrl => Context.AddressablesBaseUrl;
    public static string Sha256Of(byte[] bytes)
    {
        using (var hash = SHA256.Create()) return BitConverter.ToString(hash.ComputeHash(bytes)).Replace("-", "").ToLowerInvariant();
    }
    public static bool HasExpectedSha256(byte[] bytes, string hash) => string.Equals(Sha256Of(bytes), hash, StringComparison.OrdinalIgnoreCase);
    public static string PlatformName()
    {
#if UNITY_EDITOR
        if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.Android) return "Android";
        if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.iOS) return "iOS";
        return "StandaloneWindows64";
#else
        if (Application.platform == RuntimePlatform.Android) return "Android";
        if (Application.platform == RuntimePlatform.IPhonePlayer) return "iOS";
        return "StandaloneWindows64";
#endif
    }
    public static string ManifestUrl(string backend, string channel, string platform, string appVersion) =>
        backend.TrimEnd('/') + "/api/content/latest/" + platform;
    public static string ResolveContentUrl(string backend, string path)
    {
        var origin = new Uri(backend.TrimEnd('/') + "/");
        var result = new Uri(origin, path);
        if (origin.Scheme != result.Scheme || origin.Authority != result.Authority) throw new FormatException("内容地址必须属于后端");
        return result.AbsoluteUri;
    }
    public static string CachePathFor(string id) => Path.Combine(Application.persistentDataPath, "DevelopmentContent", id, LoadDll.DllDir, LoadDll.HotUpdateFile);
    public static void BindEditorLocalSession(string backend, string channel, string platform, string version)
    {
        Context = new StartupContext();
        Context.Channel = channel;
        Context.BackendUrl = backend;
        Context.Platform = "Editor";
        Context.Target = "Editor";
        Context.AppVersion = version;
        Context.ReleaseId = EditorLocalReleaseId;
        Context.ConfigHash = null;
        Context.Configs = null;
        Context.CatalogUrl = null;
        Context.UseLocalAssets = true;

        CurrentManifest = null;
        Context.AddressablesBaseUrl = null;
        LastErrorMessage = null;
        Context.ServerTime = Context.ServerTimeReceivedAt = DateTimeOffset.UtcNow;
        IsComplete = true;
    }
    static void Fail(string error)
    {
        LastErrorMessage = new LocalizedMessage("err.content_version_failed", new Dictionary<string, object> { ["error"] = error });
        Debug.LogError("[Bootstrap] 启动内容准备失败. Error=" + error);
    }
    static string SafeFile(string root, string relative)
    {
        if (string.IsNullOrEmpty(relative) || relative.Contains("\\") || relative.Contains(":") ||
            relative.Split('/').Any(x => x == "" || x == "." || x == "..")) throw new FormatException("内容路径无效");
        var full = Path.GetFullPath(Path.Combine(root, relative));
        if (!full.StartsWith(Path.GetFullPath(root) + Path.DirectorySeparatorChar, StringComparison.Ordinal)) throw new FormatException("内容路径越界");
        return full;
    }
    static bool ValidFile(string path, DevelopmentFile file)
    {
        if (!File.Exists(path) || new FileInfo(path).Length != file.size) return false;
        using (var hash = SHA256.Create())
        using (var input = File.OpenRead(path))
            return BitConverter.ToString(hash.ComputeHash(input)).Replace("-", "").Equals(file.sha256, StringComparison.OrdinalIgnoreCase);
    }
    public static IEnumerator FetchInto(Dictionary<string, byte[]> bytes, string backend, string channel,
        string platform, string version, Action<float> progress = null)
    {
        IsComplete = false; LastErrorMessage = null; CurrentManifest = null; Context = new StartupContext();
        string json;
        using (var request = UnityWebRequest.Get(ManifestUrl(backend, channel, platform, version)))
        {
            request.timeout = 20;
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                Fail(request.responseCode == 404 ? "尚未发布此平台内容, 请先发布当前平台的热更内容" :
                    "无法获取后端最新内容, 请检查后端和网络连接 (Android USB 调试需转发 5080). " + request.error);
                yield break;
            }
            json = request.downloadHandler.text;
        }
        DevelopmentManifest manifest;
        string cacheRoot = Path.Combine(Application.persistentDataPath, "DevelopmentContent");
        string destination;
        try
        {
            manifest = JsonUtility.FromJson<DevelopmentManifest>(json);
            if (manifest.schemaVersion != DevelopmentProtocol.Version || manifest.platform != platform ||
                !DevelopmentProtocol.ValidContentVersion(manifest.contentVersion) || manifest.hotUpdatePath != DevelopmentProtocol.HotUpdatePath ||
                !Guid.TryParse(manifest.contentId, out _) || manifest.files == null || !DateTimeOffset.TryParse(manifest.serverTime, out _)) throw new FormatException("内容协议不一致, 请更新当前平台的完整游戏包");
            destination = Path.Combine(cacheRoot, manifest.contentId);
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var file in manifest.files)
            {
                SafeFile(destination, file.path);
                if (!names.Add(file.path) || file.size < 0 || file.sha256 == null || !System.Text.RegularExpressions.Regex.IsMatch(file.sha256, "^[0-9a-fA-F]{64}$")) throw new FormatException("文件清单无效");
            }
            if (!names.Contains(manifest.hotUpdatePath) || !names.Contains(manifest.catalogPath) || !names.Contains(manifest.catalogHashPath)) throw new FormatException("内容不完整");
        }
        catch (Exception ex) { Fail(ex.Message); yield break; }
        string directoryError = null;
        try { Directory.CreateDirectory(destination); }
        catch (Exception ex) { directoryError = ex.Message; }
        if (directoryError != null) { Fail("无法创建内容缓存: " + directoryError); yield break; }
        string prefix = "/content/current/" + platform + "/" + manifest.contentId + "/";
        for (int i = 0; i < manifest.files.Length; i++)
        {
            var file = manifest.files[i];
            string path = SafeFile(destination, file.path);
            string error = null;
            bool cached = false;
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                if (!ValidFile(path, file))
                {
                    foreach (var previous in Directory.GetDirectories(cacheRoot))
                    {
                        string candidate = SafeFile(previous, file.path);
                        if (previous != destination && ValidFile(candidate, file)) { File.Copy(candidate, path, true); break; }
                    }
                }
                cached = ValidFile(path, file);
            }
            catch (Exception ex) { error = ex.Message; }
            if (error != null) { Fail(error); yield break; }
            if (!cached)
            {
                using (var request = UnityWebRequest.Get(ResolveContentUrl(backend, prefix + file.path)))
                {
                    request.downloadHandler = new DownloadHandlerFile(path + ".tmp") { removeFileOnAbort = true };
                    request.timeout = 120;
                    yield return request.SendWebRequest();
                    if (request.result != UnityWebRequest.Result.Success)
                    { Fail(request.responseCode == 409 ? "发布内容已变化, 请重试" : "下载失败: " + file.path + "; " + request.error); yield break; }
                }
                try
                {
                    if (!ValidFile(path + ".tmp", file)) throw new IOException("文件校验失败: " + file.path);
                    if (File.Exists(path)) File.Delete(path);
                    File.Move(path + ".tmp", path);
                }
                catch (Exception ex) { error = ex.Message; }
                if (error != null) { Fail(error); yield break; }
            }
            progress?.Invoke((i + 1f) / manifest.files.Length);
        }
        try
        {
            bytes[LoadDll.HotUpdateFile] = File.ReadAllBytes(SafeFile(destination, manifest.hotUpdatePath));
            Context.BackendUrl = backend; Context.Target = platform; Context.Platform = platform;
            Context.Channel = channel; Context.AppVersion = version; Context.ReleaseId = manifest.contentId;
            Context.ConfigHash = manifest.configHash; Context.Configs = manifest.configs;
            Context.CatalogUrl = new Uri(SafeFile(destination, manifest.catalogPath)).AbsoluteUri;
            Context.ServerTime = DateTimeOffset.Parse(manifest.serverTime);
            Context.ServerTimeReceivedAt = DateTimeOffset.UtcNow;
            Context.UseLocalAssets = false;
            Context.AddressablesBaseUrl = new Uri(Path.Combine(destination, "Addressables") + Path.DirectorySeparatorChar).AbsoluteUri.TrimEnd('/');
            CurrentManifest = manifest;
            // 只清除应用专用内容目录, 保留登录和玩家数据.
            foreach (var previous in Directory.GetDirectories(cacheRoot))
                if (previous != destination && Guid.TryParse(Path.GetFileName(previous), out _)) Directory.Delete(previous, true);
            string legacy = Path.Combine(Application.persistentDataPath, "Content");
            if (Directory.Exists(legacy)) Directory.Delete(legacy, true);
            IsComplete = true;
            Debug.Log("[Bootstrap] 内容准备完成. Target=" + platform + "; Content=" + manifest.contentId);
        }
        catch (Exception ex) { Fail(ex.Message); }
    }
}
