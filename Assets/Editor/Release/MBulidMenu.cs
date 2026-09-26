using System;
using AChen.Configuration;
using UnityEditor;
using UnityEngine;

public static class MBulidMenu
{
    [MenuItem("mBulid/设置版本号", false, 0)]
    static void SetVersion() => MBulidVersionWindow.Open();
    [MenuItem("mBulid/构建 DLL", false, 1)]
    static void BuildCode() { MBulidStatusWindow.Open(); MBulidPipeline.BuildCode(); }
    [MenuItem("mBulid/构建 Addressables", false, 2)]
    static void BuildResources() { MBulidStatusWindow.Open(); MBulidPipeline.BuildResources(); }
    [MenuItem("mBulid/发布版本", false, 3)]
    static void Publish() { MBulidStatusWindow.Open(); _ = MBulidPipeline.PublishAsync(); }
    [MenuItem("mBulid/设置版本号", true)]
    static bool CanSetVersion() => MBulidPipeline.CanRun;
    [MenuItem("mBulid/构建 DLL", true)]
    [MenuItem("mBulid/构建 Addressables", true)]
    [MenuItem("mBulid/发布版本", true)]
    static bool CanBuild() => MBulidPipeline.CanRun && MBulidPipeline.HasVersion;
}

public sealed class MBulidStatusWindow : EditorWindow
{
    public static void Open()
    {
        var window = GetWindow<MBulidStatusWindow>(true, "mBulid · 操作结果", true);
        window.minSize = new Vector2(460, 200);
        window.ShowUtility();
    }
    void OnInspectorUpdate() => Repaint();
    void OnGUI()
    {
        EditorGUILayout.HelpBox(MBulidPipeline.Status, MBulidPipeline.Failed ? MessageType.Error : MessageType.Info);
        EditorGUILayout.LabelField("本地版本目录", MBulidPipeline.OutputDirectory, EditorStyles.wordWrappedLabel);
        using (new EditorGUI.DisabledScope(!MBulidPipeline.HasVersion))
            if (GUILayout.Button("打开本地版本目录")) EditorUtility.RevealInFinder(MBulidPipeline.OutputDirectory);
        if (GUILayout.Button("查看后端当前平台最新版本"))
            Application.OpenURL(MBulidPipeline.BackendUrl + "/api/content/latest/" + EditorUserBuildSettings.activeBuildTarget);
    }
}

public sealed class MBulidVersionWindow : EditorWindow
{
    string version;
    string error;
    BuildTarget target;
    public static void Open()
    {
        var window = CreateInstance<MBulidVersionWindow>();
        window.target = EditorUserBuildSettings.activeBuildTarget;
        window.version = MBulidPipeline.SelectedVersion(window.target) ?? "1.0.0";
        window.titleContent = new GUIContent("mBulid · 设置版本号");
        window.minSize = new Vector2(440, 235);
        window.ShowUtility();
    }
    void OnInspectorUpdate() => Repaint();
    void OnGUI()
    {
        EditorGUILayout.LabelField("当前平台", target.ToString());
        version = EditorGUILayout.TextField("热更内容版本", version);
        if (DevelopmentProtocol.ValidContentVersion(version?.Trim()))
            EditorGUILayout.LabelField("输出目录", EditorPaths.FromProjectRoot("Version", DevelopmentProtocol.VersionFolder(version.Trim(), target.ToString())), EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField("后端", MBulidPipeline.BackendUrl);
        EditorGUILayout.HelpBox("同名版本目录存在时, 保存会删除目录内已有构建产物. 不修改主包版本号.", MessageType.Info);
        if (target != EditorUserBuildSettings.activeBuildTarget) EditorGUILayout.HelpBox("构建平台已改变, 请关闭窗口重新设置版本号", MessageType.Warning);
        if (!string.IsNullOrEmpty(error)) EditorGUILayout.HelpBox(error, MessageType.Error);
        using (new EditorGUI.DisabledScope(!MBulidPipeline.CanRun || target != EditorUserBuildSettings.activeBuildTarget || !DevelopmentProtocol.ValidContentVersion(version?.Trim())))
            if (GUILayout.Button("保存版本号并创建目录", GUILayout.Height(28)))
            {
                try { MBulidPipeline.SetVersion(version); Close(); }
                catch (Exception ex) { error = ex.Message; }
            }
        EditorGUILayout.LabelField(MBulidPipeline.Status, EditorStyles.wordWrappedLabel);
    }
}
