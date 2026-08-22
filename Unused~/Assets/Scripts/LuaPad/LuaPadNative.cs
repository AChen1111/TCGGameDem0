using System;
using System.Runtime.InteropServices;
using UnityEngine;

public static class LuaPadNative
{
    [StructLayout(LayoutKind.Sequential)]
    struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct POINT
    {
        public int x;
        public int y;
    }

    [DllImport("user32.dll")]
    static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll")]
    static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    static extern bool ClientToScreen(IntPtr hWnd, ref POINT lpPoint);

    public static Func<IntPtr> FindHwnd;

    public delegate bool MapHost(Rect host, Rect panel, out int x, out int y, out int w, out int h);

    public static MapHost TryMapHost;

    public static IntPtr UnityHwnd()
    {
        if (FindHwnd != null)
        {
            IntPtr hwnd = FindHwnd();
            if (hwnd != IntPtr.Zero)
            {
                return hwnd;
            }
        }
        return GetActiveWindow();
    }

    public static bool TryGetClientSize(IntPtr hwnd, out int width, out int height)
    {
        width = 0;
        height = 0;
        if (hwnd == IntPtr.Zero || !GetClientRect(hwnd, out RECT cr))
        {
            return false;
        }
        width = cr.right - cr.left;
        height = cr.bottom - cr.top;
        return width > 0 && height > 0;
    }

    public static void ClientToScreen(IntPtr hwnd, int x, int y, out int sx, out int sy)
    {
        var p = new POINT { x = x, y = y };
        ClientToScreen(hwnd, ref p);
        sx = p.x;
        sy = p.y;
    }

    public static void ScaleHostToClient(Rect host, Rect gameClient, float panelW, float panelH, out int x, out int y, out int w, out int h)
    {
        float sx = gameClient.width / Mathf.Max(1f, panelW);
        float sy = gameClient.height / Mathf.Max(1f, panelH);
        x = (int)(gameClient.x + host.x * sx);
        y = (int)(gameClient.y + host.y * sy);
        w = Mathf.Max(8, (int)(host.width * sx));
        h = Mathf.Max(8, (int)(host.height * sy));
    }

    public static bool TryHostBounds(Rect host, Rect panel, Rect gameDest, out int x, out int y, out int w, out int h)
    {
        x = 0;
        y = 0;
        w = 0;
        h = 0;
        if (host.width < 16f || host.height < 16f || panel.width < 16f || panel.height < 16f || gameDest.width < 16f || gameDest.height < 16f)
        {
            return false;
        }
        ScaleHostToClient(host, gameDest, panel.width, panel.height, out x, out y, out w, out h);
        return w >= 16 && h >= 16;
    }

    public static float EditorFontSize(float panelHostHeight, float screenHostHeight, float referenceFont = 18f)
    {
        return referenceFont * screenHostHeight / Mathf.Max(1f, panelHostHeight);
    }
}
