using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AChen.Configuration;
using HybridCLR.Editor;
using HybridCLR.Editor.Commands;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.Networking;

public sealed class DevelopmentWorkbench : EditorWindow
{
    Vector2 scroll;
    bool details;
    [MenuItem("Window/TCG/开发工作台")]
    [MenuItem("Tools/开发工作台")]
    public static void Open() => GetWindow<DevelopmentWorkbench>("开发工作台");
    void OnInspectorUpdate() => Repaint();
    void OnGUI()
    {
        EditorGUILayout.LabelField("当前构建平台", EditorUserBuildSettings.activeBuildTarget.ToString());
        EditorGUILayout.HelpBox(DevelopmentWorkflow.Status, DevelopmentWorkflow.Failed ? MessageType.Error : MessageType.Info);
        using (new EditorGUI.DisabledScope(DevelopmentWorkflow.Busy || DevelopmentWorkflow.HasPendingBuild ||
            EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating))
        {
            if (GUILayout.Button("编辑器运行")) _ = DevelopmentWorkflow.RunEditor();
            EditorGUILayout.Space();
            if (GUILayout.Button("生成完整包 (当前平台)", GUILayout.Height(32))) _ = DevelopmentWorkflow.Run(DevelopmentTask.Player);
            if (GUILayout.Button("打包热更代码")) _ = DevelopmentWorkflow.Run(DevelopmentTask.Code);
            if (GUILayout.Button("打包资源")) _ = DevelopmentWorkflow.Run(DevelopmentTask.Resources);
            if (GUILayout.Button("上传热更内容", GUILayout.Height(32))) _ = DevelopmentWorkflow.Run(DevelopmentTask.Upload);
            if (GUILayout.Button("打开当前平台输出目录"))
            {
                string path = DevelopmentWorkflow.OutputRoot(EditorUserBuildSettings.activeBuildTarget);
                Directory.CreateDirectory(path);
                EditorUtility.RevealInFinder(Path.GetFullPath(path));
            }
        }
        EditorGUILayout.HelpBox("完整包: Windows EXE / Android APK. 打包仅保存在本机; 上传才替换服务器上的代码和资源. 不自动安装或启动游戏.", MessageType.None);
        details = EditorGUILayout.Foldout(details, "详细日志");
        if (details)
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);
            foreach (string line in DevelopmentWorkflow.Logs) EditorGUILayout.LabelField(line, EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndScrollView();
        }
    }
}

public enum DevelopmentTask { Player, Code, Resources, Upload }

[InitializeOnLoad]
public static class DevelopmentWorkflow
{
    const string DirectoryPath = "Library/Development";
    const string Pending = "AChen.Development.PendingPlayer";
    const string PendingPasses = Pending + ".Passes";
    const string PendingGenerated = Pending + ".Generated";
    const string Backend = CodeUpdate.DefaultBackendUrl;
    public static bool Busy { get; private set; }
    public static bool Failed { get; private set; }
    public static bool HasPendingBuild => SessionState.GetInt(Pending, -1) != -1;
    public static string Status { get; private set; } = "选择当前平台的打包或上传操作";
    static bool allowPlay;
    static string statePath;
    static readonly List<string> log = new List<string>();
    public static string[] Logs => log.ToArray();
    [Serializable] sealed class State
    {
        public string config, hot, hotOutput, resources, apk, bridge, compatibility, generated, content, editorContent;
    }
    [Serializable] sealed class ServerStatus { public string project; public int protocol; public TargetStatus[] targets; }
    [Serializable] sealed class TargetStatus { public string target; public string contentId; }
    static State state;
    public static string OutputRoot(BuildTarget target) => DirectoryPath + "/" + target;
    static string PlayerPath(BuildTarget target) => OutputRoot(target) + "/Player/" +
        (target == BuildTarget.Android ? "TCGCardDem0.apk" : "TCGCardDem0.exe");
    static string CodePath(BuildTarget target) => OutputRoot(target) + "/Code/HotUpdate.dll.bytes";
    static string CompiledDll(BuildTarget target) => Path.Combine(SettingsUtil.GetHotUpdateDllsOutputDirByTarget(target), "HotUpdate.dll");
    static NamedBuildTarget NamedTarget(BuildTarget target) => target == BuildTarget.Android ? NamedBuildTarget.Android : NamedBuildTarget.Standalone;

    static DevelopmentWorkflow()
    {
        // 旧的一键安装任务不在新版工作台自动恢复.
        SessionState.SetBool("AChen.Development.PendingAndroid", false);
        EditorApplication.playModeStateChanged += change =>
        {
            if (change == PlayModeStateChange.ExitingEditMode && !allowPlay)
            {
                EditorApplication.isPlaying = false;
                EditorApplication.delayCall += () => { if (!Busy && !HasPendingBuild) _ = RunEditor(); };
            }
            if (change == PlayModeStateChange.EnteredPlayMode || change == PlayModeStateChange.EnteredEditMode) allowPlay = false;
        };
        EditorApplication.update += () =>
        {
            if (HasPendingBuild && !Busy && !EditorApplication.isCompiling && !EditorApplication.isUpdating &&
                !EditorApplication.isPlayingOrWillChangePlaymode)
            {
                if (SessionState.GetInt(Pending, -1) != (int)EditorUserBuildSettings.activeBuildTarget)
                {
                    Failure(new InvalidOperationException("构建平台已改变, 已停止原平台构建, 请重新点击"));
                }
                else _ = Run(DevelopmentTask.Player);
            }
        };
    }
    static void AddLog(string line) { log.Add(line); if (log.Count > 200) log.RemoveAt(0); }
    static void Stage(string value)
    {
        Status = value; AddLog(value);
        ALog.Log("开发流程. Stage=" + value, ALogCategories.Net);
    }
    static void Load(string path)
    {
        statePath = path;
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        state = File.Exists(path) ? JsonUtility.FromJson<State>(File.ReadAllText(path)) : new State();
        state = state ?? new State();
    }
    static void Save() => File.WriteAllText(statePath, JsonUtility.ToJson(state, true));
    static void ClearPendingPlayer()
    {
        SessionState.EraseInt(Pending);
        SessionState.EraseInt(PendingPasses);
        SessionState.EraseBool(PendingGenerated);
    }
    static void Failure(Exception ex)
    {
        ClearPendingPlayer(); Failed = true; Status = ex.Message; AddLog(ex.ToString());
        ALog.LogError("开发流程失败. Target=" + EditorUserBuildSettings.activeBuildTarget + "; Error=" + ex.Message, ALogCategories.Net);
        DevelopmentWorkbench.Open();
    }
    static async Task<string> Http(string method, string path, string zip = null)
    {
        using (var request = new UnityWebRequest(Backend + path, method))
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            request.timeout = zip == null ? 15 : 600;
            var key = PublishKeyProvider.Resolve("");
            if (!string.IsNullOrEmpty(key)) request.SetRequestHeader("X-Content-Publish-Key", key);
            if (zip != null)
            {
                request.uploadHandler = new UploadHandlerFile(Path.GetFullPath(zip));
                request.SetRequestHeader("Content-Type", "application/zip");
                request.SetRequestHeader("X-Artifact-Sha256", DevelopmentPackage.HashFile(zip));
            }
            var operation = request.SendWebRequest();
            while (!operation.isDone) await Task.Delay(50);
            if (request.result != UnityWebRequest.Result.Success)
                throw new IOException("后端 " + path + " 失败: HTTP " + request.responseCode + "; " + request.downloadHandler.text + "; " + request.error);
            return request.downloadHandler.text;
        }
    }
    static async Task<ServerStatus> EnsureBackend()
    {
        Stage("检查后端源码与服务");
        string fingerprint = await Task.Run(DevelopmentInputs.Backend);
        BackendServiceController.Refresh();
        if (BackendServiceController.State == BackendServiceState.External)
        {
            // 进程是否由当前窗口启动, 与它能否提供本项目服务是两回事.
            try
            {
                var existing = JsonUtility.FromJson<ServerStatus>(await Http("GET", "/api/dev/status"));
                ValidateBackend(existing);
                Stage("已复用本项目现有后端服务");
                return existing;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("5080 已有服务, 但本项目身份/协议/发布鉴权检查未通过. 未停止该进程. 详情: " + ex.Message, ex);
            }
        }
        bool changed = !File.Exists(DirectoryPath + "/backend.fingerprint") || File.ReadAllText(DirectoryPath + "/backend.fingerprint") != fingerprint;
        if (changed && BackendServiceController.CanStop) BackendServiceController.Stop();
        if (BackendServiceController.CanStart)
            BackendServiceController.Start(changed);
        var deadline = DateTime.UtcNow.AddMinutes(5);
        while (BackendServiceController.State != BackendServiceState.Running)
        {
            if (BackendServiceController.State == BackendServiceState.External)
                throw new InvalidOperationException("5080 被外部进程占用, 请手动关闭该进程后重试");
            if (BackendServiceController.State == BackendServiceState.Faulted)
                throw new InvalidOperationException(BackendServiceController.LastError + "\n" + string.Join("\n", BackendServiceController.LogLines.Skip(Math.Max(0, BackendServiceController.LogLines.Count - 12))));
            if (DateTime.UtcNow > deadline) throw new TimeoutException("后端准备超时");
            await Task.Delay(200);
        }
        var status = JsonUtility.FromJson<ServerStatus>(await Http("GET", "/api/dev/status"));
        ValidateBackend(status);
        File.WriteAllText(DirectoryPath + "/backend.fingerprint", fingerprint);
        return status;
    }
    static void ValidateBackend(ServerStatus status)
    {
        if (status == null || status.project != DevelopmentProtocol.Project || status.protocol != DevelopmentProtocol.Version || status.targets == null)
            throw new InvalidOperationException("后端项目身份或协议不匹配, 请更新本项目后端");
    }
    static void PrepareConfig()
    {
        string fingerprint = DevelopmentInputs.Files(DevelopmentInputs.Under(PublishedConfigBuilder.SourceRoot)
            .Concat(new[] { "Assets/Editor/GameConfig/PublishedConfigBuilder.cs" }).Concat(DevelopmentInputs.Under("Assets/Shared/Configuration")));
        if (fingerprint != state.config || !Directory.Exists(PublishedConfigBuilder.Root) ||
            GameConfigTables.Required.Any(x => !File.Exists(Path.Combine(PublishedConfigBuilder.Root, GameConfigTables.FileName(x)))))
        {
            Stage("生成并校验最新配置");
            PublishedConfigBuilder.Prepare();
            state.config = fingerprint; Save();
        }
    }
    public static async Task RunEditor()
    {
        if (Busy || HasPendingBuild) return;
        Busy = true; Failed = false;
        try
        {
            Load(DirectoryPath + "/state.json");
            var server = await EnsureBackend();
            PrepareConfig();
            var settings = UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject.Settings;
            int localBuilder = settings.DataBuilders.FindIndex(x => x is UnityEditor.AddressableAssets.Build.DataBuilders.BuildScriptFastMode);
            if (localBuilder < 0) throw new InvalidOperationException("缺少 Addressables 本地资源播放模式");
            settings.ActivePlayModeDataBuilderIndex = localBuilder;
            string zip = DevelopmentPackage.WriteEditor(out var manifest);
            var current = server.targets.FirstOrDefault(x => x.target == "Editor");
            string signature = manifest.configHash;
            string cached = DirectoryPath + "/editor-session.json";
            if (state.editorContent != signature || current == null || !File.Exists(cached) ||
                JsonUtility.FromJson<DevelopmentManifest>(File.ReadAllText(cached)).contentId != current.contentId)
            {
                Stage("同步编辑器配置, 保持 Windows/Android 发布不变");
                File.WriteAllText(cached, await Http("PUT", "/api/dev/editor-config", zip));
                state.editorContent = signature; Save();
            }
            Stage("编辑器配置已就绪");
            UnityEditor.SceneManagement.EditorSceneManager.playModeStartScene =
                AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Scenes/PreInit.unity");
            allowPlay = true; EditorApplication.isPlaying = true;
            return;
        }
        catch (Exception ex) { Failure(ex); }
        finally { Busy = false; }
    }
    public static async Task Run(DevelopmentTask task)
    {
        if (Busy) return;
        Busy = true; Failed = false;
        BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
        try
        {
            if (target != BuildTarget.Android && target != BuildTarget.StandaloneWindows64)
                throw new InvalidOperationException("请在 Unity Build Profiles 切换到 Android 或 Windows x64, 工作台跟随当前平台");
            Load(OutputRoot(target) + "/state.json");
            if (task != DevelopmentTask.Upload && !BuildPipeline.IsBuildTargetSupported(BuildPipeline.GetBuildTargetGroup(target), target))
                throw new InvalidOperationException("缺少 " + target + " 构建模块, 请在 Unity Hub 为当前编辑器安装");
            Stage(task + ": " + target);
            switch (task)
            {
                case DevelopmentTask.Code:
                    BuildCode(target);
                    Stage("热更代码已保存: " + CodePath(target));
                    break;
                case DevelopmentTask.Resources:
                    BuildResources(target);
                    Stage("资源已保存: " + OutputRoot(target) + "/Resources");
                    break;
                case DevelopmentTask.Player:
                    if (!HasPendingBuild) ClearPendingPlayer();
                    SessionState.SetInt(Pending, (int)target);
                    int passes = SessionState.GetInt(PendingPasses, 0) + 1;
                    SessionState.SetInt(PendingPasses, passes);
                    // 正常最多三轮: 切换 IL2CPP, 生成桥接代码, 构建 Player. 计数跨脚本重载保留.
                    if (passes > 3)
                        throw new InvalidOperationException("完整包准备连续重入, 已停止自动构建. 请检查 IL2CPP 设置及 HybridCLR 生成状态后重新点击");
                    if (!BuildPlayer(target)) return;
                    ClearPendingPlayer();
                    Stage("完整包已生成: " + PlayerPath(target) + "; 安装/分发整个 Player 目录, 热更内容需单独上传");
                    break;
                case DevelopmentTask.Upload:
                    await Upload(target);
                    break;
            }
        }
        catch (Exception ex) { Failure(ex); }
        finally { Busy = false; }
    }
    static void BuildCode(BuildTarget target)
    {
        string fingerprint = DevelopmentInputs.Hot();
        string path = CodePath(target);
        if (state.hot == fingerprint && File.Exists(path) && state.hotOutput == DevelopmentPackage.HashFile(path)) return;
        Stage("编译 " + target + " 热更代码");
        CompileDllCommand.CompileDll(target, true);
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.Copy(CompiledDll(target), path, true);
        state.hot = fingerprint; state.hotOutput = DevelopmentPackage.HashFile(path); Save();
    }
    static void BuildResources(BuildTarget target)
    {
        PrepareConfig();
        string fingerprint = DevelopmentInputs.Resources();
        string root = OutputRoot(target);
        if (state.resources == fingerprint && DevelopmentPackage.HasResourceSnapshot(root, target.ToString())) return;
        Stage("构建 " + target + " Addressables 资源");
        AddressableAssetSettings.BuildPlayerContent(out AddressablesPlayerBuildResult result);
        if (!string.IsNullOrEmpty(result.Error)) throw new InvalidOperationException(result.Error);
        // 使用本次构建的文件列表, 不把输出目录中的旧 catalog 混入发布包.
        DevelopmentPackage.SaveResourceSnapshot(target.ToString(), root, result.FileRegistry.GetFilePaths());
        state.resources = DevelopmentInputs.Resources(); Save();
    }
    static string Bridge(string dll)
    {
        try { return DevelopmentInputs.BridgeRequirements(dll); }
        catch (Exception ex)
        {
            Stage("无法确认 AOT 兼容性, 代码变化后需重新生成完整包: " + ex.Message);
            return "unverified:" + DevelopmentPackage.HashFile(dll);
        }
    }
    static bool BuildPlayer(BuildTarget target)
    {
        if (target == BuildTarget.Android) CheckAndroidTools();
        var named = NamedTarget(target);
        if (PlayerSettings.GetScriptingBackend(named) != ScriptingImplementation.IL2CPP)
        {
            Stage("设置 " + target + " IL2CPP, 等待脚本重载后继续");
            PlayerSettings.SetScriptingBackend(named, ScriptingImplementation.IL2CPP);
            return false;
        }
        if (target == BuildTarget.Android)
        {
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.allowedAutorotateToPortrait = false; PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true; PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            EditorUserBuildSettings.buildAppBundle = false;
            EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
        }
        PlayerSettings.insecureHttpOption = InsecureHttpOption.DevelopmentOnly;
        EditorUserBuildSettings.development = true;
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene("Assets/Scenes/PreInit.unity", true) };
        var settings = UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject.Settings;
        settings.BuildAddressablesWithPlayerBuild = AddressableAssetSettings.PlayerBuildOption.DoNotBuildWithPlayer;
        EditorUtility.SetDirty(settings); AssetDatabase.SaveAssets();
        BuildCode(target);
        // 首次完整包需要 Addressables 启动数据; 此处仅准备本地产物, 不上传.
        BuildResources(target);
        string bridge = Bridge(CodePath(target));
        // 生成产物在导入后可能变化, 不应反过来触发再次生成.
        string generation = DevelopmentInputs.PlayerSource() + bridge;
        string generatedTarget = DirectoryPath + "/generated-platform.txt";
        if (state.generated != generation || !File.Exists(generatedTarget) || File.ReadAllText(generatedTarget) != target.ToString() ||
            !Directory.Exists(SettingsUtil.GetAssembliesPostIl2CppStripDir(target)))
        {
            if (SessionState.GetBool(PendingGenerated, false))
                throw new InvalidOperationException("HybridCLR 生成后输入或平台产物仍不一致, 已停止自动构建. Target=" + target + "; Expected=" + state.generated + "; Actual=" + generation);
            SessionState.SetBool(PendingGenerated, true);
            Stage("准备 " + target + " HybridCLR AOT 和桥接代码");
            HybridCLRProjectSetup.InstallRuntime();
            HybridCLRProjectSetup.WorkbenchOwnsBuild = true;
            try
            {
                PrebuildCommand.GenerateAll();
                HybridCLRProjectSetup.CopyCompiledDlls();
                File.Copy(CompiledDll(target), CodePath(target), true);
                state.hotOutput = DevelopmentPackage.HashFile(CodePath(target));
                state.generated = DevelopmentInputs.PlayerSource() + Bridge(CodePath(target)); Save();
                File.WriteAllText(generatedTarget, target.ToString());
            }
            finally { HybridCLRProjectSetup.WorkbenchOwnsBuild = false; }
            Stage("平台代码已生成, 等待 Unity 导入后继续生成完整包");
            return false;
        }
        // 重载后 AOT 清单才是新生成的类型, 再按当前平台完整复制一次元数据.
        HybridCLRProjectSetup.CopyCompiledDlls(true);
        string input = DevelopmentInputs.Apk();
        string compatibility = CodeUpdate.Sha256Of(Encoding.UTF8.GetBytes(target + ":" + input + bridge));
        Directory.CreateDirectory("Assets/StreamingAssets");
        File.WriteAllText("Assets/StreamingAssets/development-apk.txt", compatibility);
        AssetDatabase.ImportAsset("Assets/StreamingAssets/development-apk.txt");
        Directory.CreateDirectory(Path.GetDirectoryName(PlayerPath(target)));
        // 最终构建只执行一次; 进入前消费恢复标记, 防止中断或重载后自动再次打包.
        ClearPendingPlayer();
        HybridCLRProjectSetup.WorkbenchOwnsBuild = true;
        try
        {
            Stage("生成 " + target + " 完整游戏包");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { "Assets/Scenes/PreInit.unity" }, target = target,
                locationPathName = PlayerPath(target), options = BuildOptions.Development | BuildOptions.AllowDebugging
            });
            if (report.summary.result != BuildResult.Succeeded) throw new InvalidOperationException("完整包构建未成功, 未上传任何内容");
            state.apk = DevelopmentInputs.PlayerSource(); state.bridge = bridge; state.compatibility = compatibility; Save();
        }
        finally { HybridCLRProjectSetup.WorkbenchOwnsBuild = false; }
        return true;
    }
    static async Task Upload(BuildTarget target)
    {
        string root = OutputRoot(target);
        if (!File.Exists(CodePath(target))) throw new InvalidOperationException("当前平台没有热更代码, 请先点击打包热更代码");
        if (!DevelopmentPackage.HasResourceSnapshot(root, target.ToString()))
            throw new InvalidOperationException("当前平台没有完整资源产物或文件已变化, 请先点击打包资源");
        if (string.IsNullOrEmpty(state.compatibility)) throw new InvalidOperationException("当前平台尚未生成完整包, 请先点击生成完整包");
        if (state.bridge != Bridge(CodePath(target)) || state.apk != DevelopmentInputs.PlayerSource())
            throw new InvalidOperationException("包内代码/平台设置或 AOT 引用已变化, 请先为当前平台重新生成完整包");
        string package = DevelopmentPackage.WriteSnapshot(target.ToString(), root, state.compatibility, out var manifest);
        string signature = CodeUpdate.Sha256Of(Encoding.UTF8.GetBytes(JsonUtility.ToJson(manifest)));
        var server = await EnsureBackend();
        if (EditorUserBuildSettings.activeBuildTarget != target) throw new InvalidOperationException("上传准备期间构建平台已改变, 请重新点击上传");
        var active = server.targets.FirstOrDefault(x => x.target == target.ToString());
        string receipt = root + "/published.json";
        if (state.content == signature && active != null && File.Exists(receipt) &&
            JsonUtility.FromJson<DevelopmentManifest>(File.ReadAllText(receipt)).contentId == active.contentId)
        { Stage(target + " 内容没有变化, 无需重复上传"); return; }
        Stage("上传 " + target + " 已打包代码和资源");
        File.WriteAllText(receipt, await Http("PUT", "/api/dev/content/" + target, package));
        state.content = signature; Save();
        Stage(target + " 上传完成, 该平台游戏下次启动时检查更新");
    }
    static void CheckAndroidTools()
    {
        string jdk = UnityEditor.Android.AndroidExternalToolsSettings.jdkRootPath;
        string ndk = UnityEditor.Android.AndroidExternalToolsSettings.ndkRootPath;
        if (!File.Exists(Path.Combine(jdk, "bin", "java.exe")) || !Directory.Exists(ndk))
            throw new InvalidOperationException("Android JDK/NDK 未安装, 请检查 Unity External Tools");
        string ps = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "WindowsPowerShell", "v1.0");
        if (!File.Exists(Path.Combine(ps, "powershell.exe"))) throw new InvalidOperationException("缺少 Windows PowerShell");
        // Unity 的 SDK 工具仍按名称启动 PowerShell, 修复当前进程搜索路径而不改系统设置.
        var paths = new[] { Environment.GetEnvironmentVariable("PATH"), Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine),
            Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User), ps, Path.Combine(jdk, "bin") };
        Environment.SetEnvironmentVariable("PATH", string.Join(";", paths.Where(x => !string.IsNullOrWhiteSpace(x))), EnvironmentVariableTarget.Process);
    }
}
