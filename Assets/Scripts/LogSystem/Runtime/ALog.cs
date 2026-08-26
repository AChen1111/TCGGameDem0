using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

/// <summary>
/// 分类日志系统的运行时核心:收集日志、按分类归档、解析调用堆栈。
/// 使用 ALog.Log / LogWarning / LogError 写入日志。
/// Unity原生的 Debug.LogXXX 与未处理异常会被捕获并归入 Unity_Native 分类。
/// Editor侧的控制台窗口通过 OnEntryAdded / OnCleared 订阅数据。
/// </summary>
public static class ALog
{
    public const string CategoryNative = "Unity_Native";
    public const string CategoryDefault = "Default";

    private const int Capacity = 5000;

    private static readonly List<ALogEntry> s_entries = new List<ALogEntry>();
    private static readonly SortedSet<string> s_categories = new SortedSet<string> { CategoryNative, CategoryDefault };
    private static int s_nextId;
    private static bool s_hooked;
    //转发到Unity Console时防止被原生监听重复收录
    private static bool s_forwarding;

    public static event Action<ALogEntry> OnEntryAdded;
    public static event Action OnCleared;

    public static IReadOnlyList<ALogEntry> Entries => s_entries;
    public static IEnumerable<string> Categories => s_categories;

    /// <summary>Editor始终启用;正式包看 ALogSettings.EnableInPlayer</summary>
    public static bool Enabled {
        get {
#if UNITY_EDITOR
            return true;
#else
            ALogSettings settings = ALogSettings.Instance;
            return settings == null || settings.EnableInPlayer;
#endif
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Init() {
        if (s_hooked || !Enabled)
        {
            return;
        }
        s_hooked = true;
        Application.logMessageReceived += OnUnityLog;
    }

    public static void Log(string message, string category = CategoryDefault) {
        if (!Enabled)
        {
            return;
        }
        Write(ALogLevel.Log, category, message, CaptureCSharpStack());
    }

    public static void LogWarning(string message, string category = CategoryDefault) {
        if (!Enabled)
        {
            return;
        }
        Write(ALogLevel.Warning, category, message, CaptureCSharpStack());
    }

    public static void LogError(string message, string category = CategoryDefault) {
        if (!Enabled)
        {
            return;
        }
        Write(ALogLevel.Error, category, message, CaptureCSharpStack());
    }

    public static void Clear() {
        s_entries.Clear();
        OnCleared?.Invoke();
    }

    private static void Write(ALogLevel level, string category, string message, string stackTrace) {
        var entry = NewEntry(level, category, message, stackTrace);
        entry.Frames = ALogStackParser.ParseCSharp(stackTrace);
        Add(entry);
        Forward(level, $"[{category}] {message}");
    }

    private static void OnUnityLog(string condition, string stackTrace, LogType type) {
        if (s_forwarding)
        {
            return;
        }
        var entry = NewEntry(ToLevel(type), CategoryNative, condition, stackTrace);
        entry.Frames = ALogStackParser.ParseCSharp(stackTrace);
        Add(entry);
    }

    private static ALogEntry NewEntry(ALogLevel level, string category, string message, string rawStack) {
        s_categories.Add(category);
        return new ALogEntry
        {
            Id = s_nextId++,
            Level = level,
            Category = category,
            Message = message,
            RawStack = rawStack,
            Time = DateTime.Now,
        };
    }

    private static void Add(ALogEntry entry) {
        if (s_entries.Count >= Capacity)
        {
            s_entries.RemoveAt(0);
        }
        s_entries.Add(entry);
        OnEntryAdded?.Invoke(entry);
    }

    private static void Forward(ALogLevel level, string text) {
        s_forwarding = true;
        switch (level)
        {
            case ALogLevel.Warning:
                Debug.LogWarning(text);
                break;
            case ALogLevel.Error:
                Debug.LogError(text);
                break;
            default:
                Debug.Log(text);
                break;
        }
        s_forwarding = false;
    }

    private static ALogLevel ToLevel(LogType type) {
        switch (type)
        {
            case LogType.Warning:
                return ALogLevel.Warning;
            case LogType.Error:
            case LogType.Assert:
            case LogType.Exception:
                return ALogLevel.Error;
            default:
                return ALogLevel.Log;
        }
    }

    //跳过ALog自身的两层,拿到真正的调用点
    private static string CaptureCSharpStack() {
        var trace = new StackTrace(2, true);
        var builder = new System.Text.StringBuilder();
        for (int i = 0; i < trace.FrameCount; i++)
        {
            StackFrame frame = trace.GetFrame(i);
            var method = frame.GetMethod();
            if (method == null)
            {
                continue;
            }
            string signature = $"{method.DeclaringType?.FullName}.{method.Name} ()";
            string file = frame.GetFileName();
            builder.AppendLine(string.IsNullOrEmpty(file) ? signature : $"{signature} (at {file}:{frame.GetFileLineNumber()})");
        }
        return builder.ToString();
    }
}
