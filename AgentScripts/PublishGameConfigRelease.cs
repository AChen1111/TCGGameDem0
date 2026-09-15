using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
public static class PublishGameConfigRelease
{
    public static string Run()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play Mode first.");
        if (!PublishKeyProvider.HasConfiguredKey) throw new InvalidOperationException("Publish key is not configured.");
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.StandaloneWindows64 || PlayerSettings.bundleVersion != "0.1.0")
            throw new InvalidOperationException("Expected Windows64 / 0.1.0.");
        var window = EditorWindow.GetWindow<ContentReleasePublisherWindow>("Content Release");
        var flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var type = window.GetType();
        type.GetField("m_BackendUrl", flags).SetValue(window, "http://127.0.0.1:5080");
        type.GetField("m_ContentVersion", flags).SetValue(window, "0.1.0-config." + DateTime.UtcNow.ToString("yyyyMMddHHmmss"));
        type.GetField("m_Notes", flags).SetValue(window, "统一配置 Addressables 迁移及场景更新 UI 修复");
        type.GetMethod("BuildAndPublishAsync", flags).Invoke(window, null);
        return "Publication dispatched";
    }
    public static string Status()
    {
        var windows = Resources.FindObjectsOfTypeAll<ContentReleasePublisherWindow>();
        if (windows.Length == 0) return "No publisher window";
        var w = windows[0];
        var flags = BindingFlags.Instance | BindingFlags.NonPublic;
        return "Busy=" + w.GetType().GetField("m_IsBusy", flags).GetValue(w)
            + "; Status=" + w.GetType().GetField("m_Status", flags).GetValue(w);
    }
}
