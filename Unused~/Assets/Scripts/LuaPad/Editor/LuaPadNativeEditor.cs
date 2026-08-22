using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class LuaPadNativeEditor
{
    static LuaPadNativeEditor()
    {
        LuaPadNative.FindHwnd = GameViewHwnd;
        LuaPadNative.TryMapHost = TryMapGameView;
    }

    static IntPtr GameViewHwnd()
    {
        EditorWindow gv = GameViewWindow();
        if (gv == null)
        {
            return IntPtr.Zero;
        }
        FieldInfo parentField = typeof(EditorWindow).GetField("m_Parent", BindingFlags.Instance | BindingFlags.NonPublic);
        object parent = parentField?.GetValue(gv);
        if (parent == null)
        {
            return IntPtr.Zero;
        }
        PropertyInfo handleProp = parent.GetType().GetProperty("nativeHandle", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (handleProp == null)
        {
            return IntPtr.Zero;
        }
        object v = handleProp.GetValue(parent);
        return v is IntPtr ptr ? ptr : IntPtr.Zero;
    }

    static bool TryMapGameView(Rect host, Rect panel, out int x, out int y, out int w, out int h)
    {
        x = 0;
        y = 0;
        w = 0;
        h = 0;
        EditorWindow gv = GameViewWindow();
        if (gv == null)
        {
            return false;
        }
        FieldInfo parentField = typeof(EditorWindow).GetField("m_Parent", BindingFlags.Instance | BindingFlags.NonPublic);
        object parent = parentField?.GetValue(gv);
        if (parent == null)
        {
            return false;
        }
        PropertyInfo screenPosProp = parent.GetType().GetProperty("screenPosition", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (screenPosProp == null)
        {
            return false;
        }
        var viewScreen = (Rect)screenPosProp.GetValue(parent);
        Rect gameInView = viewScreen;
        PropertyInfo tip = gv.GetType().GetProperty("targetInParent", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        if (tip != null)
        {
            var t = (Rect)tip.GetValue(gv);
            if (t.width >= 16f && t.height >= 16f)
            {
                gameInView = new Rect(viewScreen.x + t.x, viewScreen.y + t.y, t.width, t.height);
            }
        }
        float ppp = EditorGUIUtility.pixelsPerPoint;
        var gameScreen = new Rect(gameInView.x * ppp, gameInView.y * ppp, gameInView.width * ppp, gameInView.height * ppp);
        return LuaPadNative.TryHostBounds(host, panel, gameScreen, out x, out y, out w, out h);
    }

    static EditorWindow GameViewWindow()
    {
        Type gameViewType = typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");
        UnityEngine.Object[] windows = Resources.FindObjectsOfTypeAll(gameViewType);
        return windows.Length == 0 ? null : windows[0] as EditorWindow;
    }

    [MenuItem("Tools/Lua/Publish LuaPadBrowser")]
    public static void PublishBrowser()
    {
        if (File.Exists(LuaPadBrowserInstaller.HelperPath))
        {
            File.Delete(LuaPadBrowserInstaller.HelperPath);
        }
        Debug.Log("[LuaPad] " + LuaPadBrowserInstaller.EnsureHelper());
    }
}
