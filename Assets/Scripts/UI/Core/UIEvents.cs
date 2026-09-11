using AChen.Events;

/// <summary>UI 框架内部事件: 界面之间通过事件请求开关窗口, 不直接持有 UIFrame 引用.</summary>
public static class UIEvent
{
    // 窗口请求携带所属 UIFrame; 同名窗口不会跨 UIFrame 响应.
    public static readonly EventId<UIFrame, WindowOpenRequest> WindowOpenRequested = new EventId<UIFrame, WindowOpenRequest>("UI.WindowOpenRequested");
    public static readonly EventId<IUIScreenController> WindowCloseRequested = new EventId<IUIScreenController>("UI.WindowCloseRequested");
    public static readonly EventId<IUIScreenController> ScreenDestroyed = new EventId<IUIScreenController>("UI.ScreenDestroyed");
}

public readonly struct WindowOpenRequest
{
    public readonly string ScreenId;
    public readonly IWindowProperties Properties;

    public WindowOpenRequest(string screenId, IWindowProperties properties = null)
    {
        ScreenId = screenId;
        Properties = properties;
    }
}
