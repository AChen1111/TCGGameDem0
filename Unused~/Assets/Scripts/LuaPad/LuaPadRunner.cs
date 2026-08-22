using System;
using System.Text;
using UnityEngine;

public readonly struct LuaPadRunResult
{
    public readonly bool Success;
    public readonly string Output;
    public readonly string Error;

    public LuaPadRunResult(bool success, string output, string error)
    {
        Success = success;
        Output = output ?? string.Empty;
        Error = error ?? string.Empty;
    }
}

public static class LuaPadRunner
{
    public static LuaPadRunResult Run(string chunk, Action<string> doString)
    {
        var sb = new StringBuilder();
        Application.logMessageReceived += OnLog;
        try
        {
            doString(chunk);
            return new LuaPadRunResult(true, sb.ToString(), string.Empty);
        }
        catch (Exception e)
        {
            return new LuaPadRunResult(false, sb.ToString(), e.Message);
        }
        finally
        {
            Application.logMessageReceived -= OnLog;
        }

        void OnLog(string condition, string stackTrace, LogType type)
        {
            if (sb.Length > 0)
            {
                sb.Append('\n');
            }
            sb.Append(condition);
        }
    }

    public static LuaPadRunResult RunInGame(string chunk)
    {
        LuaManager lua = UnityEngine.Object.FindFirstObjectByType<LuaManager>();
        if (lua == null)
        {
            lua = LuaManager.Instance;
            if (!lua.IsDone)
            {
                lua.BeginInit();
            }
        }
        if (lua == null || !lua.IsDone)
        {
            return new LuaPadRunResult(false, string.Empty, "Lua 正在初始化，请稍后再运行");
        }
        return Run(chunk, lua.DoString);
    }
}
