using System;
using AChen.Events;
using AChen.Player;
using Cysharp.Threading.Tasks;
using LitMotion;
using UnityEngine;
using UnityEngine.UI;

/// <summary>大厅主面板: 导航按钮、入场动画, 壁纸展示交给 LobbyWallpaperView.</summary>
public class PreGameUIPanel : APanelController
{
    // --tag_start: 自动生成--
    [SerializeField] Button m_BtnPlay;
    [SerializeField] Button m_BtnDeck;
    [SerializeField] Button m_BtnShop;
    [SerializeField] Button m_BtnExit;
    [SerializeField] Button m_BtnGift;
    [SerializeField] Button m_BtnFriend;
    [SerializeField] Button m_BtnMail;
    [SerializeField] Button m_BtnSetting;
    [SerializeField] Button m_BtnAvatar;
    // --tag_end: 自动生成--
    [SerializeField] Button m_BtnChangeName;
    [SerializeField] Button m_BtnChangeWallpaper;
    [SerializeField] RectTransform m_LeftLayOut;
    [SerializeField] RectTransform m_RightLayOut;
    [SerializeField] RectTransform m_UpLayOut;
    [SerializeField] RectTransform m_DownLayOut;
    [SerializeField] CanvasGroup m_CanvasGroup;
    [SerializeField] LobbyWallpaperView m_WallpaperView;

    [SerializeField] float m_Duration = 1f;
    [SerializeField] float m_Distance = 1500f;

    bool m_isSwitchingWallpaper;

    protected override void AddListeners()
    {
        m_BtnExit.onClick.AddListener(OnExitClick);
        m_BtnShop.onClick.AddListener(OnShopClick);
        m_BtnChangeName.onClick.AddListener(OnChangeNameClick);
        m_BtnAvatar.onClick.AddListener(OnAvatarClick);
        m_BtnChangeWallpaper.onClick.AddListener(OnChangeWallpaperClick);
    }

    protected override void RemoveListeners()
    {
        EventCenter.RemoveListener(GameEvent.PlayerBackgroundChanged, OnBackgroundChanged);
        m_BtnExit.onClick.RemoveListener(OnExitClick);
        m_BtnShop.onClick.RemoveListener(OnShopClick);
        m_BtnChangeName.onClick.RemoveListener(OnChangeNameClick);
        m_BtnAvatar.onClick.RemoveListener(OnAvatarClick);
        m_BtnChangeWallpaper.onClick.RemoveListener(OnChangeWallpaperClick);
    }

    protected override void OnOpen()
    {
        m_isSwitchingWallpaper = false;
        EventCenter.AddListener(GameEvent.PlayerBackgroundChanged, OnBackgroundChanged);
        // 先应用当前背景, 后续只订阅背景变化.
        OnBackgroundChanged(PlayerSession.HasInstance ? PlayerSession.Instance.CurrentPlayer?.BackgroundId : null);
        PlayIntroAsync().Forget();
    }

    protected override void OnClose()
    {
        EventCenter.RemoveListener(GameEvent.PlayerBackgroundChanged, OnBackgroundChanged);
        m_WallpaperView.Clear();
    }

    void OnBackgroundChanged(int? backgroundId)
    {
        if (!IsOpened) return;
        m_WallpaperView.SetBackground(backgroundId, ScreenToken);
    }

    void OnChangeNameClick() => RequestOpenWindow(AddressKeys.Prefab.ChangeNameWindow);

    void OnAvatarClick() => RequestOpenWindow(AddressKeys.Prefab.SelfChooseWindow);

    void OnShopClick()
    {
        ALog.Log("打开商城窗", ALogCategories.UI);
        RequestOpenWindow(AddressKeys.Prefab.ShopWindows);
    }

    void OnExitClick() => EventCenter.Dispatch(GameEvent.GameExitRequested);

    void OnChangeWallpaperClick() => SwitchToNextWallpaperAsync().Forget();

    async UniTaskVoid SwitchToNextWallpaperAsync()
    {
        if (m_isSwitchingWallpaper || !IsOpened) return;

        m_isSwitchingWallpaper = true;
        m_WallpaperView.HideForSwitch();
        bool succeeded = await RunGuardedAsync(async token =>
        {
            await PlayerSession.Instance.SetNextBackgroundAsync(token);
            await m_WallpaperView.WaitForReadyAsync(token);
            await m_WallpaperView.PlayRevealAsync(m_Duration);
        }, "切换壁纸", "壁纸切换失败");
        if (this == null || !IsOpened) return;

        if (!succeeded) m_WallpaperView.RestoreVisible();
        m_isSwitchingWallpaper = false;
    }

    async UniTask PlayIntroAsync()
    {
        m_CanvasGroup.interactable = false;
        m_CanvasGroup.alpha = 0;
        m_WallpaperView.HideForIntro();

        try
        {
            await m_WallpaperView.WaitForReadyAsync(ScreenToken);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        ALog.Log("大厅资源就绪, 遮挡层淡出并播放入场动画.", ALogCategories.UI);

        var seq = LSequence.Create();
        seq.Append(UITween.DoFadeAnim(0, 1, m_Duration, m_CanvasGroup));
        // 遮挡与界面同时淡入, 避免先关黑层时露出 GameScene 天空盒.
        if (SceneTransitionOverlay.TryFadeOut(m_Duration, out MotionHandle overlayFade))
        {
            seq.Join(overlayFade);
        }

        if (m_LeftLayOut != null) seq.Append(UITween.DoMoveAnim(m_LeftLayOut, UITween.MoveDirection.Right, m_Distance, m_Duration));
        if (m_RightLayOut != null) seq.Join(UITween.DoMoveAnim(m_RightLayOut, UITween.MoveDirection.Left, m_Distance, m_Duration));
        if (m_UpLayOut != null) seq.Join(UITween.DoMoveAnim(m_UpLayOut, UITween.MoveDirection.Down, m_Distance, m_Duration));
        if (m_DownLayOut != null) seq.Join(UITween.DoMoveAnim(m_DownLayOut, UITween.MoveDirection.Up, m_Distance, m_Duration));

        m_WallpaperView.AppendReveal(seq, m_Duration);

        await seq.Run().AddTo(this);
        SceneTransitionOverlay.Hide();
        m_CanvasGroup.interactable = true;
    }
}
