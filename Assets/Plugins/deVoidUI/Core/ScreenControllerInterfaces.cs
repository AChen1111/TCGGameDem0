using System;

/// <summary>
/// 所有 UI 界面都要实现的接口。
/// </summary>
public interface IUIScreenController
{
    string ScreenId { get; set; }
    bool IsVisible { get; }
    bool DestroyOnClose { get; }

    void Show(IScreenProperties props = null);
    void Hide(bool animate = true);
    void Close(bool animate = true);

    Action<IUIScreenController> InTransitionFinished { get; set; }
    Action<IUIScreenController> OutTransitionFinished { get; set; }
    Action<IUIScreenController> CloseRequest { get; set; }
    Action<IUIScreenController> ScreenDestroyed { get; set; }
}

/// <summary>
/// Window 接口。
/// </summary>
public interface IWindowController : IUIScreenController
{
    bool HideOnForegroundLost { get; }
    bool IsPopup { get; }
    WindowPriority WindowPriority { get; }
}

/// <summary>
/// Panel 接口。
/// </summary>
public interface IPanelController : IUIScreenController
{
    PanelPriority Priority { get; }
}
