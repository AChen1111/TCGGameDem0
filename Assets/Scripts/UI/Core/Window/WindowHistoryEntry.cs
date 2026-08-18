/// <summary>
/// Window 历史栈 / 队列中的一条记录。
/// </summary>
public struct WindowHistoryEntry
{
    public readonly IWindowController Screen;

    public WindowHistoryEntry(IWindowController screen) {
        Screen = screen;
    }

    public void Show() {
        Screen.Show();
    }
}
