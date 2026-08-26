using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 把 ALog 的入口做成按钮挂在 Unity 内置 Console 的工具栏上(ConsoleWindow.drawCustomToolbarGui)。
/// 分类下拉复用内置搜索框过滤,不再单独维护一份日志列表。
/// </summary>
[InitializeOnLoad]
public static class ALogConsoleToolbar
{
    private const string AllCategories = "全部";

    private static readonly GUIContent s_graphContent = new GUIContent("堆栈图", "把当前选中日志的调用堆栈画成调用链");
    private static readonly GUIContent s_playerContent = new GUIContent("出包日志", "关闭后正式包内 ALog 不再写日志");

    private static string[] s_categories;

    static ALogConsoleToolbar() {
        if (ALogConsoleBridge.IsAvailable)
        {
            ALogConsoleBridge.AddToolbarGui(OnToolbarGui);
        }
    }

    /// <summary>分类取自 ALogCategories 的常量,不依赖运行时是否已经打过该分类的日志</summary>
    public static string[] Categories {
        get {
            if (s_categories != null)
            {
                return s_categories;
            }
            var names = new List<string>();
            foreach (FieldInfo field in typeof(ALogCategories).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                if (field.IsLiteral && field.FieldType == typeof(string))
                {
                    names.Add((string)field.GetRawConstantValue());
                }
            }
            names.Sort(System.StringComparer.Ordinal);
            s_categories = names.ToArray();
            return s_categories;
        }
    }

    /// <summary>分类过滤就是按日志前缀搜索,空分类表示不过滤</summary>
    public static string ToSearchText(string category) {
        return string.IsNullOrEmpty(category) || category == AllCategories ? string.Empty : $"[{category}]";
    }

    /// <summary>从内置搜索框的内容反推当前选中的分类,便于下拉按钮回显</summary>
    public static string FromSearchText(string searchText) {
        foreach (string category in Categories)
        {
            if (searchText == ToSearchText(category))
            {
                return category;
            }
        }
        return AllCategories;
    }

    private static void OnToolbarGui() {
        string current = FromSearchText(ALogConsoleBridge.GetSearchText());
        var dropdownContent = new GUIContent($"分类: {current}", "按 ALog 分类过滤内置控制台");
        Rect dropdownRect = GUILayoutUtility.GetRect(dropdownContent, EditorStyles.toolbarDropDown, GUILayout.Width(110f));
        if (EditorGUI.DropdownButton(dropdownRect, dropdownContent, FocusType.Passive, EditorStyles.toolbarDropDown))
        {
            ShowCategoryMenu(dropdownRect, current);
        }

        if (GUILayout.Button(s_graphContent, EditorStyles.toolbarButton, GUILayout.Width(56f)))
        {
            ShowStackGraph();
        }

        ALogSettings settings = ALogSettingsEditor.GetOrCreate();
        bool enableInPlayer = GUILayout.Toggle(settings.EnableInPlayer, s_playerContent, EditorStyles.toolbarButton, GUILayout.Width(64f));
        if (enableInPlayer != settings.EnableInPlayer)
        {
            settings.EnableInPlayer = enableInPlayer;
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
        }
    }

    private static void ShowCategoryMenu(Rect rect, string current) {
        var menu = new GenericMenu();
        menu.AddItem(new GUIContent(AllCategories), current == AllCategories, () => ALogConsoleBridge.SetSearchText(string.Empty));
        menu.AddSeparator(string.Empty);
        foreach (string category in Categories)
        {
            string captured = category;
            menu.AddItem(new GUIContent(category), current == category, () => ALogConsoleBridge.SetSearchText(ToSearchText(captured)));
        }
        menu.DropDown(rect);
    }

    private static void ShowStackGraph() {
        string callstack = ALogConsoleBridge.ActiveCallstack;
        if (string.IsNullOrWhiteSpace(callstack))
        {
            EditorUtility.DisplayDialog("调用堆栈图", "请先在控制台里选中一条日志。", "好");
            return;
        }
        List<ALogFrame> frames = ALogStackParser.TrimLoggingFrames(ALogStackParser.ParseCSharp(callstack));
        ALogStackGraphWindow.Show(ALogConsoleBridge.ActiveMessage, frames);
    }
}
