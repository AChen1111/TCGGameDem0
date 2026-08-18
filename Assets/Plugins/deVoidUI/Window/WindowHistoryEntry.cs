/// <summary>
/// Window 历史栈 / 队列中的一条记录。
/// </summary>
public struct WindowHistoryEntry
{
    public readonly IWindowController Screen;
    public readonly IWindowProperties Properties;

    public WindowHistoryEntry(IWindowController screen, IWindowProperties properties) {
        Screen = screen;
        Properties = properties;
    }

    public void Show() {
        Screen.Show(Properties);
    }
}
