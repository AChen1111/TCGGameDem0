using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public sealed class ContentReleaseManifest
{
    public int schemaVersion;
    public string releaseId;
    public string channel;
    public string platform;
    public string appVersion;
    public string contentVersion;
    public string publishedAt;
    public string serverTime;
    public HotUpdateArtifact config;
    public HotUpdateArtifact hotUpdate;
    public AddressablesArtifact addressables;
}

[Serializable]
public sealed class HotUpdateArtifact
{
    public string path;
    public long size;
    public string sha256;
}

[Serializable]
public sealed class AddressablesArtifact
{
    public string basePath;
    public string catalogPath;
    public string catalogHashPath;
}

public static class CodeUpdate
{
    public const string DefaultBackendUrl = "http://127.0.0.1:5080";
    public const string DefaultChannel = "development";
    const int RetryCount = 2;
    static bool s_contentNotReady;

    [Serializable]
    sealed class ContentProblem
    {
        public string code;
    }

    public static bool IsComplete { get; private set; }
    public static LocalizedMessage LastErrorMessage { get; private set; }
    public static string LastError => LastErrorMessage?.ToString();
    public static ContentReleaseManifest CurrentManifest { get; private set; }
    public static string AddressablesBaseUrl { get; private set; }

    public static string Sha256Of(byte[] data)
    {
        byte[] hash = SHA256.Create().ComputeHash(data);
        var builder = new StringBuilder(hash.Length * 2);
        for (int i = 0; i < hash.Length; i++)
        {
            builder.Append(hash[i].ToString("x2"));
        }

        return builder.ToString();
    }

    public static bool HasExpectedSha256(byte[] data, string expected)
    {
        return !string.IsNullOrWhiteSpace(expected)
            && string.Equals(Sha256Of(data), expected.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    public static string ManifestUrl(
        string backendUrl,
        string channel,
        string platform,
        string appVersion)
    {
        return backendUrl.TrimEnd('/')
            + "/api/content/manifests/latest?channel=" + UnityWebRequest.EscapeURL(channel)
            + "&platform=" + UnityWebRequest.EscapeURL(platform)
            + "&appVersion=" + UnityWebRequest.EscapeURL(appVersion);
    }

    public static string CachePathFor(string releaseId)
    {
        return Path.Combine(
            Application.persistentDataPath,
            "Content",
            releaseId,
            LoadDll.DllDir,
            LoadDll.HotUpdateFile);
    }

    public static string PlatformName()
    {
#if UNITY_EDITOR
        switch (UnityEditor.EditorUserBuildSettings.activeBuildTarget)
        {
            case UnityEditor.BuildTarget.StandaloneWindows:
            case UnityEditor.BuildTarget.StandaloneWindows64:
                return "StandaloneWindows64";
            case UnityEditor.BuildTarget.Android:
                return "Android";
            case UnityEditor.BuildTarget.iOS:
                return "iOS";
        }
#else
        switch (Application.platform)
        {
            case RuntimePlatform.WindowsPlayer:
                return "StandaloneWindows64";
            case RuntimePlatform.Android:
                return "Android";
            case RuntimePlatform.IPhonePlayer:
                return "iOS";
        }
#endif
        throw new NotSupportedException("Content delivery does not support the current platform.");
    }

    public const string EditorLocalReleaseId = "editor-local";

    /// <summary>Editor 默认: 只绑定后端地址与本地会话, 不拉 Manifest / DLL / 远程目录.</summary>
    public static void BindEditorLocalSession(string backendUrl, string channel, string platform, string appVersion)
    {
        IsComplete = true;
        LastErrorMessage = null;
        CurrentManifest = null;
        AddressablesBaseUrl = null;
        DateTimeOffset now = DateTimeOffset.UtcNow;
        AChen.Configuration.ContentSession.BackendUrl = backendUrl;
        AChen.Configuration.ContentSession.Channel = channel;
        AChen.Configuration.ContentSession.Platform = platform;
        AChen.Configuration.ContentSession.AppVersion = appVersion;
        AChen.Configuration.ContentSession.ReleaseId = EditorLocalReleaseId;
        AChen.Configuration.ContentSession.ConfigHash = string.Empty;
        AChen.Configuration.ContentSession.CatalogUrl = null;
        AChen.Configuration.ContentSession.ServerTime = now;
        AChen.Configuration.ContentSession.ServerTimeReceivedAt = now;
        AChen.Configuration.ContentSession.RestartRequired = false;
        AChen.Configuration.ContentSession.UseLocalAssets = true;
        ALog.Log("Editor 使用本地内容会话, 跳过远程 Manifest.", ALogCategories.Net);
    }

    public static IEnumerator FetchInto(
        Dictionary<string, byte[]> bytes,
        string backendUrl,
        string channel,
        string platform,
        string appVersion,
        Action<float> onProgress = null)
    {
        IsComplete = false;
        LastErrorMessage = null;
        CurrentManifest = null;
        AddressablesBaseUrl = null;

        string manifestJson = null;
        string requestError = null;
        yield return GetTextWithRetry(
            ManifestUrl(backendUrl, channel, platform, appVersion),
            value => manifestJson = value,
            error => requestError = error);
        if (!string.IsNullOrEmpty(requestError))
        {
            LastErrorMessage = new LocalizedMessage(s_contentNotReady ? "err.content_not_ready" : "err.content_version_failed", new Dictionary<string, object> { ["error"] = requestError });
            yield break;
        }

        DateTimeOffset manifestReceivedAt = DateTimeOffset.UtcNow;
        ContentReleaseManifest manifest;
        try
        {
            manifest = JsonUtility.FromJson<ContentReleaseManifest>(manifestJson);
            ValidateManifest(manifest, channel, platform, appVersion);
            AddressablesBaseUrl = ResolveContentUrl(backendUrl, manifest.addressables.basePath).TrimEnd('/');
        }
        catch (Exception exception)
        {
            LastErrorMessage = new LocalizedMessage("err.content_manifest_invalid", new Dictionary<string, object> { ["message"] = exception.Message });
            yield break;
        }

        string cachePath = CachePathFor(manifest.releaseId);
        byte[] dllBytes = null;
        if (File.Exists(cachePath))
        {
            try
            {
                byte[] cached = File.ReadAllBytes(cachePath);
                if (cached.LongLength == manifest.hotUpdate.size
                    && HasExpectedSha256(cached, manifest.hotUpdate.sha256))
                {
                    dllBytes = cached;
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[ContentDelivery] Ignore invalid cache: " + exception.Message);
            }
        }

        if (dllBytes == null)
        {
            string dllUrl;
            try
            {
                dllUrl = ResolveContentUrl(backendUrl, manifest.hotUpdate.path);
            }
            catch (Exception exception)
            {
                LastErrorMessage = new LocalizedMessage("err.hot_update_dll_url_invalid", new Dictionary<string, object> { ["message"] = exception.Message });
                yield break;
            }

            requestError = null;
            yield return GetBytesWithRetry(
                dllUrl,
                value => dllBytes = value,
                error => requestError = error,
                onProgress);
            if (!string.IsNullOrEmpty(requestError))
            {
                LastErrorMessage = new LocalizedMessage("err.hot_update_dll_download_failed", new Dictionary<string, object> { ["error"] = requestError });
                yield break;
            }

            if (dllBytes.LongLength != manifest.hotUpdate.size
                || !HasExpectedSha256(dllBytes, manifest.hotUpdate.sha256))
            {
                LastErrorMessage = new LocalizedMessage("err.hot_update_dll_checksum_failed");
                yield break;
            }

            try
            {
                WriteCacheAtomically(cachePath, dllBytes);
            }
            catch (Exception exception)
            {
                LastErrorMessage = new LocalizedMessage("err.hot_update_cache_write_failed", new Dictionary<string, object> { ["message"] = exception.Message });
                yield break;
            }
        }

        bytes[LoadDll.HotUpdateFile] = dllBytes;
        CurrentManifest = manifest;
        AChen.Configuration.ContentSession.BackendUrl = backendUrl;
        AChen.Configuration.ContentSession.Channel = channel;
        AChen.Configuration.ContentSession.Platform = platform;
        AChen.Configuration.ContentSession.AppVersion = appVersion;
        AChen.Configuration.ContentSession.ReleaseId = manifest.releaseId;
        AChen.Configuration.ContentSession.ConfigHash = manifest.config.sha256;
        AChen.Configuration.ContentSession.CatalogUrl = ResolveContentUrl(backendUrl, manifest.addressables.catalogPath);
        AChen.Configuration.ContentSession.ServerTime = DateTimeOffset.Parse(manifest.serverTime);
        AChen.Configuration.ContentSession.ServerTimeReceivedAt = manifestReceivedAt;
        AChen.Configuration.ContentSession.RestartRequired = false;
        AChen.Configuration.ContentSession.UseLocalAssets = false;
        onProgress?.Invoke(1f);
        IsComplete = true;
    }

    static void ValidateManifest(
        ContentReleaseManifest manifest,
        string channel,
        string platform,
        string appVersion)
    {
        if (manifest == null || manifest.schemaVersion != 2)
        {
            throw new InvalidDataException("不支持的 schemaVersion。");
        }

        Guid releaseId;
        if (!Guid.TryParse(manifest.releaseId, out releaseId)
            || !string.Equals(manifest.channel, channel, StringComparison.Ordinal)
            || !string.Equals(manifest.platform, platform, StringComparison.Ordinal)
            || !string.Equals(manifest.appVersion, appVersion, StringComparison.Ordinal)
            || manifest.config == null
            || manifest.config.size < 1
            || string.IsNullOrWhiteSpace(manifest.config.sha256)
            || !DateTimeOffset.TryParse(manifest.serverTime, out _)
            || manifest.hotUpdate == null
            || manifest.addressables == null
            || manifest.hotUpdate.size < 1
            || string.IsNullOrWhiteSpace(manifest.hotUpdate.sha256))
        {
            throw new InvalidDataException("清单身份或关键文件字段不完整。");
        }

        string expectedPrefix = "/content/releases/" + releaseId.ToString("D") + "/";
        if (manifest.config.path != expectedPrefix + "GameConfig/config.json"
            || !manifest.hotUpdate.path.StartsWith(expectedPrefix, StringComparison.OrdinalIgnoreCase)
            || !manifest.addressables.basePath.StartsWith(expectedPrefix, StringComparison.OrdinalIgnoreCase)
            || !manifest.addressables.catalogPath.StartsWith(expectedPrefix, StringComparison.OrdinalIgnoreCase)
            || !manifest.addressables.catalogHashPath.StartsWith(expectedPrefix, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException("清单文件不属于当前 Release。");
        }
    }

    public static string ResolveContentUrl(string backendUrl, string relativePath)
    {
        Uri origin;
        Uri resolved;
        if (!Uri.TryCreate(backendUrl.TrimEnd('/') + "/", UriKind.Absolute, out origin)
            || !Uri.TryCreate(origin, relativePath, out resolved)
            || !string.Equals(origin.Scheme, resolved.Scheme, StringComparison.OrdinalIgnoreCase)
            || !string.Equals(origin.Authority, resolved.Authority, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException("内容地址必须属于配置的后端。");
        }

        return resolved.AbsoluteUri;
    }

    static void WriteCacheAtomically(string path, byte[] data)
    {
        string directory = Path.GetDirectoryName(path);
        Directory.CreateDirectory(directory);
        string temporaryPath = path + ".tmp";
        if (File.Exists(temporaryPath))
        {
            File.Delete(temporaryPath);
        }

        File.WriteAllBytes(temporaryPath, data);
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        File.Move(temporaryPath, path);
    }

    static IEnumerator GetTextWithRetry(string url, Action<string> onDone, Action<string> onError)
    {
        byte[] bytes = null;
        yield return GetBytesWithRetry(url, value => bytes = value, onError, null);
        if (bytes != null)
        {
            onDone(Encoding.UTF8.GetString(bytes));
        }
    }

    static IEnumerator GetBytesWithRetry(
        string url,
        Action<byte[]> onDone,
        Action<string> onError,
        Action<float> onProgress)
    {
        string lastError = null;
        s_contentNotReady = false;
        long status = 0;
        for (int attempt = 0; attempt <= RetryCount; attempt++)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.timeout = 20;
                UnityWebRequestAsyncOperation operation = request.SendWebRequest();
                while (!operation.isDone)
                {
                    onProgress?.Invoke(Mathf.Max(0f, request.downloadProgress));
                    yield return null;
                }

                if (request.result == UnityWebRequest.Result.Success)
                {
                    onDone(request.downloadHandler.data);
                    yield break;
                }

                lastError = string.IsNullOrWhiteSpace(request.error)
                    ? "HTTP " + request.responseCode
                    : request.error + " (HTTP " + request.responseCode + ")";
                status = request.responseCode;
                // 启动阶段尚无语言表, 只识别服务端错误码, 不显示原始响应正文.
                try
                {
                    s_contentNotReady = JsonUtility.FromJson<ContentProblem>(request.downloadHandler.text)?.code == "CONTENT_NOT_READY";
                }
                catch (Exception) { }
                if (status >= 400 && status < 500 && status != 408 && status != 429) break;
            }
        }

        ALog.LogError("内容请求失败. Path=" + new Uri(url).AbsolutePath + "; HTTP=" + status
            + "; Code=" + (s_contentNotReady ? "CONTENT_NOT_READY" : "REQUEST_FAILED")
            + "; Error=" + lastError, ALogCategories.Localization);
        onError(lastError ?? "未知网络错误");
    }
}
