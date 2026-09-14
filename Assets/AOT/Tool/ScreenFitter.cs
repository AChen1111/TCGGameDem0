using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 锁定 16:9 设计分辨率. 更高或更宽的屏幕由 CanvasScaler Expand 留出额外空间, 不改设计宽度.
/// </summary>
public class ScreenFitter : MonoBehaviour
{
    const float DesignWidth = 1706f;
    const float DesignHeight = 960f;

    public CanvasScaler[] m_CanvasScaler;
    public Camera m_Camera;

    void Awake()
    {
        DoFit();
    }

    void DoFit()
    {
        Vector2 design = new Vector2(DesignWidth, DesignHeight);
        if (m_CanvasScaler != null)
        {
            for (int i = 0; i < m_CanvasScaler.Length; i++)
            {
                CanvasScaler scaler = m_CanvasScaler[i];
                if (scaler != null)
                {
                    scaler.referenceResolution = design;
                }
            }
        }

        if (m_Camera != null)
        {
            m_Camera.orthographicSize = DesignHeight / (2f * 100f);
        }
    }
}
