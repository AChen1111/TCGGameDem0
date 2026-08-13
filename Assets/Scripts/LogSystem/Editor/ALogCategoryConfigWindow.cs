using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>配置报错分类:填写「控制台显示名 + 英文变量名」,生成 ALogCategories / LogCategories 映射</summary>
public class ALogCategoryConfigWindow : EditorWindow
{
    private ALogCategoryConfigData m_data;
    private ScrollView m_list;
    private Label m_status;

    public static void Open() {
        var window = GetWindow<ALogCategoryConfigWindow>(true, "配置报错分类");
        window.minSize = new Vector2(480f, 360f);
    }

    private void CreateGUI() {
        m_data = ALogCategoryConfig.Load();

        var root = rootVisualElement;
        root.style.paddingLeft = 8;
        root.style.paddingRight = 8;
        root.style.paddingTop = 8;
        root.style.paddingBottom = 8;

        var tip = new Label("每行: 控制台显示的类别名称 + 英文变量名。生成后可用 ALogCategories.Xxx / LogCategories.Xxx 打日志。");
        tip.style.whiteSpace = WhiteSpace.Normal;
        tip.style.marginBottom = 6;
        tip.style.color = new Color(0.75f, 0.75f, 0.75f);
        root.Add(tip);

        var header = new VisualElement();
        header.style.flexDirection = FlexDirection.Row;
        header.Add(HeaderLabel("显示名", 1f));
        header.Add(HeaderLabel("英文变量名", 1f));
        header.Add(HeaderLabel("", 0f, 56f));
        root.Add(header);

        m_list = new ScrollView();
        m_list.style.flexGrow = 1f;
        root.Add(m_list);

        var bar = new VisualElement();
        bar.style.flexDirection = FlexDirection.Row;
        bar.style.marginTop = 8;
        bar.style.justifyContent = Justify.FlexEnd;

        var add = new Button(AddRow) { text = "添加" };
        add.style.marginRight = 6;
        bar.Add(add);

        var generate = new Button(Generate) { text = "生成映射类" };
        bar.Add(generate);
        root.Add(bar);

        m_status = new Label();
        m_status.style.marginTop = 4;
        m_status.style.whiteSpace = WhiteSpace.Normal;
        root.Add(m_status);

        RebuildList();
    }

    private void RebuildList() {
        m_list.Clear();
        for (int i = 0; i < m_data.Items.Count; i++)
        {
            m_list.Add(MakeRow(m_data.Items[i]));
        }
    }

    private VisualElement MakeRow(ALogCategoryItem item) {
        var row = new VisualElement();
        row.style.flexDirection = FlexDirection.Row;
        row.style.marginBottom = 4;

        var display = new TextField { value = item.DisplayName };
        display.style.flexGrow = 1f;
        display.style.marginRight = 6;
        display.RegisterValueChangedCallback(evt => item.DisplayName = evt.newValue);
        row.Add(display);

        var variable = new TextField { value = item.VariableName };
        variable.style.flexGrow = 1f;
        variable.style.marginRight = 6;
        variable.RegisterValueChangedCallback(evt => item.VariableName = evt.newValue);
        row.Add(variable);

        var remove = new Button(() => {
            m_data.Items.Remove(item);
            RebuildList();
        }) { text = "删除" };
        remove.style.width = 56f;
        row.Add(remove);
        return row;
    }

    private void AddRow() {
        m_data.Items.Add(new ALogCategoryItem { DisplayName = "", VariableName = "" });
        RebuildList();
    }

    private void Generate() {
        try
        {
            ALogCategoryConfig.Generate(m_data);
            m_status.text = $"已生成:\n{ALogCategoryConfig.CSharpPath}\n{ALogCategoryConfig.LuaPath}";
            m_status.style.color = new Color(0.55f, 0.85f, 0.55f);
        }
        catch (System.Exception e)
        {
            m_status.text = e.Message;
            m_status.style.color = new Color(0.95f, 0.45f, 0.4f);
        }
    }

    private static Label HeaderLabel(string text, float grow, float width = 0f) {
        var label = new Label(text);
        label.style.flexGrow = grow;
        if (width > 0f)
        {
            label.style.width = width;
            label.style.flexGrow = 0f;
        }
        label.style.unityFontStyleAndWeight = FontStyle.Bold;
        label.style.marginBottom = 4;
        return label;
    }
}
