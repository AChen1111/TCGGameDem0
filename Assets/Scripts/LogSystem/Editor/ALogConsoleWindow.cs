using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// 分类日志控制台(UI Toolkit):左侧按分类过滤,中间日志列表,下方详情与堆栈。
/// 堆栈行点击可跳转源码(Lua/C#均可),「调用堆栈图」按钮打开可视化调用链。
/// </summary>
public class ALogConsoleWindow : EditorWindow
{
    private const string UxmlPath = "Assets/Scripts/LogSystem/Editor/ALogConsoleWindow.uxml";
    public const string StyleSheetPath = "Assets/Scripts/LogSystem/Editor/ALogConsoleWindow.uss";

    private readonly List<ALogEntry> m_filtered = new List<ALogEntry>();
    private readonly HashSet<string> m_hiddenCategories = new HashSet<string>();
    private readonly bool[] m_levelVisible = { true, true, true };

    private ListView m_entryList;
    private ScrollView m_categoryList;
    private ScrollView m_detailStack;
    private Label m_detailTitle;
    private Toggle[] m_levelToggles;
    private string m_search = string.Empty;
    private ALogEntry m_selected;
    private bool m_dirty = true;
    private int m_categoryCount;

    [InitializeOnLoadMethod]
    private static void HookEditorLogs() {
        ALog.Init();
        ALogCategoryConfig.RegisterAll(ALogCategoryConfig.Load());
    }

    [MenuItem("Window/AChen/日志控制台")]
    private static void Open() {
        GetWindow<ALogConsoleWindow>("日志控制台").minSize = new Vector2(720f, 420f);
    }

    private void CreateGUI() {
        var tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UxmlPath);
        tree.CloneTree(rootVisualElement);
        rootVisualElement.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(StyleSheetPath));

        m_categoryList = rootVisualElement.Q<ScrollView>("category-list");
        m_detailStack = rootVisualElement.Q<ScrollView>("detail-stack");
        m_detailTitle = rootVisualElement.Q<Label>("detail-title");

        rootVisualElement.Q<Button>("clear-button").clicked += () => {
            ALog.Clear();
            m_dirty = true;
        };
        rootVisualElement.Q<Button>("config-category-button").clicked += ALogCategoryConfigWindow.Open;
        rootVisualElement.Q<Button>("graph-button").clicked += () => ALogStackGraphWindow.Show(m_selected);

        ALogSettings settings = ALogSettingsEditor.GetOrCreate();
        var enableInPlayer = rootVisualElement.Q<Toggle>("toggle-enable-in-player");
        enableInPlayer.value = settings.EnableInPlayer;
        enableInPlayer.RegisterValueChangedCallback(evt => {
            settings.EnableInPlayer = evt.newValue;
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
        });

        var search = rootVisualElement.Q<TextField>("search-field");
        search.RegisterValueChangedCallback(evt => {
            m_search = evt.newValue;
            m_dirty = true;
        });

        m_levelToggles = new[]
        {
            rootVisualElement.Q<Toggle>("toggle-log"),
            rootVisualElement.Q<Toggle>("toggle-warning"),
            rootVisualElement.Q<Toggle>("toggle-error"),
        };
        for (int i = 0; i < m_levelToggles.Length; i++)
        {
            int level = i;
            m_levelToggles[i].value = true;
            m_levelToggles[i].RegisterValueChangedCallback(evt => {
                m_levelVisible[level] = evt.newValue;
                m_dirty = true;
            });
        }

        m_entryList = rootVisualElement.Q<ListView>("entry-list");
        m_entryList.fixedItemHeight = 22f;
        m_entryList.selectionType = SelectionType.Single;
        m_entryList.itemsSource = m_filtered;
        m_entryList.makeItem = MakeEntryRow;
        m_entryList.bindItem = BindEntryRow;
        m_entryList.selectionChanged += OnSelectionChanged;

        ALog.OnEntryAdded += OnEntryAdded;
        ALog.OnCleared += OnCleared;
        rootVisualElement.schedule.Execute(RefreshIfDirty).Every(200);
        m_dirty = true;
    }

    private void OnDisable() {
        ALog.OnEntryAdded -= OnEntryAdded;
        ALog.OnCleared -= OnCleared;
    }

    private void OnEntryAdded(ALogEntry entry) {
        m_dirty = true;
    }

    private void OnCleared() {
        m_selected = null;
        m_dirty = true;
    }

    private void RefreshIfDirty() {
        if (!m_dirty)
        {
            return;
        }
        m_dirty = false;
        RebuildCategories();
        RebuildEntries();
        RebuildDetail();
    }

    private void RebuildCategories() {
        var categories = new List<string>(ALog.Categories);
        if (categories.Count == m_categoryCount)
        {
            return;
        }
        m_categoryCount = categories.Count;

        m_categoryList.Clear();
        foreach (string category in categories)
        {
            var toggle = new Toggle(category) { value = !m_hiddenCategories.Contains(category) };
            string captured = category;
            toggle.RegisterValueChangedCallback(evt => {
                if (evt.newValue)
                {
                    m_hiddenCategories.Remove(captured);
                }
                else
                {
                    m_hiddenCategories.Add(captured);
                }
                m_dirty = true;
            });
            m_categoryList.Add(toggle);
        }
    }

    private void RebuildEntries() {
        m_filtered.Clear();
        var counts = new int[3];
        foreach (ALogEntry entry in ALog.Entries)
        {
            counts[(int)entry.Level]++;
            if (!m_levelVisible[(int)entry.Level] || m_hiddenCategories.Contains(entry.Category))
            {
                continue;
            }
            if (m_search.Length > 0 && entry.Message.IndexOf(m_search, System.StringComparison.OrdinalIgnoreCase) < 0)
            {
                continue;
            }
            m_filtered.Add(entry);
        }

        m_levelToggles[0].label = $"Log {counts[0]}";
        m_levelToggles[1].label = $"Warning {counts[1]}";
        m_levelToggles[2].label = $"Error {counts[2]}";
        m_entryList.RefreshItems();
    }

    private static VisualElement MakeEntryRow() {
        var row = new VisualElement();
        row.AddToClassList("entry-row");
        row.Add(NewLabel("entry-row__level"));
        row.Add(NewLabel("entry-row__category"));
        row.Add(NewLabel("entry-row__message"));
        row.Add(NewLabel("entry-row__time"));
        return row;
    }

    private void BindEntryRow(VisualElement element, int index) {
        ALogEntry entry = m_filtered[index];
        var level = (Label)element[0];
        level.text = entry.Level == ALogLevel.Error ? "E" : entry.Level == ALogLevel.Warning ? "W" : "L";
        level.ClearClassList();
        level.AddToClassList("entry-row__level");
        level.AddToClassList(entry.Level == ALogLevel.Error ? "level--error" : entry.Level == ALogLevel.Warning ? "level--warning" : "level--log");

        ((Label)element[1]).text = entry.Category;
        ((Label)element[2]).text = FirstLine(entry.Message);
        ((Label)element[3]).text = entry.TimeText;
    }

    private void OnSelectionChanged(IEnumerable<object> selection) {
        m_selected = null;
        foreach (object item in selection)
        {
            m_selected = item as ALogEntry;
        }
        RebuildDetail();
    }

    private void RebuildDetail() {
        m_detailStack.Clear();
        if (m_selected == null)
        {
            m_detailTitle.text = "未选择日志";
            return;
        }

        m_detailTitle.text = $"[{m_selected.Category}] {m_selected.Message}";
        foreach (ALogFrame frame in m_selected.Frames)
        {
            var row = new Label($"{frame.Signature}    {frame.Location}");
            row.AddToClassList("stack-row");
            if (frame.CanJump)
            {
                row.AddToClassList("stack-row--clickable");
                ALogFrame captured = frame;
                row.RegisterCallback<ClickEvent>(_ => ALogSourceJump.Open(captured));
            }
            m_detailStack.Add(row);
        }
    }

    private static Label NewLabel(string className) {
        var label = new Label();
        label.AddToClassList(className);
        return label;
    }

    private static string FirstLine(string text) {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }
        int index = text.IndexOf('\n');
        return index < 0 ? text : text.Substring(0, index);
    }
}
