using UnityEngine;

/// <summary>
/// 所有 Panel 共用的属性。
/// </summary>
[System.Serializable] 
public class PanelProperties : IPanelProperties {
    [SerializeField] 
    [Tooltip("按优先级挂到不同 para-layer。可在 Panel Layer 上配置")]
    private PanelPriority priority;

    public PanelPriority Priority {
        get { return priority; }
        set { priority = value; }
    }
}
