using System.Collections.Generic;
using UnityEngine;

using AChen.Events;

/// <summary>
/// 管理 Window。有历史栈和队列，同一时间只有一个可交互，含弹窗。
/// </summary>
public class WindowUILayer : AUILayer<IWindowController>
{
    [SerializeField] private WindowParaLayer priorityParaLayer = null;

    public IWindowController CurrentWindow { get; private set; }

    private Queue<WindowHistoryEntry> windowQueue;
    private Stack<WindowHistoryEntry> windowHistory;

    public override void Initialize() {
        base.Initialize();
        windowQueue = new Queue<WindowHistoryEntry>();
        windowHistory = new Stack<WindowHistoryEntry>();
        EventCenter.RemoveListener(UIEvent.WindowCloseRequested, OnCloseRequestedByWindow);
        EventCenter.AddListener(UIEvent.WindowCloseRequested, OnCloseRequestedByWindow);
    }

    protected override void OnDestroy() {
        EventCenter.RemoveListener(UIEvent.WindowCloseRequested, OnCloseRequestedByWindow);
        base.OnDestroy();
    }

    public override void ShowScreen(IWindowController screen) {
        ShowScreen<IScreenProperties>(screen, null);
    }

    public override void ShowScreen<TProperties>(IWindowController screen, TProperties properties) {
        if (ShouldEnqueue(screen)) {
            windowQueue.Enqueue(new WindowHistoryEntry(screen, properties));
        }
        else {
            DoShow(new WindowHistoryEntry(screen, properties));
        }
    }

    public override void HideScreen(IWindowController screen) {
        if (screen == CurrentWindow) {
            windowHistory.Pop();
            screen.Close();

            CurrentWindow = null;

            if (screen.IsPopup) {
                priorityParaLayer.RefreshDarken();
            }

            if (windowQueue.Count > 0) {
                DoShow(windowQueue.Dequeue());
            }
            else if (windowHistory.Count > 0) {
                DoShow(windowHistory.Pop());
            }
        }
        else {
            Debug.LogError(
                string.Format(
                    "[WindowUILayer] Hide requested on WindowId {0} but that's not the currently open one ({1})! Ignoring request.",
                    screen.ScreenId, CurrentWindow != null ? CurrentWindow.ScreenId : "current is null"));
        }
    }

    public override void HideAll() {
        var screens = new List<IWindowController>(registeredScreens.Values);
        for (int i = 0; i < screens.Count; i++) {
            screens[i].Close();
        }
        CurrentWindow = null;
        priorityParaLayer.RefreshDarken();
        windowHistory.Clear();
        // 排队中的窗口随全部关闭一起丢弃, 否则下一次打开会先弹出过期的排队项.
        windowQueue.Clear();
    }

    public override void ReparentScreen(IUIScreenController controller, Transform screenTransform) {
        IWindowController window = controller as IWindowController;

        if (window == null) {
            Debug.LogError("[WindowUILayer] Screen " + screenTransform.name + " is not a Window!");
        }
        else {
            if (window.IsPopup) {
                priorityParaLayer.AddScreen(screenTransform);
                return;
            }
        }

        base.ReparentScreen(controller, screenTransform);
    }

    private bool ShouldEnqueue(IWindowController controller) {
        if (CurrentWindow == null && windowQueue.Count == 0) {
            return false;
        }

        return controller.WindowPriority != WindowPriority.ForceForeground;
    }

    private void DoShow(WindowHistoryEntry windowEntry) {
        if (CurrentWindow == windowEntry.Screen) {
            Debug.LogWarning(
                string.Format(
                    "[WindowUILayer] The requested WindowId ({0}) is already open! This will add a duplicate to the " +
                    "history and might cause inconsistent behaviour. It is recommended that if you need to open the same" +
                    "screen multiple times (eg: when implementing a warning message pop-up), it closes itself upon the player input" +
                    "that triggers the continuation of the flow."
                    , CurrentWindow.ScreenId));
        }
        else if (CurrentWindow != null
                 && CurrentWindow.HideOnForegroundLost
                 && !windowEntry.Screen.IsPopup) {
            CurrentWindow.Hide();
        }

        windowHistory.Push(windowEntry);

        if (windowEntry.Screen.IsPopup) {
            priorityParaLayer.DarkenBG();
        }

        windowEntry.Show();

        CurrentWindow = windowEntry.Screen;
    }

    private void OnCloseRequestedByWindow(IUIScreenController screen) {
        if (string.IsNullOrEmpty(screen.ScreenId)
            || !registeredScreens.TryGetValue(screen.ScreenId, out IWindowController owned)
            || !ReferenceEquals(owned, screen)) return;
        HideScreen(owned);
    }
}
