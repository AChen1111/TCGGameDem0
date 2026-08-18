using UnityEngine;

/// <summary>
/// 所有 Window 共用的属性。
/// </summary>
[System.Serializable] 
public class WindowProperties : IWindowProperties {
    [SerializeField] 
    protected bool hideOnForegroundLost = true;

    [SerializeField] 
    protected WindowPriority windowQueuePriority = WindowPriority.ForceForeground;

    [SerializeField]
    protected bool isPopup = false;

    public WindowProperties() {
        hideOnForegroundLost = true;
        windowQueuePriority = WindowPriority.ForceForeground;
        isPopup = false;
    }

    /// <summary>已有 Window 打开时，本窗是立刻盖上去还是排队。</summary>
    /// <value>ForceForeground 立刻打开；Enqueue 等当前窗关闭后再开。</value>
    public WindowPriority WindowQueuePriority {
        get { return windowQueuePriority; }
        set { windowQueuePriority = value; }
    }

    /// <summary>被其他 Window 盖住时是否隐藏自己（OnHide）。</summary>
    public bool HideOnForegroundLost {
        get { return hideOnForegroundLost; }
        set { hideOnForegroundLost = value; }
    }

    /// <summary>Open 传入属性时，是否不覆盖 Prefab 上配的默认值。</summary>
    public bool SuppressPrefabProperties { get; set; }

    /// <summary>弹窗：盖在其他 Window 之上，并带暗底。</summary>
    public bool IsPopup {
        get { return isPopup; }
        set { isPopup = value; }
    }

    public WindowProperties(bool suppressPrefabProperties = false) {
        WindowQueuePriority = WindowPriority.ForceForeground;
        HideOnForegroundLost = false;
        SuppressPrefabProperties = suppressPrefabProperties;
    }

    public WindowProperties(WindowPriority priority, bool hideOnForegroundLost = false, bool suppressPrefabProperties = false) {
        WindowQueuePriority = priority;
        HideOnForegroundLost = hideOnForegroundLost;
        SuppressPrefabProperties = suppressPrefabProperties;
    }
}
