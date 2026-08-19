using System;
using System.Collections.Generic;
using System.Text;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XLua;

public enum LuaUiScreenKind
{
    Panel = 0,
    Window = 1
}

[Serializable]
public class UiBindEntry
{
    public string luaName;
    public GameObject target;
    public Component component;
}

public class LuaUiComponent : LuaComponet
{
    [SerializeField]
    [Tooltip("指定物体并转发组件到Lua字段")]
    private UiBindEntry[] m_uiBinds;

    [SerializeField]
    private LuaUiScreenKind m_screenKind;

    [SerializeField]
    private bool m_isPopup;

    [SerializeField]
    private bool m_hideOnForegroundLost = true;

    [SerializeField]
    private bool m_forceForeground = true;

    [SerializeField]
    private int m_panelPriority;

    public LuaUiScreenKind ScreenKind => m_screenKind;
    public bool IsPopup => m_isPopup;
    public bool HideOnForegroundLost => m_hideOnForegroundLost;
    public bool ForceForeground => m_forceForeground;
    public int PanelPriority => m_panelPriority;

    protected override void InitComponent()
    {
        base.InitComponent();
        if (m_uiBinds != null)
        {
            foreach (var bind in m_uiBinds)
            {
                object value = bind.component != null ? bind.component : (object)bind.target;
                LuaTable.Set(bind.luaName, value);
            }
        }
        LuaTable.Set("m_uiComp", this);
        LuaTable.Set("m_nScreenKind", (int)m_screenKind);
        LuaTable.Set("m_bIsPopup", m_isPopup);
        LuaTable.Set("m_bHideOnForegroundLost", m_hideOnForegroundLost);
        LuaTable.Set("m_bForceForeground", m_forceForeground);
        LuaTable.Set("m_nPanelPriority", m_panelPriority);
        LuaTable.Set("m_strScreenId", TypeName);
    }

    public void AddClick(Button button, string methodName)
    {
        button.onClick.AddListener(() => CallLuaFunction(methodName));
    }

    [Button("收集UI引用")]
    public void CollectUiBinds()
    {
        var binds = BuildUiBindsFromChildren();
#if UNITY_EDITOR
        var so = new UnityEditor.SerializedObject(this);
        var prop = so.FindProperty("m_uiBinds");
        prop.arraySize = binds.Length;
        for (int i = 0; i < binds.Length; i++)
        {
            var elem = prop.GetArrayElementAtIndex(i);
            elem.FindPropertyRelative("luaName").stringValue = binds[i].luaName;
            elem.FindPropertyRelative("target").objectReferenceValue = binds[i].target;
            elem.FindPropertyRelative("component").objectReferenceValue = binds[i].component;
        }
        so.ApplyModifiedProperties();
#else
        m_uiBinds = binds;
#endif
    }

    [Button("复制引用")]
    public void CopyUiBindEmmyLua()
    {
        string text = BuildUiBindEmmyLua();
#if UNITY_EDITOR
        UnityEditor.EditorGUIUtility.systemCopyBuffer = text;
#endif
    }

    public string BuildUiBindEmmyLua()
    {
        UiBindEntry[] binds = CurrentUiBinds();
        if (binds == null || binds.Length == 0)
        {
            return "";
        }

        var sb = new StringBuilder();
        for (int i = 0; i < binds.Length; i++)
        {
            UiBindEntry bind = binds[i];
            Type type = bind.component != null ? bind.component.GetType() : typeof(GameObject);
            sb.Append("---@field ").Append(bind.luaName).Append(' ').Append(EmmyTypeName(type)).Append('\n');
        }
        return sb.ToString();
    }

    private UiBindEntry[] CurrentUiBinds()
    {
#if UNITY_EDITOR
        var so = new UnityEditor.SerializedObject(this);
        var prop = so.FindProperty("m_uiBinds");
        var binds = new UiBindEntry[prop.arraySize];
        for (int i = 0; i < prop.arraySize; i++)
        {
            var elem = prop.GetArrayElementAtIndex(i);
            binds[i] = new UiBindEntry
            {
                luaName = elem.FindPropertyRelative("luaName").stringValue,
                target = (GameObject)elem.FindPropertyRelative("target").objectReferenceValue,
                component = (Component)elem.FindPropertyRelative("component").objectReferenceValue
            };
        }
        return binds;
#else
        return m_uiBinds;
#endif
    }

    private static string EmmyTypeName(Type type)
    {
        string name = type.FullName;
        int tick = name.IndexOf('`');
        if (tick >= 0)
        {
            name = name.Substring(0, tick);
        }
        return name.Replace('+', '.');
    }

    private static readonly Dictionary<string, Type> s_prefixTypes = new Dictionary<string, Type>
    {
        { "Btn", typeof(Button) },
        { "Img", typeof(Image) },
        { "Txt", typeof(TextMeshProUGUI) },
        { "Tog", typeof(Toggle) },
        { "Sld", typeof(Slider) },
        { "Inp", typeof(TMP_InputField) },
        { "Scr", typeof(ScrollRect) },
        { "Raw", typeof(RawImage) },
        { "Drop", typeof(TMP_Dropdown) },
        { "Go", null },
    };

    private UiBindEntry[] BuildUiBindsFromChildren()
    {
        var list = new List<UiBindEntry>();
        var transforms = GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < transforms.Length; i++)
        {
            Transform child = transforms[i];
            if (child == transform)
            {
                continue;
            }

            string objectName = child.name;
            int split = objectName.IndexOf('_');
            if (split <= 0 || split == objectName.Length - 1)
            {
                continue;
            }

            string prefix = objectName.Substring(0, split);
            if (!s_prefixTypes.TryGetValue(prefix, out Type type))
            {
                continue;
            }

            Component component = null;
            if (type != null)
            {
                component = child.GetComponent(type);
                if (component == null)
                {
                    continue;
                }
            }

            string luaName = "m_" + prefix + objectName.Substring(split + 1);
            var entry = new UiBindEntry
            {
                luaName = luaName,
                target = child.gameObject,
                component = component
            };
            int existing = list.FindIndex(e => e.luaName == luaName);
            if (existing >= 0)
            {
                list[existing] = entry;
            }
            else
            {
                list.Add(entry);
            }
        }
        return list.ToArray();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (m_uiBinds == null)
        {
            return;
        }
        foreach (var bind in m_uiBinds)
        {
            if (bind.component != null)
            {
                bind.target = bind.component.gameObject;
            }
        }
    }
#endif
}
