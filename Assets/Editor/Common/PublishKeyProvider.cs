using System;
using UnityEditor;

/// <summary>
/// 内容发布密钥的统一来源. 解析顺序: 环境变量 > 后端服务窗口本次会话生成的密钥 > 窗口内仅内存输入.
/// 环境变量名与后端约定一致, 不随客户端重构改名.
/// </summary>
public static class PublishKeyProvider
{
    public const string EnvironmentVariable = "ACHEN_CONTENT_PUBLISH_KEY";
    public const string SessionName = "AChen.BackendService.PublishKey";

    public static string FromEnvironment => Environment.GetEnvironmentVariable(EnvironmentVariable);

    public static string FromSession => SessionState.GetString(SessionName, string.Empty);

    /// <summary>环境变量或后端服务会话中是否已有可用密钥.</summary>
    public static bool HasConfiguredKey =>
        !string.IsNullOrEmpty(FromEnvironment) || !string.IsNullOrEmpty(FromSession);

    public static string Resolve(string memoryKey)
    {
        string key = FromEnvironment;
        if (!string.IsNullOrEmpty(key))
        {
            return key;
        }

        key = FromSession;
        return string.IsNullOrEmpty(key) ? memoryKey : key;
    }

    public static string DescribeSource()
    {
        if (!string.IsNullOrEmpty(FromEnvironment))
        {
            return "来自 " + EnvironmentVariable;
        }

        return !string.IsNullOrEmpty(FromSession) ? "来自后端服务窗口会话" : "未配置";
    }

    /// <summary>绘制密钥字段: 已配置时只显示来源, 否则显示仅内存的密码框. 返回更新后的内存密钥.</summary>
    public static string DrawField(string memoryKey)
    {
        if (HasConfiguredKey)
        {
            EditorGUILayout.LabelField("发布密钥", DescribeSource());
            return memoryKey;
        }

        string value = EditorGUILayout.PasswordField("发布密钥（仅内存）", memoryKey);
        EditorGUILayout.HelpBox(
            "推荐设置 " + EnvironmentVariable + " 或先通过「后端服务」窗口启动后端。窗口内输入的密钥不会写入 EditorPrefs。",
            MessageType.Info);
        return value;
    }

    public static void RequireKey(string memoryKey)
    {
        if (string.IsNullOrWhiteSpace(Resolve(memoryKey)))
        {
            throw new InvalidOperationException("请设置发布密钥，或先通过「后端服务」窗口启动后端");
        }
    }
}
