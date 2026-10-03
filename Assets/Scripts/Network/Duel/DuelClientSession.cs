using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using AChen.Decks;
using AChen.Events;
using AChen.Networking;
using AChen.Player;
using AChen.Duel.Presentation;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using Core = AChen.Duel.Core;

namespace AChen.Duel.Client
{
    /// <summary>登录账号的房间连接。生命周期独立于Window；规则状态由服务器持有。</summary>
    public sealed class DuelClientSession
    {
        static DuelClientSession s_instance;
        public static DuelClientSession Instance => s_instance ??= new DuelClientSession();
        readonly SemaphoreSlim m_mutations = new SemaphoreSlim(1, 1);
        readonly SemaphoreSlim m_sends = new SemaphoreSlim(1, 1);
        readonly ConcurrentDictionary<Guid, string> m_pending = new ConcurrentDictionary<Guid, string>();
        CancellationTokenSource m_roomLife;
        ClientWebSocket m_socket;
        NetworkDuelSession m_live;
        ReplayDuelSession m_replay;
        int m_connectionGeneration;
        bool m_enteringBattle;
        bool m_returning;
        bool m_openReplayAfterLobby;
        BackendHttpClient Http => new BackendHttpClient(PlayerSession.Instance.DuelBackendConfig);
        public DuelRoomDto Room { get; private set; }
        public IReadOnlyList<DuelRoomSummaryDto> Rooms { get; private set; } = Array.Empty<DuelRoomSummaryDto>();
        public IReadOnlyList<DuelReplaySummaryDto> Replays { get; private set; } = Array.Empty<DuelReplaySummaryDto>();
        public IReadOnlyList<DeckData> SavedDecks { get; private set; } = Array.Empty<DeckData>();
        public Guid SelectedDeckId { get; private set; }
        public bool IsBusy { get; private set; }
        public string StatusMessage { get; private set; } = "";
        public DuelSessionMode Mode { get; private set; } = DuelSessionMode.Offline;
        public bool IsLocalReady => Room != null && Room.Players.Single(p => p.Seat == Room.LocalSeat).Ready;
        public bool ReplayPaused => m_replay != null && m_replay.Paused;
        public IDuelPresentationSource PresentationSource => Mode == DuelSessionMode.Replay ? (IDuelPresentationSource)m_replay : m_live;
        public event Action Changed = delegate { };

        public static void Initialize()
        {
            if (s_instance != null) s_instance.StopConnection();
            s_instance = new DuelClientSession();
            EventCenter.RemoveListener(GameEvent.PlayerLoggedOut, OnLoggedOut);
            EventCenter.AddListener(GameEvent.PlayerLoggedOut, OnLoggedOut);
        }
        static void OnLoggedOut(AuthUser user)
        {
            var session = Instance;
            session.StopConnection();
            if (session.m_live != null) session.m_live.Dispose();
            session.m_live = null; session.m_replay = null;
            session.Room = null; session.Mode = DuelSessionMode.Offline;
        }
        async UniTask<T> Request<T>(string method, string path, object body, CancellationToken token) =>
            await PlayerSession.Instance.DuelAuthenticatedAsync((access, ct) => Http.SendAsync<T>(method, path, body, access, ct), token);
        async UniTask Request(string method, string path, object body, CancellationToken token) =>
            await PlayerSession.Instance.DuelAuthenticatedAsync(async (access, ct) =>
            { await Http.SendAsync(method, path, body, access, ct); return true; }, token);
        async UniTask Run(Func<CancellationToken, UniTask> operation, CancellationToken token)
        {
            await m_mutations.WaitAsync(token);
            IsBusy = true; Changed();
            try { await operation(token); }
            catch (BackendApiException ex) { StatusMessage = ex.Message; throw; }
            finally { IsBusy = false; m_mutations.Release(); Changed(); }
        }
        public UniTask RefreshRoomsAsync(CancellationToken token = default) => Run(async ct =>
        {
            SavedDecks = await PlayerSession.Instance.GetDecksAsync(ct);
            Rooms = await Request<DuelRoomSummaryDto[]>("GET", "/api/duel/rooms", null, ct);
            if (Room == null)
            {
                var json = await PlayerSession.Instance.DuelAuthenticatedAsync((access, requestToken) =>
                    Http.SendAsync("GET", "/api/duel/rooms/current", null, access, requestToken), ct);
                var current = JsonConvert.DeserializeObject<DuelRoomDto>(json, BackendJson.Settings);
                if (current != null) AdoptRoom(current);
            }
            StatusMessage = "";
        }, token);
        public UniTask CreateRoomAsync(CancellationToken token = default) => Run(async ct =>
        { AdoptRoom(await Request<DuelRoomDto>("POST", "/api/duel/rooms", null, ct)); StatusMessage = "房间已创建"; }, token);
        public UniTask JoinRoomAsync(string code, CancellationToken token = default) => Run(async ct =>
        { AdoptRoom(await Request<DuelRoomDto>("POST", "/api/duel/rooms/join", new { Code = code.Trim().Replace(" ", "") }, ct)); StatusMessage = "已加入房间"; }, token);
        void AdoptRoom(DuelRoomDto room)
        {
            Room = room;
            if (m_roomLife == null)
            {
                m_roomLife = CancellationTokenSource.CreateLinkedTokenSource(SingletonManager.Instance.GetCancellationTokenOnDestroy());
                ConnectLoopAsync(room.Id, ++m_connectionGeneration, m_roomLife.Token).Forget();
            }
            Changed();
        }
        public UniTask SelectDeckAsync(Guid deckId, CancellationToken token = default) => Run(async ct =>
        {
            var deck = await PlayerSession.Instance.GetDeckAsync(deckId, ct);
            var body = new DuelDeckDto
            {
                MainDeck = deck.MainDeck.SelectMany(e => Enumerable.Repeat(e.CardId, e.Count)).ToArray(),
                ExtraDeck = deck.ExtraDeck.SelectMany(e => Enumerable.Repeat(e.CardId, e.Count)).ToArray()
            };
            ApplyNotice(new DuelNoticeDto { Room = await Request<DuelRoomDto>("PUT", $"/api/duel/rooms/{Room.Id}/deck", body, ct) });
            SelectedDeckId = deckId; StatusMessage = "牌组已提交";
        }, token);
        public UniTask SetReadyAsync(bool ready, CancellationToken token = default) => Run(async ct =>
        { ApplyNotice(new DuelNoticeDto { Room = await Request<DuelRoomDto>("POST", $"/api/duel/rooms/{Room.Id}/ready", new { Ready = ready }, ct) }); StatusMessage = ready ? "等待双方准备" : "已取消准备"; }, token);
        public UniTask LeaveRoomAsync(CancellationToken token = default) => Run(LeaveCore, token);
        async UniTask LeaveCore(CancellationToken token)
        {
            if (Room == null) return;
            await Request("DELETE", $"/api/duel/rooms/{Room.Id}", null, token);
            StopConnection(); Room = null; SelectedDeckId = Guid.Empty;
            if (m_live != null) m_live.Dispose();
            m_live = null; StatusMessage = "已退出房间";
        }
        public UniTask RefreshReplaysAsync(CancellationToken token = default) => Run(async ct =>
        { Replays = await Request<DuelReplaySummaryDto[]>("GET", "/api/duel/replays", null, ct); StatusMessage = ""; }, token);
        public UniTask WatchReplayAsync(Guid replayId, CancellationToken token = default) => Run(async ct =>
        {
            if (IsLocalReady) throw new InvalidOperationException("请先取消准备，再观看回放");
            var replay = await Request<DuelReplayDto>("GET", $"/api/duel/replays/{replayId}", null, ct);
            m_replay = new ReplayDuelSession(replay); Mode = DuelSessionMode.Replay;
            await SceneLoader.LoadScene(AddressKeys.Scene.BattleScene);
        }, token);
        public UniTask<DuelPreviewDto> PreviewAsync(int seat, Core.DuelZone zone, CancellationToken token) =>
            Request<DuelPreviewDto>("GET", $"/api/duel/rooms/{Room.Id}/preview?seat={seat}&zone={(int)zone}", null, token);
        public UniTask<DuelNamePageDto> QueryNamesAsync(string actionToken, string query, int offset, CancellationToken token) =>
            Request<DuelNamePageDto>("GET", $"/api/duel/rooms/{Room.Id}/names?actionToken={Uri.EscapeDataString(actionToken)}&query={Uri.EscapeDataString(query)}&offset={offset}", null, token);

        async UniTaskVoid ConnectLoopAsync(Guid roomId, int generation, CancellationToken token)
        {
            DateTime? disconnectedAt = null;
            bool reconnecting = false;
            try
            {
                while (!token.IsCancellationRequested)
                {
                    using var socket = new ClientWebSocket();
                    try
                    {
                        await PlayerSession.Instance.DuelAuthenticatedAsync(async (access, ct) =>
                        {
                            await Http.SendAsync("GET", $"/api/duel/rooms/{roomId}", null, access, ct);
                            socket.Options.SetRequestHeader("Authorization", "Bearer " + access);
                            var baseUrl = PlayerSession.Instance.DuelBackendConfig.BaseUrl;
                            var uri = new UriBuilder(baseUrl) { Scheme = baseUrl.StartsWith("https:", StringComparison.Ordinal) ? "wss" : "ws" };
                            uri.Path = uri.Path.TrimEnd('/') + $"/api/duel/rooms/{roomId}/socket";
                            uri.Query = "protocolVersion=" + Core.DuelRulePackage.CurrentProtocolVersion;
                            await socket.ConnectAsync(uri.Uri, ct);
                            return true;
                        }, token);
                        if (generation != m_connectionGeneration || token.IsCancellationRequested) return;
                        m_socket = socket; StatusMessage = "已连接房间"; Changed(); disconnectedAt = null;
                        using var connected = CancellationTokenSource.CreateLinkedTokenSource(token);
                        PingAsync(socket, connected.Token).Forget();
                        bool firstNotice = true;
                        var buffer = new byte[8192];
                        try
                        {
                        while (socket.State == WebSocketState.Open)
                        {
                            using var body = new MemoryStream();
                            WebSocketReceiveResult received;
                            do
                            {
                                received = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), token);
                                if (received.MessageType == WebSocketMessageType.Close) break;
                                body.Write(buffer, 0, received.Count);
                            } while (!received.EndOfMessage);
                            if (received.MessageType == WebSocketMessageType.Close) break;
                            var notice = BackendJson.DeserializeResponse<DuelNoticeDto>(Encoding.UTF8.GetString(body.ToArray()));
                            await UniTask.SwitchToMainThread(token);
                            if (generation != m_connectionGeneration) return;
                            if (notice.Room.Id != roomId) throw new InvalidDataException("房间通知与连接不匹配");
                            if (firstNotice && reconnecting && m_live != null)
                                m_live.SetRecoveryPending(!m_pending.IsEmpty);
                            if (notice.Result != null) m_pending.TryRemove(notice.Result.RequestId, out _);
                            ApplyNotice(notice, firstNotice && reconnecting);
                            if (notice.Result != null && m_pending.IsEmpty && m_live != null)
                                m_live.SetRecoveryPending(false);
                            if (firstNotice)
                                foreach (var pending in m_pending.Values) await SendTextAsync(socket, pending, token);
                            firstNotice = false;
                        }
                        }
                        finally { connected.Cancel(); }
                    }
                    catch (OperationCanceledException) when (!token.IsCancellationRequested)
                    { if (generation != m_connectionGeneration) return; ALog.LogWarning("决斗连接已中断，准备重连", ALogCategories.Net); }
                    catch (WebSocketException ex)
                    { if (generation != m_connectionGeneration || token.IsCancellationRequested) return; ALog.LogWarning("决斗连接中断: " + ex.Message, ALogCategories.Net); }
                    catch (BackendApiException ex) when (ex.StatusCode == 404 || ex.StatusCode == 403)
                    { if (generation != m_connectionGeneration || token.IsCancellationRequested) return; StopConnection(); Room = null; StatusMessage = ex.Message; Changed(); return; }
                    catch (BackendApiException ex)
                    { if (generation != m_connectionGeneration || token.IsCancellationRequested) return; ALog.LogWarning("决斗连接失败: " + ex.Code, ALogCategories.Net); }
                    if (token.IsCancellationRequested) return;
                    reconnecting = true;
                    disconnectedAt ??= DateTime.UtcNow;
                    StatusMessage = "连接中断，正在重连"; Changed();
                    if ((DateTime.UtcNow - disconnectedAt.Value).TotalSeconds >= 60)
                    {
                        var final = await Request<DuelRoomDto>("GET", $"/api/duel/rooms/{roomId}", null, token);
                        if (generation != m_connectionGeneration || token.IsCancellationRequested) return;
                        ApplyNotice(new DuelNoticeDto { Room = final });
                        if (final.Status == "Finished") return;
                    }
                    await UniTask.Delay(2000, cancellationToken: token);
                }
            }
            catch (OperationCanceledException) { }
        }
        async UniTaskVoid PingAsync(ClientWebSocket socket, CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                { await UniTask.Delay(5000, cancellationToken: token); await SendTextAsync(socket, "{\"kind\":\"ping\"}", token); }
            }
            catch (OperationCanceledException) { }
            catch (WebSocketException) { }
        }
        async UniTask SendTextAsync(ClientWebSocket socket, string text, CancellationToken token)
        {
            await m_sends.WaitAsync(token);
            try { var bytes = Encoding.UTF8.GetBytes(text); await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, token); }
            finally { m_sends.Release(); }
        }
        void SendCommand(Core.SeatInput input)
        {
            var requestId = Guid.NewGuid();
            var text = BackendJson.Serialize(new { Kind = "command", RequestId = requestId, Input = input });
            m_pending.TryAdd(requestId, text);
            SendCommandAsync(text).Forget();
        }
        async UniTaskVoid SendCommandAsync(string text)
        {
            try { await SendTextAsync(m_socket, text, m_roomLife.Token); }
            catch (WebSocketException) { StatusMessage = "等待重新连接后恢复操作"; Changed(); }
            catch (OperationCanceledException) { }
        }
        void ApplyNotice(DuelNoticeDto notice, bool resynchronizing = false)
        {
            if (Room != null && notice.Room.Sequence < Room.Sequence) return;
            Room = notice.Room;
            if (Room.Status is "Running" or "Finished")
            {
                if (m_live == null) m_live = new NetworkDuelSession(Room, SendCommand, PreviewAsync, QueryNamesAsync);
                m_live.Receive(notice, resynchronizing);
                if (Mode != DuelSessionMode.Replay && !m_enteringBattle && Mode != DuelSessionMode.Online)
                    EnterBattleAsync().Forget();
            }
            Changed();
        }
        async UniTaskVoid EnterBattleAsync()
        {
            m_enteringBattle = true; Mode = DuelSessionMode.Online;
            try { await SceneLoader.LoadScene(AddressKeys.Scene.BattleScene); }
            finally { m_enteringBattle = false; }
        }
        void StopConnection()
        {
            m_connectionGeneration++;
            if (m_roomLife != null) { m_roomLife.Cancel(); m_roomLife.Dispose(); m_roomLife = null; }
            if (m_socket != null) { m_socket.Abort(); m_socket = null; }
            m_pending.Clear();
        }
        public UniTask SurrenderAsync(CancellationToken token = default)
        { m_live.Submit(new SurrenderDuel()); return UniTask.CompletedTask; }
        public async UniTask ReturnToLobbyAsync(CancellationToken token = default)
        {
            if (m_returning) return;
            m_returning = true;
            try
            {
                await SceneTransitionOverlay.ShowAsync(token);
                await LeaveRoomAsync(token); Mode = DuelSessionMode.Offline;
                await SceneLoader.LoadScene(AddressKeys.Scene.GameScene);
            }
            catch { SceneTransitionOverlay.Hide(); throw; }
            finally { m_returning = false; }
        }
        public void ToggleReplayPause() { m_replay.TogglePause(); Changed(); }
        public void SwitchReplayView() { m_replay.SwitchView(); Changed(); }
        public async UniTask ExitReplayAsync(CancellationToken token = default)
        {
            if (m_returning) return;
            m_returning = true;
            try
            {
                await SceneTransitionOverlay.ShowAsync(token);
                m_replay = null; Mode = DuelSessionMode.Offline; m_openReplayAfterLobby = true;
                await SceneLoader.LoadScene(AddressKeys.Scene.GameScene);
            }
            finally { m_returning = false; }
        }
        public void OnLobbyReady(UIFrame frame)
        {
            if (!m_openReplayAfterLobby) return;
            m_openReplayAfterLobby = false; frame.OpenWindow(AddressKeys.Prefab.DuelReplayWindow);
        }
    }
}
