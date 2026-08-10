using System;
using UnityEngine;
using XLua;
/// <summary>
/// 负责管理把生命周期函数转发到Main.lua
/// </summary>
public class LuaManager : MonoBehaviour
{
    private static LuaManager _instance;
    public static LuaManager Instance => _instance;
    private LuaEnvironment m_luaEnvironment;
    private LuaTable m_mainLuaTable;
    private LuaEnv m_luaEnv;


    //负责获取对应类型的LuaTable
    private Func<string, GameObject, LuaTable> m_onInit;
    //负责运行时重新加载
    private Action<string> m_onRuntimeReload;
    private void Awake() {
        //设置单例
        if(_instance != null)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        //初始化Lua环境
        m_luaEnvironment = new LuaEnvironment();
        m_luaEnvironment.Init();
        m_luaEnv = m_luaEnvironment.LuaEnv;

        //加载Main.lua
        m_luaEnv.DoString("require 'Main'");
        m_mainLuaTable = m_luaEnv.Global.Get<LuaTable>("Main");

    
        //注册初始化函数
        m_onInit = m_mainLuaTable.Get<Func<string, GameObject, LuaTable>>("Init");
        //注册运行时重新加载函数
        m_onRuntimeReload = m_mainLuaTable.Get<Action<string>>("runtimeReload");
    }

    //获取对应类型的LuaTable
    public LuaTable GetLuaTable(string typeName, GameObject gameObject) {
        return m_onInit(typeName, gameObject);
    }


    public void RuntimeReload(string typeName) {
        m_onRuntimeReload?.Invoke(typeName);
    }
}
