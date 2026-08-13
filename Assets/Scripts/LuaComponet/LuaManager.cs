using System;
using System.Collections.Generic;
using UnityEngine;
using XLua;
/// <summary>
/// 负责管理把生命周期函数转发到Main.lua
/// </summary>
public class LuaManager : PersistentMonoSingleton<LuaManager>
{
    private LuaEnvironment m_luaEnvironment;
    private LuaTable m_mainLuaTable;
    private LuaEnv m_luaEnv;


    //负责获取对应类型的LuaTable
    private Func<string, GameObject, LuaTable> m_onInit;
    //负责运行时重新加载
    private Action<string> m_onRuntimeReload;
    //负责运行时重新加载全部模块
    private Action m_onRuntimeReloadAll;
    //负责获取全部模块名
    private Func<LuaTable> m_onGetModuleNames;
    protected override void OnInit() {
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
        m_onRuntimeReloadAll = m_mainLuaTable.Get<Action>("runtimeReloadAll");
        m_onGetModuleNames = m_mainLuaTable.Get<Func<LuaTable>>("getModuleNames");

        m_mainLuaTable.Get<Action>("OnLuaReady")?.Invoke();

        IsDone = true;
    }

    //获取对应类型的LuaTable
    public LuaTable GetLuaTable(string typeName, GameObject gameObject) {
        return m_onInit(typeName, gameObject);
    }


    public void RuntimeReload(string typeName) {
        //重载前刷新文件索引,保证运行期间新增的.lua能被require到
        m_luaEnvironment.BuildFileIndex();
        try
        {
            m_onRuntimeReload?.Invoke(typeName);
        }
        catch (Exception e)
        {
            //在工程脚本里打日志,Console双击才会走OnOpenAsset,从而跳到真正的.lua
            //若异常冒泡到Odin,LogException的调用点在工程外的DefaultMethodDrawer,双击打不开Lua
            Debug.LogException(e);
        }
    }

    public void RuntimeReloadAll() {
        m_luaEnvironment.BuildFileIndex();
        m_onRuntimeReloadAll?.Invoke();
    }

    public void DoString(string chunk)
    {
        m_luaEnv.DoString(chunk);
    }

    //获取全部模块名,供Editor侧列出
    public List<string> GetModuleNames() {
        var names = new List<string>();
        using (LuaTable table = m_onGetModuleNames())
        {
            for (int i = 1; i <= table.Length; i++)
            {
                names.Add(table.Get<int, string>(i));
            }
        }
        return names;
    }
}
