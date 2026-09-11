using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

enum BackendServiceState
{
    Stopped,
    Building,
    Starting,
    Running,
    External,
    Faulted
}

static class BackendServiceController
{
    public const string BaseUrl = "http://127.0.0.1:5080";
    public const int Port = 5080;

    const string AuthKeyEnvironmentVariable = "ACHEN_BACKEND_AUTH_SIGNING_KEY";
    const string AuthKeySessionName = "AChen.BackendService.AuthSigningKey";
    const int MaximumLogLines = 200;

    static readonly ConcurrentQueue<string> s_PendingLogs = new();
    static readonly ConcurrentQueue<Action> s_MainThreadActions = new();
    static readonly List<string> s_LogLines = new();

    static Process s_BuildProcess;
    static Process s_BackendProcess;
    static bool s_StopRequested;
    static double s_NextProbeAt;
    static double s_StartedAt;
    static int s_OpenWindowCount;

    public static event Action Changed;

    public static BackendServiceState State { get; private set; } = BackendServiceState.Stopped;
    public static string LastError { get; private set; } = string.Empty;
    public static IReadOnlyList<string> LogLines => s_LogLines;
    public static bool CanStart => State == BackendServiceState.Stopped || State == BackendServiceState.Faulted;
    public static bool CanStop => IsAlive(s_BuildProcess) || TryGetOwnedBackendProcess() != null;

    // IsPortOpen 是同步 TcpClient.Connect,在有回环过滤驱动的机器上一次拒绝要 2 秒.
    // 只有真的有人在看服务状态时才轮询,否则它每 0.5 秒就会卡住编辑器主线程.
    static bool ShouldProbe =>
        s_OpenWindowCount > 0 ||
        State == BackendServiceState.Building ||
        State == BackendServiceState.Starting ||
        State == BackendServiceState.Running;

    static string ProjectRoot => EditorPaths.ProjectRoot;
    static string BackendDirectory => EditorPaths.FromProjectRoot("Backend", "src", "AChen.Backend.Api");
    static string BackendProjectPath => Path.Combine(BackendDirectory, "AChen.Backend.Api.csproj");
    static string BackendDllPath => Path.Combine(BackendDirectory, "bin", "Debug", "net8.0", "AChen.Backend.Api.dll");
    static string OwnershipFilePath => EditorPaths.FromProjectRoot("Library", "AChenBackendService.pid");
    static string DotnetExecutable => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
        "dotnet",
        "dotnet.exe");

    static BackendServiceController()
    {
        EditorApplication.update += Update;
        EditorApplication.quitting += OnEditorQuitting;
        EditorApplication.delayCall += RecoverState;
    }

    public static void Start()
    {
        RecoverState();
        if (!CanStart)
        {
            return;
        }

        if (IsPortOpen())
        {
            SetState(
                BackendServiceState.External,
                "端口 5080 已被其他进程占用。为避免误杀，Editor 不会接管该进程。");
            return;
        }

        if (!File.Exists(BackendProjectPath))
        {
            SetState(BackendServiceState.Faulted, "找不到后端项目：" + BackendProjectPath);
            return;
        }

        try
        {
            EnsureDevelopmentSecrets();
            s_StopRequested = false;
            LastError = string.Empty;
            s_LogLines.Clear();
            AddLog("开始构建 ASP.NET Core 后端…");
            SetState(BackendServiceState.Building);
            s_BuildProcess = EditorProcessRunner.Create(
                DotnetExecutable,
                "build " + EditorProcessRunner.Quote(BackendProjectPath) + " -c Debug --nologo",
                ProjectRoot,
                line => QueueLog("BUILD", line));
            s_BuildProcess.EnableRaisingEvents = true;
            s_BuildProcess.Exited += OnBuildExited;
            EditorProcessRunner.StartRedirected(s_BuildProcess);
        }
        catch (Exception exception)
        {
            EditorProcessRunner.Dispose(ref s_BuildProcess);
            SetState(BackendServiceState.Faulted, "无法启动后端构建：" + exception.Message);
        }
    }

    public static void Stop()
    {
        Stop(showExternalWarning: true);
    }

    public static void Refresh()
    {
        RecoverState();
    }

    public static void OpenRegistrationPage()
    {
        Application.OpenURL(BaseUrl + "/register");
    }

    static void Stop(bool showExternalWarning)
    {
        s_StopRequested = true;
        bool stoppedAnyProcess = StopProcess(ref s_BuildProcess, "构建进程");

        Process backend = TryGetOwnedBackendProcess();
        if (backend != null)
        {
            if (!ReferenceEquals(backend, s_BackendProcess))
            {
                s_BackendProcess = backend;
            }

            stoppedAnyProcess |= StopProcess(ref s_BackendProcess, "后端服务");
            ClearOwnershipFile();
        }

        if (!stoppedAnyProcess && State == BackendServiceState.External)
        {
            LastError = "当前 5080 服务不是由本 Unity Editor 启动，已拒绝终止。";
            if (showExternalWarning)
            {
                EditorUtility.DisplayDialog("未关闭后端服务", LastError, "知道了");
            }

            NotifyChanged();
            return;
        }

        AddLog("后端服务已停止。");
        SetState(BackendServiceState.Stopped);
    }

    static void OnBuildExited(object sender, EventArgs eventArgs)
    {
        Process process = sender as Process;
        int exitCode = EditorProcessRunner.SafeExitCode(process);
        s_MainThreadActions.Enqueue(() =>
        {
            if (ReferenceEquals(s_BuildProcess, process))
            {
                EditorProcessRunner.Dispose(ref s_BuildProcess);
            }

            if (s_StopRequested)
            {
                SetState(BackendServiceState.Stopped);
                return;
            }

            if (exitCode != 0)
            {
                SetState(BackendServiceState.Faulted, "后端构建失败，退出码：" + exitCode + "。请查看窗口日志。");
                return;
            }

            LaunchBackend();
        });
    }

    static void LaunchBackend()
    {
        if (!File.Exists(BackendDllPath))
        {
            SetState(BackendServiceState.Faulted, "构建完成但未找到后端程序集：" + BackendDllPath);
            return;
        }

        if (IsPortOpen())
        {
            SetState(BackendServiceState.External, "构建期间端口 5080 被其他进程占用，未启动后端。");
            return;
        }

        try
        {
            string publishKey = EnsurePublishKey();
            string authSigningKey = EnsureAuthSigningKey();
            Process process = EditorProcessRunner.Create(
                DotnetExecutable,
                EditorProcessRunner.Quote(BackendDllPath) + " --urls " + EditorProcessRunner.Quote(BaseUrl),
                BackendDirectory,
                line => QueueLog("SERVER", line));
            process.StartInfo.EnvironmentVariables["ASPNETCORE_ENVIRONMENT"] = "Development";
            process.StartInfo.EnvironmentVariables["DOTNET_ENVIRONMENT"] = "Development";
            process.StartInfo.EnvironmentVariables["Auth__SigningKey"] = authSigningKey;
            process.StartInfo.EnvironmentVariables["ContentDelivery__PublishKey"] = publishKey;
            process.EnableRaisingEvents = true;
            process.Exited += OnBackendExited;
            EditorProcessRunner.StartRedirected(process);

            s_BackendProcess = process;
            s_StartedAt = EditorApplication.timeSinceStartup;
            PersistOwnership(process);
            AddLog("后端进程已创建，PID " + process.Id + "。");
            SetState(BackendServiceState.Starting);
        }
        catch (Exception exception)
        {
            EditorProcessRunner.Dispose(ref s_BackendProcess);
            ClearOwnershipFile();
            SetState(BackendServiceState.Faulted, "无法启动后端服务：" + exception.Message);
        }
    }

    static void OnBackendExited(object sender, EventArgs eventArgs)
    {
        Process process = sender as Process;
        int exitCode = EditorProcessRunner.SafeExitCode(process);
        s_MainThreadActions.Enqueue(() =>
        {
            if (ReferenceEquals(s_BackendProcess, process))
            {
                EditorProcessRunner.Dispose(ref s_BackendProcess);
            }

            ClearOwnershipFile();
            if (s_StopRequested)
            {
                SetState(BackendServiceState.Stopped);
            }
            else
            {
                SetState(
                    BackendServiceState.Faulted,
                    "后端服务意外退出，退出码：" + exitCode + "。请查看窗口日志。");
            }
        });
    }

    public static void RegisterWindow()
    {
        s_OpenWindowCount++;
    }

    public static void UnregisterWindow()
    {
        s_OpenWindowCount = Math.Max(0, s_OpenWindowCount - 1);
    }

    static void Update()
    {
        while (s_MainThreadActions.TryDequeue(out Action action))
        {
            action();
        }

        bool addedLog = false;
        while (s_PendingLogs.TryDequeue(out string line))
        {
            AddLog(line);
            addedLog = true;
        }

        if (addedLog)
        {
            NotifyChanged();
        }

        if (!ShouldProbe)
        {
            return;
        }

        if (EditorApplication.timeSinceStartup < s_NextProbeAt)
        {
            return;
        }

        s_NextProbeAt = EditorApplication.timeSinceStartup + 0.5d;
        Process backend = TryGetOwnedBackendProcess();
        if (backend != null)
        {
            if (State != BackendServiceState.Building)
            {
                if (IsPortOpen())
                {
                    SetState(BackendServiceState.Running);
                }
                else if (EditorApplication.timeSinceStartup - s_StartedAt > 20d)
                {
                    SetState(BackendServiceState.Faulted, "后端进程已启动，但 20 秒内没有监听 5080 端口。");
                }
                else
                {
                    SetState(BackendServiceState.Starting);
                }
            }

            return;
        }

        if (IsAlive(s_BuildProcess))
        {
            SetState(BackendServiceState.Building);
            return;
        }

        if (IsPortOpen())
        {
            SetState(BackendServiceState.External, "检测到 5080 端口已有服务，但它不是由当前 Unity Editor 启动的。");
        }
        else if (State == BackendServiceState.External || State == BackendServiceState.Starting || State == BackendServiceState.Running)
        {
            SetState(BackendServiceState.Stopped);
        }
    }

    static void RecoverState()
    {
        Process backend = TryGetOwnedBackendProcess();
        if (backend != null)
        {
            s_BackendProcess = backend;
            s_StartedAt = EditorApplication.timeSinceStartup;
            SetState(IsPortOpen() ? BackendServiceState.Running : BackendServiceState.Starting);
        }
        else if (IsPortOpen())
        {
            SetState(BackendServiceState.External, "检测到 5080 端口已有服务，但它不是由当前 Unity Editor 启动的。");
        }
        else if (State != BackendServiceState.Building && State != BackendServiceState.Faulted)
        {
            SetState(BackendServiceState.Stopped);
        }
    }

    static Process TryGetOwnedBackendProcess()
    {
        if (IsAlive(s_BackendProcess))
        {
            return s_BackendProcess;
        }

        if (!File.Exists(OwnershipFilePath))
        {
            return null;
        }

        try
        {
            string[] values = File.ReadAllText(OwnershipFilePath).Split('|');
            if (values.Length != 2 || !int.TryParse(values[0], out int processId) || !long.TryParse(values[1], out long startTicks))
            {
                ClearOwnershipFile();
                return null;
            }

            Process process = Process.GetProcessById(processId);
            process.Refresh();
            bool isOwned = !process.HasExited
                && string.Equals(process.ProcessName, "dotnet", StringComparison.OrdinalIgnoreCase)
                && process.StartTime.ToUniversalTime().Ticks == startTicks;
            if (!isOwned)
            {
                process.Dispose();
                ClearOwnershipFile();
                return null;
            }

            return process;
        }
        catch
        {
            ClearOwnershipFile();
            return null;
        }
    }

    static void PersistOwnership(Process process)
    {
        File.WriteAllText(
            OwnershipFilePath,
            process.Id + "|" + process.StartTime.ToUniversalTime().Ticks);
    }

    static void ClearOwnershipFile()
    {
        try
        {
            if (File.Exists(OwnershipFilePath))
            {
                File.Delete(OwnershipFilePath);
            }
        }
        catch (Exception exception)
        {
            Debug.LogWarning("[BackendService] 无法清理进程标记：" + exception.Message);
        }
    }

    static bool StopProcess(ref Process process, string displayName)
    {
        bool stopped = EditorProcessRunner.Stop(ref process, 3000, out int processId, out string error);
        if (stopped)
        {
            AddLog(displayName + "已终止，PID " + processId + "。");
        }
        else if (error != null)
        {
            LastError = "终止" + displayName + "失败：" + error;
            AddLog(LastError);
        }

        return stopped;
    }

    static bool IsAlive(Process process) => EditorProcessRunner.IsAlive(process);

    static bool IsPortOpen()
    {
        try
        {
            using var client = new TcpClient();
            client.Connect("127.0.0.1", Port);
            return true;
        }
        catch (SocketException)
        {
            return false;
        }
    }

    static void EnsureDevelopmentSecrets()
    {
        EnsureAuthSigningKey();
        EnsurePublishKey();
    }

    static string EnsureAuthSigningKey()
    {
        string configured = Environment.GetEnvironmentVariable(AuthKeyEnvironmentVariable);
        if (!string.IsNullOrEmpty(configured))
        {
            ValidateSecret(configured, AuthKeyEnvironmentVariable);
            return configured;
        }

        string generated = SessionState.GetString(AuthKeySessionName, string.Empty);
        if (generated.Length < 32)
        {
            generated = GenerateSecret();
            SessionState.SetString(AuthKeySessionName, generated);
        }

        return generated;
    }

    static string EnsurePublishKey()
    {
        string configured = PublishKeyProvider.FromEnvironment;
        if (!string.IsNullOrEmpty(configured))
        {
            ValidateSecret(configured, PublishKeyProvider.EnvironmentVariable);
            return configured;
        }

        string generated = PublishKeyProvider.FromSession;
        if (generated.Length < 32)
        {
            generated = GenerateSecret();
            SessionState.SetString(PublishKeyProvider.SessionName, generated);
        }

        Environment.SetEnvironmentVariable(
            PublishKeyProvider.EnvironmentVariable,
            generated,
            EnvironmentVariableTarget.Process);
        return generated;
    }

    static void ValidateSecret(string secret, string variableName)
    {
        if (secret.Length < 32)
        {
            throw new InvalidOperationException(variableName + " 至少需要 32 个字符。");
        }
    }

    static string GenerateSecret()
    {
        var bytes = new byte[48];
        using (RandomNumberGenerator random = RandomNumberGenerator.Create())
        {
            random.GetBytes(bytes);
        }

        return Convert.ToBase64String(bytes);
    }

    static void QueueLog(string prefix, string message)
    {
        if (!string.IsNullOrWhiteSpace(message))
        {
            s_PendingLogs.Enqueue("[" + prefix + "] " + message);
        }
    }

    static void AddLog(string message)
    {
        s_LogLines.Add(DateTime.Now.ToString("HH:mm:ss") + "  " + message);
        if (s_LogLines.Count > MaximumLogLines)
        {
            s_LogLines.RemoveRange(0, s_LogLines.Count - MaximumLogLines);
        }
    }

    static void SetState(BackendServiceState state, string error = null)
    {
        bool changed = State != state;
        State = state;
        if (error != null)
        {
            LastError = error;
            AddLog(error);
            changed = true;
        }
        else if (state != BackendServiceState.Faulted && state != BackendServiceState.External)
        {
            LastError = string.Empty;
        }

        if (changed)
        {
            NotifyChanged();
        }
    }

    static void NotifyChanged()
    {
        Changed?.Invoke();
    }

    static void OnEditorQuitting()
    {
        Stop(showExternalWarning: false);
    }
}

public sealed class BackendServiceWindow : EditorWindow
{
    Vector2 m_LogScroll;

    [MenuItem(EditorMenus.Window + "后端服务")]
    [MenuItem(EditorMenus.Backend + "打开窗口", false, 1)]
    static void Open()
    {
        GetWindow<BackendServiceWindow>("后端服务");
    }

    [MenuItem(EditorMenus.Backend + "启动", false, 10)]
    static void StartFromMenu()
    {
        BackendServiceController.Start();
        Open();
    }

    [MenuItem(EditorMenus.Backend + "启动", true)]
    static bool CanStartFromMenu()
    {
        return BackendServiceController.CanStart;
    }

    [MenuItem(EditorMenus.Backend + "关闭", false, 11)]
    static void StopFromMenu()
    {
        BackendServiceController.Stop();
        Open();
    }

    [MenuItem(EditorMenus.Backend + "关闭", true)]
    static bool CanStopFromMenu()
    {
        return BackendServiceController.CanStop;
    }

    void OnEnable()
    {
        minSize = new Vector2(480f, 420f);
        BackendServiceController.Changed += Repaint;
        BackendServiceController.RegisterWindow();
    }

    void OnDisable()
    {
        BackendServiceController.Changed -= Repaint;
        BackendServiceController.UnregisterWindow();
    }

    void OnGUI()
    {
        EditorGUILayout.Space(10f);
        EditorGUILayout.LabelField("ASP.NET Core 后端服务", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("从 Unity Editor 构建、启动并安全关闭当前项目后端。", EditorStyles.wordWrappedLabel);
        EditorGUILayout.Space(12f);

        DrawStatusPanel();
        EditorGUILayout.Space(12f);
        DrawPrimaryAction();

        using (new EditorGUILayout.HorizontalScope())
        {
            using (new EditorGUI.DisabledScope(
                BackendServiceController.State != BackendServiceState.Running
                && BackendServiceController.State != BackendServiceState.External))
            {
                if (GUILayout.Button("打开注册页", GUILayout.Height(28f)))
                {
                    BackendServiceController.OpenRegistrationPage();
                }
            }

            if (GUILayout.Button("重新检测", GUILayout.Height(28f)))
            {
                BackendServiceController.Refresh();
                Repaint();
            }
        }

        EditorGUILayout.Space(14f);
        EditorGUILayout.LabelField("最近日志", EditorStyles.boldLabel);
        m_LogScroll = EditorGUILayout.BeginScrollView(m_LogScroll, EditorStyles.helpBox, GUILayout.ExpandHeight(true));
        IReadOnlyList<string> logs = BackendServiceController.LogLines;
        if (logs.Count == 0)
        {
            EditorGUILayout.LabelField("尚无日志。", EditorStyles.miniLabel);
        }
        else
        {
            for (int index = 0; index < logs.Count; index++)
            {
                EditorGUILayout.SelectableLabel(logs[index], EditorStyles.miniLabel, GUILayout.Height(17f));
            }
        }
        EditorGUILayout.EndScrollView();
    }

    static void DrawStatusPanel()
    {
        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                Rect dot = GUILayoutUtility.GetRect(12f, 12f, GUILayout.Width(12f));
                dot.y += 3f;
                EditorGUI.DrawRect(dot, StatusColor(BackendServiceController.State));
                EditorGUILayout.LabelField(StatusText(BackendServiceController.State), EditorStyles.boldLabel);
            }

            EditorGUILayout.LabelField("地址", BackendServiceController.BaseUrl);
            EditorGUILayout.LabelField("端口", BackendServiceController.Port.ToString());
            if (!string.IsNullOrEmpty(BackendServiceController.LastError))
            {
                EditorGUILayout.HelpBox(BackendServiceController.LastError, MessageType.Warning);
            }
        }
    }

    static void DrawPrimaryAction()
    {
        bool canStop = BackendServiceController.CanStop;
        bool canStart = BackendServiceController.CanStart;
        string label = canStop ? "关闭后端服务" : "一键启动后端服务";
        Color previous = GUI.backgroundColor;
        GUI.backgroundColor = canStop ? new Color(0.92f, 0.58f, 0.50f) : new Color(0.48f, 0.78f, 0.58f);
        using (new EditorGUI.DisabledScope(!canStop && !canStart))
        {
            if (GUILayout.Button(label, GUILayout.Height(42f)))
            {
                if (canStop)
                {
                    BackendServiceController.Stop();
                }
                else
                {
                    BackendServiceController.Start();
                }
            }
        }
        GUI.backgroundColor = previous;
    }

    static string StatusText(BackendServiceState state)
    {
        return state switch
        {
            BackendServiceState.Stopped => "已停止",
            BackendServiceState.Building => "正在构建后端",
            BackendServiceState.Starting => "正在启动",
            BackendServiceState.Running => "运行中",
            BackendServiceState.External => "检测到外部服务",
            BackendServiceState.Faulted => "启动失败",
            _ => state.ToString()
        };
    }

    static Color StatusColor(BackendServiceState state)
    {
        return state switch
        {
            BackendServiceState.Running => new Color(0.35f, 0.78f, 0.48f),
            BackendServiceState.Building => new Color(0.92f, 0.70f, 0.30f),
            BackendServiceState.Starting => new Color(0.92f, 0.70f, 0.30f),
            BackendServiceState.External => new Color(0.38f, 0.65f, 0.88f),
            BackendServiceState.Faulted => new Color(0.90f, 0.38f, 0.34f),
            _ => new Color(0.48f, 0.50f, 0.46f)
        };
    }
}
