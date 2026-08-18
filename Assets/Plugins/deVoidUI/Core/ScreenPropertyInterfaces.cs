/// <summary>
/// 界面属性基接口。
/// </summary>
public interface IScreenProperties { }

/// <summary>
/// Panel 属性。
/// </summary>
public interface IPanelProperties : IScreenProperties
{
    PanelPriority Priority { get; set; }
}

/// <summary>
/// Window 属性。
/// </summary>
public interface IWindowProperties : IScreenProperties
{
    WindowPriority WindowQueuePriority { get; set; }
    bool HideOnForegroundLost { get; set; }
    bool IsPopup { get; set; }
    bool SuppressPrefabProperties { get; set; }
}
