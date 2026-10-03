using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AChen.Configuration;
using HybridCLR.Editor.Commands;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;
using UnityEngine.Networking;

[InitializeOnLoad]
public static class MBulidPipeline
{
    [Serializable] sealed class Selection { public string android; public string windows; }
    [Serializable] sealed class Operation { public string platform; public string version; public string action; public string status; public string message; }
    const string BusyKey = "TCG.mBulid.Busy";
    static string SelectionPath => EditorPaths.FromProjectRoot("UserSettings", "mBulid.json");
    static string StatusPath => EditorPaths.FromProjectRoot("Temp", "mBulid", "status.json");
    static MBulidPackage Package => new MBulidPackage(EditorPaths.ProjectRoot);
    public static bool Busy => SessionState.GetBool(BusyKey, false);
    public static string Status { get; private set; } = "选择 mBulid 操作";
    public static bool Failed { get; private set; }
    public static bool Supported => EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android ||
        EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows64;
    public static bool CanRun => Supported && !Busy && !BuildPipeline.isBuildingPlayer && !EditorApplication.isPlayingOrWillChangePlaymode &&
        !EditorApplication.isCompiling && !EditorApplication.isUpdating;
    public static string BackendUrl => (Environment.GetEnvironmentVariable("ACHEN_BACKEND_URL") ?? CodeUpdate.DefaultBackendUrl).TrimEnd('/');

    static MBulidPipeline()
    {
        if (Busy)
        {
            SessionState.SetBool(BusyKey, false);
            Failed = true;
            Status = "mBulid 操作被脚本重载中断, 请检查构建产物或后端状态; 不会自动重复执行";
            Directory.CreateDirectory(Path.GetDirectoryName(StatusPath));
            File.WriteAllText(StatusPath, JsonUtility.ToJson(new Operation { status = "interrupted", message = Status }, true));
        }
    }
    static Selection ReadSelection() => File.Exists(SelectionPath) ? JsonUtility.FromJson<Selection>(File.ReadAllText(SelectionPath)) ?? new Selection() : new Selection();
    public static string SelectedVersion(BuildTarget target)
    {
        var selection = ReadSelection();
        return target == BuildTarget.Android ? selection.android : target == BuildTarget.StandaloneWindows64 ? selection.windows : null;
    }
    public static bool HasVersion
    {
        get
        {
            string version = SelectedVersion(EditorUserBuildSettings.activeBuildTarget);
            return Supported && DevelopmentProtocol.ValidContentVersion(version) && Directory.Exists(Package.DirectoryFor(EditorUserBuildSettings.activeBuildTarget.ToString(), version));
        }
    }
    public static string OutputDirectory => HasVersion ? Package.DirectoryFor(EditorUserBuildSettings.activeBuildTarget.ToString(), SelectedVersion(EditorUserBuildSettings.activeBuildTarget)) : "尚未设置版本号";
    public static void SetVersion(string version)
    {
        RequireIdle();
        BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
        version = version?.Trim();
        Package.ResetVersion(target.ToString(), version);
        var selection = ReadSelection();
        if (target == BuildTarget.Android) selection.android = version; else selection.windows = version;
        Directory.CreateDirectory(Path.GetDirectoryName(SelectionPath));
        File.WriteAllText(SelectionPath, JsonUtility.ToJson(selection, true));
        Report(target, version, "设置版本号", "completed", "版本目录已创建: " + OutputDirectory);
    }
    static void RequireIdle()
    {
        if (!CanRun) throw new InvalidOperationException("请在 Android 或 Windows x64 平台的编辑模式下等待编译和其他 mBulid 操作完成");
    }
    static (BuildTarget target, string version) Begin(string action)
    {
        RequireIdle();
        if (!HasVersion) throw new InvalidOperationException("请先设置当前平台的版本号");
        var target = EditorUserBuildSettings.activeBuildTarget;
        string version = SelectedVersion(target);
        SessionState.SetBool(BusyKey, true);
        Report(target, version, action, "running", action + ": " + version + " / " + target);
        return (target, version);
    }
    static void Report(BuildTarget target, string version, string action, string status, string message)
    {
        Status = message; Failed = status == "failed" || status == "interrupted";
        Directory.CreateDirectory(Path.GetDirectoryName(StatusPath));
        File.WriteAllText(StatusPath, JsonUtility.ToJson(new Operation { platform = target.ToString(), version = version, action = action, status = status, message = message }, true));
        if (Failed) ALog.LogError("mBulid: " + message, ALogCategories.Net);
        else ALog.Log("mBulid: " + message, ALogCategories.Net);
    }
    static void Run(string action, Action<BuildTarget, string> execute)
    {
        (BuildTarget target, string version) operation;
        try { operation = Begin(action); }
        catch (Exception ex) { Debug.LogError("mBulid: " + ex.Message); return; }
        try
        {
            execute(operation.target, operation.version);
            Report(operation.target, operation.version, action, "completed", action + "完成: " + Package.DirectoryFor(operation.target.ToString(), operation.version));
        }
        catch (Exception ex) { Report(operation.target, operation.version, action, "failed", ex.Message); }
        finally { SessionState.SetBool(BusyKey, false); }
    }
    public static void BuildCode() => Run("构建 DLL", (target, version) =>
    {
        Package.BeginCode(target.ToString(), version);
        string temporary = EditorPaths.FromProjectRoot("Temp", "mBulid", Guid.NewGuid().ToString("N"), "Compile");
        var errors = new ConcurrentQueue<string>();
        Application.LogCallback capture = (message, stack, type) => { if (type == LogType.Error || type == LogType.Exception) errors.Enqueue(message); };
        Application.logMessageReceivedThreaded += capture;
        try { CompileDllCommand.CompileDll(temporary, target, EditorUserBuildSettings.development); }
        finally { Application.logMessageReceivedThreaded -= capture; }
        if (!errors.IsEmpty) throw new InvalidOperationException("DLL 编译报错, 未登记为完成: " + string.Join("\n", errors));
        Package.CompleteCode(target.ToString(), version, Path.Combine(temporary, "HotUpdate.dll"));
    });
    public static void BuildResources() => Run("构建 Addressables", (target, version) =>
    {
        Package.BeginResources(target.ToString(), version);
        PublishedConfigBuilder.Prepare();
        UiAtlasPrebuild.Prepare();
        var settings = AddressableAssetSettingsDefaultObject.Settings ?? throw new InvalidOperationException("缺少 Addressables 设置");
        var sharedGroup = settings.GetSharedBundleGroup();
        var sharedSchema = sharedGroup != null ? sharedGroup.GetSchema<BundledAssetGroupSchema>() : null;
        if (sharedSchema == null || sharedSchema.BuildPath.GetName(settings) != "Remote.BuildPath" ||
            sharedSchema.LoadPath.GetName(settings) != "Remote.LoadPath")
            throw new InvalidOperationException("Addressables 公共包必须使用 Remote.BuildPath / Remote.LoadPath。请将 Shared Bundle Settings 指向 Remote_Shared，否则 monoscripts 等依赖会错误地从 APK 读取。");
        string profile = settings.activeProfileId;
        string previousPath = settings.profileSettings.GetValueByName(profile, "Remote.BuildPath");
        string previousVersion = settings.OverridePlayerVersion;
        int previousBuilder = settings.ActivePlayerDataBuilderIndex;
        int builder = settings.DataBuilders.FindIndex(x => x is BuildScriptPackedMode);
        if (builder < 0) throw new InvalidOperationException("缺少 Addressables Packed Mode 构建器");
        try
        {
            settings.profileSettings.SetValue(profile, "Remote.BuildPath", Package.DirectoryFor(target.ToString(), version).Replace('\\', '/') + "/Addressables");
            settings.OverridePlayerVersion = version;
            settings.ActivePlayerDataBuilderIndex = builder;
            UnityEditor.AddressableAssets.Settings.AddressableAssetSettings.BuildPlayerContent(out AddressablesPlayerBuildResult result);
            if (result == null || !string.IsNullOrEmpty(result.Error)) throw new InvalidOperationException("Addressables 构建失败: " + result?.Error);
            Package.CompleteResources(target.ToString(), version, result.FileRegistry.GetFilePaths(), EditorPaths.FromProjectRoot(PublishedConfigBuilder.Root));
        }
        finally
        {
            settings.profileSettings.SetValue(profile, "Remote.BuildPath", previousPath);
            settings.OverridePlayerVersion = previousVersion;
            settings.ActivePlayerDataBuilderIndex = previousBuilder;
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssetIfDirty(settings);
        }
    });
    public static async Task PublishAsync()
    {
        (BuildTarget target, string version) operation;
        try { operation = Begin("发布版本"); }
        catch (Exception ex) { Debug.LogError("mBulid: " + ex.Message); return; }
        try
        {
            string key = PublishKeyProvider.Resolve("");
            var backend = new Uri(BackendUrl);
            if (backend.Scheme != "http" && backend.Scheme != "https") throw new FormatException("后端地址必须是 HTTP/HTTPS 地址");
            string zip = Package.CreateArchive(operation.target.ToString(), operation.version, out var expected);
            using (var check = UnityWebRequest.Get(BackendUrl + "/api/dev/status"))
            {
                if (!string.IsNullOrWhiteSpace(key)) check.SetRequestHeader("X-Content-Publish-Key", key);
                check.timeout = 15;
                await Send(check);
                var identity = JsonUtility.FromJson<BackendIdentity>(check.downloadHandler.text);
                if (identity == null || identity.project != DevelopmentProtocol.Project || identity.protocol != DevelopmentProtocol.Version)
                    throw new InvalidOperationException("后端项目或协议不匹配, 未提交发布");
            }
            if (EditorUserBuildSettings.activeBuildTarget != operation.target) throw new InvalidOperationException("发布准备期间平台已改变, 未提交发布");
            using (var request = new UnityWebRequest(BackendUrl + "/api/dev/content/" + operation.target, "PUT"))
            {
                request.downloadHandler = new DownloadHandlerBuffer(); request.uploadHandler = new UploadHandlerFile(zip);
                request.SetRequestHeader("Content-Type", "application/zip");
                if (!string.IsNullOrWhiteSpace(key)) request.SetRequestHeader("X-Content-Publish-Key", key);
                request.SetRequestHeader("X-Artifact-Sha256", MBulidPackage.HashFile(zip)); request.timeout = 600;
                Report(operation.target, operation.version, "发布版本", "running", "正在发布 " + operation.version + "; 后端将先删除此平台旧版本");
                try { await Send(request); }
                catch (Exception ex)
                {
                    throw new IOException(request.responseCode == 0 ? "上传结果不确定, 请查询后端最新版本后再决定是否重试; 不会自动重发. " + ex.Message :
                        "发布失败, 该平台旧版本可能已删除; 请检查后端状态. " + ex.Message);
                }
                var published = JsonUtility.FromJson<DevelopmentManifest>(request.downloadHandler.text);
                if (published == null || published.schemaVersion != DevelopmentProtocol.Version || published.platform != expected.platform ||
                    published.contentVersion != expected.contentVersion || !Guid.TryParse(published.contentId, out _) ||
                    published.files == null || published.files.Length != expected.files.Length ||
                    !expected.files.All(file => published.files.Any(x => x.path == file.path && x.size == file.size && string.Equals(x.sha256, file.sha256, StringComparison.OrdinalIgnoreCase))))
                    throw new IOException("发布回执不一致, 请查询后端状态; 不会自动重发");
                File.WriteAllText(Path.Combine(Path.GetDirectoryName(zip), "published.json"), request.downloadHandler.text);
                Report(operation.target, operation.version, "发布版本", "completed", "发布完成: " + operation.version + " / " + operation.target + "; Content=" + published.contentId);
            }
        }
        catch (Exception ex) { Report(operation.target, operation.version, "发布版本", "failed", ex.Message); }
        finally { SessionState.SetBool(BusyKey, false); }
    }
    [Serializable] sealed class BackendIdentity { public string project; public int protocol; }
    static async Task Send(UnityWebRequest request)
    {
        if (request.downloadHandler == null) request.downloadHandler = new DownloadHandlerBuffer();
        var operation = request.SendWebRequest();
        while (!operation.isDone) await Task.Delay(50);
        if (request.result != UnityWebRequest.Result.Success) throw new IOException("HTTP " + request.responseCode + ": " + request.error + "; " + request.downloadHandler.text);
    }
}
