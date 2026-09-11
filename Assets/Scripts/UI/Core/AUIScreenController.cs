using System;
using System.Threading;
using AChen.Events;
using AChen.Networking;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// UI 界面基类。业务请继承 AWindowController 或 APanelController。
/// </summary>
public abstract class AUIScreenController : MonoBehaviour, IUIScreenController
{
    [Tooltip("关闭后销毁物体。下次打开会从 Prefab 再创建。Window 仅 Close 时销毁，被盖住的 Hide 不销毁。")]
    [SerializeField]
    bool m_destroyOnClose;

    bool m_opened;
    bool m_destroyNotified;
    CancellationTokenSource m_screenCancellation;

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
        m_opened = false;
        CancelScreenWork();
        NotifyDestroyed();
        RemoveListeners();
    }

    /// <summary>本次打开周期的取消令牌: Close 或销毁时取消; 被盖住(Hide)期间保持有效.</summary>
    protected CancellationToken ScreenToken =>
        m_screenCancellation?.Token ?? new CancellationToken(canceled: true);

    /// <summary>界面当前是否处于打开周期内(已 OnOpen 且未 Close).</summary>
    protected bool IsOpened => m_opened && m_screenCancellation != null;

    /// <summary>
    /// 执行一次会访问后端的界面命令: 界面关闭即取消; BackendApiException 直接把服务端文案提示给玩家;
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
            ShowMessage(exception.Message);
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

    protected void ShowMessage(string message)
    {
        RequestOpenWindow(AddressKeys.Prefab.MessageWindow, new MessageWindowProperties(message, 2f));
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
        OnHide();
        gameObject.SetActive(false);
        IsVisible = false;
    }

    /// <summary>真正关闭。Window 出栈时走这里，回调 OnClose。勾选销毁则 Destroy。</summary>
    public void Close()
    {
        m_opened = false;
        OnClose();
        CancelScreenWork();
        gameObject.SetActive(false);
        IsVisible = false;
        if (m_destroyOnClose)
        {
            NotifyDestroyed();
            DestroyScreenObject();
        }
    }

    /// <summary>显示界面。首次走 OnOpen，再次走 OnResume。</summary>
    public void Show(IScreenProperties properties = null)
    {
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
