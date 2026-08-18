/// <summary>
/// 不需要自定义 Properties 的 Panel 基类。
/// </summary>
public abstract class APanelController : APanelController<PanelProperties> { }

/// <summary>
/// Panel 基类。
/// </summary>
public abstract class APanelController<T> : AUIScreenController<T>, IPanelController where T : IPanelProperties {
    public PanelPriority Priority {
        get {
            if (Properties != null) {
                return Properties.Priority;
            }
            else {
                return PanelPriority.None;
            }
        }
    }

    protected sealed override void SetProperties(T props) {
        base.SetProperties(props);
    }
}
