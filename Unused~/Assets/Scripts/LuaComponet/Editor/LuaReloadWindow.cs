using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Play模式下热重载Lua代码:列出moduleList中的所有模块,支持单个重载与全部重载。
/// 重载只更新代码(模块表原地更新),实例上已有的运行时状态会被保留。
/// </summary>
public class LuaReloadWindow : EditorWindow
{
    private const string ReloadAllMenu = "Tools/Lua/重载全部Lua %#r";

    private List<string> m_moduleNames;
    private Vector2 m_scroll;

    [MenuItem("Tools/Lua/Lua重载窗口")]
    private static void Open() {
        GetWindow<LuaReloadWindow>("Lua重载");
    }

    [MenuItem(ReloadAllMenu)]
    private static void ReloadAllMenuItem() {
        ReloadAll();
    }

    [MenuItem(ReloadAllMenu, true)]
    private static bool ReloadAllMenuItemValidate() {
        return Application.isPlaying && LuaManager.Instance != null;
    }

    private void OnInspectorUpdate() {
        Repaint();
    }

    private void OnGUI() {
        if (!Application.isPlaying || LuaManager.Instance == null)
        {
            EditorGUILayout.HelpBox("进入Play模式后才能重载Lua", MessageType.Info);
            m_moduleNames = null;
            return;
        }

        if (m_moduleNames == null)
        {
            m_moduleNames = LuaManager.Instance.GetModuleNames();
        }

        if (GUILayout.Button("重载全部 (Ctrl+Shift+R)", GUILayout.Height(28)))
        {
            ReloadAll();
            m_moduleNames = LuaManager.Instance.GetModuleNames();
        }

        m_scroll = EditorGUILayout.BeginScrollView(m_scroll);
        foreach (string moduleName in m_moduleNames)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(moduleName);
            if (GUILayout.Button("重载", GUILayout.Width(60)))
            {
                ReloadSingle(moduleName);
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();
    }

    private static void ReloadAll() {
        LuaManager.Instance.RuntimeReloadAll();
        Debug.Log($"[LuaReloadWindow] 已重载全部Lua模块,刷新 {RefreshInstances(null)} 个实例");
    }

    private static void ReloadSingle(string moduleName) {
        LuaManager.Instance.RuntimeReload(moduleName);
        Debug.Log($"[LuaReloadWindow] 已重载 {moduleName},刷新 {RefreshInstances(moduleName)} 个实例");
    }

    //刷新场景中的LuaComponet实例,moduleName为null时刷新全部
    private static int RefreshInstances(string moduleName) {
        int count = 0;
        //用FindObjectsOfTypeAll才能覆盖未激活的对象,但它会带出Prefab资产上的组件,需要用scene过滤
        foreach (LuaComponet componet in Resources.FindObjectsOfTypeAll<LuaComponet>())
        {
            if (!componet.gameObject.scene.IsValid())
            {
                continue;
            }
            if (moduleName != null && componet.TypeName != moduleName)
            {
                continue;
            }
            componet.RefreshInstance();
            count++;
        }
        return count;
    }
}
