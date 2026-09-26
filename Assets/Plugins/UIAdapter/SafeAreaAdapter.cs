// 项目修改: 移除 Feif.UI 命名空间, 按所属 Canvas 转换安全区并避免编辑态自动修改布局.
using UnityEngine;
using UnityEngine.UI;

public class SafeAreaAdapter : AdapterBase
{
    [Header("是否每一帧都计算")]
    public bool CalculateEveryFrame = false;
    private RectTransform rect;
    private static CanvasScaler scaler;
    private bool needsInitialAdaptation;

    public static void Init(CanvasScaler scaler)
    {
        SafeAreaAdapter.scaler = scaler;
    }

    private void OnEnable()
    {
        if (!Application.IsPlaying(gameObject)) return;

        needsInitialAdaptation = true;
        Canvas.willRenderCanvases += OnWillRenderCanvases;
        Adapt();
    }

    private void OnDisable()
    {
        Canvas.willRenderCanvases -= OnWillRenderCanvases;
    }

    private void OnWillRenderCanvases()
    {
        if (!Application.IsPlaying(gameObject)) return;

        if (needsInitialAdaptation || CalculateEveryFrame)
        {
            // 首次渲染前再适配一次, 让 CanvasScaler 和父布局先完成尺寸初始化.
            needsInitialAdaptation = !TryAdapt();
        }
    }

    public override void Adapt()
    {
        TryAdapt();
    }

    private bool TryAdapt()
    {
        if (Screen.width <= 0 || Screen.height <= 0) return false;
        if (rect == null) rect = GetComponent<RectTransform>();
        var parent = rect.parent as RectTransform;
        if (parent == null) return false;

        var parentRect = parent.rect;
        if (parentRect.width <= 0 || parentRect.height <= 0) return false;

        var canvas = GetComponentInParent<Canvas>();
        // 保留 Init 接口, 但不让其它 Canvas 的缩放配置覆盖所属 Canvas.
        if (canvas == null && scaler != null) canvas = scaler.GetComponent<Canvas>();
        if (canvas == null) return false;
        canvas = canvas.rootCanvas;
        if (canvas.renderMode == RenderMode.WorldSpace) return false;

        var camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        if (camera != null && camera.targetTexture != null) return false;

        var safeArea = Screen.safeArea;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, safeArea.min, camera, out var safeMin) ||
            !RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, safeArea.max, camera, out var safeMax))
        {
            return false;
        }

        // 安全区先转换到父节点坐标, 再取交集, 不依赖 CanvasScaler 的缩放模式.
        var anchorMin = new Vector2(
            Mathf.Clamp01((safeMin.x - parentRect.xMin) / parentRect.width),
            Mathf.Clamp01((safeMin.y - parentRect.yMin) / parentRect.height));
        var anchorMax = new Vector2(
            Mathf.Clamp01((safeMax.x - parentRect.xMin) / parentRect.width),
            Mathf.Clamp01((safeMax.y - parentRect.yMin) / parentRect.height));

        if (rect.anchorMin != anchorMin) rect.anchorMin = anchorMin;
        if (rect.anchorMax != anchorMax) rect.anchorMax = anchorMax;
        if (rect.offsetMin != Vector2.zero) rect.offsetMin = Vector2.zero;
        if (rect.offsetMax != Vector2.zero) rect.offsetMax = Vector2.zero;
        return true;
    }
}
