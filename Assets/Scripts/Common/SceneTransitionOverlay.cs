using System;
using LitMotion;
using AChen.Events;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading;
using LitMotion.Extensions;

/// <summary>
/// 跨场景加载界面.切场景前显示 LoadIN,目标界面就绪后淡出并隐藏。
/// </summary>
public static class SceneTransitionOverlay
{
    public const string Address = "UI/LoadIN";
    public const float DuelTransitionDuration = .35f;

    static GameObject s_root;
    static CanvasGroup s_canvasGroup;
    static SceneTransitionOverlayView s_view;
    static bool s_activityGate, s_wasVisible, s_hideRequested;
    static MotionHandle s_opacityMotion;
    static int s_opacityVersion;
    static bool s_fadingIn;
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
        StopOpacityMotion();
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
        // SceneLoadStarted会再次请求Show，不打断已开始的渐入。
        if (s_fadingIn && !s_activityGate) return;
        StopOpacityMotion();
        s_canvasGroup.alpha = 1f;
        s_canvasGroup.blocksRaycasts = true;
        bool alreadyVisible = s_root.activeSelf;
        s_root.SetActive(true);
        if (!alreadyVisible)
        {
            ALog.Log("打开跨场景 LoadIN 加载界面.", ALogCategories.UI);
        }
    }

    public static async UniTask ShowAsync(CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        Ensure();
        if (s_fadingIn)
        {
            await s_opacityMotion.ToUniTask(CancelBehavior.Complete, token);
            return;
        }
        float from = IsVisible ? s_canvasGroup.alpha : 0f;
        StopOpacityMotion();
        s_root.SetActive(true);
        s_canvasGroup.alpha = from;
        s_canvasGroup.blocksRaycasts = true;
        if (s_activityGate || from >= 1f) { s_canvasGroup.alpha = 1f; return; }
        int version = s_opacityVersion;
        s_fadingIn = true;
        s_opacityMotion = Fade(from, 1f, DuelTransitionDuration);
        try { await s_opacityMotion.ToUniTask(CancelBehavior.Complete, token); }
        finally { if (version == s_opacityVersion) s_fadingIn = false; }
    }

    public static async UniTask FadeOutAsync(CancellationToken token = default)
    {
        while (true)
        {
            // 活动加载期间维持不透明，直到原活动门禁正常结束。
            await UniTask.WaitUntil(() => !s_activityGate, cancellationToken: token);
            if (!TryFadeOut(DuelTransitionDuration, out MotionHandle handle)) return;
            s_canvasGroup.blocksRaycasts = true;
            int version = s_opacityVersion;
            await handle.ToUniTask(CancelBehavior.Complete, token);
            if (version != s_opacityVersion)
            {
                if (s_activityGate) continue;
                return;
            }
            Hide();
            return;
        }
    }

    public static void Hide()
    {
        if (s_activityGate) { s_hideRequested = true; return; }
        if (s_root == null || !s_root.activeSelf)
        {
            return;
        }

        StopOpacityMotion();
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

        float from = s_canvasGroup.alpha;
        StopOpacityMotion();
        s_canvasGroup.alpha = from;
        s_canvasGroup.blocksRaycasts = false;
        handle = s_opacityMotion = Fade(from, 0f, duration);
        return true;
    }

    static MotionHandle Fade(float from, float to, float duration) => LMotion.Create(from, to, duration)
        .WithEase(Ease.OutCubic).WithScheduler(MotionScheduler.UpdateIgnoreTimeScale).BindToAlpha(s_canvasGroup);

    static void StopOpacityMotion()
    {
        ++s_opacityVersion;
        var previous = s_opacityMotion;
        s_opacityMotion = default; s_fadingIn = false;
        previous.TryComplete();
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
