using System;
using System.Collections.Generic;
using System.Linq;
using Core = AChen.Duel.Core;

namespace AChen.Duel.Presentation
{
    /// <summary>离线规则宿主。视图只提交合法动作标识，所有业务写入由引擎执行。</summary>
    public sealed class LocalDuelSession : IDuelPresentationSource
    {
        enum Stage { Response, Ability, Selection, Target, Position, Name, Zone, Decision }
        readonly Core.DuelCardCatalog m_catalog = Core.DuelCardCatalog.CreateDefault();
        readonly Func<Core.DuelStartRecord> m_start;
        readonly DuelPlayerView[] m_players;
        readonly Dictionary<string, DuelCardSpec> m_specs;
        readonly Queue<DuelViewChange> m_presentations = new Queue<DuelViewChange>();
        readonly float[] m_seconds = { 180, 180 };
        Core.DuelEngine m_engine;
        Core.SeatProjection[] m_projections;
        Core.DuelSeatSnapshot m_snapshot;
        Dictionary<string, Core.DuelAction> m_actions;
        Dictionary<string, int> m_handles;
        Core.ProjectedAction m_action;
        Core.SeatInput m_input;
        readonly Queue<Stage> m_stages = new Queue<Stage>();
        Stage m_stage;
        DuelSelectionView m_choice = DuelSelectionView.Empty;
        PendingActionView m_pending = PendingActionView.Empty;
        long m_choiceId;
        int m_viewer;
        bool m_animating;
        readonly Dictionary<ZoneRef, string> m_zoneAnswers = new Dictionary<ZoneRef, string>();
        public DuelView Current { get; private set; }
        public event Action<DuelViewChange> Changed = delegate { };
        public IEnumerable<DuelCardSpec> Definitions => m_specs.Values;
        public int AttackPreviewSource { get; private set; }
        public int AttackPreviewTarget { get; private set; }
        public bool HasAttackPreview { get; private set; }
        public int DeclaredAttacker => m_engine.State.Attacker.InstanceId;
        public int DeclaredTarget => m_engine.State.AttackTarget.InstanceId;
        public LocalDuelSession(Func<Core.DuelStartRecord> start, IEnumerable<DuelCardSpec> specs, IEnumerable<DuelPlayerView> players)
        {
            m_start = start; m_specs = specs.GroupBy(x => x.CardId).ToDictionary(g => g.Key, g => g.First());
            m_players = players.ToArray(); Initialize();
        }
        void Initialize()
        {
            var record = Core.DuelRulePackage.CreateDefault(m_catalog).Freeze(m_start());
            var missing = Core.CardRuleCatalog.CreateDefault(m_catalog).CheckDeckSupport(record.MainDecks.SelectMany(x => x)
                .Concat(record.ExtraDecks.SelectMany(x => x)));
            if (missing.Count != 0) throw new InvalidOperationException("卡组包含尚未支持的规则");
            m_engine = new Core.DuelEngine(m_catalog, record);
            m_projections = new[] { new Core.SeatProjection(0), new Core.SeatProjection(1) };
            m_viewer = record.FirstPlayer; m_seconds[0] = m_seconds[1] = 180;
            m_presentations.Clear(); m_stages.Clear(); m_choice = DuelSelectionView.Empty;
            m_pending = PendingActionView.Empty; m_animating = false; HasAttackPreview = false;
            Refresh();
        }
        int DisplaySeat(int seat) => seat == m_viewer ? 0 : 1;
        public DuelCardSpec DefinitionForInstance(int id) => m_specs[m_engine.State.Cards.Single(c => c.InstanceId == id).DefinitionId];
        public static DuelZone MapZone(Core.DuelZone zone) => zone switch
        {
            Core.DuelZone.Deck => DuelZone.MainDeck, Core.DuelZone.ExtraDeck => DuelZone.ExtraDeck,
            Core.DuelZone.Hand => DuelZone.Hand, Core.DuelZone.Monster => DuelZone.Monster,
            Core.DuelZone.SpellTrap => DuelZone.SpellTrap, Core.DuelZone.Field => DuelZone.Field,
            Core.DuelZone.Graveyard => DuelZone.Graveyard, Core.DuelZone.Banished => DuelZone.Banished,
            Core.DuelZone.ExtraMonster => DuelZone.ExtraMonster, Core.DuelZone.Material => DuelZone.Material,
            _ => throw new ArgumentOutOfRangeException(nameof(zone))
        };
        public static CardPosition MapPosition(Core.CardPosition position) => position switch
        {
            Core.CardPosition.FaceDown => CardPosition.FaceDown, Core.CardPosition.FaceUp => CardPosition.FaceUp,
            Core.CardPosition.FaceUpAttack => CardPosition.FaceUpAttack, Core.CardPosition.FaceUpDefense => CardPosition.FaceUpDefense,
            Core.CardPosition.FaceDownDefense => CardPosition.FaceDownDefense, _ => throw new ArgumentOutOfRangeException(nameof(position))
        };
        static Core.CardPosition RulePosition(CardPosition position) => position switch
        {
            CardPosition.FaceDown => Core.CardPosition.FaceDown, CardPosition.FaceUp => Core.CardPosition.FaceUp,
            CardPosition.FaceUpAttack => Core.CardPosition.FaceUpAttack, CardPosition.FaceUpDefense => Core.CardPosition.FaceUpDefense,
            CardPosition.FaceDownDefense => Core.CardPosition.FaceDownDefense, _ => throw new ArgumentOutOfRangeException(nameof(position))
        };
        ZoneRef Zone(Core.CardLastKnown card) => new ZoneRef(MapZone(card.Zone), DisplaySeat(card.Controller),
            card.Zone == Core.DuelZone.ExtraMonster && m_viewer == 1 ? 1 - card.Slot : card.Slot);
        static DuelActionKind ActionKind(Core.DuelCommandKind kind) => kind switch
        {
            Core.DuelCommandKind.NormalSummon => DuelActionKind.NormalSummon, Core.DuelCommandKind.SetMonster => DuelActionKind.SetMonster,
            Core.DuelCommandKind.SpecialSummon => DuelActionKind.SpecialSummon, Core.DuelCommandKind.Activate => DuelActionKind.Activate,
            Core.DuelCommandKind.SetSpellTrap => DuelActionKind.SetSpellTrap, Core.DuelCommandKind.ChangePosition => DuelActionKind.ChangePosition,
            Core.DuelCommandKind.Attack => DuelActionKind.Attack, _ => throw new ArgumentOutOfRangeException(nameof(kind))
        };
        static DuelPhase Phase(Core.DuelPhase phase) => phase switch
        {
            Core.DuelPhase.Draw => DuelPhase.Draw, Core.DuelPhase.Standby => DuelPhase.Standby,
            Core.DuelPhase.Main1 => DuelPhase.Main1, Core.DuelPhase.Battle => DuelPhase.Battle,
            Core.DuelPhase.Main2 => DuelPhase.Main2, Core.DuelPhase.End => DuelPhase.End,
            _ => throw new ArgumentOutOfRangeException(nameof(phase))
        };
        Core.CardLastKnown Fact(Core.DuelCardState c) => new Core.CardLastKnown
        {
            Ref = c.Ref, DefinitionId = c.DefinitionId, Owner = c.Owner, Controller = c.Controller, Zone = c.Zone,
            Slot = c.Slot, Position = c.Position, Attack = c.CurrentAtk, Defense = c.CurrentDef, Level = c.CurrentLevel,
            Negated = c.Negated, HostInstanceId = c.HostInstanceId, MaterialCount = c.Materials.Count,
            VisibleToMask = Core.DuelEngine.IsPublic(c) ? 3 : (1 << c.Controller) | c.RevealedToMask
        };
        DuelView View(IEnumerable<Core.CardLastKnown> facts, int[] life, Core.DuelPhase phase, int turn)
        {
            var cards = facts.Select(c =>
            {
                bool known = (c.VisibleToMask & (1 << m_viewer)) != 0 && c.Zone != Core.DuelZone.Deck;
                var actions = m_animating ? Array.Empty<DuelActionView>() : m_snapshot.Actions
                    .Where(a => a.SourceViewCardId.Length != 0 && m_handles[a.SourceViewCardId] == c.Ref.InstanceId)
                    .Select(a => new DuelActionView(a.ActionToken, ActionKind(a.Kind), a.Slots.Count != 0,
                        a.Positions.Select(p => new DuelActionPositionView(MapPosition(p), Array.Empty<ZoneRef>())))
                    { Label = a.Label }).ToArray();
                var spec = known ? m_specs[c.DefinitionId] : new DuelCardSpec("", "", CardKind.Monster, CardFrame.Normal,
                    CardSpellTrapType.None, CardFlags.None, Array.Empty<DuelActionProfile>());
                return new CardView(c.Ref.InstanceId, DisplaySeat(c.Controller), spec, Zone(c), MapPosition(c.Position),
                    actions.Any(a => a.Kind == DuelActionKind.Activate), Array.Empty<ZoneRef>(),
                    actions.Where(a => a.Kind == DuelActionKind.ChangePosition).SelectMany(a => a.Positions), actions)
                { Known = known, Negated = known && c.Negated, Attack = known ? c.Attack : 0, Defense = known ? c.Defense : null,
                  Level = known ? m_catalog.Get(c.DefinitionId).MonsterType == Core.RuleMonsterType.Link ? m_catalog.Get(c.DefinitionId).LinkRating
                    : m_catalog.Get(c.DefinitionId).MonsterType == Core.RuleMonsterType.Xyz ? m_catalog.Get(c.DefinitionId).Rank : c.Level : 0, HostInstanceId = c.HostInstanceId, MaterialCount = c.MaterialCount, Generation = c.Ref.Generation };
            }).ToArray();
            var players = Enumerable.Range(0, 2).Select(display =>
            {
                int seat = display == 0 ? m_viewer : 1 - m_viewer;
                return new DuelPlayerView(m_players[seat].Name, m_players[seat].AvatarId, life[seat]);
            });
            return new DuelView(cards, cards.Where(c => c.EffectAvailable && !c.Zone.IsSlot).Select(c => c.Zone).Distinct(),
                DisplaySeat(m_engine.State.TurnPlayer), turn, Phase(phase), m_animating, PlacementView.Empty,
                new[] { m_seconds[m_viewer], m_seconds[1 - m_viewer] }, m_animating, m_snapshot.Actions
                    .Where(a => a.Kind == Core.DuelCommandKind.AdvancePhase).Select(a => Phase(a.Phase)), m_pending, players)
            { Choice = m_animating ? DuelSelectionView.Empty : m_choice, ViewingSeat = m_viewer, Finished = !m_animating && m_engine.State.Finished,
                Outcome = m_engine.State.Finished ? (m_engine.State.Winner < 0 ? "对局结束" : m_players[m_engine.State.Winner].Name + " 获胜")
                    + " · " + EndReason(m_engine.State.EndReason) : "" };
        }
        static string EndReason(string reason) => reason switch
        { "TIMEOUT" => "行动超时", "SURRENDER" => "投降", "DRAW_EMPTY_DECK" => "牌库耗尽", "LIFE_POINTS_ZERO" => "生命值归零", _ => "对局结束" };
        void Project()
        {
            var raw = m_engine.QueryLegalActions(m_viewer);
            m_snapshot = m_projections[m_viewer].Project(m_engine.State, raw);
            m_actions = m_snapshot.Actions.ToDictionary(a => a.ActionToken, a => raw.Single(r => r.Id == ActionId(a, raw)));
            m_handles = m_engine.State.Cards.ToDictionary(c => m_projections[m_viewer].CardHandle(c), c => c.InstanceId);
        }
        string ActionId(Core.ProjectedAction action, IReadOnlyList<Core.DuelAction> raw)
            => raw[m_snapshot.Actions.ToList().IndexOf(action)].Id;
        void Refresh()
        {
            if (!m_engine.State.Finished && !m_choice.Active && m_engine.State.WaitingSeat >= 0
                && (m_engine.State.Window == Core.TimingWindow.Open || m_engine.State.Window == Core.TimingWindow.Decision
                    || m_engine.QueryLegalActions(m_engine.State.WaitingSeat).Any(a=>a.Kind==Core.DuelCommandKind.Activate)))
                m_viewer = m_engine.State.WaitingSeat;
            Project();
            Current = View(m_engine.State.Cards.Select(Fact), m_engine.State.Players.Select(p => p.LifePoints).ToArray(), m_engine.State.Phase, m_engine.State.Turn);
        }
        void Notify(DuelChangeKind kind, int id = 0, string message = "") => Changed(new DuelViewChange(kind, Current, id, message));
        void Reject(string reason) => Notify(DuelChangeKind.Rejected, message: reason);
        public void Start() => Settle();
        public void Tick(float seconds)
        {
            if (m_animating || m_engine.State.Finished || m_engine.State.WaitingSeat < 0) return;
            m_seconds[m_engine.State.WaitingSeat] = Math.Max(0, m_seconds[m_engine.State.WaitingSeat] - seconds);
            if (m_seconds[m_engine.State.WaitingSeat] == 0)
            { Execute(new Core.DuelCommand { Kind = Core.DuelCommandKind.Timeout, Player = m_engine.State.WaitingSeat }); return; }
            var view = Current;
            Current = new DuelView(view.Cards,view.AvailablePiles,view.ActivePlayer,view.Turn,view.Phase,false,view.Placement,
                new[]{m_seconds[m_viewer],m_seconds[1-m_viewer]},false,view.AvailablePhases,view.PendingAction,view.Players)
            { Choice=view.Choice,ViewingSeat=view.ViewingSeat,Finished=view.Finished,Outcome=view.Outcome };
            Notify(DuelChangeKind.Timer);
        }
        public void Submit(DuelInputCommand command)
        {
            if (command is ResetDuel) { Initialize(); Notify(DuelChangeKind.Reset); Settle(); return; }
            if (command is AnimationCompleted) { AdvancePresentation(); return; }
            if (m_animating || m_engine.State.Finished) return;
            if (command is CancelCardAction)
            {
                if (m_pending.InstanceId != 0 && m_stage != Stage.Decision)
                { ClearAction(); Refresh(); Notify(DuelChangeKind.CancelAction); Settle(); return; }
                if (!m_choice.Active || !m_choice.CanCancel) return;
                if (m_stage == Stage.Response) { Pass(); return; }
                if (m_stage == Stage.Decision)
                { SubmitChoice(new ConfirmDuelSelection(m_choice.Id, Array.Empty<string>())); return; }
                ClearAction(); Refresh(); Notify(DuelChangeKind.CancelAction); Settle(); return;
            }
            if (command is PreviewDuelTarget preview)
            { AttackPreviewTarget = preview.CardId; HasAttackPreview = preview.CardId >= 0; Notify(DuelChangeKind.State); return; }
            if (command is ConfirmDuelSelection selection) { SubmitChoice(selection); return; }
            if (command is BeginCardAction begin) { Begin(begin.ActionId); return; }
            if (command is ConfirmActionTarget target) { SelectZone(target.Target); return; }
            if (command is ChooseActionPosition position)
            { m_input.Position = RulePosition(position.Position); NextStage(); return; }
            if (command is PassDuelResponse) { Pass(); return; }
            if (command is SurrenderDuel)
            { Execute(new Core.DuelCommand { Kind = Core.DuelCommandKind.Surrender, Player = m_viewer }); return; }
            if (command is ChangePhase change)
            { var action = m_snapshot.Actions.Single(a => a.Kind == Core.DuelCommandKind.AdvancePhase && Phase(a.Phase) == change.Phase); Begin(action.ActionToken); return; }
            if (command is EndTurn)
            { if(Current.AvailablePhases.Contains(DuelPhase.End)) Begin(m_snapshot.Actions.Single(a=>a.Kind==Core.DuelCommandKind.AdvancePhase && a.Phase==Core.DuelPhase.End).ActionToken);
              else Reject("请先进入主要阶段2，再结束回合"); }
        }
        void ClearAction()
        { m_choice = DuelSelectionView.Empty; m_pending = PendingActionView.Empty; m_stages.Clear(); HasAttackPreview = false; }
        void Begin(string token)
        {
            if (!m_actions.ContainsKey(token)) { Reject("操作已变化，请重新选择"); return; }
            m_action = m_snapshot.Actions.Single(a => a.ActionToken == token);
            m_input = new Core.SeatInput { Revision = m_engine.State.Revision, ActionToken = token };
            m_choice = DuelSelectionView.Empty; m_stages.Clear();
            if (m_action.SelectionViewCardIds.Count > 0) m_stages.Enqueue(Stage.Selection);
            if (m_action.TargetViewCardIds.Count > 0 || m_action.CanAttackDirectly) m_stages.Enqueue(Stage.Target);
            if (m_action.Positions.Count > 1) m_stages.Enqueue(Stage.Position);
            else if (m_action.Positions.Count == 1) m_input.Position = m_action.Positions[0];
            if (m_action.RequiresNameDeclaration) m_stages.Enqueue(Stage.Name);
            if (m_action.Slots.Count > 0) m_stages.Enqueue(Stage.Zone);
            NextStage();
        }
        DuelSelectionOption CardOption(string key, int id)
        {
            var card = m_engine.State.Cards.Single(c => c.InstanceId == id);
            bool known = card.Controller == m_viewer || Core.DuelEngine.IsPublic(card) || (card.RevealedToMask & (1 << m_viewer)) != 0;
            return new DuelSelectionOption(key, known ? m_catalog.Get(card.DefinitionId).Name : "未知卡牌", known ? card.DefinitionId : "", id);
        }
        void Choice(Stage stage, string prompt, int min, int max, bool cancel, IEnumerable<DuelSelectionOption> options,
            bool ordered = false, bool search = false, bool preview = false)
        {
            m_stage = stage; m_choice = new DuelSelectionView(++m_choiceId, prompt, min, max, cancel, options, ordered, search, preview);
            Refresh(); Notify(DuelChangeKind.Action);
        }
        void NextStage()
        {
            m_choice = DuelSelectionView.Empty; m_pending = PendingActionView.Empty;
            if (m_stages.Count == 0) { ResolveInput(); return; }
            var stage = m_stages.Dequeue();
            switch (stage)
            {
                case Stage.Selection:
                    Choice(stage, m_action.SelectionIsTarget ? "请选择效果对象" : "请选择费用、祭品或召唤素材", m_action.MinSelections,
                        m_action.MaxSelections, true, m_action.SelectionViewCardIds.Select(h => CardOption(h, m_handles[h]))); break;
                case Stage.Target:
                    var options = m_action.TargetViewCardIds.Select(h => CardOption(h, m_handles[h])).ToList();
                    if (m_action.CanAttackDirectly) options.Add(new DuelSelectionOption("direct", "直接攻击"));
                    AttackPreviewSource = m_actions[m_action.ActionToken].Card.InstanceId;
                    Choice(stage, m_action.Kind == Core.DuelCommandKind.Attack ? "请选择攻击目标" : "请选择效果目标", 1, 1, true,
                        options, preview: m_action.Kind == Core.DuelCommandKind.Attack); break;
                case Stage.Position:
                    Choice(stage, "请选择表示形式", 1, 1, true, m_action.Positions.Select(p => new DuelSelectionOption(p.ToString(), BattleLabels.Position(MapPosition(p))))); break;
                case Stage.Name:
                    var rawNameAction = m_actions[m_action.ActionToken];
                    var declaration = (Core.IActivationNameDeclaration)m_engine.Abilities.Get(rawNameAction.AbilityId);
                    var candidates = new HashSet<string>(declaration.NameCandidates(new Core.EffectContext(m_engine,m_viewer,rawNameAction.Card.InstanceId)),StringComparer.Ordinal);
                    Choice(stage, "请选择宣言的卡名", 1, 1, true, Core.CardNameCatalog.CreateDefault().Names.Where(n=>candidates.Contains(n.NameId))
                        .Select(n => new DuelSelectionOption(n.NameId, n.Chinese)), search: true); break;
                case Stage.Zone:
                    var raw = m_actions[m_action.ActionToken]; var source = m_engine.State.Cards.Single(c => c.InstanceId == raw.Card.InstanceId);
                    var definition = m_catalog.Get(source.DefinitionId);
                    var zone = raw.Kind == Core.DuelCommandKind.SetSpellTrap || raw.Kind == Core.DuelCommandKind.Activate
                        ? definition.SpellTrapType == Core.RuleSpellTrapType.Field ? DuelZone.Field : DuelZone.SpellTrap : DuelZone.Monster;
                    var zones = raw.Slots.Select(slot => slot >= 5 ? new ZoneRef(DuelZone.ExtraMonster, -1,
                        m_viewer == 1 ? 6 - slot : slot - 5) : new ZoneRef(zone, 0, slot)).ToArray();
                    m_stage = stage;
                    m_pending = new PendingActionView(source.InstanceId, m_action.ActionToken, ActionKind(raw.Kind), false, true,
                        MapPosition(m_input.Position), Array.Empty<CardPosition>(), zones);
                    Refresh(); Notify(DuelChangeKind.Action); break;
            }
        }
        void SelectZone(ZoneRef zone)
        {
            if (m_stage == Stage.Decision)
            { if (m_zoneAnswers.TryGetValue(zone, out string token)) Resolve(new Core.SeatInput { Revision = m_engine.State.Revision, OptionTokens = new[] { token } });
              else Reject("请选择标记的合法区域"); return; }
            if (!m_pending.Targets.Contains(zone)) { Reject("请选择标记的合法区域"); return; }
            m_input.Slot = zone.Kind == DuelZone.ExtraMonster ? 5 + (m_viewer == 1 ? 1 - zone.Slot : zone.Slot) : zone.Slot;
            NextStage();
        }
        void SubmitChoice(ConfirmDuelSelection selection)
        {
            if (selection.ChoiceId != m_choice.Id) { Reject("选择已变化，请重新选择"); return; }
            if (selection.Keys.Length < m_choice.Min || selection.Keys.Length > m_choice.Max
                || selection.Keys.Distinct().Count() != selection.Keys.Length || selection.Keys.Any(k => !m_choice.Options.Any(o => o.Key == k)))
            { Reject("请选择规定数量的选项"); return; }
            switch (m_stage)
            {
                case Stage.Response:
                    int id = int.Parse(selection.Keys[0]); var abilities = m_snapshot.Actions.Where(a => a.Kind == Core.DuelCommandKind.Activate
                        && m_handles[a.SourceViewCardId] == id).ToArray();
                    if (abilities.Length == 1) Begin(abilities[0].ActionToken);
                    else Choice(Stage.Ability, "请选择发动的效果", 1, 1, true, abilities.Select((a, index) => new DuelSelectionOption(a.ActionToken,
                        a.Label.Length > 0 ? a.Label : "效果 " + (index + 1), DefinitionForInstance(id).CardId, id)));
                    return;
                case Stage.Ability: Begin(selection.Keys[0]); return;
                case Stage.Selection: m_input.SelectionViewCardIds = selection.Keys; break;
                case Stage.Target: m_input.TargetViewCardId = selection.Keys[0] == "direct" ? "" : selection.Keys[0]; break;
                case Stage.Position: m_input.Position = (Core.CardPosition)Enum.Parse(typeof(Core.CardPosition), selection.Keys[0]); break;
                case Stage.Name: m_input.NameId = selection.Keys[0]; break;
                case Stage.Decision:
                    var answer = new Core.SeatInput { Revision = m_engine.State.Revision, OptionTokens = selection.Keys };
                    if (m_snapshot.Decision.Kind == Core.DecisionKind.DeclareName)
                    { answer.NameId = selection.Keys[0]; answer.OptionTokens = Array.Empty<string>(); }
                    Resolve(answer); return;
            }
            NextStage();
        }
        void ResolveInput() => Resolve(m_input);
        void Resolve(Core.SeatInput input)
        {
            if (!m_projections[m_viewer].TryResolveInput(m_engine.State, input, out var command))
            { ClearAction(); Refresh(); Reject("操作已变化，请重新选择"); Settle(); return; }
            Execute(command);
        }
        void Pass() => Execute(new Core.DuelCommand { Kind = Core.DuelCommandKind.Pass, Player = m_engine.State.WaitingSeat });
        void Execute(Core.DuelCommand command)
        {
            int turn = m_engine.State.Turn;
            var before = Current; var result = m_engine.Apply(command);
            if (!result.Accepted) { ClearAction(); Refresh(); Settle(); Reject(result.Error); return; }
            ClearAction();
            if (m_engine.State.Turn != turn) m_seconds[0] = m_seconds[1] = 180;
            m_animating = true;
            var life = new[] { before.Players[DisplaySeat(0)].LP, before.Players[DisplaySeat(1)].LP };
            bool attacked = false;
            foreach (var fact in result.Events)
            {
                var kind = fact.Kind switch
                {
                    Core.DuelEventKind.Moved => DuelChangeKind.Move, Core.DuelEventKind.Revealed => DuelChangeKind.Position,
                    Core.DuelEventKind.PositionChanged => DuelChangeKind.Position, Core.DuelEventKind.Activated => DuelChangeKind.Effect,
                    Core.DuelEventKind.Resolved => DuelChangeKind.ChainResolved, Core.DuelEventKind.Negated => DuelChangeKind.ChainResolved,
                    Core.DuelEventKind.PhaseChanged => DuelChangeKind.Phase, Core.DuelEventKind.TurnChanged => DuelChangeKind.Turn,
                    _ => DuelChangeKind.State
                };
                bool impact = !attacked && fact.BattleAttacker.InstanceId != 0 &&
                    (fact.Kind == Core.DuelEventKind.Damaged && fact.Cause == Core.MoveCause.Battle
                    || fact.Kind == Core.DuelEventKind.BattleStepChanged && fact.Amount == (int)Core.BattleStep.AfterCalculation);
                if (impact)
                {
                    var impactLife = (int[])life.Clone();
                    foreach(var damage in result.Events.Where(e => e.Kind == Core.DuelEventKind.Damaged && e.Cause == Core.MoveCause.Battle))
                        impactLife[damage.Player] = Math.Max(0,impactLife[damage.Player]-damage.Amount);
                    m_presentations.Enqueue(new DuelViewChange(DuelChangeKind.Attack, View(fact.PresentCards, life, fact.PhaseAtEvent, fact.TurnAtEvent),
                        fact.BattleAttacker.InstanceId) { AttackerId = fact.BattleAttacker.InstanceId, TargetId = fact.BattleTarget.InstanceId,
                            ImpactLifePoints = new[] { impactLife[m_viewer], impactLife[1-m_viewer] } });
                    attacked = true;
                }
                if (fact.Kind == Core.DuelEventKind.Damaged) life[fact.Player] = Math.Max(0,life[fact.Player]-fact.Amount);
                if (fact.Kind == Core.DuelEventKind.Recovered) life[fact.Player] += fact.Amount;
                var view = View(fact.PresentCards, life, fact.PhaseAtEvent, fact.TurnAtEvent);
                m_presentations.Enqueue(new DuelViewChange(kind, view, fact.HasCard ? fact.Card.InstanceId : 0)
                { ChainId = fact.ChainId, LinkNumber = fact.LinkNumber,
                    AttackerId = fact.BattleAttacker.InstanceId, TargetId = fact.BattleTarget.InstanceId,
                    Origin = fact.HasCard ? Zone(fact.After) : default, IsNegated = fact.Kind == Core.DuelEventKind.Negated });
            }
            AdvancePresentation();
        }
        public void PresentImpact(IReadOnlyList<int> life)
        {
            var view = Current;
            Current = new DuelView(view.Cards, view.AvailablePiles, view.ActivePlayer, view.Turn, view.Phase, true, view.Placement,
                view.Seconds.ToArray(), true, view.AvailablePhases, view.PendingAction,
                view.Players.Select((p,i) => new DuelPlayerView(p.Name, p.AvatarId, life[i]))) { ViewingSeat = view.ViewingSeat };
            Notify(DuelChangeKind.Timer);
        }
        void AdvancePresentation()
        {
            if (m_presentations.Count > 0)
            { var change = m_presentations.Dequeue(); Current = change.View; Changed(change); return; }
            m_animating = false; Refresh(); Notify(DuelChangeKind.AnimationCompleted); Settle();
        }
        void Settle()
        {
            if (m_animating || m_engine.State.Finished) return;
            Refresh();
            if (m_engine.State.Window == Core.TimingWindow.Decision)
            {
                var decision = m_snapshot.Decision;
                if (decision.Kind == Core.DecisionKind.ChooseZone)
                {
                    m_zoneAnswers.Clear(); m_stage = Stage.Decision;
                    for (int i = 0; i < decision.Options.Count; i++)
                    {
                        var raw = m_engine.State.PendingDecision.Options.Single(o => o.Label == decision.Options[i].Label);
                        int slot = int.Parse(raw.Value.Length > 0 ? raw.Value : raw.Id);
                        var zone = raw.DestinationZone == Core.DuelZone.Field ? new ZoneRef(DuelZone.Field, 0)
                            : raw.DestinationZone == Core.DuelZone.SpellTrap ? new ZoneRef(DuelZone.SpellTrap, 0, slot)
                            : slot >= 5 ? new ZoneRef(DuelZone.ExtraMonster, -1, m_viewer == 1 ? 6 - slot : slot - 5)
                            : new ZoneRef(DuelZone.Monster, 0, slot);
                        m_zoneAnswers.Add(zone, decision.Options[i].OptionToken);
                    }
                    m_pending = new PendingActionView(m_engine.State.PendingDecision.SourceId, "decision", DuelActionKind.SpecialSummon,
                        false, true, CardPosition.FaceUpAttack, Array.Empty<CardPosition>(), m_zoneAnswers.Keys);
                    Refresh(); Notify(DuelChangeKind.Action); return;
                }
                var options = decision.Options.Select(o => new DuelSelectionOption(o.OptionToken, o.Label, o.DefinitionId, o.CardReference.InstanceId));
                if (decision.Kind == Core.DecisionKind.DeclareName)
                    options = Core.CardNameCatalog.CreateDefault().Names.Select(n => new DuelSelectionOption(n.NameId, n.Chinese));
                Choice(Stage.Decision, decision.Prompt, decision.Kind == Core.DecisionKind.DeclareName ? 1 : decision.Min,
                    decision.Kind == Core.DecisionKind.DeclareName ? 1 : decision.Max, decision.CanCancel && decision.Min == 0,
                    options, decision.Kind == Core.DecisionKind.OrderCards, decision.Kind == Core.DecisionKind.DeclareName); return;
            }
            if (m_engine.State.Window == Core.TimingWindow.FastResponse || m_engine.State.Window == Core.TimingWindow.ChainResponse)
            {
                var responses = m_snapshot.Actions.Where(a => a.Kind == Core.DuelCommandKind.Activate).ToArray();
                if (responses.Length == 0) { Pass(); return; }
                string prompt = m_engine.State.Chain.Count == 0 ? "当前时点，是否发动效果？"
                    : "「" + m_catalog.Get(m_engine.State.Chain.Last().DefinitionId).Name + "」发动时，是否连锁？";
                Choice(Stage.Response, prompt, 1, 1, true, responses.Select(a => m_handles[a.SourceViewCardId]).Distinct()
                    .Select(id => CardOption(id.ToString(), id))); return;
            }
            Notify(DuelChangeKind.State);
        }
    }
}
