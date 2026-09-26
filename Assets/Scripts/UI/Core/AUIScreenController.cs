using System;
using System.Collections.Generic;
using System.Threading;
using AChen.Events;
using AChen.Networking;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

/// <summary>
/// UI 界面基类。业务请继承 AWindowController 或 APanelController。
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public abstract class AUIScreenController : MonoBehaviour, IUIScreenController
{
    [Tooltip("关闭后销毁物体。下次打开会从 Prefab 再创建。Window 仅 Close 时销毁，被盖住的 Hide 不销毁。")]
    [SerializeField]
    bool m_destroyOnClose;

    bool m_opened;
    bool m_destroyNotified;
    CancellationTokenSource m_screenCancellation;
    CanvasGroup m_transitionGroup;
    CancellationTokenSource m_transitionCancellation;
    bool m_exiting;
    bool m_closing;
    bool m_restoreRaycasts;
    bool m_restoreInteraction;
    bool m_interactionSuppressed;
    Action m_afterClose;

    protected virtual float TransitionDuration => 0.25f;
    protected bool IsResuming { get; private set; }

    protected CanvasGroup TransitionCanvasGroup
    {
        get
        {
            ResolveTransitionGroup();
            return m_transitionGroup;
        }
    }

    /// <summary>子类可重写入场动画, 或 await base.PlayEnterTransition(cancellationToken) 后追加效果.</summary>
    protected virtual UniTask PlayEnterTransition(CancellationToken cancellationToken) => FadeToAsync(1f, cancellationToken);

    /// <summary>动画完成后才隐藏/销毁界面, 自定义动画须传递取消令牌.</summary>
    protected virtual UniTask PlayExitTransition(CancellationToken cancellationToken) => FadeToAsync(0f, cancellationToken);

    // 自定义入场动画在退出前交还透明度控制权.
    protected virtual void FinishIntro() { }

    /// <summary>界面 Id，默认与 Prefab 名相同。</summary>
    public string ScreenId { get; set; }

    protected IScreenProperties Properties { get; private set; }

    /// <summary>当前是否可见。</summary>
    public bool IsVisible { get; private set; }

    /// <summary>关闭后是否销毁。Window 被盖住时的 Hide 不会销毁。</summary>
    public bool DestroyOnClose {
        get { return m_destroyOnClose; }
    }

    //它所属的UIFrame
    protected UIFrame m_UIFrame;

    internal void SetUIFrame(UIFrame uiFrame)
    {
        m_UIFrame = uiFrame;
    }

    protected virtual void Awake()
    {
        AddListeners();
        m_UIFrame = GetComponentInParent<UIFrame>();//获取它所属的UIFrame
    }

    protected virtual void OnDestroy()
    {
        StopTransition();
        m_opened = false;
        CancelScreenWork();
        NotifyDestroyed();
        RemoveListeners();
    }

    protected virtual void OnDisable()
    {
        StopTransition();
        FinishIntro();
        if (m_exiting) FinishExit();
    }

    /// <summary>本次打开周期的取消令牌: Close 或销毁时取消; 被盖住(Hide)期间保持有效.</summary>
    protected CancellationToken ScreenToken =>
        m_screenCancellation?.Token ?? new CancellationToken(canceled: true);

    /// <summary>界面当前是否处于打开周期内(已 OnOpen 且未 Close).</summary>
    protected bool IsOpened => m_opened && m_screenCancellation != null;

    /// <summary>
    /// 执行一次会访问后端的界面命令: 界面关闭即取消; BackendApiException 按错误 key 提示给玩家;
    /// 其它异常记日志并提示 fallbackMessage. 返回是否成功完成.
    /// </summary>
    protected async UniTask<bool> RunGuardedAsync(
        Func<CancellationToken, UniTask> action,
        string operation,
        string fallbackMessage)
    {
        CancellationToken token = ScreenToken;
        if (token.IsCancellationRequested) return false;

        try
        {
            await action(token);
            token.ThrowIfCancellationRequested();
            return true;
        }
        catch (OperationCanceledException)
        {
            return false;
        }
        catch (BackendApiException exception)
        {
            if (token.IsCancellationRequested) return false;
            ALog.LogWarning($"{operation}失败. Screen={ScreenId}; Code={exception.Code}; Status={exception.StatusCode}", ALogCategories.UI);
            ShowMessage(exception.UserMessage);
            return false;
        }
        catch (Exception exception)
        {
            if (token.IsCancellationRequested) return false;
            ALog.LogError($"{operation}异常. Screen={ScreenId}; Error={exception.Message}", ALogCategories.UI);
            ShowMessage(fallbackMessage);
            return false;
        }
    }

    void CancelScreenWork()
    {
        m_screenCancellation?.Cancel();
        m_screenCancellation?.Dispose();
        m_screenCancellation = null;
    }

    void NotifyDestroyed()
    {
        if (m_destroyNotified) return;
        m_destroyNotified = true;
        EventCenter.Dispatch(UIEvent.ScreenDestroyed, (IUIScreenController)this);
    }

    protected void RequestOpenWindow(string screenId, IWindowProperties properties = null)
    {
        if (m_UIFrame == null)
        {
            ALog.LogError($"打开窗口请求失败. Screen={ScreenId}; Target={screenId}; 原因=未绑定 UIFrame", ALogCategories.UI);
            return;
        }
        EventCenter.Dispatch(UIEvent.WindowOpenRequested, m_UIFrame, new WindowOpenRequest(screenId, properties));
    }

    protected void ShowMessage(string key) => ShowMessage(new LocalizedMessage(key));

    protected void ShowMessage(LocalizedMessage message)
    {
        RequestOpenWindow(AddressKeys.Prefab.MessageWindow, new MessageWindowProperties(message, 2f));
    }

    protected void Confirm(string key, Action onOk, IReadOnlyDictionary<string, object> arguments = null)
    {
        ALog.Log($"打开确认弹窗. Screen={ScreenId}; Key={key}", ALogCategories.UI);
        RequestOpenWindow(
            AddressKeys.Prefab.ChooseWindow,
            new ChooseWindowProperties(new LocalizedMessage(key, arguments), onOk, null));
    }

    /// <summary>绑定事件，默认在 Awake 调用。</summary>
    protected virtual void AddListeners()
    {
    }

    /// <summary>解绑事件，默认在 OnDestroy 调用。</summary>
    protected virtual void RemoveListeners()
    {
    }

    /// <summary>首次打开。</summary>
    protected virtual void OnOpen()
    {
    }

    protected virtual void SetProperties(IScreenProperties properties)
    {
        Properties = properties;
    }

    /// <summary>暂时隐藏（例如 Window 被盖住），之后再显示会走 OnResume。</summary>
    protected virtual void OnHide()
    {
    }

    /// <summary>真正关闭（例如 Window 出栈）。之后再打开会走 OnOpen。</summary>
    protected virtual void OnClose()
    {
    }

    /// <summary>曾经打开过、隐藏后再显示。</summary>
    protected virtual void OnResume()
    {
    }

    /// <summary>Hierarchy 调整后、打开前调用。Window 默认把自己放到同层最后。</summary>
    protected virtual void HierarchyFixOnShow()
    {
    }

    /// <summary>暂时隐藏。Panel 的 HidePanel、Window 被盖住时走这里，回调 OnHide。</summary>
    public void Hide()
    {
        if (m_exiting) return;
        BeginExit(false);
    }

    /// <summary>真正关闭。Window 出栈时走这里，回调 OnClose。勾选销毁则 Destroy。</summary>
    public void Close()
    {
        Close(null);
    }

    internal void Close(Action afterClose)
    {
        m_afterClose += afterClose;
        if (m_closing) return;
        BeginExit(true);
    }

    /// <summary>显示界面。首次走 OnOpen，再次走 OnResume。</summary>
    public void Show(IScreenProperties properties = null)
    {
        bool wasVisible = IsVisible && isActiveAndEnabled;
        StopTransition();
        FinishIntro();
        // 快速重开时结束上一轮业务清理, 但不销毁即将复用的窗口.
        if (m_exiting)
        {
            bool wasClosing = m_closing;
            m_exiting = m_closing = false;
            if (wasClosing) OnClose();
            else OnHide();
            var completed = m_afterClose;
            m_afterClose = null;
            completed?.Invoke();
        }
        ResolveTransitionGroup();
        if (m_transitionGroup != null)
        {
            m_transitionGroup.blocksRaycasts = m_restoreRaycasts;
            if (m_interactionSuppressed) m_transitionGroup.interactable = m_restoreInteraction;
            m_interactionSuppressed = false;
            if (!wasVisible) m_transitionGroup.alpha = Application.isPlaying ? 0f : 1f;
        }
        IsResuming = m_opened;
        if (properties != null)
        {
            SetProperties(properties);
        }

        HierarchyFixOnShow();
        gameObject.SetActive(true);
        IsVisible = true;
        if (m_opened)
        {
            OnResume();
        }
        else
        {
            CancelScreenWork();
            m_screenCancellation = new CancellationTokenSource();
            m_opened = true;
            OnOpen();
        }
        if (IsVisible && !m_exiting) StartTransition(true, null);
    }

    void ResolveTransitionGroup()
    {
        if (m_transitionGroup != null) return;
        m_transitionGroup = GetComponent<CanvasGroup>();
        if (m_transitionGroup != null) m_restoreRaycasts = m_transitionGroup.blocksRaycasts;
    }

    void BeginExit(bool close)
    {
        ResolveTransitionGroup();
        float alpha = m_transitionGroup != null ? m_transitionGroup.alpha : 1f;
        StopTransition();
        FinishIntro();
        if (m_transitionGroup != null) m_transitionGroup.alpha = alpha;
        m_exiting = true;
        m_closing = close;
        IsVisible = false;
        if (close)
        {
            m_opened = false;
            CancelScreenWork();
        }
        if (m_transitionGroup != null)
        {
            if (!m_interactionSuppressed) m_restoreInteraction = m_transitionGroup.interactable;
            m_interactionSuppressed = true;
            m_transitionGroup.interactable = false;
            // 淡出期间拦住鼠标, 避免同一次点击落到下层窗口.
            m_transitionGroup.blocksRaycasts = m_restoreRaycasts;
        }
        StartTransition(false, FinishExit);
    }

    void FinishExit()
    {
        bool close = m_closing;
        m_exiting = m_closing = false;
        if (close) OnClose();
        else OnHide();
        gameObject.SetActive(false);
        var completed = m_afterClose;
        m_afterClose = null;
        if (close && m_destroyOnClose)
        {
            NotifyDestroyed();
            DestroyScreenObject();
        }
        completed?.Invoke();
    }

    void StopTransition()
    {
        var cancellation = m_transitionCancellation;
        m_transitionCancellation = null;
        cancellation?.Cancel();
    }

    void StartTransition(bool entering, Action completed)
    {
        if (!Application.isPlaying || !isActiveAndEnabled || m_transitionGroup == null)
        {
            if (m_transitionGroup != null) m_transitionGroup.alpha = entering ? 1f : 0f;
            completed?.Invoke();
            return;
        }
        var cancellation = new CancellationTokenSource();
        m_transitionCancellation = cancellation;
        RunTransitionAsync(entering, completed, cancellation).Forget();
    }

    async UniTask RunTransitionAsync(bool entering, Action completed, CancellationTokenSource cancellation)
    {
        CancellationToken token = cancellation.Token;
        try
        {
            try
            {
                if (entering) await PlayEnterTransition(token);
                else await PlayExitTransition(token);
                token.ThrowIfCancellationRequested();
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                if (token.IsCancellationRequested || this == null) return;
                ALog.LogError($"界面过渡异常, 恢复目标状态. Screen={ScreenId}; Entering={entering}; Error={exception.Message}", ALogCategories.UI);
                FinishIntro();
                if (m_transitionGroup != null) m_transitionGroup.alpha = entering ? 1f : 0f;
            }

            if (this == null || token.IsCancellationRequested || !ReferenceEquals(m_transitionCancellation, cancellation)) return;
            m_transitionCancellation = null;
            ALog.Log($"界面过渡完成. Screen={ScreenId}; Entering={entering}", ALogCategories.UI);
            completed?.Invoke();
        }
        finally
        {
            if (ReferenceEquals(m_transitionCancellation, cancellation)) m_transitionCancellation = null;
            cancellation.Dispose();
        }
    }

    protected UniTask FadeToAsync(float target, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        CanvasGroup group = TransitionCanvasGroup;
        if (TransitionDuration <= 0f)
        {
            group.alpha = target;
            return UniTask.CompletedTask;
        }
        // 使用真实时间, 暂停游戏时仍能完成界面切换.
        return LMotion.Create(group.alpha, target, TransitionDuration)
            .WithEase(Ease.OutCubic)
            .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
            .BindToAlpha(group)
            .AddTo(this)
            .ToUniTask(cancellationToken);
    }

    void DestroyScreenObject()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            DestroyImmediate(gameObject);
            return;
        }
#endif
        Destroy(gameObject);
    }
}
