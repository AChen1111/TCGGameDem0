using LitMotion;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 跨场景全屏黑遮挡.切场景前 Show,目标界面就绪后 FadeOut / Hide.
/// </summary>
public static class SceneTransitionOverlay
{
    const int SortingOrder = 32767;

    static GameObject s_root;
    static Image s_image;

    public static bool IsVisible => s_root != null && s_root.activeSelf;

    public static void Show()
    {
        Ensure();
        Color color = s_image.color;
        color.a = 1f;
        s_image.color = color;
        bool alreadyVisible = s_root.activeSelf;
        s_root.SetActive(true);
        if (!alreadyVisible)
        {
            ALog.Log("打开跨场景遮挡层.", ALogCategories.UI);
        }
    }

    public static void Hide()
    {
        if (s_root == null || !s_root.activeSelf)
        {
            return;
        }

        s_root.SetActive(false);
        ALog.Log("关闭跨场景遮挡层.", ALogCategories.UI);
    }

    public static bool TryFadeOut(float duration, out MotionHandle handle)
    {
        if (!IsVisible)
        {
            handle = default;
            return false;
        }

        handle = UITween.DoFadeAnim(1f, 0f, duration, s_image);
        return true;
    }

    static void Ensure()
    {
        if (s_root != null)
        {
            return;
        }

        s_root = new GameObject("SceneTransitionOverlay");
        Object.DontDestroyOnLoad(s_root);

        var canvas = s_root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = SortingOrder;
        s_root.AddComponent<GraphicRaycaster>();

        var blocker = new GameObject("Blocker", typeof(RectTransform));
        blocker.transform.SetParent(s_root.transform, false);
        var rect = blocker.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        s_image = blocker.AddComponent<Image>();
        s_image.color = Color.black;
        s_image.raycastTarget = true;
    }
}
