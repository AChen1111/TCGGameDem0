using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AChen.Duel.Client;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

public sealed class DuelRoomWindow : AWindowController
{
    [SerializeField] UnityEngine.UI.Button m_BtnClose;
    [SerializeField] UnityEngine.UI.Button m_BtnReplay;
    [SerializeField] UnityEngine.UI.Button m_BtnRefresh;
    [SerializeField] UnityEngine.UI.Button m_BtnCreate;
    [SerializeField] UnityEngine.UI.Button m_BtnJoin;
    [SerializeField] UnityEngine.UI.Button m_BtnLeave;
    [SerializeField] UnityEngine.UI.Button m_BtnReady;
    [SerializeField] TMP_InputField m_InpCode;
    [SerializeField] TMP_Dropdown m_DropDeck;
    [SerializeField] TMP_Text m_TxtRoom;
    [SerializeField] TMP_Text m_TxtReady;
    [SerializeField] TMP_Text m_TxtStatus;
    [SerializeField] TMP_Text m_TxtEmpty;
    [SerializeField] TMP_Text m_TxtNoRoom;
    [SerializeField] RectTransform m_ListContent;
    [SerializeField] DuelLobbyRow m_RowPrefab;
    [SerializeField] GameObject m_GoRoom;
    [SerializeField] AvatarPortraitView[] m_Portraits;
    [SerializeField] TMP_Text[] m_Names;
    [SerializeField] TMP_Text[] m_Readies;
    readonly List<DuelLobbyRow> m_rows = new();
    bool m_busy;
    DuelClientSession Session => DuelClientSession.Instance;

    protected override void AddListeners()
    {
        m_BtnClose.onClick.AddListener(UI_Close);
        m_BtnReplay.onClick.AddListener(OpenReplay);
        m_BtnRefresh.onClick.AddListener(Refresh);
        m_BtnCreate.onClick.AddListener(Create);
        m_BtnJoin.onClick.AddListener(JoinCode);
        m_BtnLeave.onClick.AddListener(Leave);
        m_BtnReady.onClick.AddListener(Ready);
        m_DropDeck.onValueChanged.AddListener(SelectDeck);
    }
    protected override void RemoveListeners()
    {
        m_BtnClose.onClick.RemoveListener(UI_Close);
        m_BtnReplay.onClick.RemoveListener(OpenReplay);
        m_BtnRefresh.onClick.RemoveListener(Refresh);
        m_BtnCreate.onClick.RemoveListener(Create);
        m_BtnJoin.onClick.RemoveListener(JoinCode);
        m_BtnLeave.onClick.RemoveListener(Leave);
        m_BtnReady.onClick.RemoveListener(Ready);
        m_DropDeck.onValueChanged.RemoveListener(SelectDeck);
        Session.Changed -= Render;
    }
    protected override void OnOpen()
    {
        Session.Changed += Render;
        Render(); Refresh();
    }
    protected override void OnResume() { Render(); Refresh(); }
    protected override void OnClose() => Session.Changed -= Render;

    void Refresh() => Execute(Session.RefreshRoomsAsync, "读取决斗房间", "err.duel.rooms");
    void Create() => Execute(Session.CreateRoomAsync, "创建决斗房间", "err.duel.create");
    void Leave() => Execute(Session.LeaveRoomAsync, "退出决斗房间", "err.duel.leave");
    void JoinCode() => Join(m_InpCode.text.Trim());
    void Join(string code) => Execute(ct => Session.JoinRoomAsync(code, ct), "加入决斗房间", "err.duel.join");
    void Ready()
    {
        var local = Session.Room.Players.First(p => p.Seat == Session.Room.LocalSeat);
        Execute(ct => Session.SetReadyAsync(!local.Ready, ct), "修改准备状态", "err.duel.ready");
    }
    void SelectDeck(int index)
    {
        if (index == 0) return;
        Execute(ct => Session.SelectDeckAsync(Session.SavedDecks[index - 1].Id, ct), "选择决斗牌组", "err.duel.deck");
    }
    void OpenReplay()
    {
        UI_Close();
        RequestOpenWindow(AddressKeys.Prefab.DuelReplayWindow);
    }

    void Execute(Func<CancellationToken, UniTask> command, string operation, string messageKey) => RunCommand(command, operation, messageKey).Forget();
    async UniTask RunCommand(Func<CancellationToken, UniTask> command, string operation, string messageKey)
    {
        if (m_busy || Session.IsBusy) return;
        m_busy = true; Render();
        await RunGuardedAsync(command, operation, messageKey);
        m_busy = false;
        if (IsOpened) Render();
    }
    void Render()
    {
        var session = Session; var busy = m_busy || session.IsBusy;
        foreach (var row in m_rows) Destroy(row.gameObject);
        m_rows.Clear();
        foreach (var room in session.Rooms)
        {
            var row = Instantiate(m_RowPrefab, m_ListContent);
            row.Bind(room.Name, room.HostNickname + "  /  " + room.Code, room.PlayerCount + " / 2",
                room.HostAvatarId, room.HostAvatarFrameId, () => Join(room.Code));
            row.SetInteractable(!busy && session.Room == null); m_rows.Add(row);
        }
        m_TxtEmpty.text = "暂无可加入的房间";
        m_TxtEmpty.gameObject.SetActive(session.Rooms.Count == 0);
        m_TxtStatus.text = session.StatusMessage;
        m_BtnRefresh.interactable = !busy;
        var hasRoom = session.Room != null;
        m_GoRoom.SetActive(hasRoom); m_TxtNoRoom.gameObject.SetActive(!hasRoom);
        m_BtnCreate.interactable = m_BtnJoin.interactable = m_InpCode.interactable = !busy && !hasRoom;
        if (!hasRoom) return;

        var current = session.Room;
        m_TxtRoom.text = current.Players.First(p => p.Seat == 0).Nickname + "的房间\n" + current.Code;
        for (var seat = 0; seat < 2; seat++)
        {
            var player = current.Players.FirstOrDefault(p => p.Seat == seat);
            // 空席位是正常的等待状态；头像引用始终由 Prefab 绑定。
            m_Portraits[seat].gameObject.SetActive(player != null);
            m_Names[seat].text = player == null ? "等待加入" : player.Nickname;
            m_Readies[seat].text = player == null ? "空闲席位" : player.Ready ? "已准备" : "未准备";
            if (player != null) m_Portraits[seat].SetPortrait(player.AvatarId, player.AvatarFrameId);
        }
        var localPlayer = current.Players.First(p => p.Seat == current.LocalSeat);
        m_DropDeck.ClearOptions();
        var deckOptions = new List<string> { "请选择已保存牌组" };
        deckOptions.AddRange(session.SavedDecks.Select(d => d.Name));
        m_DropDeck.AddOptions(deckOptions);
        var selectedIndex = session.SavedDecks.ToList().FindIndex(d => d.Id == session.SelectedDeckId);
        m_DropDeck.SetValueWithoutNotify(selectedIndex + 1);
        m_DropDeck.RefreshShownValue();
        m_DropDeck.interactable = !busy && !localPlayer.Ready && session.SavedDecks.Count > 0;
        m_BtnReady.interactable = !busy && localPlayer.HasDeck;
        m_TxtReady.text = localPlayer.Ready ? "取消准备" : "准备";
        m_BtnLeave.interactable = !busy;
    }
}
