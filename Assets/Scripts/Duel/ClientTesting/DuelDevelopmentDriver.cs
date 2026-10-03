#if (DEVELOPMENT_BUILD && UNITY_STANDALONE) || UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using AChen.Duel.Client;
using AChen.Duel.Presentation;
using AChen.Networking;
using AChen.Player;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AChen.Duel.ClientTesting
{
    /// <summary>仅由显式命令行启用的开发联调入口，所有操作经过真实账号和对战会话。</summary>
    public sealed class DuelDevelopmentDriver : MonoBehaviour
    {
        static DuelDevelopmentDriver s_driver;
        UIFrame m_frame;
        string m_controlPath, m_accountsPath;
        int m_accountIndex;
        long m_sequence;
        float m_nextRead;
        bool m_executing;
        public static void Attach(UIFrame frame)
        {
            if (!Debug.isDebugBuild && !Application.isEditor) return;
            if (s_driver != null) { s_driver.m_frame = frame; return; }
            var args = Environment.GetCommandLineArgs();
            int index = Array.IndexOf(args, "-duel-test-control");
            if (index < 0 || index + 1 == args.Length || !File.Exists(args[index + 1])) return;
            var go = new GameObject("DuelDevelopmentDriver");
            DontDestroyOnLoad(go);
            s_driver = go.AddComponent<DuelDevelopmentDriver>();
            s_driver.m_controlPath = Path.GetFullPath(args[index + 1]);
            s_driver.m_frame = frame;
        }
        void Update()
        {
            if (m_executing || Time.unscaledTime < m_nextRead) return;
            m_nextRead = Time.unscaledTime + .15f;
            // 写入方采用临时文件替换；读取到完整递增命令后只执行一次。
            try
            {
                var command = JObject.Parse(File.ReadAllText(m_controlPath));
                long sequence = (long?)command["seq"] ?? 0;
                if (sequence <= m_sequence) return;
                m_sequence = sequence; m_executing = true;
                ExecuteAsync(command).Forget();
            }
            catch (IOException) { }
            catch (Newtonsoft.Json.JsonException) { }
        }
        JObject Account(JObject command)
        {
            if (command["privateAccountsPath"] != null) m_accountsPath = (string)command["privateAccountsPath"];
            if (command["accountIndex"] != null) m_accountIndex = (int)command["accountIndex"];
            var document = JObject.Parse(File.ReadAllText(m_accountsPath));
            return (JObject)document["accounts"][m_accountIndex];
        }
        async UniTask ExecuteAsync(JObject command)
        {
            string statePath = (string)command["statusPath"] ?? Path.ChangeExtension(m_controlPath, ".state.json");
            WriteState(statePath, "executing", "");
            try
            {
                var session = DuelClientSession.Instance;
                switch ((string)command["command"])
                {
                    case "login":
                        var account = Account(command);
                        await PlayerSession.Instance.LoginAsync((string)account["username"], (string)account["password"]);
                        break;
                    case "openRooms":
                        m_frame.OpenWindow(AddressKeys.Prefab.DuelRoomWindow);
                        await session.RefreshRoomsAsync(); break;
                    case "create": await session.CreateRoomAsync(); break;
                    case "join": await session.JoinRoomAsync((string)command["code"]); break;
                    case "selectDeck": await session.SelectDeckAsync(Guid.Parse((string)Account(command)["deckId"])); break;
                    case "selectSavedDeck": await session.SelectDeckAsync(session.SavedDecks[(int?)command["deckIndex"] ?? 0].Id); break;
                    case "ready": await session.SetReadyAsync((bool)command["ready"]); break;
                    case "closeWindow": m_frame.CloseCurrentWindow(); break;
                    case "browse":
                        UnityEngine.Object.FindFirstObjectByType<BattleSceneController>().OpenZone(new ZoneRef(
                            Enum.Parse<DuelZone>((string)command["zone"]), (int)command["player"])); break;
                    case "inspect": UnityEngine.Object.FindFirstObjectByType<BattleSceneController>().OpenDetail((int)command["cardId"]); break;
                    case "snapshot": break;
                    case "submit": Submit(session.PresentationSource, command); break;
                    case "surrender": await session.SurrenderAsync(); break;
                    case "returnLobby": await session.ReturnToLobbyAsync(); break;
                    case "openReplayList":
                        m_frame.OpenWindow(AddressKeys.Prefab.DuelReplayWindow);
                        await session.RefreshReplaysAsync(); break;
                    case "watchFirstReplay": await session.WatchReplayAsync(session.Replays[0].Id); break;
                    case "togglePause": session.ToggleReplayPause(); break;
                    case "switchView": session.SwitchReplayView(); break;
                    case "exitReplay": await session.ExitReplayAsync(); break;
                    case "screenshot":
                        ScreenCapture.CaptureScreenshot(Path.GetFullPath((string)command["screenshotPath"]));
                        await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate); break;
                    default: throw new InvalidOperationException("Unknown development command");
                }
                WriteState(statePath, "complete", "");
            }
            catch (Exception exception)
            {
                // 不序列化异常正文，避免私有账号解析错误携带凭据片段。
                string error = exception is BackendApiException api ? exception.GetType().Name + ":" + api.StatusCode : exception.GetType().Name;
                WriteState(statePath, "failed", error);
            }
            finally { m_executing = false; }
        }
        static void Submit(IDuelPresentationSource source, JObject command)
        {
            switch ((string)command["input"])
            {
                case "BeginAction": source.Submit(new BeginCardAction((int)command["cardId"], (string)command["actionId"])); break;
                case "ConfirmSelection":
                    source.Submit(new ConfirmDuelSelection((long)command["choiceId"], command["keys"].Values<string>())); break;
                case "ConfirmTarget":
                    source.Submit(new ConfirmActionTarget(new ZoneRef(Enum.Parse<DuelZone>((string)command["zone"]), (int)command["player"], (int?)command["slot"] ?? 0))); break;
                case "ChoosePosition": source.Submit(new ChooseActionPosition(Enum.Parse<CardPosition>((string)command["position"]))); break;
                case "ChangePhase": source.Submit(new ChangePhase(Enum.Parse<DuelPhase>((string)command["phase"]))); break;
                case "EndTurn": source.Submit(new EndTurn()); break;
                case "Cancel": source.Submit(new CancelCardAction()); break;
                case "PassResponse": source.Submit(new PassDuelResponse()); break;
                case "Surrender": source.Submit(new SurrenderDuel()); break;
                default: throw new InvalidOperationException("Unknown development input");
            }
        }
        void WriteState(string path, string status, string error)
        {
            var session = DuelClientSession.Instance;
            var source = session.PresentationSource;
            var view = source?.Current;
            File.WriteAllText(path, BackendJson.Serialize(new
            {
                seq = m_sequence, status, error, scene = SceneManager.GetActiveScene().name,
                mode = session.Mode.ToString(), authenticated = PlayerSession.Instance.IsAuthenticated,
                windowBusy = m_frame.IsWindowBusy, selectionPanel = m_frame.IsPanelOpen(AddressKeys.Prefab.BattleSelectionPanel),
                room = session.Room == null ? null : new { session.Room.Code, session.Room.Sequence, session.Room.Status, session.Room.LocalSeat,
                    players = session.Room.Players.Select(p => new { p.Seat, p.Nickname, p.Ready }) },
                deckIds = session.SavedDecks.Select(d => new { d.Id, d.Name }), replayIds = session.Replays.Select(r => r.Id),
                clientStatus = session.StatusMessage, busy = session.IsBusy, replayPaused = session.ReplayPaused,
                battle = view == null ? null : new
                {
                    view.Turn, phase = view.Phase.ToString(), view.ActivePlayer, view.Animating, view.Finished, view.ReadOnly,
                    view.CanInteract, view.CanBrowseCards, LP = view.Players.Select(p => p.LP), seconds = view.Seconds,
                    cards = view.Cards.Select(c => new { cardId = c.InstanceId, c.Owner, c.Known,
                        definitionId = c.Known ? c.Definition.CardId : "", zone = c.Zone.Kind.ToString(), c.Zone.Player, c.Zone.Slot,
                        position = c.Position.ToString(), actions = c.Actions.Select(a => new { a.Id, kind = a.Kind.ToString(), a.Label,
                            positions = a.Positions.Select(p => new { position = p.ToString(), targets = a.TargetsFor(p).Select(Zone) }) }) }),
                    pending = new { view.PendingAction.InstanceId, view.PendingAction.ActionId, kind = view.PendingAction.Kind.ToString(),
                        view.PendingAction.NeedsPosition, positions = view.PendingAction.Positions.Select(p => p.ToString()), targets = view.PendingAction.Targets.Select(Zone) },
                    choice = new { view.Choice.Id, view.Choice.Active, view.Choice.Prompt, view.Choice.Min, view.Choice.Max, view.Choice.CanCancel,
                        options = view.Choice.Options.Select(o => new { o.Key, o.Label, o.DefinitionId, o.CardId }) },
                    phases = view.AvailablePhases.Select(p => p.ToString())
                }
            }));
        }
        static object Zone(ZoneRef zone) => new { kind = zone.Kind.ToString(), zone.Player, zone.Slot };
    }
}
#endif
