using UnityEngine;
using System;

/// <summary>
/// UI 界面基类。业务请继承 AWindowController 或 APanelController。
/// </summary>
public abstract class AUIScreenController<TProps> : MonoBehaviour, IUIScreenController
    where TProps : IScreenProperties
{
    [Header("界面动画")]
    [Tooltip("打开动画")]
    [SerializeField]
    private ATransitionComponent animIn;

    [Tooltip("关闭动画")]
    [SerializeField]
    private ATransitionComponent animOut;

    [Header("界面属性")]
    [Tooltip("本界面的数据和设置。可在 Prefab 上配置，也可在 Show 时传入")]
    [SerializeField]
    private TProps properties;

    [Tooltip("关闭后销毁物体。下次打开会从 Prefab 再创建。Window 仅 Close 时销毁，被盖住的 Hide 不销毁。")]
    [SerializeField]
    bool m_destroyOnClose;

    bool m_opened;
    bool m_pendingDestroy;

    /// <summary>界面 Id，默认与 Prefab 名相同。</summary>
    public string ScreenId { get; set; }

    /// <summary>打开动画。</summary>
    public ATransitionComponent AnimIn
    {
        get { return animIn; }
        set { animIn = value; }
    }

    /// <summary>关闭动画。</summary>
    public ATransitionComponent AnimOut
    {
        get { return animOut; }
        set { animOut = value; }
    }

    /// <summary>打开动画播完。</summary>
    public Action<IUIScreenController> InTransitionFinished { get; set; }

    /// <summary>关闭动画播完。</summary>
    public Action<IUIScreenController> OutTransitionFinished { get; set; }

    /// <summary>请求所属 Layer 关闭自己。</summary>
    public Action<IUIScreenController> CloseRequest { get; set; }

    /// <summary>界面被销毁时通知 Layer。</summary>
    public Action<IUIScreenController> ScreenDestroyed { get; set; }

    /// <summary>当前是否可见。</summary>
    public bool IsVisible { get; private set; }

    /// <summary>关闭后是否销毁。Window 被盖住时的 Hide 不会销毁。</summary>
    public bool DestroyOnClose {
        get { return m_destroyOnClose; }
    }

    /// <summary>界面属性。Prefab 序列化值，或 Show 时传入。</summary>
    protected TProps Properties
    {
        get { return properties; }
        set { properties = value; }
    }

    protected virtual void Awake()
    {
        AddListeners();
    }

    protected virtual void OnDestroy()
    {
        m_opened = false;
        if (ScreenDestroyed != null)
        {
            ScreenDestroyed(this);
        }

        InTransitionFinished = null;
        OutTransitionFinished = null;
        CloseRequest = null;
        ScreenDestroyed = null;
        RemoveListeners();
    }

    /// <summary>绑定事件，默认在 Awake 调用。</summary>
    protected virtual void AddListeners()
    {
    }

    /// <summary>解绑事件，默认在 OnDestroy 调用。</summary>
    protected virtual void RemoveListeners()
    {
    }

    /// <summary>首次打开。此时 Properties 已就绪。</summary>
    protected virtual void OnOpen()
    {
    }

    /// <summary>暂时隐藏（例如 Window 被盖住），之后再显示会走 OnResume。</summary>
    protected virtual void OnHide()
    {
    }

    /// <summary>真正关闭（例如 Window 出栈）。之后再打开会走 OnOpen。</summary>
    protected virtual void OnClose()
    {
    }

    /// <summary>曾经打开过、隐藏后再显示。此时 Properties 已就绪。</summary>
    protected virtual void OnResume()
    {
    }

    /// <summary>写入 Properties。可在子类里按条件改写。</summary>
    protected virtual void SetProperties(TProps props)
    {
        properties = props;
    }

    /// <summary>Hierarchy 调整后、打开前调用。Window 默认把自己放到同层最后。</summary>
    protected virtual void HierarchyFixOnShow()
    {
    }

    /// <summary>暂时隐藏。Panel 的 HidePanel、Window 被盖住时走这里，回调 OnHide。</summary>
    public void Hide(bool animate = true)
    {
        DoAnimation(animate ? animOut : null, OnTransitionOutFinished, false);
        OnHide();
    }

    /// <summary>真正关闭。Window 出栈时走这里，回调 OnClose。勾选销毁则动画结束后 Destroy。</summary>
    public void Close(bool animate = true)
    {
        m_opened = false;
        m_pendingDestroy = m_destroyOnClose;
        OnClose();
        DoAnimation(animate ? animOut : null, OnTransitionOutFinished, false);
    }

    /// <summary>显示界面。首次走 OnOpen，再次走 OnResume。</summary>
    public void Show(IScreenProperties props = null)
    {
        if (props != null)
        {
            if (props is TProps)
            {
                SetProperties((TProps) props);
            }
            else
            {
                Debug.LogError("Properties passed have wrong type! (" + props.GetType() + " instead of " +
                               typeof(TProps) + ")");
                return;
            }
        }

        HierarchyFixOnShow();
        if (m_opened)
        {
            OnResume();
        }
        else
        {
            OnOpen();
            m_opened = true;
        }

        if (!gameObject.activeSelf)
        {
            DoAnimation(animIn, OnTransitionInFinished, true);
        }
        else
        {
            if (InTransitionFinished != null)
            {
                InTransitionFinished(this);
            }
        }
    }

    private void DoAnimation(ATransitionComponent caller, Action callWhenFinished, bool isVisible)
    {
        if (caller == null)
        {
            gameObject.SetActive(isVisible);
            if (callWhenFinished != null)
            {
                callWhenFinished();
            }
        }
        else
        {
            if (isVisible && !gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            caller.Animate(transform, callWhenFinished);
        }
    }

    private void OnTransitionInFinished()
    {
        IsVisible = true;

        if (InTransitionFinished != null)
        {
            InTransitionFinished(this);
        }
    }

    private void OnTransitionOutFinished()
    {
        IsVisible = false;
        gameObject.SetActive(false);

        if (OutTransitionFinished != null)
        {
            OutTransitionFinished(this);
        }

        if (m_pendingDestroy)
        {
            m_pendingDestroy = false;
            if (ScreenDestroyed != null)
            {
                ScreenDestroyed(this);
            }
            ScreenDestroyed = null;
            DestroyScreenObject();
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
