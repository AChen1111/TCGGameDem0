using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class AddToLuaComponetWindow : EditorWindow
{
    private const string UxmlPath = "Assets/Scripts/LuaComponet/Editor/AddToLuaComponetWindow.uxml";
    private const string UssPath = "Assets/Scripts/LuaComponet/Editor/AddToLuaComponetWindow.uss";

    private Object m_source;
    private List<LuaComponet> m_targets;
    private LuaComponet m_selected;
    private TextField m_nameField;
    private Button m_addButton;

    [MenuItem("GameObject/AddToLuaComponet", false, 0)]
    private static void FromGameObject()
    {
        Open(Selection.activeGameObject);
    }

    [MenuItem("GameObject/AddToLuaComponet", true)]
    private static bool FromGameObjectValidate()
    {
        return Selection.activeGameObject != null;
    }

    [MenuItem("CONTEXT/Component/AddToLuaComponet", false, 0)]
    private static void FromComponent(MenuCommand command)
    {
        Open(command.context);
    }

    public static void Open(Object source)
    {
        var window = CreateInstance<AddToLuaComponetWindow>();
        window.titleContent = new GUIContent("AddToLuaComponet");
        window.m_source = source;
        window.minSize = new Vector2(380f, 360f);
        window.ShowUtility();
    }

    private void CreateGUI()
    {
        var tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UxmlPath);
        tree.CloneTree(rootVisualElement);
        rootVisualElement.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(UssPath));

        GameObject sourceGo = m_source as GameObject ?? ((Component)m_source).gameObject;
        m_targets = new List<LuaComponet>(LuaComponetBinder.FindInTree(sourceGo));

        rootVisualElement.Q<Label>("source-label").text = m_source.name + " (" + m_source.GetType().Name + ")";

        m_nameField = rootVisualElement.Q<TextField>("name-field");
        m_nameField.value = LuaComponetBinder.DefaultName(m_source);

        var list = rootVisualElement.Q<ListView>("target-list");
        list.itemsSource = m_targets;
        list.fixedItemHeight = 22f;
        list.selectionType = SelectionType.Single;
        list.makeItem = () => new Label();
        list.bindItem = (el, i) =>
        {
            var c = m_targets[i];
            string typeName = string.IsNullOrEmpty(c.TypeName) ? "" : " " + c.TypeName;
            ((Label)el).text = c.gameObject.name + " (" + c.GetType().Name + ")" + typeName;
        };
        list.selectionChanged += OnTargetSelected;

        m_addButton = rootVisualElement.Q<Button>("add-button");
        m_addButton.SetEnabled(false);
        m_addButton.clicked += OnAddClicked;
    }

    private void OnTargetSelected(IEnumerable<object> selected)
    {
        m_selected = null;
        foreach (object item in selected)
        {
            m_selected = (LuaComponet)item;
            break;
        }

        m_addButton.SetEnabled(m_selected != null);
        if (m_selected == null)
        {
            return;
        }

        Selection.activeObject = m_selected;
        EditorGUIUtility.PingObject(m_selected);
    }

    private void OnAddClicked()
    {
        LuaComponetBinder.AddReference(m_selected, m_nameField.value, m_source);
        Close();
    }
}
