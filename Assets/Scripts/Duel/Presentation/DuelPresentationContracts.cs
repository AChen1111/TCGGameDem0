using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace AChen.Duel.Presentation
{
    public enum DuelZone { MainDeck, ExtraDeck, Hand, Monster, SpellTrap, Field, Graveyard, Banished, ExtraMonster, Material }
    public enum CardPosition { FaceUpAttack, FaceUpDefense, FaceDownDefense, FaceUp, FaceDown }
    public enum DuelPhase { Draw, Standby, Main1, Battle, Main2, End }
    public enum DuelChangeKind { Reset, Move, Position, Highlight, Phase, Turn, Placement, CancelPlacement, Timer, AnimationCompleted, Rejected, Action, CancelAction, Effect, Attack, ChainResolved, State, Summoned }
    public enum DuelSessionMode { Offline, Online, Replay }

    public sealed class ZonePreviewCard
    {
        public int CardId { get; }
        public string DefinitionId { get; }
        public bool Known { get; }
        public int Count { get; }
        public ZonePreviewCard(int cardId, string definitionId, bool known, int count = 1)
        { CardId = cardId; DefinitionId = definitionId; Known = known; Count = count; }
    }
    public enum DuelActionKind { NormalSummon, SetMonster, SpecialSummon, Activate, SetSpellTrap, Pendulum, ChangePosition, DebugPlacement, Attack }

    public sealed class DuelTargetSlots
    {
        public DuelZone Kind { get; }
        public IReadOnlyList<int> Slots { get; }
        public DuelTargetSlots(DuelZone kind, params int[] slots)
        { Kind = kind; Slots = Array.AsReadOnly((int[])slots.Clone()); }
    }

    /// <summary>演示配置明确提供动作和目标；表现层不推导召唤资格。</summary>
    public sealed class DuelActionProfile
    {
        public string Id { get; }
        public DuelActionKind Kind { get; }
        public IReadOnlyList<DuelZone> Sources { get; }
        public IReadOnlyList<CardPosition> Positions { get; }
        public IReadOnlyList<DuelTargetSlots> TargetSlots { get; }
        public bool RequiresEffectAvailable { get; }
        public bool HasTarget => TargetSlots.Count != 0;
        public DuelActionProfile(string id, DuelActionKind kind, IEnumerable<DuelZone> sources,
            IEnumerable<CardPosition> positions, IEnumerable<DuelTargetSlots> targetSlots, bool requiresEffectAvailable = false)
        { Id = id; Kind = kind; Sources = Array.AsReadOnly(sources.ToArray()); Positions = Array.AsReadOnly(positions.ToArray());
          TargetSlots = Array.AsReadOnly(targetSlots.ToArray()); RequiresEffectAvailable = requiresEffectAvailable; }
    }

    public sealed class DuelActionPositionView
    {
        public CardPosition Position { get; }
        public IReadOnlyList<ZoneRef> Targets { get; }
        public DuelActionPositionView(CardPosition position, IEnumerable<ZoneRef> targets)
        { Position = position; Targets = Array.AsReadOnly(targets.ToArray()); }
    }

    public sealed class DuelActionView
    {
        public string Label { get; internal set; } = "";
        public string Id { get; }
        public DuelActionKind Kind { get; }
        public bool HasTarget { get; }
        public IReadOnlyList<CardPosition> Positions { get; }
        readonly IReadOnlyList<DuelActionPositionView> m_choices;
        public DuelActionView(string id, DuelActionKind kind, bool hasTarget, IEnumerable<DuelActionPositionView> choices)
        { Id = id; Kind = kind; HasTarget = hasTarget; m_choices = Array.AsReadOnly(choices.ToArray());
          Positions = Array.AsReadOnly(m_choices.Select(x => x.Position).ToArray()); }
        public IReadOnlyList<ZoneRef> TargetsFor(CardPosition position) => m_choices.First(x => x.Position == position).Targets;
    }

    public sealed class PendingActionView
    {
        public int InstanceId { get; }
        public string ActionId { get; }
        public DuelActionKind Kind { get; }
        public bool NeedsPosition { get; }
        public bool HasTarget { get; }
        public CardPosition Position { get; }
        public IReadOnlyList<CardPosition> Positions { get; }
        public IReadOnlyList<ZoneRef> Targets { get; }
        public PendingActionView(int instanceId, string actionId, DuelActionKind kind, bool needsPosition, bool hasTarget,
            CardPosition position, IEnumerable<CardPosition> positions, IEnumerable<ZoneRef> targets)
        { InstanceId = instanceId; ActionId = actionId; Kind = kind; NeedsPosition = needsPosition; HasTarget = hasTarget;
          Position = position; Positions = Array.AsReadOnly(positions.ToArray()); Targets = Array.AsReadOnly(targets.ToArray()); }
        public static PendingActionView Empty => new PendingActionView(0, "", default, false, false, default,
            Array.Empty<CardPosition>(), Array.Empty<ZoneRef>());
    }

    public sealed class DuelPlayerView
    {
        public string Name { get; }
        public int AvatarId { get; }
        public int AvatarFrameId { get; }
        public int LP { get; }
        public DuelPlayerView(string name, int avatarId, int lp = 8000, int avatarFrameId = 1030001)
        { Name = name; AvatarId = avatarId; LP = lp; AvatarFrameId = avatarFrameId; }
    }

    public readonly struct ZoneRef : IEquatable<ZoneRef>
    {
        public DuelZone Kind { get; }
        public int Player { get; }
        public int Slot { get; }
        public ZoneRef(DuelZone kind, int player, int slot = 0)
        { Kind = kind; Player = kind == DuelZone.ExtraMonster ? -1 : player; Slot = slot; }
        public bool IsSlot => Kind is DuelZone.Monster or DuelZone.SpellTrap or DuelZone.Field or DuelZone.ExtraMonster;
        public bool Equals(ZoneRef other) => Kind == other.Kind && Player == other.Player && Slot == other.Slot;
        public override bool Equals(object obj) => obj is ZoneRef other && Equals(other);
        public override int GetHashCode() => ((int)Kind * 31 + Player) * 31 + Slot;
        public override string ToString() => $"{Player}:{Kind}:{Slot}";
    }

    public sealed class DuelCardSpec
    {
        public string CardId { get; }
        public string SourcePool { get; }
        public CardKind Kind { get; }
        public CardFrame Frame { get; }
        public CardSpellTrapType SpellType { get; }
        public CardFlags Flags { get; }
        public bool IsPendulum => (Flags & CardFlags.Pendulum) != 0;
        public IReadOnlyList<DuelActionProfile> ActionProfiles { get; }
        public bool IsExtra => Frame is CardFrame.Fusion or CardFrame.Synchro or CardFrame.Xyz or CardFrame.Link;
        public DuelCardSpec(string id, string pool, CardKind kind, CardFrame frame, CardSpellTrapType spellType = CardSpellTrapType.None)
            : this(id, pool, kind, frame, spellType, CardFlags.None) { }
        public DuelCardSpec(string id, string pool, CardKind kind, CardFrame frame, CardSpellTrapType spellType, CardFlags flags)
            : this(id, pool, kind, frame, spellType, flags, DuelDemoActionProfiles.ForCard(kind, frame, spellType, flags)) { }
        public DuelCardSpec(string id, string pool, CardKind kind, CardFrame frame, CardSpellTrapType spellType, CardFlags flags,
            IEnumerable<DuelActionProfile> actionProfiles)
        { CardId = id; SourcePool = pool; Kind = kind; Frame = frame; SpellType = spellType; Flags = flags;
          ActionProfiles = Array.AsReadOnly(actionProfiles.ToArray()); }
    }

    public sealed class CardView
    {
        public bool Known { get; internal set; } = true;
        public bool Negated { get; internal set; }
        public int Attack { get; internal set; }
        public int? Defense { get; internal set; }
        public int Level { get; internal set; }
        public int HostInstanceId { get; internal set; }
        public int MaterialCount { get; internal set; }
        public int Generation { get; internal set; }
        public int InstanceId { get; }
        public int Owner { get; }
        public DuelCardSpec Definition { get; }
        public ZoneRef Zone { get; }
        public CardPosition Position { get; }
        public bool EffectAvailable { get; }
        public IReadOnlyList<ZoneRef> PlacementTargets { get; }
        public IReadOnlyList<CardPosition> AvailablePositions { get; }
        public IReadOnlyList<DuelActionView> Actions { get; }
        public CardView(int id, int owner, DuelCardSpec definition, ZoneRef zone, CardPosition position,
            bool available, IEnumerable<ZoneRef> targets, IEnumerable<CardPosition> positions)
            : this(id, owner, definition, zone, position, available, targets, positions, Array.Empty<DuelActionView>()) { }
        public CardView(int id, int owner, DuelCardSpec definition, ZoneRef zone, CardPosition position,
            bool available, IEnumerable<ZoneRef> targets, IEnumerable<CardPosition> positions, IEnumerable<DuelActionView> actions)
        { InstanceId = id; Owner = owner; Definition = definition; Zone = zone; Position = position;
          EffectAvailable = available; PlacementTargets = Array.AsReadOnly(targets.ToArray()); AvailablePositions = Array.AsReadOnly(positions.ToArray());
          Actions = Array.AsReadOnly(actions.ToArray()); }
    }

    public sealed class PlacementView
    {
        public int InstanceId { get; }
        public ZoneRef Target { get; }
        public IReadOnlyList<CardPosition> Positions { get; }
        public PlacementView(int id, ZoneRef target, IEnumerable<CardPosition> positions)
        { InstanceId = id; Target = target; Positions = Array.AsReadOnly(positions.ToArray()); }
        public static PlacementView Empty => new PlacementView(0, default, Array.Empty<CardPosition>());
    }

    public sealed class DuelView
    {
        public IReadOnlyList<CardView> Cards { get; }
        public IReadOnlyList<ZoneRef> AvailablePiles { get; }
        public int ActivePlayer { get; }
        public int Turn { get; }
        public DuelPhase Phase { get; }
        public bool Animating { get; }
        public bool HasPlacement => Placement.InstanceId != 0;
        public bool HasPendingAction => PendingAction.InstanceId != 0;
        public bool ReadOnly { get; internal set; }
        public bool CanInteract => !ReadOnly && !Animating && !HasPlacement && !HasPendingAction && !Choice.Active && !Finished;
        public bool CanBrowseCards => !Animating && !HasPlacement && !HasPendingAction && (!Finished || ReadOnly) && !Choice.Active;
        public DuelSelectionView Choice { get; internal set; } = DuelSelectionView.Empty;
        public int ViewingSeat { get; internal set; }
        public bool Finished { get; internal set; }
        public string Outcome { get; internal set; } = "";
        public PendingActionView PendingAction { get; }
        public IReadOnlyList<DuelPlayerView> Players { get; }
        public PlacementView Placement { get; }
        public IReadOnlyList<float> Seconds { get; }
        public bool TimerPaused { get; }
        public IReadOnlyList<DuelPhase> AvailablePhases { get; }
        public DuelView(IEnumerable<CardView> cards, IEnumerable<ZoneRef> piles, int active, int turn,
            DuelPhase phase, bool animating, PlacementView placement, float[] seconds, bool paused, IEnumerable<DuelPhase> phases)
            : this(cards, piles, active, turn, phase, animating, placement, seconds, paused, phases, PendingActionView.Empty,
                new[] { new DuelPlayerView("玩家", 1010001), new DuelPlayerView("对手", 1010002) }) { }
        public DuelView(IEnumerable<CardView> cards, IEnumerable<ZoneRef> piles, int active, int turn,
            DuelPhase phase, bool animating, PlacementView placement, float[] seconds, bool paused, IEnumerable<DuelPhase> phases,
            PendingActionView pendingAction, IEnumerable<DuelPlayerView> players)
        { Cards = Array.AsReadOnly(cards.ToArray()); AvailablePiles = Array.AsReadOnly(piles.ToArray());
          ActivePlayer = active; Turn = turn; Phase = phase; Animating = animating; Placement = placement;
          PendingAction = pendingAction; Players = Array.AsReadOnly(players.ToArray());
          Seconds = Array.AsReadOnly((float[])seconds.Clone()); TimerPaused = paused; AvailablePhases = Array.AsReadOnly(phases.ToArray()); }
        public IReadOnlyList<CardView> InZone(ZoneRef zone) => Array.AsReadOnly(Cards.Where(x => x.Zone.Equals(zone)).ToArray());
        public CardView Card(int id) => Cards.First(x => x.InstanceId == id);
    }

    public sealed class DuelViewChange
    {
        public string DefinitionId { get; internal set; } = "";
        public IReadOnlyList<int> ImpactLifePoints { get; internal set; } = Array.Empty<int>();
        public long ChainId { get; internal set; }
        public int LinkNumber { get; internal set; }
        public int AttackerId { get; internal set; }
        public int TargetId { get; internal set; }
        public ZoneRef Origin { get; internal set; }
        public bool IsNegated { get; internal set; }
        public DuelChangeKind Kind { get; }
        public DuelView View { get; }
        public int InstanceId { get; }
        public string Message { get; }
        public DuelViewChange(DuelChangeKind kind, DuelView view, int id = 0, string message = "")
        { Kind = kind; View = view; InstanceId = id; Message = message; }
    }

    public interface IDuelPresentationSource
    {
        DuelSessionMode Mode { get; }
        string OperationHint { get; }
        IEnumerable<DuelCardSpec> Definitions { get; }
        DuelView Current { get; }
        bool HasAttackPreview { get; }
        int AttackPreviewSource { get; }
        int AttackPreviewTarget { get; }
        int DeclaredAttacker { get; }
        int DeclaredTarget { get; }
        event Action<DuelViewChange> Changed;
        DuelCardSpec DefinitionForInstance(int id);
        void Start();
        void Tick(float delta);
        void PresentImpact(IReadOnlyList<int> life);
        void FinishPresentation();
        UniTask<ZonePreviewCard[]> PreviewZoneAsync(ZoneRef zone, CancellationToken token);
        void Submit(DuelInputCommand command);
    }
    public abstract class DuelInputCommand { }
    public sealed class BeginCardAction : DuelInputCommand
    { public int CardId { get; } public string ActionId { get; } public BeginCardAction(int id, string actionId) { CardId = id; ActionId = actionId; } }
    public sealed class BeginDebugPlacement : DuelInputCommand
    { public int CardId { get; } public BeginDebugPlacement(int id) { CardId = id; } }
    public sealed class ChooseActionPosition : DuelInputCommand
    { public CardPosition Position { get; } public ChooseActionPosition(CardPosition position) { Position = position; } }
    public sealed class ConfirmActionTarget : DuelInputCommand
    { public ZoneRef Target { get; } public ConfirmActionTarget(ZoneRef target) { Target = target; } }
    public sealed class CancelCardAction : DuelInputCommand { }
    public sealed class PreparePlacement : DuelInputCommand
    { public int CardId { get; } public ZoneRef Target { get; } public PreparePlacement(int id, ZoneRef target) { CardId = id; Target = target; } }
    public sealed class ConfirmPlacement : DuelInputCommand
    { public CardPosition Position { get; } public ConfirmPlacement(CardPosition position) { Position = position; } }
    public sealed class CancelPlacement : DuelInputCommand { }
    public sealed class MoveCard : DuelInputCommand
    { public int CardId { get; } public ZoneRef Target { get; } public MoveCard(int id, ZoneRef target) { CardId = id; Target = target; } }
    public sealed class ChangePosition : DuelInputCommand
    { public int CardId { get; } public CardPosition Position { get; } public ChangePosition(int id, CardPosition position) { CardId = id; Position = position; } }
    public sealed class SetCardEffectAvailable : DuelInputCommand
    { public int CardId { get; } public bool Available { get; } public SetCardEffectAvailable(int id, bool available) { CardId = id; Available = available; } }
    public sealed class SetPileEffectAvailable : DuelInputCommand
    { public ZoneRef Zone { get; } public bool Available { get; } public SetPileEffectAvailable(ZoneRef zone, bool available) { Zone = zone; Available = available; } }
    public sealed class ChangePhase : DuelInputCommand
    { public DuelPhase Phase { get; } public ChangePhase(DuelPhase phase) { Phase = phase; } }
    public sealed class EndTurn : DuelInputCommand { }
    public sealed class AnimationCompleted : DuelInputCommand { }
    public sealed class PauseTimer : DuelInputCommand
    { public bool Paused { get; } public PauseTimer(bool paused) { Paused = paused; } }
    public sealed class ResetTimer : DuelInputCommand { }
    public sealed class ResetDuel : DuelInputCommand { }
    public sealed class ConfirmDuelSelection : DuelInputCommand
    {
        public long ChoiceId { get; }
        public string[] Keys { get; }
        public ConfirmDuelSelection(long id, IEnumerable<string> keys) { ChoiceId = id; Keys = keys.ToArray(); }
    }
    public sealed class PreviewDuelTarget : DuelInputCommand
    { public int CardId { get; } public PreviewDuelTarget(int id) { CardId = id; } }
    public sealed class PassDuelResponse : DuelInputCommand { }
    public sealed class SurrenderDuel : DuelInputCommand { }

    public sealed class DuelSelectionOption
    {
        public string Key { get; }
        public string Label { get; }
        public string DefinitionId { get; }
        public int CardId { get; }
        public DuelSelectionOption(string key, string label, string definitionId = "", int cardId = 0)
        { Key = key; Label = label; DefinitionId = definitionId; CardId = cardId; }
    }
    public sealed class DuelSelectionView
    {
        public bool IsResponse { get; internal set; }
        public long Id { get; }
        public string Prompt { get; }
        public int Min { get; }
        public int Max { get; }
        public bool CanCancel { get; }
        public bool Ordered { get; }
        public bool Searchable { get; }
        public bool PreviewAttack { get; }
        public IReadOnlyList<DuelSelectionOption> Options { get; }
        public bool Active => Id != 0;
        public DuelSelectionView(long id, string prompt, int min, int max, bool cancel,
            IEnumerable<DuelSelectionOption> options, bool ordered = false, bool searchable = false, bool previewAttack = false)
        { Id = id; Prompt = prompt; Min = min; Max = max; CanCancel = cancel; Ordered = ordered;
          Searchable = searchable; PreviewAttack = previewAttack; Options = Array.AsReadOnly(options.ToArray()); }
        public static DuelSelectionView Empty => new DuelSelectionView(0, "", 0, 0, false, Array.Empty<DuelSelectionOption>());
    }
}
