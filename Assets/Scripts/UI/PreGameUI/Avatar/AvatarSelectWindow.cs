using System;
using System.Collections.Generic;
using System.Threading;
using AChen.Events;
using AChen.Networking;
using AChen.Player;
using Cysharp.Threading.Tasks;
using LitMotion;
using UnityEngine;
using UnityEngine.UI;

/// <summary>头像选择窗口, 根据玩家资产和配置变化刷新可选头像.</summary>
public class AvatarSelectWindow : AWindowController
{
    [SerializeField] GridListController m_AvatarListController;
    [SerializeField] Button m_BtnConfirm;
    [SerializeField] Button m_BtnClose;
    [SerializeField] float m_ScrollToSelectedDuration = 0.25f;
    [SerializeField] Ease m_ScrollToSelectedEase = Ease.InOutCubic;

    List<ShopOwnedItemData> m_avatars;
    Guid? m_playerId;
    int? m_selectedAvatarId;
    bool m_isLoading;
    bool m_isSubmitting;
    bool m_loadFailureShown;
    GameConfigSnapshot m_configSnapshot;
    CancellationTokenSource m_loadCancellation;

    protected override void AddListeners()
    {
        m_BtnConfirm.onClick.AddListener(OnConfirmClick);
        m_BtnClose.onClick.AddListener(UI_Close);
    }

    protected override void RemoveListeners()
    {
        StopObserving();
        m_BtnConfirm.onClick.RemoveListener(OnConfirmClick);
        m_BtnClose.onClick.RemoveListener(UI_Close);
    }

    protected override void OnOpen()
    {
        PlayerData player = PlayerSession.HasInstance ? PlayerSession.Instance.CurrentPlayer : null;
        m_playerId = player?.Id;
        m_selectedAvatarId = player?.AvatarId;
        m_avatars = null;
        m_isSubmitting = false;
        m_loadFailureShown = false;
        m_configSnapshot = GameConfigManager.HasInstance ? GameConfigManager.Instance.Store.Snapshot : null;
        EventCenter.AddListener(GameEvent.PlayerOwnedAvatarsChanged, OnInventoryChanged);
        EventCenter.AddListener(GameEvent.GameConfigChanged, OnGameConfigChanged);
        BindList();
    }

    protected override void OnResume()
    {
        BindList();
    }

    protected override void OnHide()
    {
        CancelListLoad();
    }

    protected override void OnClose()
    {
        StopObserving();
    }

    void StopObserving()
    {
        EventCenter.RemoveListener(GameEvent.PlayerOwnedAvatarsChanged, OnInventoryChanged);
        EventCenter.RemoveListener(GameEvent.GameConfigChanged, OnGameConfigChanged);
        CancelListLoad();
    }

    void CancelListLoad()
    {
        m_loadCancellation?.Cancel();
        m_loadCancellation?.Dispose();
        m_loadCancellation = null;
    }

    void OnInventoryChanged(PlayerData player)
    {
        if (m_playerId != player?.Id)
        {
            m_playerId = player?.Id;
            m_selectedAvatarId = player?.AvatarId;
        }
        BindList();
    }

    void OnGameConfigChanged(GameConfigSnapshot snapshot, bool isStale)
    {
        if (ReferenceEquals(m_configSnapshot, snapshot)) return;
        m_configSnapshot = snapshot;
        BindList();
    }

    void BindList()
    {
        if (!IsVisible || !IsOpened) return;
        CancelListLoad();
        m_loadCancellation = CancellationTokenSource.CreateLinkedTokenSource(ScreenToken);
        BindListAsync(m_loadCancellation.Token).Forget();
    }

    // 列表加载与提交分开取消: 隐藏或重新刷新只取消加载, 提交随窗口关闭取消.
    async UniTask BindListAsync(CancellationToken token)
    {
        m_isLoading = true;
        UpdateConfirmButton();
        try
        {
            PlayerData player = PlayerSession.HasInstance ? PlayerSession.Instance.CurrentPlayer : null;
            List<ShopOwnedItemData> avatars = player == null
                ? new List<ShopOwnedItemData>()
                : await ShopCatalogQuery.LoadCosmeticsAsync(ShopCatalogTypes.Avatar, visibleOnly: false, cancellationToken: token);
            token.ThrowIfCancellationRequested();
            SortOwnedThenId(avatars);
            int selected = avatars.FindIndex(item => item.Owned && item.Id == m_selectedAvatarId);
            if (selected < 0)
            {
                selected = avatars.FindIndex(item => item.Owned && item.Id == player?.AvatarId);
            }

            await m_AvatarListController.InitList(
                AddressKeys.Prefab.AvatarItemPrefab,
                avatars,
                OnAvatarSelected,
                selected,
                cancellationToken: token);
            token.ThrowIfCancellationRequested();
            m_avatars = avatars;
            m_selectedAvatarId = selected >= 0 ? avatars[selected].Id : (int?)null;
            // 等列表首帧创建可视行; 关闭或新刷新会取消旧加载, 避免回写过期列表.
            await UniTask.Yield(cancellationToken: token);
            token.ThrowIfCancellationRequested();
            m_AvatarListController.MoveToSelectedIfHidden(m_ScrollToSelectedDuration, m_ScrollToSelectedEase);
            m_loadFailureShown = false;
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception exception)
        {
            if (token.IsCancellationRequested) return;
            m_avatars = null;
            ALog.LogError($"头像列表加载失败: Error={exception.Message}", ALogCategories.UI);
            // 恢复窗口会重新加载, 同一次失败只弹一次提示, 避免错误弹窗循环.
            if (!m_loadFailureShown)
            {
                m_loadFailureShown = true;
                ShowMessage("头像列表加载失败，请稍后重试");
            }
        }
        finally
        {
            if (this != null && !token.IsCancellationRequested)
            {
                m_isLoading = false;
                UpdateConfirmButton();
            }
        }
    }

    void OnAvatarSelected(int index)
    {
        if (m_isLoading || m_avatars == null || index < 0 || index >= m_avatars.Count) return;
        m_selectedAvatarId = m_avatars[index].Id;
        UpdateConfirmButton();
    }

    void UpdateConfirmButton()
    {
        m_BtnConfirm.interactable = !m_isLoading && !m_isSubmitting && m_avatars != null &&
                                   m_avatars.Exists(item => item.Owned && item.Id == m_selectedAvatarId);
    }

    // 已拥有在前, 同组再按 Id
    static void SortOwnedThenId(List<ShopOwnedItemData> avatars)
    {
        avatars.Sort(static (a, b) =>
        {
            int owned = b.Owned.CompareTo(a.Owned);
            return owned != 0 ? owned : a.Id.CompareTo(b.Id);
        });
    }

    void OnConfirmClick()
    {
        SubmitAsync().Forget();
    }

    async UniTaskVoid SubmitAsync()
    {
        if (m_isLoading || m_isSubmitting || !IsOpened) return;
        ShopOwnedItemData selected = m_avatars?.Find(item => item.Id == m_selectedAvatarId);
        if (selected == null || !selected.Owned)
        {
            ALog.LogWarning($"确认头像失败: Id={selected?.Id}, 原因={(selected == null ? "未选中" : "未拥有")}", ALogCategories.UI);
            return;
        }

        m_isSubmitting = true;
        UpdateConfirmButton();
        bool succeeded = await RunGuardedAsync(
            token => PlayerSession.Instance.SetAvatarAsync(selected.Id, token),
            "修改头像",
            "修改头像失败，请稍后重试");
        if (this == null || !IsOpened) return;

        m_isSubmitting = false;
        UpdateConfirmButton();
        if (succeeded) UI_Close();
    }
}
