using System;
using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Presentation
{
    /// <summary>固定演示动作数据；正式对战由规则数据源替换。</summary>
    public static class DuelDemoActionProfiles
    {
        public static IEnumerable<DuelActionProfile> ForCard(CardKind kind, CardFrame frame, CardSpellTrapType spellType, CardFlags flags)
        {
            if (kind == CardKind.Monster && frame is not CardFrame.Fusion and not CardFrame.Synchro and not CardFrame.Xyz and not CardFrame.Link)
            {
                yield return new DuelActionProfile("normal-summon", DuelActionKind.NormalSummon, new[] { DuelZone.Hand },
                    new[] { CardPosition.FaceUpAttack }, new[] { new DuelTargetSlots(DuelZone.Monster, 0, 1, 2, 3, 4) });
                yield return new DuelActionProfile("set-monster", DuelActionKind.SetMonster, new[] { DuelZone.Hand },
                    new[] { CardPosition.FaceDownDefense }, new[] { new DuelTargetSlots(DuelZone.Monster, 0, 1, 2, 3, 4) });
            }
            if (kind == CardKind.Monster)
            {
                bool extra = frame is CardFrame.Fusion or CardFrame.Synchro or CardFrame.Xyz or CardFrame.Link;
                var destinations = new List<DuelTargetSlots>();
                if (frame != CardFrame.Link) destinations.Add(new DuelTargetSlots(DuelZone.Monster, 0, 1, 2, 3, 4));
                if (extra) destinations.Add(new DuelTargetSlots(DuelZone.ExtraMonster, 0, 1));
                yield return new DuelActionProfile("special-summon", DuelActionKind.SpecialSummon,
                    extra ? new[] { DuelZone.ExtraDeck, DuelZone.Graveyard, DuelZone.Banished }
                        : new[] { DuelZone.Hand, DuelZone.Graveyard, DuelZone.Banished },
                    frame == CardFrame.Link ? new[] { CardPosition.FaceUpAttack }
                        : new[] { CardPosition.FaceUpAttack, CardPosition.FaceUpDefense }, destinations);
            }
            else if (kind is CardKind.Spell or CardKind.Trap)
            {
                var target = spellType == CardSpellTrapType.Field ? new DuelTargetSlots(DuelZone.Field, 0)
                    : new DuelTargetSlots(DuelZone.SpellTrap, 0, 1, 2, 3, 4);
                yield return new DuelActionProfile("set-spell-trap", DuelActionKind.SetSpellTrap, new[] { DuelZone.Hand },
                    new[] { CardPosition.FaceDown }, new[] { target });
                if (kind == CardKind.Spell)
                    yield return new DuelActionProfile("activate-spell", DuelActionKind.Activate, new[] { DuelZone.Hand },
                        new[] { CardPosition.FaceUp }, new[] { target });
            }
            yield return new DuelActionProfile("activate-effect", DuelActionKind.Activate,
                kind == CardKind.Monster
                    ? new[] { DuelZone.Hand, DuelZone.Monster, DuelZone.ExtraMonster, DuelZone.Graveyard, DuelZone.Banished }
                    : new[] { DuelZone.SpellTrap, DuelZone.Field, DuelZone.Graveyard, DuelZone.Banished },
                new[] { CardPosition.FaceUp }, Array.Empty<DuelTargetSlots>(), true);
            var positions = kind == CardKind.Monster
                ? frame == CardFrame.Link ? new[] { CardPosition.FaceUpAttack }
                    : frame is CardFrame.Fusion or CardFrame.Synchro or CardFrame.Xyz
                        ? new[] { CardPosition.FaceUpAttack, CardPosition.FaceUpDefense }
                        : new[] { CardPosition.FaceUpAttack, CardPosition.FaceUpDefense, CardPosition.FaceDownDefense }
                : new[] { CardPosition.FaceUp, CardPosition.FaceDown };
            yield return new DuelActionProfile("change-position", DuelActionKind.ChangePosition,
                kind == CardKind.Monster ? new[] { DuelZone.Monster, DuelZone.ExtraMonster }
                    : new[] { DuelZone.SpellTrap, DuelZone.Field }, positions, Array.Empty<DuelTargetSlots>());
            if ((flags & CardFlags.Pendulum) != 0)
            {
                yield return new DuelActionProfile("pendulum", DuelActionKind.Pendulum, new[] { DuelZone.Hand },
                    new[] { CardPosition.FaceUp }, new[] { new DuelTargetSlots(DuelZone.SpellTrap, 0, 4) });
                yield return new DuelActionProfile("change-pendulum-position", DuelActionKind.ChangePosition, new[] { DuelZone.SpellTrap },
                    new[] { CardPosition.FaceUp, CardPosition.FaceDown }, Array.Empty<DuelTargetSlots>());
            }
        }
    }

    public sealed class DuelDemoSession : IDuelPresentationSource
    {
        sealed class CardState
        {
            public int Id, Owner;
            public DuelCardSpec Spec;
            public ZoneRef Zone;
            public CardPosition Position;
            public bool Available;
        }
        readonly IReadOnlyList<DuelCardSpec> m_main, m_extra;
        readonly int m_openingHand;
        readonly float m_duration;
        readonly List<CardState> m_cards = new List<CardState>();
        readonly HashSet<ZoneRef> m_availablePiles = new HashSet<ZoneRef>();
        readonly float[] m_seconds = new float[2];
        int m_active, m_turn;
        DuelPhase m_phase;
        bool m_animating, m_paused;
        PlacementView m_placement = PlacementView.Empty;
        PendingActionView m_pendingAction = PendingActionView.Empty;
        DuelActionView m_selectedAction = new DuelActionView("", default, false, Array.Empty<DuelActionPositionView>());
        readonly IReadOnlyList<DuelPlayerView> m_players;
        public DuelView Current { get; private set; }
        public event Action<DuelViewChange> Changed = delegate { };
        public DuelDemoSession(IReadOnlyList<DuelCardSpec> main, IReadOnlyList<DuelCardSpec> extra, int openingHand = 5, float seconds = 180)
            : this(main, extra, openingHand, seconds, new[] { new DuelPlayerView("玩家", 1010001), new DuelPlayerView("对手", 1010002) }) { }
        public DuelDemoSession(IReadOnlyList<DuelCardSpec> main, IReadOnlyList<DuelCardSpec> extra, int openingHand,
            float seconds, IEnumerable<DuelPlayerView> players)
        { m_main = Array.AsReadOnly(main.ToArray()); m_extra = Array.AsReadOnly(extra.ToArray()); m_openingHand = openingHand;
          m_duration = seconds; m_players = Array.AsReadOnly(players.ToArray()); Initialize(); }

        void Initialize()
        {
            m_cards.Clear(); m_availablePiles.Clear(); m_active = 0; m_turn = 1; m_phase = DuelPhase.Main1;
            m_animating = false; m_paused = false; m_placement = PlacementView.Empty;
            m_pendingAction = PendingActionView.Empty;
            m_seconds[0] = m_seconds[1] = m_duration;
            for (int player = 0; player < 2; player++)
            {
                for (int i = 0; i < m_main.Count; i++) Add(m_main[i], player, i < m_openingHand ? DuelZone.Hand : DuelZone.MainDeck);
                foreach (var spec in m_extra) Add(spec, player, DuelZone.ExtraDeck);
            }
            Refresh();
        }
        void Add(DuelCardSpec spec, int player, DuelZone zone) => m_cards.Add(new CardState
        { Id = m_cards.Count + 1, Owner = player, Spec = spec, Zone = new ZoneRef(zone, player),
          Position = zone == DuelZone.Hand ? CardPosition.FaceUp : CardPosition.FaceDown });

        IEnumerable<ZoneRef> Targets(CardState card)
        {
            return StructuralTargets(card).Where(zone => !m_cards.Any(other => other.Id != card.Id && other.Zone.Equals(zone)))
                .Where(zone => zone.Kind != DuelZone.ExtraMonster || !m_cards.Any(other => other.Id != card.Id
                    && other.Owner == card.Owner && other.Zone.Kind == DuelZone.ExtraMonster));
        }
        IEnumerable<ZoneRef> StructuralTargets(CardState card)
        {
            if (card.Spec.Kind == CardKind.Monster)
            {
                if (card.Spec.Frame != CardFrame.Link)
                    for (int i = 0; i < 5; i++) yield return new ZoneRef(DuelZone.Monster, card.Owner, i);
                if (card.Spec.IsExtra)
                    for (int i = 0; i < 2; i++) yield return new ZoneRef(DuelZone.ExtraMonster, -1, i);
                if (card.Spec.IsPendulum)
                { yield return new ZoneRef(DuelZone.SpellTrap, card.Owner, 0); yield return new ZoneRef(DuelZone.SpellTrap, card.Owner, 4); }
            }
            else if (card.Spec.SpellType == CardSpellTrapType.Field) yield return new ZoneRef(DuelZone.Field, card.Owner);
            else for (int i = 0; i < 5; i++) yield return new ZoneRef(DuelZone.SpellTrap, card.Owner, i);
        }
        static CardPosition[] Positions(CardState card)
        {
            if (card.Spec.Frame == CardFrame.Link) return new[] { CardPosition.FaceUpAttack };
            if (card.Spec.Kind == CardKind.Monster) return card.Spec.IsExtra
                ? new[] { CardPosition.FaceUpAttack, CardPosition.FaceUpDefense }
                : new[] { CardPosition.FaceUpAttack, CardPosition.FaceUpDefense, CardPosition.FaceDownDefense };
            return card.Spec.Kind == CardKind.Trap ? new[] { CardPosition.FaceDown } : new[] { CardPosition.FaceUp, CardPosition.FaceDown };
        }
        static CardPosition[] PlacementPositions(CardState card, ZoneRef zone) => card.Spec.IsPendulum && zone.Kind == DuelZone.SpellTrap
            ? new[] { CardPosition.FaceUp } : Positions(card);
        bool IsFreeTarget(CardState card, ZoneRef zone) => !card.Zone.Equals(zone)
            && !m_cards.Any(other => other.Id != card.Id && other.Zone.Equals(zone))
            && (zone.Kind != DuelZone.ExtraMonster || !m_cards.Any(other => other.Id != card.Id
                && other.Owner == card.Owner && other.Zone.Kind == DuelZone.ExtraMonster));
        IEnumerable<DuelActionView> Actions(CardState card)
        {
            if (card.Owner != m_active || m_phase is not DuelPhase.Main1 and not DuelPhase.Main2) yield break;
            foreach (var profile in card.Spec.ActionProfiles.Where(x => x.Sources.Contains(card.Zone.Kind)
                && (!x.RequiresEffectAvailable || card.Available)))
            {
                var targets = profile.TargetSlots.SelectMany(x => x.Slots.Select(slot => new ZoneRef(x.Kind, card.Owner, slot)))
                    .Where(x => IsFreeTarget(card, x)).ToArray();
                if (profile.HasTarget && targets.Length == 0) continue;
                var positions = profile.Kind == DuelActionKind.ChangePosition
                    ? profile.Positions.Where(x => x != card.Position).ToArray() : profile.Positions.ToArray();
                if (positions.Length == 0) continue;
                yield return new DuelActionView(profile.Id, profile.Kind, profile.HasTarget,
                    positions.Select(position => new DuelActionPositionView(position, targets)));
            }
        }
        void Refresh() => Current = new DuelView(m_cards.Select(c => new CardView(c.Id, c.Owner, c.Spec,
            c.Zone, c.Position, c.Available, Targets(c), AvailablePositions(c.Id), Actions(c))), m_availablePiles, m_active, m_turn, m_phase,
            m_animating, m_placement, m_seconds, m_paused, AvailablePhases, m_pendingAction, m_players);
        void Publish(DuelChangeKind kind, int id = 0, string message = "")
        { Refresh(); Changed(new DuelViewChange(kind, Current, id, message)); }
        void Move(CardState card, ZoneRef zone, CardPosition position)
        {
            m_cards.Remove(card); card.Zone = zone; card.Position = position; m_cards.Add(card);
            m_placement = PlacementView.Empty; m_pendingAction = PendingActionView.Empty;
            m_animating = true; Publish(DuelChangeKind.Move, card.Id);
        }
        void CompleteTargetlessAction(int id, CardPosition position)
        {
            var kind = m_selectedAction.Kind;
            m_pendingAction = PendingActionView.Empty;
            if (kind == DuelActionKind.ChangePosition) m_cards.First(x => x.Id == id).Position = position;
            m_animating = true;
            Publish(kind == DuelActionKind.ChangePosition ? DuelChangeKind.Position : DuelChangeKind.Effect, id);
        }
        void BeginAction(CardState card, DuelActionView action)
        {
            m_selectedAction = action;
            var position = action.Positions[0];
            bool needsPosition = action.Positions.Count > 1;
            m_pendingAction = new PendingActionView(card.Id, action.Id, action.Kind, needsPosition, action.HasTarget,
                position, action.Positions, needsPosition ? Array.Empty<ZoneRef>() : action.TargetsFor(position));
            if (!needsPosition && !action.HasTarget) CompleteTargetlessAction(card.Id, position);
            else Publish(DuelChangeKind.Action, card.Id);
        }
        DuelActionView DebugAction(CardState card)
        {
            var positions = Positions(card).AsEnumerable();
            if (card.Spec.IsPendulum) positions = positions.Append(CardPosition.FaceUp);
            var choices = positions.Select(position => new DuelActionPositionView(position,
                Targets(card).Where(zone => IsFreeTarget(card, zone)).Where(zone => card.Spec.Kind != CardKind.Monster
                    || (zone.Kind == DuelZone.SpellTrap) == (position == CardPosition.FaceUp))));
            return new DuelActionView("debug-placement", DuelActionKind.DebugPlacement, true, choices.Where(x => x.Targets.Count != 0));
        }
        public IReadOnlyList<DuelPhase> AvailablePhases => m_phase switch
        {
            DuelPhase.Draw => new[] { DuelPhase.Standby },
            DuelPhase.Standby => new[] { DuelPhase.Main1 },
            DuelPhase.Main1 => m_turn == 1 ? new[] { DuelPhase.End } : new[] { DuelPhase.Battle, DuelPhase.End },
            DuelPhase.Battle => new[] { DuelPhase.Main2, DuelPhase.End },
            DuelPhase.Main2 => new[] { DuelPhase.End },
            _ => Array.Empty<DuelPhase>()
        };
        public IReadOnlyList<CardPosition> AvailablePositions(int id)
        {
            var card = m_cards.First(x => x.Id == id);
            if (card.Zone.IsSlot)
            {
                if (card.Zone.Kind is DuelZone.SpellTrap or DuelZone.Field) return new[] { CardPosition.FaceUp, CardPosition.FaceDown };
                if (card.Spec.Kind == CardKind.Trap) return new[] { CardPosition.FaceUp, CardPosition.FaceDown };
                return Positions(card);
            }
            return card.Zone.Kind == DuelZone.Banished ? new[] { CardPosition.FaceUp, CardPosition.FaceDown } : Array.Empty<CardPosition>();
        }
        public void Tick(float deltaSeconds)
        {
            if (m_paused || m_seconds[m_active] <= 0) return;
            float before = m_seconds[m_active];
            m_seconds[m_active] = Math.Max(0, before - deltaSeconds);
            if (Math.Ceiling(before) != Math.Ceiling(m_seconds[m_active])) Publish(DuelChangeKind.Timer);
        }
        bool CanMove(CardState card, ZoneRef target)
        {
            if (target.IsSlot) return Targets(card).Contains(target);
            if (target.Player != card.Owner) return false;
            return target.Kind switch
            {
                DuelZone.MainDeck or DuelZone.Hand => !card.Spec.IsExtra,
                DuelZone.ExtraDeck => card.Spec.IsExtra,
                DuelZone.Graveyard or DuelZone.Banished => true,
                _ => false
            };
        }
        static CardPosition DefaultPosition(CardState card, ZoneRef zone) => zone.IsSlot ? PlacementPositions(card, zone)[0]
            : zone.Kind is DuelZone.MainDeck or DuelZone.ExtraDeck ? CardPosition.FaceDown : CardPosition.FaceUp;
        public void Submit(DuelInputCommand command)
        {
            if ((m_animating && command is not AnimationCompleted and not ResetDuel and not PauseTimer and not ResetTimer)
                || (m_placement.InstanceId != 0 && command is not ConfirmPlacement and not CancelPlacement and not ResetDuel and not PauseTimer and not ResetTimer)
                || (m_pendingAction.InstanceId != 0 && command is not ChooseActionPosition and not ConfirmActionTarget and not CancelCardAction
                    and not ResetDuel and not PauseTimer and not ResetTimer))
            { Publish(DuelChangeKind.Rejected, message: "请先完成当前操作"); return; }
            switch (command)
            {
                case BeginCardAction begin:
                    var actor = m_cards.First(x => x.Id == begin.CardId);
                    var actions = Actions(actor).Where(x => x.Id == begin.ActionId).ToArray();
                    if (actions.Length == 0)
                    { Publish(DuelChangeKind.Rejected, message: "当前没有这个可用动作"); break; }
                    BeginAction(actor, actions[0]); break;
                case BeginDebugPlacement debug:
                    var debugging = m_cards.First(x => x.Id == debug.CardId);
                    var debugAction = DebugAction(debugging);
                    if (debugAction.Positions.Count == 0)
                    { Publish(DuelChangeKind.Rejected, message: "没有可放置的空闲区域"); break; }
                    BeginAction(debugging, debugAction); break;
                case ConfirmActionTarget confirmAction:
                    if (m_pendingAction.InstanceId == 0 || m_pendingAction.NeedsPosition
                        || !m_pendingAction.Targets.Contains(confirmAction.Target))
                    { Publish(DuelChangeKind.Rejected, message: "请选择当前动作允许的空闲区域"); break; }
                    Move(m_cards.First(x => x.Id == m_pendingAction.InstanceId), confirmAction.Target, m_pendingAction.Position); break;
                case ChooseActionPosition choose:
                    if (m_pendingAction.InstanceId == 0 || !m_pendingAction.NeedsPosition
                        || !m_pendingAction.Positions.Contains(choose.Position))
                    { Publish(DuelChangeKind.Rejected, message: "请选择当前动作允许的表示形式"); break; }
                    m_pendingAction = new PendingActionView(m_pendingAction.InstanceId, m_pendingAction.ActionId, m_pendingAction.Kind,
                        false, m_pendingAction.HasTarget, choose.Position, m_pendingAction.Positions, m_selectedAction.TargetsFor(choose.Position));
                    if (!m_pendingAction.HasTarget)
                    { CompleteTargetlessAction(m_pendingAction.InstanceId, choose.Position); break; }
                    Publish(DuelChangeKind.Action, m_pendingAction.InstanceId); break;
                case CancelCardAction:
                    m_pendingAction = PendingActionView.Empty; Publish(DuelChangeKind.CancelAction); break;
                case PreparePlacement prepare:
                    var card = m_cards.First(x => x.Id == prepare.CardId);
                    if (!Targets(card).Contains(prepare.Target))
                    { Publish(DuelChangeKind.Rejected, message: "请选择空闲且适合这张卡的区域"); break; }
                    m_placement = new PlacementView(card.Id, prepare.Target, PlacementPositions(card, prepare.Target));
                    Publish(DuelChangeKind.Placement, card.Id); break;
                case ConfirmPlacement confirm:
                    if (m_placement.InstanceId == 0 || !m_placement.Positions.Contains(confirm.Position))
                    { Publish(DuelChangeKind.Rejected, message: "请选择允许的表示形式"); break; }
                    Move(m_cards.First(x => x.Id == m_placement.InstanceId), m_placement.Target, confirm.Position); break;
                case CancelPlacement:
                    m_placement = PlacementView.Empty; Publish(DuelChangeKind.CancelPlacement); break;
                case AnimationCompleted:
                    m_animating = false; Publish(DuelChangeKind.AnimationCompleted); break;
                case ResetDuel:
                    Initialize(); Publish(DuelChangeKind.Reset); break;
                case MoveCard move:
                    var moving = m_cards.First(x => x.Id == move.CardId);
                    if (!CanMove(moving, move.Target) || moving.Zone.Equals(move.Target))
                    { Publish(DuelChangeKind.Rejected, message: "这张卡不能移到所选区域"); break; }
                    Move(moving, move.Target, DefaultPosition(moving, move.Target)); break;
                case ChangePosition change:
                    if (!AvailablePositions(change.CardId).Contains(change.Position))
                    { Publish(DuelChangeKind.Rejected, message: "当前卡牌不支持该表示形式"); break; }
                    m_cards.First(x => x.Id == change.CardId).Position = change.Position;
                    m_animating = true; Publish(DuelChangeKind.Position, change.CardId); break;
                case SetCardEffectAvailable highlight:
                    m_cards.First(x => x.Id == highlight.CardId).Available = highlight.Available;
                    Publish(DuelChangeKind.Highlight, highlight.CardId); break;
                case SetPileEffectAvailable pile:
                    if (pile.Available) m_availablePiles.Add(pile.Zone); else m_availablePiles.Remove(pile.Zone);
                    Publish(DuelChangeKind.Highlight); break;
                case ChangePhase phase:
                    if (!AvailablePhases.Contains(phase.Phase))
                    { Publish(DuelChangeKind.Rejected, message: "请选择后续可进入的阶段"); break; }
                    m_phase = phase.Phase; m_animating = true; Publish(DuelChangeKind.Phase); break;
                case EndTurn:
                    if (m_phase is DuelPhase.Draw or DuelPhase.Standby)
                    { Publish(DuelChangeKind.Rejected, message: "请先进入主要阶段"); break; }
                    m_active = 1 - m_active; m_turn++; m_phase = DuelPhase.Draw; m_seconds[m_active] = m_duration;
                    m_animating = true; Publish(DuelChangeKind.Turn); break;
                case PauseTimer pause:
                    m_paused = pause.Paused; Publish(DuelChangeKind.Timer); break;
                case ResetTimer:
                    m_seconds[m_active] = m_duration; Publish(DuelChangeKind.Timer); break;
            }
        }
    }
}
