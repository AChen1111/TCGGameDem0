using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using XLua;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class ObjectReference
{
    public string name;
    public UnityEngine.Object value;
}

//普通数据类型枚举,用于在Inspector中选择要传入的数据类型
public enum DataValueType
{
    Int,
    Float,
    String,
    Bool
}

//可在Inspector中配置的普通数据类型引用,和ObjectReference作用类似,但传入的是基础数据而非Unity组件
[System.Serializable]
public class DataReference
{
    public string name;
    public DataValueType valueType;

    [ShowIf("valueType", DataValueType.Int)]
    public int intValue;

    [ShowIf("valueType", DataValueType.Float)]
    public float floatValue;

    [ShowIf("valueType", DataValueType.String)]
    public string stringValue;

    [ShowIf("valueType", DataValueType.Bool)]
    public bool boolValue;

    //根据valueType返回对应的值
    public object GetValue()
    {
        switch (valueType)
        {
            case DataValueType.Int: return intValue;
            case DataValueType.Float: return floatValue;
            case DataValueType.String: return stringValue;
            case DataValueType.Bool: return boolValue;
            default: return null;
        }
    }
}
//挂载场景上面根据Type找到对应的lua脚本并初始化,并负责把变量注入到LuaTable中
public class LuaComponet : MonoBehaviour
{
    [SerializeField]
    [Tooltip("lua类型名")]
    private string m_typeName;

    [SerializeField]
    [Tooltip("对象引用")]
    private ObjectReference[] m_objectReferences;

    [SerializeField]
    [Tooltip("普通数据引用")]
    private DataReference[] m_dataReferences;

    private LuaTable m_luaTable;

    //负责存储Lua函数
    private Dictionary<string, Action<LuaTable>> m_onFunctions = new Dictionary<string, Action<LuaTable>>();

    private void Awake() {
        if (LuaManager.Instance == null)
        {
            Debug.LogError("[LuaComponet] LuaManager.Instance is null. Ensure LuaManager Awake runs first (Script Execution Order).");
            return;
        }

        //初始化Lua表
        m_luaTable = LuaManager.Instance.GetLuaTable(m_typeName, gameObject);
        if(m_luaTable == null) {
            Debug.LogError($"LuaTable not found: {m_typeName}");
            return;
        }

        //注入对象
        InitComponent();

        //初始化生命周期函数
        InitOnFunctions();

        //Call Awake
        CallLuaFunction("Awake");
    }

    private void InitComponent() {
        if (m_objectReferences != null)
        {
            foreach (var objectReference in m_objectReferences)
            {
                m_luaTable.Set(objectReference.name, objectReference.value);
            }
        }

        if (m_dataReferences != null)
        {
            foreach (var dataReference in m_dataReferences)
            {
                m_luaTable.Set(dataReference.name, dataReference.GetValue());
            }
        }
    }
    private void Start() {
        CallLuaFunction("Start");
    }
    private void OnDestroy() {
        CallLuaFunction("OnDestroy");
    }
    private void OnEnable() {
        CallLuaFunction("OnEnable");
    }
    private void OnDisable() {
        CallLuaFunction("OnDisable");
    }

    //初始化生命周期函数
    private void InitOnFunctions() {
        m_onFunctions.Add("Awake", m_luaTable.Get<Action<LuaTable>>("Awake"));
        m_onFunctions.Add("Start", m_luaTable.Get<Action<LuaTable>>("Start"));
        m_onFunctions.Add("OnDestroy", m_luaTable.Get<Action<LuaTable>>("OnDestroy"));
        m_onFunctions.Add("OnEnable", m_luaTable.Get<Action<LuaTable>>("OnEnable"));
        m_onFunctions.Add("OnDisable", m_luaTable.Get<Action<LuaTable>>("OnDisable"));
    }
    
    //根据名称调用Lua函数
    public void CallLuaFunction(string functionName) 
    {
        if (m_luaTable == null || m_onFunctions == null)
        {
            return;
        }

        if(m_onFunctions.TryGetValue(functionName, out var action)) {
            action?.Invoke(m_luaTable);
            return;
        }

        var func = m_luaTable.Get<Action<LuaTable>>(functionName);
        if(func != null) {
            m_onFunctions.Add(functionName, func);
            func.Invoke(m_luaTable);
        }
    }

    [Button("重写读取脚本")]
    public void RuntimeReload() {
        LuaManager.Instance.RuntimeReload(m_typeName);
        m_onFunctions.Clear();
        InitOnFunctions();
        InitComponent();
    }

#if UNITY_EDITOR
    //根据m_typeName在LuaRaw目录下查找同名.lua文件并用编辑器打开
    [Button("打开代码文件")]
    public void OpenLuaScript() {
        if (string.IsNullOrEmpty(m_typeName))
        {
            Debug.LogError("[LuaComponet] m_typeName为空,无法定位Lua文件");
            return;
        }

        string assetPath = FindLuaAssetPath(m_typeName);
        if (string.IsNullOrEmpty(assetPath))
        {
            Debug.LogError($"[LuaComponet] 未在LuaRaw目录下找到: {m_typeName}.lua");
            return;
        }

        var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
        AssetDatabase.OpenAsset(asset);
    }

    //在Assets/Scripts/LuaRaw目录下按文件名(不含扩展名)查找.lua文件路径
    private static string FindLuaAssetPath(string typeName) {
        string[] guids = AssetDatabase.FindAssets(typeName, new[] { "Assets/Scripts/LuaRaw" });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.EndsWith(".lua") && System.IO.Path.GetFileNameWithoutExtension(path) == typeName)
            {
                return path;
            }
        }
        return null;
    }
#endif
}
