using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>展示单条日志调用堆栈的可视化窗口,由日志控制台的「调用堆栈图」按钮打开</summary>
public class ALogStackGraphWindow : EditorWindow
{
    private ALogEntry m_entry;
    private ALogStackGraph m_graph;

    public static void Show(ALogEntry entry) {
        var window = GetWindow<ALogStackGraphWindow>(true, "调用堆栈图");
        window.minSize = new Vector2(540f, 420f);
        window.m_entry = entry;
        window.Rebuild();
    }

    private void CreateGUI() {
        var style = AssetDatabase.LoadAssetAtPath<StyleSheet>(ALogConsoleWindow.StyleSheetPath);
        if (style != null)
        {
            rootVisualElement.styleSheets.Add(style);
        }
        rootVisualElement.AddToClassList("stack-graph-root");

        var header = new Label();
        header.name = "graph-header";
        header.AddToClassList("stack-graph-header");
        rootVisualElement.Add(header);

        m_graph = new ALogStackGraph();
        var scroll = new ScrollView();
        scroll.style.flexGrow = 1f;
        scroll.Add(m_graph);
        rootVisualElement.Add(scroll);

        Rebuild();
    }

    private void Rebuild() {
        if (m_graph == null)
        {
            return;
        }
        var header = rootVisualElement.Q<Label>("graph-header");
        header.text = m_entry == null ? "未选择日志" : $"[{m_entry.Category}] {m_entry.Message}";
        m_graph.SetFrames(m_entry?.Frames);
    }
}
