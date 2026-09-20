using System;
using System.Collections.Generic;
using AChen.Networking;
using AChen.Player;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FriendWindow : AWindowController
{
    // --tag_start: 自动生成--
    [SerializeField] TMP_InputField m_InpName;
    [SerializeField] Button m_BtnFind;
    [SerializeField] Button m_BtnClose;
    [SerializeField] ScrollRect m_ScrList;
    [SerializeField] TMP_Text m_TxtEmpty;
    // --tag_end: 自动生成--
    [SerializeField] GridListController m_ListController;

    bool m_busy;
    bool m_searchMode;
    List<FriendSearchHitData> m_rows;
    WindowCanvasFade m_fade;

    WindowCanvasFade Fade => m_fade ??= new WindowCanvasFade(this);

    protected override void AddListeners()
    {
        m_BtnClose.onClick.AddListener(UI_Close);
        m_BtnFind.onClick.AddListener(OnFindClick);
    }

    protected override void RemoveListeners()
    {
        m_BtnClose.onClick.RemoveListener(UI_Close);
        m_BtnFind.onClick.RemoveListener(OnFindClick);
    }

    protected override void OnOpen()
    {
        m_busy = false;
        m_searchMode = false;
        Fade.PlayOpen();
        LoadFriendsAsync().Forget();
    }

    public override void UI_Close()
    {
        Fade.RequestClose(base.UI_Close);
    }

    protected override void OnClose()
    {
        Fade.Reset();
    }

    public void HandleRowAction(FriendSearchHitData data)
    {
        if (data == null || m_busy || !IsOpened)
        {
            return;
        }

        var args = new Dictionary<string, object> { ["name"] = data.Nickname };
        if (data.IsPending)
        {
            return;
        }

        if (data.IsFriend)
        {
            Confirm("ui.friends.confirm_duel", () =>
            {
                ALog.Log($"好友决斗占位. Friend={data.Id}; Name={data.Nickname}", ALogCategories.UI);
            }, args);
            return;
        }

        Confirm("ui.friends.confirm_add", () => SendRequestAsync(data).Forget(), args);
    }

    void OnFindClick()
    {
        string needle = m_InpName.text?.Trim() ?? string.Empty;
        if (needle.Length == 0)
        {
            LoadFriendsAsync().Forget();
            return;
        }

        SearchAsync(needle).Forget();
    }

    async UniTaskVoid LoadFriendsAsync()
    {
        if (m_busy || !IsOpened) return;

        m_busy = true;
        m_searchMode = false;
        bool succeeded = await RunGuardedAsync(async token =>
        {
            IReadOnlyList<FriendSummaryData> friends = await PlayerSession.Instance.GetFriendsAsync(token);
            var rows = new List<FriendSearchHitData>(friends.Count);
            for (int i = 0; i < friends.Count; i++)
            {
                rows.Add(FriendSearchHitData.FromFriend(friends[i]));
            }

            await BindRowsAsync(rows, token);
            ALog.Log($"好友列表已刷新. Count={rows.Count}", ALogCategories.UI);
        }, "加载好友列表", "err.friend_request_failed");
        if (this == null || !IsOpened) return;
        m_busy = false;
        if (!succeeded) ShowEmpty(true);
    }

    async UniTaskVoid SearchAsync(string nickname)
    {
        if (m_busy || !IsOpened) return;

        m_busy = true;
        m_searchMode = true;
        bool succeeded = await RunGuardedAsync(async token =>
        {
            IReadOnlyList<FriendSearchHitData> hits = await PlayerSession.Instance.SearchFriendsAsync(nickname, token);
            var rows = new List<FriendSearchHitData>(hits);
            await BindRowsAsync(rows, token);
            ALog.Log($"好友搜索完成. Nickname={nickname}; Count={rows.Count}", ALogCategories.UI);
        }, "搜索玩家", "err.friend_request_failed");
        if (this == null || !IsOpened) return;
        m_busy = false;
        if (!succeeded) ShowEmpty(true);
    }

    async UniTaskVoid SendRequestAsync(FriendSearchHitData data)
    {
        if (m_busy || !IsOpened) return;

        m_busy = true;
        bool succeeded = await RunGuardedAsync(async token =>
        {
            await PlayerSession.Instance.SendFriendRequestAsync(data.Id, token);
            MarkPending(data.Id);
            await BindRowsAsync(m_rows, token);
            ALog.Log($"已发送好友申请. Target={data.Id}; Name={data.Nickname}", ALogCategories.UI);
        }, "发送好友申请", "err.friend_request_failed");
        if (this == null || !IsOpened) return;
        m_busy = false;
    }

    async UniTask BindRowsAsync(List<FriendSearchHitData> rows, System.Threading.CancellationToken token)
    {
        m_rows = rows ?? new List<FriendSearchHitData>();
        await m_ListController.InitList(AddressKeys.Prefab.FriendRowPrefab, m_rows, cancellationToken: token);
        ShowEmpty(m_rows.Count == 0);
    }

    void MarkPending(Guid playerId)
    {
        if (m_rows == null)
        {
            return;
        }

        for (int i = 0; i < m_rows.Count; i++)
        {
            FriendSearchHitData row = m_rows[i];
            if (row != null && row.Id == playerId)
            {
                m_rows[i] = row.AsPending();
            }
        }
    }

    void ShowEmpty(bool empty)
    {
        if (m_TxtEmpty == null) return;
        m_TxtEmpty.gameObject.SetActive(empty);
        if (!empty) return;
        m_TxtEmpty.Localized().SetKey(m_searchMode ? "ui.friends.search_empty" : "ui.friends.empty");
    }
}
