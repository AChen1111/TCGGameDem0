using System;
using System.Reflection;
using UnityEditor;

public static class RestartBackend
{
    const string ControllerName = "BackendServiceController";

    public static string Status()
    {
        Invoke(ControllerName, "Refresh");
        return Describe();
    }

    public static string Start()
    {
        Invoke(ControllerName, "Refresh");
        bool started = EditorApplication.ExecuteMenuItem("Tools/后端服务/启动");
        return "menuStart=" + started + "; " + Describe();
    }

    public static string Stop()
    {
        bool stopped = EditorApplication.ExecuteMenuItem("Tools/后端服务/关闭");
        return "menuStop=" + stopped + "; " + Describe();
    }

    static string Describe()
    {
        Type type = FindType(ControllerName);
        object state = type.GetProperty("State", BindingFlags.Public | BindingFlags.Static).GetValue(null);
        object canStart = type.GetProperty("CanStart", BindingFlags.Public | BindingFlags.Static).GetValue(null);
        object canStop = type.GetProperty("CanStop", BindingFlags.Public | BindingFlags.Static).GetValue(null);
        object lastError = type.GetProperty("LastError", BindingFlags.Public | BindingFlags.Static).GetValue(null);
        return
            "state=" + state +
            "; canStart=" + canStart +
            "; canStop=" + canStop +
            "; hasKey=" + PublishKeyProvider.HasConfiguredKey +
            "; lastError=" + lastError;
    }

    static void Invoke(string typeName, string methodName)
    {
        Type type = FindType(typeName);
        type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static).Invoke(null, null);
    }

    static Type FindType(string typeName)
    {
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type type = assembly.GetType(typeName);
            if (type != null)
            {
                return type;
            }
        }

        throw new Exception("找不到类型 " + typeName);
    }
}
