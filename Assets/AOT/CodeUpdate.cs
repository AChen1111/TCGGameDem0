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

    public static bool IsComplete { get; private set; }
    public static string LastError { get; private set; }
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

    public static IEnumerator FetchInto(
        Dictionary<string, byte[]> bytes,
        string backendUrl,
        string channel,
        string platform,
        string appVersion,
        Action<float> onProgress = null)
    {
        IsComplete = false;
        LastError = null;
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
            LastError = "获取内容版本失败：" + requestError;
            yield break;
        }

        ContentReleaseManifest manifest;
        try
        {
            manifest = JsonUtility.FromJson<ContentReleaseManifest>(manifestJson);
            ValidateManifest(manifest, channel, platform, appVersion);
            AddressablesBaseUrl = ResolveContentUrl(backendUrl, manifest.addressables.basePath).TrimEnd('/');
        }
        catch (Exception exception)
        {
            LastError = "内容清单无效：" + exception.Message;
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
                LastError = "热更 DLL 地址无效：" + exception.Message;
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
                LastError = "下载热更 DLL 失败：" + requestError;
                yield break;
            }

            if (dllBytes.LongLength != manifest.hotUpdate.size
                || !HasExpectedSha256(dllBytes, manifest.hotUpdate.sha256))
            {
                LastError = "热更 DLL 的长度或 SHA-256 校验失败。";
                yield break;
            }

            try
            {
                WriteCacheAtomically(cachePath, dllBytes);
            }
            catch (Exception exception)
            {
                LastError = "写入热更缓存失败：" + exception.Message;
                yield break;
            }
        }

        bytes[LoadDll.HotUpdateFile] = dllBytes;
        CurrentManifest = manifest;
        onProgress?.Invoke(1f);
        IsComplete = true;
    }

    static void ValidateManifest(
        ContentReleaseManifest manifest,
        string channel,
        string platform,
        string appVersion)
    {
        if (manifest == null || manifest.schemaVersion != 1)
        {
            throw new InvalidDataException("不支持的 schemaVersion。");
        }

        Guid releaseId;
        if (!Guid.TryParse(manifest.releaseId, out releaseId)
            || !string.Equals(manifest.channel, channel, StringComparison.Ordinal)
            || !string.Equals(manifest.platform, platform, StringComparison.Ordinal)
            || !string.Equals(manifest.appVersion, appVersion, StringComparison.Ordinal)
            || manifest.hotUpdate == null
            || manifest.addressables == null
            || manifest.hotUpdate.size < 1
            || string.IsNullOrWhiteSpace(manifest.hotUpdate.sha256))
        {
            throw new InvalidDataException("清单身份或关键文件字段不完整。");
        }

        string expectedPrefix = "/content/releases/" + releaseId.ToString("D") + "/";
        if (!manifest.hotUpdate.path.StartsWith(expectedPrefix, StringComparison.OrdinalIgnoreCase)
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
        for (int attempt = 0; attempt <= RetryCount; attempt++)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
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
            }
        }

        onError(lastError ?? "未知网络错误");
    }
}
