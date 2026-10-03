using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AChen.Duel.Client;
using Cysharp.Threading.Tasks;
using Core = AChen.Duel.Core;

namespace AChen.Duel.Presentation
{
    /// <summary>在线表现只消费服务端投影，并把完整动作草稿提交为 SeatInput。</summary>
    public sealed class NetworkDuelSession : IDuelPresentationSource, IDisposable
    {
        enum Stage { Response, Ability, Selection, Target, Position, Name, Zone, Decision }
        sealed class Step
        {
            public DuelRoomDto Room;
            public DuelFrameDto Frame;
            public bool Attack;
            public int[] Impact;
        }
        readonly DuelNetworkProjection m_projection;
        readonly Action<Core.SeatInput> m_send;
        readonly Func<int, Core.DuelZone, CancellationToken, UniTask<DuelPreviewDto>> m_preview;
        readonly Func<string, string, int, CancellationToken, UniTask<DuelNamePageDto>> m_names;
        readonly Queue<Step> m_steps = new();
        readonly Queue<Stage> m_stages = new();
        readonly Dictionary<ZoneRef, string> m_zoneAnswers = new();
        readonly float[] m_seconds = new float[2];
        CancellationTokenSource m_draft = new();
        DuelRoomDto m_room;
        DuelRoomDto m_authority;
        DuelSeatDto m_display;
        DuelActionDto m_action;
        Core.SeatInput m_input;
        DuelSelectionView m_choice = DuelSelectionView.Empty;
        PendingActionView m_pending = PendingActionView.Empty;
        Stage m_stage;
        long m_sequence, m_choiceId;
        bool m_started, m_animating, m_sent, m_loadingNames;
        bool m_recoveringInput;
        public DuelView Current { get; private set; }
        public DuelSessionMode Mode => DuelSessionMode.Online;
        public string OperationHint
        {
            get
            {
                if (Current.Finished) return Current.Outcome;
                if (m_recoveringInput) return "正在恢复已提交的操作";
                if (m_authority.Paused) return "连接中断，等待双方恢复连接";
                if (m_animating) return m_display.Chain.Length > 0 ? "正在播放连锁与效果处理" : "正在播放对局表现";
                if (m_sent) return "等待服务器处理操作";
                var waiting = m_authority.Duel.WaitingSeat;
                if (waiting >= 0 && waiting != m_projection.Seat)
                    return m_authority.Duel.Window == Core.TimingWindow.ChainResponse ? "等待对手选择是否连锁"
                        : m_authority.Duel.Window == Core.TimingWindow.Decision ? "对手正在处理效果或选择卡牌" : "对手正在操作";
                if (m_choice.Active) return m_choice.Prompt;
                if (m_pending.InstanceId != 0)
                    return m_stage == Stage.Decision ? m_room.Duel.Decision.Prompt
                        : m_pending.NeedsPosition ? "请选择卡牌的表示形式" : "请选择合法的目标或放置区域";
                if (m_loadingNames) return "正在读取可宣言的卡名";
                return m_authority.Duel.Chain.Length > 0 ? "请选择连锁响应" : "请选择本方操作";
            }
        }
        public IEnumerable<DuelCardSpec> Definitions => m_projection.Definitions;
        public bool HasAttackPreview { get; private set; }
        public int AttackPreviewSource { get; private set; }
        public int AttackPreviewTarget { get; private set; }
        public int DeclaredAttacker { get; private set; }
        public int DeclaredTarget { get; private set; }
        public event Action<DuelViewChange> Changed = delegate { };
        public NetworkDuelSession(DuelRoomDto initialRoom, Action<Core.SeatInput> send,
            Func<int, Core.DuelZone, CancellationToken, UniTask<DuelPreviewDto>> preview,
            Func<string, string, int, CancellationToken, UniTask<DuelNamePageDto>> names)
        {
            m_projection = new DuelNetworkProjection(initialRoom.LocalSeat);
            m_authority = m_room = initialRoom; m_display = initialRoom.Duel; m_sequence = initialRoom.Sequence;
            m_send = send; m_preview = preview; m_names = names;
            SyncClock(initialRoom); Rebuild();
        }
        public void Dispose() { m_draft.Cancel(); m_draft.Dispose(); }
        public DuelCardSpec DefinitionForInstance(int id) => Current.Card(id).Definition;
        public void Start() { m_started = true; Advance(); }
        public void SetRecoveryPending(bool pending)
        {
            m_recoveringInput = pending; m_sent = pending; Rebuild();
            if (!pending && m_started && !m_animating) Settle();
        }
        void SyncClock(DuelRoomDto room)
        { for (var seat = 0; seat < 2; seat++) m_seconds[seat] = (float)room.RemainingSeconds[seat]; }
        void Rebuild()
        {
            Current = m_projection.View(m_display, m_room, m_animating, m_sent || m_loadingNames, m_seconds, m_choice, m_pending);
            if (!Current.Cards.Any(c => c.InstanceId == DeclaredAttacker)) DeclaredAttacker = DeclaredTarget = 0;
            else if (DeclaredTarget != 0 && !Current.Cards.Any(c => c.InstanceId == DeclaredTarget)) DeclaredTarget = 0;
        }
        void Notify(DuelChangeKind kind, string message = "") => Changed(new DuelViewChange(kind, Current, message: message));
        void Reject(string reason) => Notify(DuelChangeKind.Rejected, reason);
        public void Receive(DuelNoticeDto notice, bool resynchronizing = false)
        {
            if (notice.Result != null && !notice.Result.Accepted)
            {
                ClearAction(); m_sent = false;
                if (notice.Room.Duel != null) { m_room = notice.Room; m_display = m_room.Duel; SyncClock(m_room); Rebuild(); }
                if (m_started) { Settle(); Reject(notice.Result.Error); }
            }
            if (resynchronizing)
            {
                m_steps.Clear(); ClearAction(); m_sent = m_recoveringInput; m_animating = false;
                DeclaredAttacker = DeclaredTarget = 0;
                m_authority = m_room = notice.Room; m_display = m_room.Duel; m_sequence = m_room.Sequence; SyncClock(m_room); Rebuild();
                if (m_started) { Notify(DuelChangeKind.Reset); Settle(); }
                return;
            }
            if (notice.Room.Sequence <= m_sequence)
            {
                if (notice.Room.Sequence == m_sequence && notice.Room.Duel != null)
                { m_authority = notice.Room; SyncClock(notice.Room); if (m_started) UpdateClockView(); }
                return;
            }
            m_sequence = notice.Room.Sequence;
            if (notice.Room.Duel == null) return;
            m_authority = notice.Room;
            ClearAction(); m_sent = m_recoveringInput; SyncClock(notice.Room);
            var attack = false; int[] attackLife = null;
            foreach (var frame in notice.Frames)
            {
                var evt = frame.Event;
                if (!attack && evt.ImpactLifePoints.Length == 2 && evt.AttackerViewCardId.Length != 0 &&
                    (evt.Kind == Core.DuelEventKind.Damaged || evt.Kind == Core.DuelEventKind.BattleStepChanged && evt.Amount == (int)Core.BattleStep.AfterCalculation))
                {
                    var impact = notice.Frames.LastOrDefault(f => f.Event.AttackerViewCardId == evt.AttackerViewCardId &&
                        (f.Event.Kind == Core.DuelEventKind.Damaged || f.Event.Kind == Core.DuelEventKind.BattleStepChanged && f.Event.Amount == (int)Core.BattleStep.AfterCalculation));
                    attackLife = impact.Event.ImpactLifePoints;
                    m_steps.Enqueue(new Step { Room = notice.Room, Frame = frame, Attack = true, Impact = attackLife }); attack = true;
                }
                m_steps.Enqueue(new Step { Room = notice.Room, Frame = frame,
                    Impact = attack && evt.Kind == Core.DuelEventKind.Damaged && evt.AttackerViewCardId.Length != 0 ? attackLife : null });
            }
            m_steps.Enqueue(new Step { Room = notice.Room });
            if (m_started && !m_animating) Advance();
        }
        public void FinishPresentation() => Advance();
        void Advance()
        {
            if (m_steps.Count == 0)
            {
                m_animating = false; Rebuild(); Notify(DuelChangeKind.AnimationCompleted); Settle(); return;
            }
            var step = m_steps.Dequeue(); m_room = step.Room;
            if (step.Frame == null)
            {
                m_display = step.Room.Duel;
                if (m_steps.Count != 0) { Advance(); return; }
                m_animating = false; Rebuild(); Notify(DuelChangeKind.AnimationCompleted); Settle(); return;
            }
            var previous = Current; var beforeLife = previous.Players.Select(p => p.LP).ToArray();
            m_projection.Alias(step.Frame.Event); m_display = step.Frame.Snapshot; m_animating = true; Rebuild();
            var evt = step.Frame.Event;
            if (evt.Kind == Core.DuelEventKind.AttackDeclared)
            { DeclaredAttacker = m_projection.Handle(evt.AttackerViewCardId); DeclaredTarget = m_projection.Handle(evt.TargetViewCardId); }
            if (evt.Kind == Core.DuelEventKind.PhaseChanged || evt.Kind == Core.DuelEventKind.TurnChanged)
                DeclaredAttacker = DeclaredTarget = 0;
            if (step.Attack)
            {
                Current = WithLife(Current, beforeLife);
                Changed(new DuelViewChange(DuelChangeKind.Attack, Current, m_projection.Handle(evt.AttackerViewCardId))
                {
                    AttackerId = m_projection.Handle(evt.AttackerViewCardId), TargetId = m_projection.Handle(evt.TargetViewCardId),
                    ImpactLifePoints = new[] { step.Impact[m_projection.Seat], step.Impact[1 - m_projection.Seat] }
                });
            }
            else
            {
                if (step.Impact != null) Current = WithLife(Current, new[] { step.Impact[m_projection.Seat], step.Impact[1 - m_projection.Seat] });
                Changed(m_projection.Change(step.Frame, Current, previous));
            }
        }
        static DuelView WithLife(DuelView view, IReadOnlyList<int> life) => new(view.Cards, view.AvailablePiles, view.ActivePlayer,
            view.Turn, view.Phase, view.Animating, view.Placement, view.Seconds.ToArray(), view.TimerPaused,
            view.AvailablePhases, view.PendingAction, view.Players.Select((p, i) => new DuelPlayerView(p.Name, p.AvatarId, life[i], p.AvatarFrameId)))
        { ViewingSeat = view.ViewingSeat, ReadOnly = view.ReadOnly, Choice = view.Choice, Finished = view.Finished, Outcome = view.Outcome };
        public void PresentImpact(IReadOnlyList<int> life) { Current = WithLife(Current, life); Notify(DuelChangeKind.Timer); }
        public void Tick(float seconds)
        {
            var waiting = m_authority.Duel.WaitingSeat;
            if (m_authority.Paused || m_authority.Duel.Finished || waiting < 0) return;
            m_seconds[waiting] = Math.Max(0, m_seconds[waiting] - seconds);
            // 演出期间仍显示服务端行动时钟；超时由服务端处理。
            UpdateClockView();
        }
        void UpdateClockView()
        {
            var view = Current;
            Current = new DuelView(view.Cards, view.AvailablePiles, view.ActivePlayer, view.Turn, view.Phase, view.Animating,
                view.Placement, new[] { m_seconds[m_projection.Seat], m_seconds[1 - m_projection.Seat] }, m_authority.Paused,
                view.AvailablePhases, view.PendingAction, view.Players)
            { ViewingSeat = view.ViewingSeat, ReadOnly = view.ReadOnly, Choice = view.Choice, Finished = view.Finished, Outcome = view.Outcome };
            Notify(DuelChangeKind.Timer);
        }
        public async UniTask<ZonePreviewCard[]> PreviewZoneAsync(ZoneRef zone, CancellationToken token)
        {
            if (zone.Kind == DuelZone.MainDeck && zone.Player == 0)
                return m_display.MainDeckDefinitions.OrderBy(id => id, StringComparer.Ordinal).Select(id => new ZonePreviewCard(0, id, true)).ToArray();
            var result = await m_preview(m_projection.Actual(zone.Player), DuelNetworkProjection.RuleZone(zone.Kind), token);
            return result.Cards.Select(c => new ZonePreviewCard(m_projection.Handle(c.ViewCardId), c.DefinitionId, c.Known, c.Count)).ToArray();
        }
        public void Submit(DuelInputCommand command)
        {
            if (command is AnimationCompleted) { Advance(); return; }
            if (m_animating || m_sent || m_room.Duel.Finished) return;
            if (command is SurrenderDuel) { SendAction(Core.DuelCommandKind.Surrender); return; }
            if (command is CancelCardAction)
            {
                if (m_stage == Stage.Response && m_choice.Active) { Pass(); return; }
                if (m_stage == Stage.Decision && m_room.Duel.Decision != null)
                {
                    if (m_room.Duel.Decision.CanCancel && m_room.Duel.Decision.Min == 0)
                        Send(new Core.SeatInput { Revision = m_room.Duel.Revision });
                    return;
                }
                ClearAction(); Rebuild(); Notify(DuelChangeKind.CancelAction); Settle(); return;
            }
            if (command is PreviewDuelTarget preview)
            { AttackPreviewTarget = preview.CardId; HasAttackPreview = preview.CardId >= 0; Notify(DuelChangeKind.State); return; }
            if (command is ConfirmDuelSelection selection) { SubmitChoice(selection); return; }
            if (command is BeginCardAction begin) { Begin(begin.ActionId); return; }
            if (command is ConfirmActionTarget target)
            {
                if (m_stage == Stage.Decision)
                {
                    if (m_zoneAnswers.TryGetValue(target.Target, out var option))
                        Send(new Core.SeatInput { Revision = m_room.Duel.Revision, OptionTokens = new[] { option } });
                    else Reject("请选择标记的合法区域");
                    return;
                }
                if (!m_pending.Targets.Contains(target.Target)) { Reject("请选择标记的合法区域"); return; }
                m_input.Slot = target.Target.Kind == DuelZone.ExtraMonster ? 5 + (m_projection.Seat == 1 ? 1 - target.Target.Slot : target.Target.Slot) : target.Target.Slot;
                NextStage(); return;
            }
            if (command is ChooseActionPosition position)
            { m_input.Position = RulePosition(position.Position); NextStage(); return; }
            if (command is PassDuelResponse) { Pass(); return; }
            if (command is ChangePhase phase)
            { Begin(m_room.Duel.Actions.Single(a => a.Kind == Core.DuelCommandKind.AdvancePhase && DuelNetworkProjection.Phase(a.Phase) == phase.Phase).ActionToken); return; }
            if (command is EndTurn)
            {
                var end = m_room.Duel.Actions.FirstOrDefault(a => a.Kind == Core.DuelCommandKind.AdvancePhase && a.Phase == Core.DuelPhase.End);
                if (end == null) Reject("请先进入主要阶段2，再结束回合"); else Begin(end.ActionToken);
            }
        }
        static Core.CardPosition RulePosition(CardPosition position) => position switch
        {
            CardPosition.FaceDown => Core.CardPosition.FaceDown, CardPosition.FaceDownDefense => Core.CardPosition.FaceDownDefense,
            CardPosition.FaceUp => Core.CardPosition.FaceUp, CardPosition.FaceUpAttack => Core.CardPosition.FaceUpAttack,
            CardPosition.FaceUpDefense => Core.CardPosition.FaceUpDefense, _ => throw new ArgumentOutOfRangeException(nameof(position))
        };
        void ClearAction()
        {
            m_draft.Cancel(); m_draft.Dispose(); m_draft = new CancellationTokenSource();
            m_choice = DuelSelectionView.Empty; m_pending = PendingActionView.Empty; m_stages.Clear(); m_zoneAnswers.Clear(); HasAttackPreview = false;
            m_loadingNames = false;
        }
        void Begin(string token)
        {
            m_action = m_room.Duel.Actions.FirstOrDefault(a => a.ActionToken == token);
            if (m_action == null) { Reject("操作已变化，请重新选择"); return; }
            ClearAction();
            m_choice = DuelSelectionView.Empty; m_stages.Clear(); m_input = new Core.SeatInput { Revision = m_room.Duel.Revision, ActionToken = token };
            if (m_action.SelectionViewCardIds.Length > 0) m_stages.Enqueue(Stage.Selection);
            if (m_action.TargetViewCardIds.Length > 0 || m_action.CanAttackDirectly) m_stages.Enqueue(Stage.Target);
            if (m_action.Positions.Length > 1) m_stages.Enqueue(Stage.Position);
            else if (m_action.Positions.Length == 1) m_input.Position = m_action.Positions[0];
            if (m_action.RequiresNameDeclaration) m_stages.Enqueue(Stage.Name);
            if (m_action.Slots.Length > 0) m_stages.Enqueue(Stage.Zone);
            NextStage();
        }
        DuelSelectionOption CardOption(string key, string handle)
        {
            var card = m_display.Cards.Single(c => c.ViewCardId == handle);
            return new DuelSelectionOption(key, card.DefinitionId.Length == 0 ? "未知卡牌" : Core.DuelCardCatalog.CreateDefault().Get(card.DefinitionId).Name,
                card.DefinitionId, m_projection.Handle(handle));
        }
        void Choice(Stage stage, string prompt, int min, int max, bool cancel, IEnumerable<DuelSelectionOption> options,
            bool ordered = false, bool searchable = false, bool preview = false)
        {
            m_stage = stage; m_choice = new DuelSelectionView(++m_choiceId, prompt, min, max, cancel, options, ordered, searchable, preview)
                { IsResponse = stage == Stage.Response };
            Rebuild(); Notify(DuelChangeKind.Action);
        }
        void NextStage()
        {
            m_choice = DuelSelectionView.Empty; m_pending = PendingActionView.Empty;
            if (m_stages.Count == 0) { Send(m_input); return; }
            m_stage = m_stages.Dequeue();
            switch (m_stage)
            {
                case Stage.Selection:
                    Choice(m_stage, m_action.SelectionIsTarget ? "请选择效果对象" : "请选择费用、祭品或召唤素材", m_action.MinSelections,
                        m_action.MaxSelections, true, m_action.SelectionViewCardIds.Select(h => CardOption(h, h))); break;
                case Stage.Target:
                    var targets = m_action.TargetViewCardIds.Select(h => CardOption(h, h)).ToList();
                    if (m_action.CanAttackDirectly) targets.Add(new DuelSelectionOption("direct", "直接攻击"));
                    AttackPreviewSource = m_projection.Handle(m_action.SourceViewCardId);
                    Choice(m_stage, m_action.Kind == Core.DuelCommandKind.Attack ? "请选择攻击目标" : "请选择效果目标", 1, 1, true,
                        targets, preview: m_action.Kind == Core.DuelCommandKind.Attack); break;
                case Stage.Position:
                    Choice(m_stage, "请选择表示形式", 1, 1, true, m_action.Positions.Select(p => new DuelSelectionOption(p.ToString(), BattleLabels.Position(LocalDuelSession.MapPosition(p))))); break;
                case Stage.Name: LoadNamesAsync(m_action.ActionToken, m_draft.Token).Forget(); break;
                case Stage.Zone:
                    var definition = m_projection.Spec(m_display.Cards.Single(c => c.ViewCardId == m_action.SourceViewCardId).DefinitionId);
                    var zone = m_action.Kind is Core.DuelCommandKind.SetSpellTrap or Core.DuelCommandKind.Activate
                        ? definition.SpellType == CardSpellTrapType.Field ? DuelZone.Field : DuelZone.SpellTrap : DuelZone.Monster;
                    var zones = m_action.Slots.Select(slot => slot >= 5 ? new ZoneRef(DuelZone.ExtraMonster, -1, m_projection.Seat == 1 ? 6 - slot : slot - 5)
                        : new ZoneRef(zone, 0, slot));
                    m_pending = new PendingActionView(m_projection.Handle(m_action.SourceViewCardId), m_action.ActionToken,
                        DuelNetworkProjection.ActionKind(m_action.Kind), false, true, LocalDuelSession.MapPosition(m_input.Position), Array.Empty<CardPosition>(), zones);
                    Rebuild(); Notify(DuelChangeKind.Action); break;
            }
        }
        async UniTask LoadNamesAsync(string token, CancellationToken cancellation)
        {
            m_loadingNames = true; Rebuild(); Notify(DuelChangeKind.Action);
            try
            {
                var options = new List<DuelSelectionOption>(); var offset = 0;
                while (true)
                {
                    var page = await m_names(token, "", offset, cancellation); cancellation.ThrowIfCancellationRequested();
                    options.AddRange(page.Items.Select(n => new DuelSelectionOption(n.NameId, n.Chinese)));
                    offset += page.Items.Length; if (offset >= page.TotalCount) break;
                }
                m_loadingNames = false;
                Choice(Stage.Name, "请选择宣言的卡名", 1, 1, true, options, searchable: true);
            }
            catch (OperationCanceledException) { }
            catch (Exception e) { ClearAction(); Rebuild(); Reject(e.Message); Settle(); }
        }
        void SubmitChoice(ConfirmDuelSelection selection)
        {
            if (selection.ChoiceId != m_choice.Id) { Reject("选择已变化，请重新选择"); return; }
            if (selection.Keys.Length < m_choice.Min || selection.Keys.Length > m_choice.Max || selection.Keys.Distinct().Count() != selection.Keys.Length
                || selection.Keys.Any(key => !m_choice.Options.Any(o => o.Key == key))) { Reject("请选择规定数量的选项"); return; }
            switch (m_stage)
            {
                case Stage.Response:
                    var abilities = m_room.Duel.Actions.Where(a => a.Kind == Core.DuelCommandKind.Activate && a.SourceViewCardId == selection.Keys[0]).ToArray();
                    if (abilities.Length == 1) Begin(abilities[0].ActionToken);
                    else Choice(Stage.Ability, "请选择发动的效果", 1, 1, true, abilities.Select(a => new DuelSelectionOption(a.ActionToken, a.Label,
                        m_display.Cards.Single(c => c.ViewCardId == a.SourceViewCardId).DefinitionId, m_projection.Handle(a.SourceViewCardId))));
                    return;
                case Stage.Ability: Begin(selection.Keys[0]); return;
                case Stage.Selection: m_input.SelectionViewCardIds = selection.Keys; break;
                case Stage.Target: m_input.TargetViewCardId = selection.Keys[0] == "direct" ? "" : selection.Keys[0]; break;
                case Stage.Position: m_input.Position = (Core.CardPosition)Enum.Parse(typeof(Core.CardPosition), selection.Keys[0]); break;
                case Stage.Name: m_input.NameId = selection.Keys[0]; break;
                case Stage.Decision:
                    var answer = new Core.SeatInput { Revision = m_room.Duel.Revision, OptionTokens = selection.Keys };
                    if (m_room.Duel.Decision.Kind == Core.DecisionKind.DeclareName) { answer.NameId = selection.Keys[0]; answer.OptionTokens = Array.Empty<string>(); }
                    Send(answer); return;
            }
            NextStage();
        }
        void Send(Core.SeatInput input)
        {
            ClearAction(); m_sent = true; Rebuild(); Notify(DuelChangeKind.State); m_send(input);
        }
        void SendAction(Core.DuelCommandKind kind)
        {
            var action = m_room.Duel.Actions.FirstOrDefault(a => a.Kind == kind);
            if (action == null) { Reject("当前不能执行该操作"); return; }
            Send(new Core.SeatInput { Revision = m_room.Duel.Revision, ActionToken = action.ActionToken });
        }
        void Pass() => SendAction(Core.DuelCommandKind.Pass);
        void Settle()
        {
            if (m_animating || m_sent || m_recoveringInput || m_room.Duel.Finished || m_room.Duel.WaitingSeat != m_projection.Seat) { Rebuild(); Notify(DuelChangeKind.State); return; }
            var decision = m_room.Duel.Decision;
            if (decision != null)
            {
                if (decision.Kind == Core.DecisionKind.ChooseZone)
                {
                    m_zoneAnswers.Clear(); m_stage = Stage.Decision;
                    foreach (var option in decision.Options)
                    {
                        var zone = option.Slot >= 5 ? new ZoneRef(DuelZone.ExtraMonster, -1,
                            m_projection.Seat == 1 ? 6 - option.Slot : option.Slot - 5)
                            : new ZoneRef(LocalDuelSession.MapZone(option.DestinationZone), 0, option.Slot);
                        m_zoneAnswers.Add(zone, option.OptionToken);
                    }
                    // 区域选择没有来源卡实体，-1仅用于使现有HUD显示区域选择提示。
                    m_pending = new PendingActionView(-1, "decision", DuelActionKind.SpecialSummon, false, true,
                        CardPosition.FaceUpAttack, Array.Empty<CardPosition>(), m_zoneAnswers.Keys);
                    Rebuild(); Notify(DuelChangeKind.Action); return;
                }
                var options = decision.Options.Select(o => new DuelSelectionOption(o.OptionToken, o.Label, o.DefinitionId, m_projection.Handle(o.ViewCardId)));
                if (decision.Kind == Core.DecisionKind.DeclareName)
                    options = Core.CardNameCatalog.CreateDefault().Names.Select(n => new DuelSelectionOption(n.NameId, n.Chinese));
                Choice(Stage.Decision, decision.Prompt, decision.Kind == Core.DecisionKind.DeclareName ? 1 : decision.Min,
                    decision.Kind == Core.DecisionKind.DeclareName ? 1 : decision.Max, decision.CanCancel && decision.Min == 0,
                    options, ordered: decision.Kind == Core.DecisionKind.OrderCards, searchable: decision.Kind == Core.DecisionKind.DeclareName); return;
            }
            if (m_room.Duel.Window is Core.TimingWindow.FastResponse or Core.TimingWindow.ChainResponse)
            {
                var responses = m_room.Duel.Actions.Where(a => a.Kind == Core.DuelCommandKind.Activate).ToArray();
                if (responses.Length == 0) { Pass(); return; }
                var chain = m_room.Duel.Chain;
                var prompt = chain.Length == 0 ? "当前时点，是否发动效果？" : "「" + Core.DuelCardCatalog.CreateDefault().Get(chain.Last().DefinitionId).Name + "」发动时，是否连锁？";
                Choice(Stage.Response, prompt, 1, 1, true, responses.Select(a => a.SourceViewCardId).Distinct().Select(h => CardOption(h, h))); return;
            }
            Rebuild(); Notify(DuelChangeKind.State);
        }
    }
}
