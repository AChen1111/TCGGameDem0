using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

[Serializable]
public class ALogCategoryItem
{
    public string DisplayName;
    public string VariableName;
}

[Serializable]
public class ALogCategoryConfigData
{
    public List<ALogCategoryItem> Items = new List<ALogCategoryItem>();
}

/// <summary>
/// 报错分类配置:显示名(控制台/打日志用的分类字符串) + 英文变量名。
/// 生成 ALogCategories.cs 的 变量名->显示名 映射。
/// </summary>
public static class ALogCategoryConfig
{
    public const string ConfigPath = "Assets/Scripts/LogSystem/Editor/ALogCategoryConfig.json";
    public const string CSharpPath = "Assets/Scripts/LogSystem/Runtime/ALogCategories.cs";

    private static readonly Regex s_identifier = new Regex(@"^[A-Za-z_][A-Za-z0-9_]*$", RegexOptions.Compiled);

    public static ALogCategoryConfigData Load() {
        if (!File.Exists(ConfigPath))
        {
            return new ALogCategoryConfigData();
        }
        return JsonUtility.FromJson<ALogCategoryConfigData>(File.ReadAllText(ConfigPath)) ?? new ALogCategoryConfigData();
    }

    public static void Save(ALogCategoryConfigData data) {
        Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath));
        File.WriteAllText(ConfigPath, JsonUtility.ToJson(data, true));
        AssetDatabase.Refresh();
    }

    public static void RegisterAll(ALogCategoryConfigData data) {
        foreach (ALogCategoryItem item in data.Items)
        {
            ALog.RegisterCategory(item.DisplayName);
        }
    }

    public static string Validate(ALogCategoryConfigData data) {
        var vars = new HashSet<string>();
        var displays = new HashSet<string>();
        foreach (ALogCategoryItem item in data.Items)
        {
            if (string.IsNullOrWhiteSpace(item.DisplayName))
            {
                return "显示名不能为空";
            }
            if (string.IsNullOrWhiteSpace(item.VariableName) || !s_identifier.IsMatch(item.VariableName))
            {
                return $"变量名非法: {item.VariableName}";
            }
            if (!vars.Add(item.VariableName))
            {
                return $"变量名重复: {item.VariableName}";
            }
            if (!displays.Add(item.DisplayName))
            {
                return $"显示名重复: {item.DisplayName}";
            }
        }
        return null;
    }

    public static void Generate(ALogCategoryConfigData data) {
        string error = Validate(data);
        if (error != null)
        {
            throw new InvalidOperationException(error);
        }

        File.WriteAllText(CSharpPath, BuildCSharp(data), Encoding.UTF8);
        Save(data);
        RegisterAll(data);
        AssetDatabase.Refresh();
    }

    public static string BuildCSharp(ALogCategoryConfigData data) {
        var sb = new StringBuilder();
        sb.AppendLine("// 由「日志控制台 > 配置分类」自动生成,请勿手改");
        sb.AppendLine("public static class ALogCategories");
        sb.AppendLine("{");
        foreach (ALogCategoryItem item in data.Items)
        {
            sb.AppendLine($"    public const string {item.VariableName} = \"{Escape(item.DisplayName)}\";");
        }
        sb.AppendLine("}");
        return sb.ToString();
    }

    private static string Escape(string value) {
        return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }
}
