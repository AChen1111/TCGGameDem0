using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.Networking;

public sealed class ContentReleasePublisherWindow : EditorWindow
{
    const string BackendUrlPreference = "AChen.ContentDelivery.BackendUrl";
    const string Channel = "development";

    string m_BackendUrl = CodeUpdate.DefaultBackendUrl;
    string m_ContentVersion = string.Empty;
    string m_Notes = string.Empty;
    string m_MemoryPublishKey = string.Empty;
    string m_Status = "等待发布";
    float m_Progress;
    bool m_IsBusy;

    [MenuItem(EditorMenus.Window + "内容发布")]
    [MenuItem(EditorMenus.Release + "Build And Publish Release", false, 20)]
    static void Open()
    {
        GetWindow<ContentReleasePublisherWindow>("Content Release");
    }

    [MenuItem(EditorMenus.Release + "Build And Publish Release From Env", false, 21)]
    static void PublishFromEnv()
    {
        var window = GetWindow<ContentReleasePublisherWindow>("Content Release");
        string version = Environment.GetEnvironmentVariable("ACHEN_CONTENT_VERSION");
        window.m_ContentVersion = string.IsNullOrWhiteSpace(version) ? "0.2.0" : version.Trim();
        window.m_Notes = "folder cleanup remote UI art";
        _ = window.BuildAndPublishAsync();
    }

    [MenuItem(EditorMenus.Release + "Build Addressables", false, 10)]
    static void BuildAddressables()
    {
        AddressableAssetSettings.BuildPlayerContent();
    }

    void OnEnable()
    {
        m_BackendUrl = EditorPrefs.GetString(BackendUrlPreference, CodeUpdate.DefaultBackendUrl);
        minSize = new Vector2(560f, 380f);
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("构建并发布不可变 Release", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        using (new EditorGUI.DisabledScope(m_IsBusy))
        {
            string backend = EditorGUILayout.TextField("后端地址", m_BackendUrl);
            if (!string.Equals(backend, m_BackendUrl, StringComparison.Ordinal))
            {
                m_BackendUrl = backend;
                EditorPrefs.SetString(BackendUrlPreference, m_BackendUrl);
            }

            EditorGUILayout.LabelField("渠道", Channel);
            string platform = ContentReleasePackageBuilder.TryPlatformName(
                EditorUserBuildSettings.activeBuildTarget,
                out string platformName)
                ? platformName
                : "不支持：" + EditorUserBuildSettings.activeBuildTarget;
            EditorGUILayout.LabelField("平台", platform);
            EditorGUILayout.LabelField("App 版本", PlayerSettings.bundleVersion);
            m_ContentVersion = EditorGUILayout.TextField("内容版本（SemVer）", m_ContentVersion);
            EditorGUILayout.LabelField("备注");
            m_Notes = EditorGUILayout.TextArea(m_Notes, GUILayout.MinHeight(70f));

            m_MemoryPublishKey = PublishKeyProvider.DrawField(m_MemoryPublishKey);

            EditorGUILayout.Space();
            if (GUILayout.Button("构建并发布到 development", GUILayout.Height(36f)))
            {
                _ = BuildAndPublishAsync();
            }
        }

        EditorGUILayout.Space();
        Rect progressRect = EditorGUILayout.GetControlRect(false, 22f);
        EditorGUI.ProgressBar(progressRect, m_Progress, m_Status);
    }

    async Task BuildAndPublishAsync()
    {
        if (m_IsBusy)
        {
            return;
        }

        string publishKey = PublishKeyProvider.Resolve(m_MemoryPublishKey);
        try
        {
            ValidateInputs(publishKey);
            m_IsBusy = true;
            SetStatus(0.02f, "构建 Addressables…");
            AddressableAssetSettings.BuildPlayerContent(out AddressablesPlayerBuildResult addressablesResult);
            if (!string.IsNullOrEmpty(addressablesResult.Error))
            {
                throw new InvalidOperationException("Addressables 构建失败：" + addressablesResult.Error);
            }

            SetStatus(0.16f, "编译 HybridCLR DLL…");
            HybridCLRProjectSetup.CopyDlls();

            SetStatus(0.24f, "生成 Release ZIP 与 SHA-256 清单…");
            ContentReleasePackage package = ContentReleasePackageBuilder.Build(
                EditorUserBuildSettings.activeBuildTarget,
                PlayerSettings.bundleVersion,
                m_ContentVersion.Trim());

            SetStatus(0.30f, "创建 Release…");
            ReleaseResponse release = await CreateReleaseAsync(publishKey, package);

            SetStatus(0.35f, "上传 Release…");
            await UploadArtifactAsync(publishKey, release.id, package);

            SetStatus(0.92f, "读取当前 development 版本…");
            string currentReleaseId = await GetActiveReleaseIdAsync(publishKey, package);

            SetStatus(0.96f, "切换 development 版本…");
            await SetActiveReleaseAsync(publishKey, release.id, currentReleaseId, package);

            SetStatus(1f, "发布成功：" + m_ContentVersion.Trim());
            Debug.Log($"[ContentDelivery] Release {release.id} 已发布到 {Channel}。");
        }
        catch (Exception exception)
        {
            SetStatus(0f, "发布失败：" + exception.Message);
            Debug.LogException(exception);
            EditorUtility.DisplayDialog("内容发布失败", exception.Message, "关闭");
        }
        finally
        {
            m_IsBusy = false;
            Repaint();
        }
    }

    void ValidateInputs(string publishKey)
    {
        EditorBackendHttp.ValidateBaseUrl(m_BackendUrl);
        if (!ContentReleasePackageBuilder.IsStrictSemVer(m_ContentVersion.Trim()))
        {
            throw new InvalidOperationException("内容版本必须是 SemVer，例如 0.1.1 或 1.0.0-beta.1。");
        }

        if (string.IsNullOrWhiteSpace(PlayerSettings.bundleVersion))
        {
            throw new InvalidOperationException("PlayerSettings.bundleVersion 不能为空。");
        }

        if (string.IsNullOrWhiteSpace(publishKey))
        {
            throw new InvalidOperationException("请设置发布密钥。");
        }
    }

    async Task<ReleaseResponse> CreateReleaseAsync(string publishKey, ContentReleasePackage package)
    {
        var body = new CreateReleaseRequest
        {
            platform = package.Platform,
            appVersion = package.AppVersion,
            contentVersion = package.ContentVersion,
            notes = string.IsNullOrWhiteSpace(m_Notes) ? null : m_Notes.Trim()
        };
        string response = await EditorBackendHttp.SendJsonAsync(
            m_BackendUrl,
            "POST",
            "/api/content/releases",
            JsonUtility.ToJson(body),
            publishKey);
        return JsonUtility.FromJson<ReleaseResponse>(response);
    }

    async Task UploadArtifactAsync(string publishKey, string releaseId, ContentReleasePackage package)
    {
        using (var request = new UnityWebRequest(
            EditorBackendHttp.BuildUrl(m_BackendUrl, "/api/content/releases/" + releaseId + "/artifact"),
            UnityWebRequest.kHttpVerbPUT))
        {
            request.uploadHandler = new UploadHandlerFile(package.ZipPath);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/zip");
            request.SetRequestHeader("X-Artifact-Sha256", package.ArchiveSha256);
            request.SetRequestHeader(EditorBackendHttp.PublishKeyHeader, publishKey);

            await EditorBackendHttp.AwaitAsync(
                request,
                progress => SetStatus(0.35f + progress * 0.55f, "上传 Release…"));
            EditorBackendHttp.ThrowIfFailed(request);
        }
    }

    async Task<string> GetActiveReleaseIdAsync(string publishKey, ContentReleasePackage package)
    {
        string response = await EditorBackendHttp.GetAsync(m_BackendUrl, ActivePath(package), publishKey, allowNotFound: true);
        return response == null ? null : JsonUtility.FromJson<ActiveReleaseResponse>(response).releaseId;
    }

    Task SetActiveReleaseAsync(
        string publishKey,
        string releaseId,
        string expectedCurrentReleaseId,
        ContentReleasePackage package)
    {
        var body = new SetActiveReleaseRequest
        {
            releaseId = releaseId,
            expectedCurrentReleaseId = expectedCurrentReleaseId
        };
        return EditorBackendHttp.SendJsonAsync(m_BackendUrl, "PUT", ActivePath(package), JsonUtility.ToJson(body), publishKey);
    }

    static string ActivePath(ContentReleasePackage package)
    {
        return "/api/content/active-releases/" + Channel + "/" + package.Platform + "/"
            + UnityWebRequest.EscapeURL(package.AppVersion);
    }

    void SetStatus(float progress, string status)
    {
        m_Progress = Mathf.Clamp01(progress);
        m_Status = status;
        Repaint();
    }

    [Serializable]
    sealed class CreateReleaseRequest
    {
        public string platform;
        public string appVersion;
        public string contentVersion;
        public string notes;
    }

    [Serializable]
    sealed class SetActiveReleaseRequest
    {
        public string releaseId;
        public string expectedCurrentReleaseId;
    }

    [Serializable]
    sealed class ReleaseResponse
    {
        public string id = string.Empty;
    }

    [Serializable]
    sealed class ActiveReleaseResponse
    {
        public string releaseId = string.Empty;
    }
}

public sealed class ContentReleasePackage
{
    public string ZipPath { get; set; }
    public string ArchiveSha256 { get; set; }
    public string Platform { get; set; }
    public string AppVersion { get; set; }
    public string ContentVersion { get; set; }
}

public static class ContentReleasePackageBuilder
{
    const string HotUpdateRelativePath = "HybridCLR/HotUpdate.dll.bytes";
    static readonly Regex SemVerPattern = new Regex(
        @"^(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)(?:-((?:0|[1-9]\d*|\d*[A-Za-z-][0-9A-Za-z-]*)(?:\.(?:0|[1-9]\d*|\d*[A-Za-z-][0-9A-Za-z-]*))*))?(?:\+([0-9A-Za-z-]+(?:\.[0-9A-Za-z-]+)*))?$",
        RegexOptions.CultureInvariant);

    public static bool IsStrictSemVer(string value)
    {
        return !string.IsNullOrWhiteSpace(value) && SemVerPattern.IsMatch(value);
    }

    public static string PlatformName(BuildTarget target)
    {
        switch (target)
        {
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
                return "StandaloneWindows64";
            case BuildTarget.Android:
                return "Android";
            case BuildTarget.iOS:
                return "iOS";
            default:
                throw new NotSupportedException("内容发布不支持 BuildTarget：" + target);
        }
    }

    public static bool TryPlatformName(BuildTarget target, out string platform)
    {
        try
        {
            platform = PlatformName(target);
            return true;
        }
        catch (NotSupportedException)
        {
            platform = string.Empty;
            return false;
        }
    }

    public static ContentReleasePackage Build(BuildTarget target, string appVersion, string contentVersion)
    {
        string platform = PlatformName(target);
        string addressablesRoot = Path.GetFullPath(Path.Combine("ServerData", platform));
        string dllPath = Path.GetFullPath(Path.Combine(
            "Assets",
            "StreamingAssets",
            LoadDll.DllDir,
            LoadDll.HotUpdateFile));
        string packageDirectory = Path.GetFullPath(Path.Combine("Temp", "ContentRelease"));
        return BuildFromFiles(
            platform,
            appVersion,
            contentVersion,
            addressablesRoot,
            dllPath,
            packageDirectory);
    }

    public static ContentReleasePackage BuildFromFiles(
        string platform,
        string appVersion,
        string contentVersion,
        string addressablesRoot,
        string dllPath,
        string packageDirectory)
    {
        if (!new[] { "StandaloneWindows64", "Android", "iOS" }.Contains(platform))
        {
            throw new NotSupportedException("内容发布不支持平台：" + platform);
        }

        if (!IsStrictSemVer(contentVersion))
        {
            throw new InvalidOperationException("内容版本不是严格 SemVer。");
        }

        addressablesRoot = Path.GetFullPath(addressablesRoot);
        dllPath = Path.GetFullPath(dllPath);
        packageDirectory = Path.GetFullPath(packageDirectory);
        if (!Directory.Exists(addressablesRoot))
        {
            throw new DirectoryNotFoundException("Addressables 输出目录不存在：" + addressablesRoot);
        }

        if (!File.Exists(dllPath))
        {
            throw new FileNotFoundException("HotUpdate DLL 不存在。", dllPath);
        }

        string[] catalogs = Directory.GetFiles(addressablesRoot, "catalog_*.bin", SearchOption.TopDirectoryOnly);
        if (catalogs.Length != 1)
        {
            throw new InvalidOperationException("Addressables 输出必须且只能包含一个 catalog_*.bin。");
        }

        string catalogHash = Path.ChangeExtension(catalogs[0], ".hash");
        if (!File.Exists(catalogHash))
        {
            throw new FileNotFoundException("Catalog hash 不存在。", catalogHash);
        }

        string[] bundles = Directory.GetFiles(addressablesRoot, "*.bundle", SearchOption.AllDirectories);
        if (bundles.Length == 0)
        {
            throw new InvalidOperationException("Addressables 输出不包含任何 .bundle。");
        }

        var sources = new List<PackageSource>
        {
            new PackageSource(dllPath, HotUpdateRelativePath),
            new PackageSource(catalogs[0], "Addressables/" + Path.GetFileName(catalogs[0])),
            new PackageSource(catalogHash, "Addressables/" + Path.GetFileName(catalogHash))
        };
        sources.AddRange(bundles.Select(path => new PackageSource(
            path,
            "Addressables/" + RelativePath(addressablesRoot, path))));
        sources = sources.OrderBy(value => value.RelativePath, StringComparer.Ordinal).ToList();

        var manifest = new ReleaseManifest
        {
            schemaVersion = 1,
            platform = platform,
            appVersion = appVersion,
            contentVersion = contentVersion,
            hotUpdatePath = HotUpdateRelativePath,
            catalogPath = "Addressables/" + Path.GetFileName(catalogs[0]),
            catalogHashPath = "Addressables/" + Path.GetFileName(catalogHash),
            files = sources.Select(source => new ReleaseFile
            {
                path = source.RelativePath,
                size = new FileInfo(source.SourcePath).Length,
                sha256 = Sha256OfFile(source.SourcePath)
            }).ToArray()
        };

        Directory.CreateDirectory(packageDirectory);
        string zipPath = Path.Combine(
            packageDirectory,
            platform + "-" + appVersion + "-" + contentVersion + ".zip");
        if (File.Exists(zipPath))
        {
            File.Delete(zipPath);
        }

        using (ZipArchive archive = ZipFile.Open(zipPath, ZipArchiveMode.Create))
        {
            foreach (PackageSource source in sources)
            {
                archive.CreateEntryFromFile(source.SourcePath, source.RelativePath, System.IO.Compression.CompressionLevel.Optimal);
            }

            ZipArchiveEntry manifestEntry = archive.CreateEntry("release-manifest.json", System.IO.Compression.CompressionLevel.Optimal);
            using (Stream stream = manifestEntry.Open())
            using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
            {
                writer.Write(JsonUtility.ToJson(manifest, true));
            }
        }

        return new ContentReleasePackage
        {
            ZipPath = zipPath,
            ArchiveSha256 = Sha256OfFile(zipPath),
            Platform = platform,
            AppVersion = appVersion,
            ContentVersion = contentVersion
        };
    }

    public static string Sha256OfFile(string path)
    {
        using (FileStream stream = File.OpenRead(path))
        using (SHA256 sha256 = SHA256.Create())
        {
            return ToLowerHex(sha256.ComputeHash(stream));
        }
    }

    static string RelativePath(string root, string path)
    {
        Uri rootUri = new Uri(root.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar);
        string value = Uri.UnescapeDataString(rootUri.MakeRelativeUri(new Uri(path)).ToString());
        return value.Replace('\\', '/');
    }

    static string ToLowerHex(byte[] bytes)
    {
        var builder = new StringBuilder(bytes.Length * 2);
        foreach (byte value in bytes)
        {
            builder.Append(value.ToString("x2"));
        }

        return builder.ToString();
    }

    sealed class PackageSource
    {
        public PackageSource(string sourcePath, string relativePath)
        {
            SourcePath = sourcePath;
            RelativePath = relativePath.Replace('\\', '/');
        }

        public string SourcePath { get; }
        public string RelativePath { get; }
    }

    [Serializable]
    sealed class ReleaseManifest
    {
        public int schemaVersion;
        public string platform;
        public string appVersion;
        public string contentVersion;
        public string hotUpdatePath;
        public string catalogPath;
        public string catalogHashPath;
        public ReleaseFile[] files;
    }

    [Serializable]
    sealed class ReleaseFile
    {
        public string path;
        public long size;
        public string sha256;
    }
}
