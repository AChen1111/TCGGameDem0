using System;
using LitMotion;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 跨场景加载界面.切场景前显示 LoadIN,目标界面就绪后淡出并隐藏。
/// </summary>
public static class SceneTransitionOverlay
{
    const int SortingOrder = 32767;

    static GameObject s_root;
    static CanvasGroup s_canvasGroup;

    public static bool IsVisible => s_root != null && s_root.activeSelf;

    public static void Show()
    {
        Ensure();
        s_canvasGroup.alpha = 1f;
        s_canvasGroup.blocksRaycasts = true;
        bool alreadyVisible = s_root.activeSelf;
        s_root.SetActive(true);
        if (!alreadyVisible)
        {
            ALog.Log("打开跨场景 LoadIN 加载界面.", ALogCategories.UI);
        }
    }

    public static void Hide()
    {
        if (s_root == null || !s_root.activeSelf)
        {
            return;
        }

        s_root.SetActive(false);
        ALog.Log("关闭跨场景 LoadIN 加载界面.", ALogCategories.UI);
    }

    public static bool TryFadeOut(float duration, out MotionHandle handle)
    {
        if (!IsVisible)
        {
            handle = default;
            return false;
        }

        s_canvasGroup.blocksRaycasts = false;
        handle = UITween.DoFadeAnim(1f, 0f, duration, s_canvasGroup);
        return true;
    }

    static void Ensure()
    {
        if (s_root != null)
        {
            return;
        }

        GameObject prefab = Resources.Load<GameObject>("LoadIN");
        if (prefab == null)
        {
            ALog.LogError("打开跨场景加载界面失败: Resources/LoadIN.prefab 不存在.", ALogCategories.UI);
            throw new InvalidOperationException("Resources/LoadIN.prefab does not exist.");
        }

        s_root = UnityEngine.Object.Instantiate(prefab);
        s_root.name = "SceneTransitionOverlay";
        s_root.transform.localScale = Vector3.one;
        UnityEngine.Object.DontDestroyOnLoad(s_root);

        Canvas canvas = s_root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = SortingOrder;

        s_canvasGroup = s_root.GetComponent<CanvasGroup>();
        if (s_canvasGroup == null)
        {
            s_canvasGroup = s_root.AddComponent<CanvasGroup>();
        }

        Image background = s_root.GetComponentInChildren<Image>(true);
        if (background != null)
        {
            RectTransform rect = background.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        s_root.SetActive(false);
    }
}
