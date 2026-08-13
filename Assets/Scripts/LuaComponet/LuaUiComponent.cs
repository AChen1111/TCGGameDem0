using System;
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
