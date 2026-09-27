using UnityEngine;


/// <summary>
/// 保持原比例缩放
/// </summary>
[ExecuteAlways]
public class AspectScaler : MonoBehaviour
{
    public enum ScaleMode
    {
        FitSmaller, //只缩小,不放大
        MatchRatio, //按比例缩放
    }

    [SerializeField] Vector2 m_designResolution = new Vector2(1707, 960);
    [SerializeField] ScaleMode m_scaleMode = ScaleMode.FitSmaller;

    public Vector2 DesignResolution
    {
        get => m_designResolution;
        set => m_designResolution = value;
    }

    public ScaleMode Mode
    {
        get => m_scaleMode;
        set => m_scaleMode = value;
    }

    void OnEnable()
    {
        Canvas.willRenderCanvases += Apply;
        Apply();
    }

    void OnDisable()
    {
        Canvas.willRenderCanvases -= Apply;
    }

    public void Apply()
    {
        if (Screen.height <= 0 || m_designResolution.y <= 0)
        {
            return;
        }

        float designRatio = m_designResolution.x / m_designResolution.y;
        float screenRatio = (float)Screen.width / Screen.height;
        float scale = CalculateScale(screenRatio, designRatio, m_scaleMode);
        transform.localScale = new Vector3(scale, scale, 1f);
    }

    public static float CalculateScale(float currentRatio, float designRatio, ScaleMode mode)
    {
        float factor = currentRatio / designRatio;
        return mode == ScaleMode.FitSmaller ? Mathf.Min(1f, factor) : factor;
    }
}
