/// <summary>
/// 不需要自定义 Properties 的 Window 基类。
/// </summary>
public abstract class AWindowController : AWindowController<WindowProperties> { }

/// <summary>
/// Window 基类。不需要特殊 Properties 时直接继承无泛型版本。
/// </summary>
public abstract class AWindowController<TProps> : AUIScreenController<TProps>, IWindowController
    where TProps : IWindowProperties
{
    public bool HideOnForegroundLost {
        get { return Properties.HideOnForegroundLost; }
    }

    public bool IsPopup {
        get { return Properties.IsPopup; }
    }

    public WindowPriority WindowPriority {
        get { return Properties.WindowQueuePriority; }
    }

    /// <summary>
    /// 给 Inspector 绑按钮用的关闭入口。真正出栈清理走 OnClose。
    /// </summary>
    public virtual void UI_Close() {
        CloseRequest(this);
    }

    protected sealed override void SetProperties(TProps props) {
        if (props != null) {
            if (!props.SuppressPrefabProperties) {
                props.HideOnForegroundLost = Properties.HideOnForegroundLost;
                props.WindowQueuePriority = Properties.WindowQueuePriority;
                props.IsPopup = Properties.IsPopup;
            }

            Properties = props;
        }
    }

    protected override void HierarchyFixOnShow() {
        transform.SetAsLastSibling();
    }
}
