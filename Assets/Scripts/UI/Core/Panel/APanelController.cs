using UnityEngine;

/// <summary>
/// Panel 基类。
/// </summary>
public abstract class APanelController : AUIScreenController, IPanelController {
    [SerializeField]
    [Tooltip("按优先级挂到不同 para-layer。可在 Panel Layer 上配置")]
    PanelPriority priority;

    public PanelPriority Priority {
        get { return priority; }
    }
}
