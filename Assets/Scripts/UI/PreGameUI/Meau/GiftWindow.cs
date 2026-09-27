using System.Collections.Generic;
using AChen.Networking;
using AChen.Player;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GiftWindow : AWindowController
{
    // --tag_start: 自动生成--
    [SerializeField] Button m_BtnClose;
    [SerializeField] ScrollRect m_ScrList;
    [SerializeField] TMP_Text m_TxtEmpty;
    // --tag_end: 自动生成--
    [SerializeField] InboxListController m_ListController;

    bool m_busy;

    protected override void AddListeners()
    {
        m_BtnClose.onClick.AddListener(UI_Close);
    }

    protected override void RemoveListeners()
    {
        m_BtnClose.onClick.RemoveListener(UI_Close);
    }

    protected override void OnOpen()
    {
        m_busy = false;
        LoadInboxAsync().Forget();
    }

    public void HandleRowAction(InboxItemData data, InboxRowAction action)
    {
        if (data == null || m_busy || !IsOpened)
        {
            return;
        }

        var nameArgs = new Dictionary<string, object> { ["name"] = data.Nickname };
        switch (action)
        {
            case InboxRowAction.Accept:
                Confirm("ui.gifts.confirm_accept", () => AcceptAsync(data).Forget(), nameArgs);
                break;
            case InboxRowAction.Reject:
                Confirm("ui.gifts.confirm_reject", () => RejectAsync(data).Forget(), nameArgs);
                break;
            case InboxRowAction.Claim:
                Confirm("ui.gifts.confirm_claim", () => ClaimAsync(data).Forget());
                break;
        }
    }

    async UniTaskVoid LoadInboxAsync()
    {
        if (m_busy || !IsOpened) return;

        m_busy = true;
        bool succeeded = await RunGuardedAsync(async token =>
        {
            IReadOnlyList<InboxItemData> items = await PlayerSession.Instance.GetInboxAsync(token);
            var rows = new List<InboxItemData>(items);
            await m_ListController.BindAsync(rows, token);
            ShowEmpty(rows.Count == 0);
            ALog.Log($"礼品箱已刷新. Count={rows.Count}", ALogCategories.UI);
        }, "加载礼品箱", "err.gift_claim_failed");
        if (this == null || !IsOpened) return;
        m_busy = false;
        if (!succeeded) ShowEmpty(true);
    }

    async UniTaskVoid AcceptAsync(InboxItemData data)
    {
        if (m_busy || !IsOpened) return;

        m_busy = true;
        bool succeeded = await RunGuardedAsync(async token =>
        {
            await PlayerSession.Instance.AcceptFriendRequestAsync(data.Id, token);
            ALog.Log($"已同意好友申请. Request={data.Id}; From={data.Nickname}", ALogCategories.UI);
            await RefreshInboxAsync(token);
        }, "同意好友申请", "err.friend_accept_failed");
        if (this == null || !IsOpened) return;
        m_busy = false;
        if (!succeeded) ShowEmpty(true);
    }

    async UniTaskVoid RejectAsync(InboxItemData data)
    {
        if (m_busy || !IsOpened) return;

        m_busy = true;
        bool succeeded = await RunGuardedAsync(async token =>
        {
            await PlayerSession.Instance.RejectFriendRequestAsync(data.Id, token);
            ALog.Log($"已拒绝好友申请. Request={data.Id}; From={data.Nickname}", ALogCategories.UI);
            await RefreshInboxAsync(token);
        }, "拒绝好友申请", "err.friend_reject_failed");
        if (this == null || !IsOpened) return;
        m_busy = false;
        if (!succeeded) ShowEmpty(true);
    }

    async UniTaskVoid ClaimAsync(InboxItemData data)
    {
        if (m_busy || !IsOpened) return;

        m_busy = true;
        bool succeeded = await RunGuardedAsync(async token =>
        {
            GiftClaimResult result = await PlayerSession.Instance.ClaimGiftAsync(data.Id, token);
            PlayerData player = result.Player;
            ALog.Log($"已领取礼品. Gift={data.Id}; Gold={player.Gold}; Revision={player.Revision}", ALogCategories.UI);
            await RefreshInboxAsync(token);
            if (result.UrGained > 0) RequestOpenWindow(AddressKeys.Prefab.UrNoticeWindow, new UrNoticeProperties(new LocalizedMessage("ui.workshop.gift_overflow", new Dictionary<string, object> { ["amount"] = result.UrGained })));
        }, "领取礼品", "err.gift_claim_failed");
        if (this == null || !IsOpened) return;
        m_busy = false;
        if (!succeeded) ShowEmpty(true);
    }

    async UniTask RefreshInboxAsync(System.Threading.CancellationToken token)
    {
        IReadOnlyList<InboxItemData> items = await PlayerSession.Instance.GetInboxAsync(token);
        var rows = new List<InboxItemData>(items);
        await m_ListController.BindAsync(rows, token);
        ShowEmpty(rows.Count == 0);
    }

    void ShowEmpty(bool empty)
    {
        if (m_TxtEmpty == null) return;
        m_TxtEmpty.gameObject.SetActive(empty);
        if (empty)
        {
            m_TxtEmpty.Localized().SetKey("ui.gifts.empty");
        }
    }
}
