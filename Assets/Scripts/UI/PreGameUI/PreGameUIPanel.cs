using System;
using System.Collections.Generic;
using AChen.Networking;
using AChen.Player;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class PreGameUIPanel : APanelController, IPlayerDataView
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
    [SerializeField] private Button m_BtnChangeName;
    [SerializeField] private RectTransform m_LeftLayOut;
    [SerializeField] private RectTransform m_RightLayOut;
    [SerializeField] private RectTransform m_UpLayOut;
    [SerializeField] private RectTransform m_DownLayOut;
    [SerializeField] private CanvasGroup m_CanvasGroup;

    [SerializeField] private float m_Duration = 1f;
    [SerializeField] private float m_Distance = 1500f;
    [SerializeField] private GameObject m_wallpaper;
    [SerializeField] private GameObject m_heroSprite;
    [SerializeField] private Button m_BtnChangeWallpaper;//切换按钮
    [SerializeField] WallpaperDisplayConfig m_WallpaperDisplayConfig;
    private Image m_heroImage;
    private Image m_wallpaperImage;
    private int? m_backgroundId;
    private UniTask m_backgroundLoad = UniTask.CompletedTask;
    private bool m_isSwitchingWallpaper;

    protected override void Awake()
    {
        m_heroImage = m_heroSprite.GetComponent<Image>();
        m_wallpaperImage = m_wallpaper.GetComponent<Image>();
        base.Awake();
    }

    protected override void OnDestroy()
    {
        PlayerDataViews.Unbind(this);
        base.OnDestroy();
    }

    protected override void AddListeners()
    {
        m_BtnExit.onClick.AddListener(OnExitClick);
        m_BtnShop.onClick.AddListener(OnShopClick);
        m_BtnChangeName.onClick.AddListener(OnChangeNameClick);
        m_BtnAvatar.onClick.AddListener(OnAvatarClick);
        m_BtnChangeWallpaper.onClick.AddListener(OnChangeWallpaperClick);
    }

    private void OnChangeWallpaperClick()
    {
        SwitchToNextWallpaperAsync().Forget();
    }

    async UniTaskVoid SwitchToNextWallpaperAsync()
    {
        if (m_isSwitchingWallpaper)
        {
            return;
        }

        PlayerData player = PlayerSession.Instance.CurrentPlayer;
        IReadOnlyList<int> owned = player.OwnedBackgroundIds;
        if (owned == null || owned.Count == 0)
        {
            ALog.LogWarning("切换壁纸中止: 没有已拥有壁纸", ALogCategories.UI);
            return;
        }

        int nextId = GetNextOwnedBackgroundId(owned, player.BackgroundId);
        m_isSwitchingWallpaper = true;
        try
        {
            if (nextId != player.BackgroundId)
            {
                // 先藏旧图,避免换图完成前仍以不透明状态露出新图
                SetImageAlpha(m_wallpaperImage, 0f);
                m_heroSprite.SetActive(false);

                await PlayerSession.Instance.UpdatePlayerProfileAsync(
                    player.Nickname,
                    player.AvatarId,
                    nextId,
                    player.Revision,
                    this.GetCancellationTokenOnDestroy());
                await m_backgroundLoad;
            }

            ALog.Log($"切换壁纸成功: Id={nextId}, Owned={owned.Count}", ALogCategories.UI);
            await PlayWallpaperRevealAsync();
        }
        catch (OperationCanceledException)
        {
        }
        catch (BackendApiException exception)
        {
            RestoreWallpaperVisible();
            ALog.LogError(
                $"切换壁纸失败: Id={nextId}, Code={exception.Code}, Status={exception.StatusCode}",
                ALogCategories.UI);
            m_UIFrame.OpenWindow(
                AddressKeys.Prefab.MessageWindow,
                new MessageWindowProperties(exception.Message, 2f));
        }
        catch (Exception exception)
        {
            RestoreWallpaperVisible();
            ALog.LogError($"切换壁纸失败: Id={nextId}, 原因={exception.Message}", ALogCategories.UI);
            m_UIFrame.OpenWindow(
                AddressKeys.Prefab.MessageWindow,
                new MessageWindowProperties("壁纸切换失败", 2f));
        }
        finally
        {
            m_isSwitchingWallpaper = false;
        }
    }

    static int GetNextOwnedBackgroundId(IReadOnlyList<int> owned, int? current)
    {
        int[] ids = new int[owned.Count];
        for (int i = 0; i < owned.Count; i++)
        {
            ids[i] = owned[i];
        }

        Array.Sort(ids);
        int index = current is int currentId ? Array.IndexOf(ids, currentId) : -1;
        // 从当前下一张开始,到末尾后回到第一张
        return ids[(index + 1) % ids.Length];
    }

    static void SetImageAlpha(Image image, float alpha)
    {
        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }

    void RestoreWallpaperVisible()
    {
        SetImageAlpha(m_wallpaperImage, 1f);
        m_heroSprite.SetActive(true);
    }

    void AppendWallpaperReveal(MotionSequenceBuilder seq)
    {
        seq.Append(UITween.DoFadeAnim(0, 1, m_Duration, m_wallpaperImage));
        m_heroSprite.SetActive(true);
        seq.Append(UITween.DoVerticalReveal(m_heroImage, m_Duration));
    }

    async UniTask PlayWallpaperRevealAsync()
    {
        var seq = LSequence.Create();
        AppendWallpaperReveal(seq);
        await seq.Run().AddTo(this);
    }

    protected override void RemoveListeners()
    {
        m_BtnExit.onClick.RemoveListener(OnExitClick);
        m_BtnShop.onClick.RemoveListener(OnShopClick);
        m_BtnChangeName.onClick.RemoveListener(OnChangeNameClick);
        m_BtnAvatar.onClick.RemoveListener(OnAvatarClick);
        m_BtnChangeWallpaper.onClick.RemoveListener(OnChangeWallpaperClick);
    }

    private void OnChangeNameClick()
    {
        m_UIFrame.OpenWindow(AddressKeys.Prefab.ChangeNameWindow);
    }

    private void OnAvatarClick()
    {
        OpenAvatarAsync().Forget();
    }

    private void OnShopClick()
    {
        ALog.Log("打开商城窗", ALogCategories.UI);
        m_UIFrame.OpenWindow(AddressKeys.Prefab.ShopWindows, new ShopWindowProperties());
    }

    async UniTaskVoid OpenAvatarAsync()
    {
        try
        {
            List<AvatarItemData> avatars = await ServerShopDataSource.LoadAvatarSelectionAsync();
            int? avatarId = PlayerSession.HasInstance ? PlayerSession.Instance.CurrentPlayer?.AvatarId : null;
            int selected = avatarId.HasValue ? avatars.FindIndex(item => item.Id == avatarId.Value) : -1;
            ALog.Log($"打开头像窗: Count={avatars.Count}, Selected={selected}", ALogCategories.UI);
            m_UIFrame.OpenWindow(
                AddressKeys.Prefab.SelfChooseWindow,
                new AvatarSelectWindowProperties(avatars, selected));
        }
        catch (Exception exception)
        {
            ALog.LogError("打开头像窗失败: " + exception.Message, ALogCategories.UI);
            m_UIFrame.OpenWindow(
                AddressKeys.Prefab.MessageWindow,
                new MessageWindowProperties("头像数据加载失败", 2f));
        }
    }

    private void OnExitClick()
    {
        Application.Quit();
    }


    protected override void OnOpen()
    {
        // Bind 会同步回调一次 OnPlayerDataChanged，先绑定再播动画以便等待背景加载
        PlayerDataViews.Bind(this);
        DoStartAnimAsync().Forget();
    }

    protected override void OnClose()
    {
        PlayerDataViews.Unbind(this);
    }

    public void OnPlayerDataChanged(PlayerData data)
    {
        int? backgroundId = data?.BackgroundId;
        if (backgroundId == m_backgroundId)
        {
            return;
        }

        m_backgroundId = backgroundId;
        if (backgroundId is int id)
        {
            m_backgroundLoad = LoadWallpaperAndSpriteAsync(
                id,
                AddressKeys.GetBackgroundDownAddress(id),
                AddressKeys.GetBackgroundSpriteAddress(id));
        }
    }

    [Button("开始动画")]
    private async UniTask DoStartAnimAsync()
    {
        m_CanvasGroup.interactable = false;
        m_wallpaper.SetActive(false);
        m_heroSprite.SetActive(false);
        m_CanvasGroup.alpha = 0;

        await m_backgroundLoad;

        ALog.Log("大厅资源就绪,遮挡层淡出并播放入场动画.", ALogCategories.UI);

        var seq = LSequence.Create();
        seq.Append(UITween.DoFadeAnim(0, 1, m_Duration, m_CanvasGroup));
        // 遮挡与界面同时淡入,避免先关黑层时露出 GameScene 天空盒
        if (SceneTransitionOverlay.TryFadeOut(m_Duration, out MotionHandle overlayFade))
        {
            seq.Join(overlayFade);
        }

        //step1 布局向中心移动
        if (m_LeftLayOut != null) seq.Append(UITween.DoMoveAnim(m_LeftLayOut, UITween.MoveDirection.Right, m_Distance, m_Duration));
        if (m_RightLayOut != null) seq.Join(UITween.DoMoveAnim(m_RightLayOut, UITween.MoveDirection.Left, m_Distance, m_Duration));
        if (m_UpLayOut != null) seq.Join(UITween.DoMoveAnim(m_UpLayOut, UITween.MoveDirection.Down, m_Distance, m_Duration));
        if (m_DownLayOut != null) seq.Join(UITween.DoMoveAnim(m_DownLayOut, UITween.MoveDirection.Up, m_Distance, m_Duration));
        

        //step2-3 背景淡入后英雄显现
        AppendWallpaperReveal(seq);

        await seq.Run().AddTo(this);
        SceneTransitionOverlay.Hide();
        m_CanvasGroup.interactable = true;
    }

    private async UniTask LoadWallpaperAndSpriteAsync(int wallpaperId, string wallpaper, string sprite)
    {
        var wallpaperSprite = AddressableLoader.Instance.LoadSprite(wallpaper);
        var spriteSprite = AddressableLoader.Instance.LoadSprite(sprite);
        m_heroImage.sprite = await spriteSprite;
        m_wallpaperImage.sprite = await wallpaperSprite;
        m_heroImage.SetNativeSize();
        m_wallpaperImage.SetNativeSize();
        ApplyWallpaperOffsets(wallpaperId);
    }

    void ApplyWallpaperOffsets(int wallpaperId)
    {
        Vector3 spriteOffset = Vector3.zero;
        Vector3 downOffset = Vector3.zero;
        if (m_WallpaperDisplayConfig != null)
        {
            m_WallpaperDisplayConfig.GetOffsets(wallpaperId, out spriteOffset, out downOffset);
        }

        m_wallpaper.transform.localPosition = downOffset;
        m_heroSprite.transform.localPosition = spriteOffset;
    }
}
