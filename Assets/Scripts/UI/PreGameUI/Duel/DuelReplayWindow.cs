using System;
using System.Collections.Generic;
using System.Linq;
using AChen.Duel.Client;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

public sealed class DuelReplayWindow : AWindowController
{
    [SerializeField] UnityEngine.UI.Button m_BtnClose;
    [SerializeField] UnityEngine.UI.Button m_BtnRooms;
    [SerializeField] UnityEngine.UI.Button m_BtnRefresh;
    [SerializeField] UnityEngine.UI.Button m_BtnWatch;
    [SerializeField] TMP_Text m_TxtStatus;
    [SerializeField] TMP_Text m_TxtEmpty;
    [SerializeField] TMP_Text m_TxtOpponent;
    [SerializeField] TMP_Text m_TxtInfo;
    [SerializeField] AvatarPortraitView m_Portrait;
    [SerializeField] GameObject m_GoDetail;
    [SerializeField] RectTransform m_ListContent;
    [SerializeField] DuelLobbyRow m_RowPrefab;
    readonly List<DuelLobbyRow> m_rows = new();
    Guid m_selected;
    bool m_busy;
    DuelClientSession Session => DuelClientSession.Instance;

    protected override void AddListeners()
    {
        m_BtnClose.onClick.AddListener(UI_Close); m_BtnRooms.onClick.AddListener(OpenRooms);
        m_BtnRefresh.onClick.AddListener(Refresh); m_BtnWatch.onClick.AddListener(Watch);
    }
    protected override void RemoveListeners()
    {
        m_BtnClose.onClick.RemoveListener(UI_Close); m_BtnRooms.onClick.RemoveListener(OpenRooms);
        m_BtnRefresh.onClick.RemoveListener(Refresh); m_BtnWatch.onClick.RemoveListener(Watch);
        Session.Changed -= Render;
    }
    protected override void OnOpen() { Session.Changed += Render; Render(); Refresh(); }
    protected override void OnResume() { Render(); Refresh(); }
    protected override void OnClose() => Session.Changed -= Render;
    void OpenRooms()
    {
        UI_Close();
        RequestOpenWindow(AddressKeys.Prefab.DuelRoomWindow);
    }
    void Refresh() => RefreshAsync().Forget();
    async UniTask RefreshAsync()
    {
        if (m_busy || Session.IsBusy) return;
        m_busy = true; Render();
        await RunGuardedAsync(Session.RefreshReplaysAsync, "读取决斗回放", "err.duel.replays");
        m_busy = false; if (IsOpened) Render();
    }
    void Watch() => WatchAsync().Forget();
    async UniTask WatchAsync()
    {
        if (Session.Room != null && Session.Room.Players.First(p => p.Seat == Session.Room.LocalSeat).Ready)
        {
            m_TxtStatus.text = "请先取消房间准备，再观看回放";
            return;
        }
        if (m_busy || Session.IsBusy) return;
        m_busy = true; Render();
        await RunGuardedAsync(ct => Session.WatchReplayAsync(m_selected, ct), "观看决斗回放", "err.duel.watch");
        m_busy = false; if (IsOpened) Render();
    }
    void Select(Guid id) { m_selected = id; Render(); }
    static string Result(int winner, int localSeat) => winner < 0 ? "平局" : winner == localSeat ? "胜利" : "败北";
    void Render()
    {
        var session = Session; var busy = m_busy || session.IsBusy;
        foreach (var row in m_rows) Destroy(row.gameObject);
        m_rows.Clear();
        if (session.Replays.Count > 0 && !session.Replays.Any(r => r.Id == m_selected)) m_selected = session.Replays[0].Id;
        foreach (var replay in session.Replays)
        {
            var opponent = replay.Players.First(p => p.Seat != replay.LocalSeat);
            var row = Instantiate(m_RowPrefab, m_ListContent);
            row.Bind(opponent.Nickname, replay.FinishedAt.ToOffset(TimeSpan.FromHours(8)).ToString("yyyy-MM-dd HH:mm") + "  /  " + replay.TurnCount + " 回合",
                Result(replay.Winner, replay.LocalSeat), opponent.AvatarId, opponent.AvatarFrameId, () => Select(replay.Id));
            row.SetSelected(replay.Id == m_selected); row.SetInteractable(!busy); m_rows.Add(row);
        }
        m_TxtEmpty.text = "还没有已结束的对局回放";
        m_TxtEmpty.gameObject.SetActive(session.Replays.Count == 0);
        m_GoDetail.SetActive(session.Replays.Count > 0);
        m_TxtStatus.text = session.StatusMessage; m_BtnRefresh.interactable = !busy;
        m_BtnWatch.interactable = !busy && session.Replays.Count > 0;
        if (session.Replays.Count == 0) return;
        var selected = session.Replays.First(r => r.Id == m_selected);
        var other = selected.Players.First(p => p.Seat != selected.LocalSeat);
        m_Portrait.SetPortrait(other.AvatarId, other.AvatarFrameId);
        m_TxtOpponent.text = "对手  " + other.Nickname;
        m_TxtInfo.text = Result(selected.Winner, selected.LocalSeat) + "\n\n" + selected.FinishedAt.ToOffset(TimeSpan.FromHours(8)).ToString("yyyy-MM-dd HH:mm") + "\n\n" + selected.TurnCount + " 回合";
    }
}
