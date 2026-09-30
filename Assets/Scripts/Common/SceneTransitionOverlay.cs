using System;
using LitMotion;
using AChen.Events;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// 跨场景加载界面.切场景前显示 LoadIN,目标界面就绪后淡出并隐藏。
/// </summary>
public static class SceneTransitionOverlay
{
    public const string Address = "UI/LoadIN";

    static GameObject s_root;
    static CanvasGroup s_canvasGroup;
    static SceneTransitionOverlayView s_view;
    static bool s_activityGate, s_wasVisible, s_hideRequested;
    public static bool IsActivityBlocking => s_activityGate;
    public static void Bind(SceneTransitionOverlayView view) { s_view = view; s_canvasGroup = view.Group; }
    public static void ActivityProgress(string text, float value)
    {
        if (!s_activityGate) { s_wasVisible = IsVisible; s_hideRequested = false; s_activityGate = true; }
        Show(); s_view.Progress(text, value);
    }
    public static UniTask ActivityRetryAsync(string reason, System.Threading.CancellationToken token)
    { ActivityProgress("活动配置加载失败", 0); return s_view.WaitForRetryAsync(reason, token); }
    public static void EndActivities()
    {
        bool hide = s_activityGate && (!s_wasVisible || s_hideRequested); s_activityGate = false;
        if (hide) Hide();
    }
    static AsyncOperationHandle<GameObject> s_prefab;

    public static void ResetState()
    {
        if (s_root != null) UnityEngine.Object.Destroy(s_root);
        s_root = null;
        s_canvasGroup = null; s_view = null; s_activityGate = false;
        if (s_prefab.IsValid()) Addressables.Release(s_prefab);
        s_prefab = default;
    }

    public static async UniTask PreloadAsync()
    {
        s_prefab = Addressables.LoadAssetAsync<GameObject>(Address);
        try
        {
            await s_prefab.Task;
            if (s_prefab.Status != AsyncOperationStatus.Succeeded || s_prefab.Result == null)
                throw new InvalidOperationException("跨场景加载窗口加载失败", s_prefab.OperationException);
        }
        catch
        {
            if (s_prefab.IsValid()) Addressables.Release(s_prefab);
            s_prefab = default;
            throw;
        }
    }

    public static bool IsVisible => s_root != null && s_root.activeSelf;

    public static void Initialize()
    {
        EventCenter.RemoveListener(GameEvent.SceneLoadStarted, OnSceneLoadStarted);
        EventCenter.RemoveListener(GameEvent.SceneLoadFailed, OnSceneLoadFailed);
        EventCenter.RemoveListener(GameEvent.LobbyEntering, Show);
        EventCenter.RemoveListener(GameEvent.LobbyEntryFailed, OnLobbyEntryFailed);
        EventCenter.AddListener(GameEvent.SceneLoadStarted, OnSceneLoadStarted);
        EventCenter.AddListener(GameEvent.SceneLoadFailed, OnSceneLoadFailed);
        EventCenter.AddListener(GameEvent.LobbyEntering, Show);
        EventCenter.AddListener(GameEvent.LobbyEntryFailed, OnLobbyEntryFailed);
    }

    static void OnSceneLoadStarted(string sceneName, LoadSceneMode loadMode)
    {
        if (loadMode == LoadSceneMode.Single) Show();
    }
    static void OnSceneLoadFailed(string sceneName, Exception exception) => Hide();
    static void OnLobbyEntryFailed(string message) => Hide();

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
        if (s_activityGate) { s_hideRequested = true; return; }
        if (s_root == null || !s_root.activeSelf)
        {
            return;
        }

        s_root.SetActive(false);
        ALog.Log("关闭跨场景 LoadIN 加载界面.", ALogCategories.UI);
    }

    public static bool TryFadeOut(float duration, out MotionHandle handle)
    {
        if (s_activityGate || !IsVisible)
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

        GameObject prefab = s_prefab.IsValid() ? s_prefab.Result : null;
        if (prefab == null)
        {
            throw new InvalidOperationException("跨场景加载窗口尚未预加载: " + Address);
        }

        s_root = UnityEngine.Object.Instantiate(prefab);
        s_root.name = "SceneTransitionOverlay";
        s_root.transform.localScale = Vector3.one;
        UnityEngine.Object.DontDestroyOnLoad(s_root);

        s_root.SetActive(false);
    }
}
