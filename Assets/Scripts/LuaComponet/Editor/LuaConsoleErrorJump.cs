using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditorInternal;
using UnityEngine;

/// <summary>
/// 劫持Unity Console双击"打开资源"的行为:
/// XLua抛出的报错都是LuaException,双击时Unity默认会打开ThrowExceptionFromError所在的C#源码(没有意义)。
/// 这里改为解析报错信息里携带的"lua文件绝对路径:行号",直接跳转到真正出错的Lua源码。
/// 依赖 LuaEnvironment.CustomLoader 把chunkname设置为源文件绝对路径这一约定。
/// 可在 Tools/Lua/启用Lua报错跳转 菜单开关此功能。
/// </summary>
public static class LuaConsoleErrorJump
{
    private const string MenuPath = "Tools/Lua/启用Lua报错跳转";
    private const string PrefsKey = "LuaConsoleErrorJump.Enabled";

    //匹配报错信息里的"绝对路径:行号:"片段,例如 D:/.../BaseUI.lua:12: attempt to...
    private static readonly Regex s_luaErrorPattern = new Regex(@"([A-Za-z]:[\\/][^:]+?\.lua):(\d+):", RegexOptions.Compiled);

    private static bool Enabled => EditorPrefs.GetBool(PrefsKey, true);

    [MenuItem(MenuPath)]
    private static void ToggleEnabled() {
        EditorPrefs.SetBool(PrefsKey, !Enabled);
    }

    [MenuItem(MenuPath, true)]
    private static bool ToggleEnabledValidate() {
        Menu.SetChecked(MenuPath, Enabled);
        return true;
    }

    // Unity 6.5+ 只认 EntityId 签名;旧的 int instanceID 不会被调用。
    // line>0 才拦截:Console 双击会带上行号,Project 窗口双击资源时 line=0。
    [OnOpenAsset(0)]
    private static bool OnOpenAsset(EntityId entityId, int line) {
        if (!Enabled || line <= 0)
        {
            return false;
        }

        string activeLogText = GetActiveConsoleLogText();
        if (!TryParseLuaLocation(activeLogText, out string path, out int luaLine))
        {
            return false;
        }

        JumpToLuaFile(path, luaLine);
        return true;
    }

    public static bool TryParseLuaLocation(string logText, out string path, out int line) {
        path = null;
        line = 0;
        if (string.IsNullOrEmpty(logText))
        {
            return false;
        }

        Match match = s_luaErrorPattern.Match(logText);
        if (!match.Success)
        {
            return false;
        }

        path = match.Groups[1].Value;
        line = int.Parse(match.Groups[2].Value);
        return true;
    }

    //通过反射读取Console窗口当前选中日志的完整文本,Unity没有对外公开这个信息
    private static string GetActiveConsoleLogText() {
        Type consoleWindowType = Type.GetType("UnityEditor.ConsoleWindow,UnityEditor");
        FieldInfo activeTextField = consoleWindowType?.GetField("m_ActiveText", BindingFlags.NonPublic | BindingFlags.Instance);
        if (activeTextField == null)
        {
            return null;
        }

        UnityEngine.Object[] consoleWindows = Resources.FindObjectsOfTypeAll(consoleWindowType);
        if (consoleWindows.Length == 0)
        {
            return null;
        }

        return activeTextField.GetValue(consoleWindows[0]) as string;
    }

    //用当前配置的外部代码编辑器打开Lua文件并跳转到指定行,适用于任意扩展名(Lua不是Unity认识的脚本资源)
    private static void JumpToLuaFile(string absolutePath, int line) {
        string path = absolutePath.Replace('\\', '/');
        if (!File.Exists(path))
        {
            Debug.LogWarning($"[LuaConsoleErrorJump] 文件不存在: {path}");
            return;
        }

        if (!InternalEditorUtility.OpenFileAtLineExternal(path, line))
        {
            Debug.LogWarning($"[LuaConsoleErrorJump] 打开失败,请检查Preferences > External Tools中的代码编辑器配置: {path}:{line}");
        }
    }
}
