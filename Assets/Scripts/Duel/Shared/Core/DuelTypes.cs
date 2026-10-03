using System;
using System.Collections.Generic;

namespace AChen.Duel.Core
{
    public enum DuelPhase { Draw, Standby, Main1, Battle, Main2, End }
    public enum DuelZone { Deck, Hand, Monster, SpellTrap, Field, Graveyard, Banished, ExtraDeck, ExtraMonster, Material }
    public enum CardPosition { FaceDown, FaceUpAttack, FaceUpDefense, FaceDownDefense, FaceUp }
    public enum TimingWindow { Open, FastResponse, ChainResponse, Resolving, Decision, Finished }
    public enum BattleStep { None, Declaration, DamageStart, BeforeCalculation, Calculation, AfterCalculation, DamageEnd }
    public enum DecisionKind { ChooseCards, ChooseMode, OrderCards, DeclareName, ChooseZone, ChoosePosition, YesNo }
    public enum DuelCommandKind { Pass, AdvancePhase, NormalSummon, SetMonster, SetSpellTrap, ChangePosition, SpecialSummon, Activate, Attack, Answer, Surrender, Timeout, DisconnectTimeout, Abort }
    public enum DuelEventKind { Started, Moved, Revealed, Shuffled, Summoned, PositionChanged, Activated, Resolved, Negated, Damaged, Drawn, PhaseChanged, TurnChanged, DecisionOpened, Finished, AttackDeclared, Destroyed, Recovered, BattleStepChanged }
    public enum MoveCause { Rule, Cost, Effect, Battle, Draw, SummonMaterial, Tribute, Discard, ActivationNegated }
    public enum SummonMethod { Normal, Flip, Effect, Procedure, Fusion, Synchro, Xyz, Link, MaskChange, Contact }

    public readonly struct CardRef : IEquatable<CardRef>
    {
        public int InstanceId { get; }
        public int Generation { get; }
        public CardRef(int instanceId, int generation) { InstanceId = instanceId; Generation = generation; }
        public bool Equals(CardRef other) => InstanceId == other.InstanceId && Generation == other.Generation;
        public override bool Equals(object value) => value is CardRef other && Equals(other);
        public override int GetHashCode() => unchecked(InstanceId * 397 ^ Generation);
        public override string ToString() => InstanceId + ":" + Generation;
    }

    public sealed class DuelCardState
    {
        public int InstanceId;
        public int Generation;
        public CardRef Ref => new CardRef(InstanceId, Generation);
        public string DefinitionId = "";
        public int Owner;
        public int Controller;
        public DuelZone Zone;
        public int Slot;
        public CardPosition Position;
        public int CurrentAtk;
        public int? CurrentDef;
        public int CurrentLevel;
        public int CurrentAttribute;
        public int CurrentRace;
        public string CurrentNameId = "";
        public int SummonedTurn;
        public int PositionChangedTurn;
        public int AttacksThisTurn;
        public bool DirectAttackDeclaredThisTurn;
        public List<CardRef> AttackedTargetsThisTurn = new List<CardRef>();
        public int SetTurn;
        public bool ProperlySummoned;
        public SummonMethod SummonMethod;
        public List<string> SummonMaterialDefinitions = new List<string>();
        public bool Negated;
        public bool CurrentNormal;
        public bool SummonModifiersSuppressed;
        public int HostInstanceId;
        public CardRef EquipTarget;
        public int TrackingEpoch;
        public int RevealedToMask;
        public List<int> Materials = new List<int>();
        public Dictionary<string, int> Counters = new Dictionary<string, int>(StringComparer.Ordinal);
    }

    public sealed class DuelPlayerState
    {
        public int LifePoints = 8000;
        public int NormalSummonsThisTurn;
        public List<int> Deck = new List<int>();
        public List<int> ExtraDeck = new List<int>();
    }

    public sealed class DecisionOption
    {
        public DuelZone DestinationZone = DuelZone.Monster;
        public string Id = "";
        public string Label = "";
        public CardRef Card;
        public bool HasCard;
        public string Value = "";
    }

    public sealed class DuelDecision
    {
        public long Id;
        public int Player;
        public DecisionKind Kind;
        public string Prompt = "";
        public int Min;
        public int Max;
        public bool CanCancel;
        public List<DecisionOption> Options = new List<DecisionOption>();
        public List<List<CardRef>> AllowedCardGroups = new List<List<CardRef>>();
        public string MaterialRecipeId = "";
        public bool UniqueOriginalNames;
        public bool RequireSetCapacity;
        public string Continuation = "";
        public int SourceId;
        public List<int> Selected = new List<int>();
        public List<string> Answers = new List<string>();
        public DecisionKind AnswerKind;
    }

    public sealed class DuelChainLink
    {
        public int Number;
        public int Player;
        public CardRef Source;
        public string DefinitionId = "";
        public string AbilityId = "";
        public int Speed;
        public string ModeId = "";
        public EffectCategories Categories;
        public RuleCardKind ActivationKind;
        public DuelZone ActivationZone;
        public bool FieldEffectNegated;
        public bool IsCardActivation;
        public bool ActivationNegated;
        public bool EffectNegated;
        public List<CardRef> Targets = new List<CardRef>();
        public List<CardRef> Costs = new List<CardRef>();
        public List<int> Selected = new List<int>();
        public List<string> Answers = new List<string>();
        public DecisionKind AnswerKind;
        public string Program = "";
        public string ReplacementCardId = "";
        public string ReplacementProgram = "";
        public CardLastKnown ActivationSource;
        public int Step;
        public Dictionary<string, int> Values = new Dictionary<string, int>(StringComparer.Ordinal);
        public Dictionary<string, string> StringValues = new Dictionary<string, string>(StringComparer.Ordinal);
        public DuelEvent TriggerEvent;
        public long ResolutionGroupId;
        public Dictionary<int, DuelDestructionOperation> Destructions = new Dictionary<int, DuelDestructionOperation>();
    }

    public sealed class DuelState
    {
        public long Revision;
        public int Turn = 1;
        public int TurnPlayer;
        public DuelPhase Phase = DuelPhase.Draw;
        public TimingWindow Window;
        public int WaitingSeat;
        public int ConsecutivePasses;
        public int PendingPhase = -1;
        public BattleStep BattleStep;
        public CardRef Attacker;
        public CardRef AttackTarget;
        public List<CardRef> BattleDestroyed = new List<CardRef>();
        public List<int> AttackTargetsAtDeclaration = new List<int>();
        public List<CardRef> AttackTargetRefsAtDeclaration = new List<CardRef>();
        public bool Finished;
        public int Winner = -1;
        public string EndReason = "";
        public ulong RandomState;
        public long NextDecisionId = 1;
        public int NextInstanceId = 1;
        public long NextEventId = 1;
        public DuelPlayerState[] Players = { new DuelPlayerState(), new DuelPlayerState() };
        public List<DuelCardState> Cards = new List<DuelCardState>();
        public List<DuelChainLink> Chain = new List<DuelChainLink>();
        public List<CardRef> ChainCleanup = new List<CardRef>();
        public DuelDecision PendingDecision;
        public Dictionary<string, int> UsedAbilities = new Dictionary<string, int>(StringComparer.Ordinal);
        public List<DuelEffectRecord> Effects = new List<DuelEffectRecord>();
        public List<DuelEvent> PendingFacts = new List<DuelEvent>();
        public List<DuelEvent> LastCheckpointEvents = new List<DuelEvent>();
        public List<PendingTrigger> PendingTriggers = new List<PendingTrigger>();
        public PendingTrigger ActiveTrigger;
        public int TriggerTargetId;
        public string TriggerModeId = "";
        public List<int> TriggerCostIds = new List<int>();
        public long NextEffectId = 1;
        public long NextChainId = 1;
        public long CurrentChainId;
        public int NextDestructionId = 1;
        public DuelDestructionOperation PendingDestruction;
        public long NextEventGroupId = 1;
        public long CurrentEventGroupId;
        public string RuleVersion = "";
        public string CatalogHash = "";
        public int ProtocolVersion = 1;
        public string RulePackageHash = "";
        public string NameCatalogHash = "";
        public string BanlistHash = "";
        public string ContinuationAfterTriggers = "";
        public List<DuelEvent> TurnFacts = new List<DuelEvent>();
        public List<int> PrivateTriggerPlayers = new List<int>();
    }

    public sealed class DuelEvent
    {
        public int TurnAtEvent;
        public long Id;
        public DuelPhase PhaseAtEvent;
        public long ChainId;
        public int LinkNumber;
        public RuleCardKind ActivationKind;
        public bool ActivationNegated;
        public bool IsCardActivation;
        public DuelEventKind Kind;
        public int Player;
        public CardRef Card;
        public bool HasCard;
        public string DefinitionId = "";
        public int VisibleToMask;
        public DuelZone From;
        public DuelZone To;
        public int Amount;
        public int Attack;
        public int? Defense;
        public string Detail = "";
        public MoveCause Cause;
        public CardRef EffectSource;
        public int EffectPlayer = -1;
        public long GroupId;
        public CardLastKnown Before;
        public CardLastKnown After;
        public SummonMethod SummonMethod;
        public bool WasSummonMaterial;
        public SummonMethod MaterialMethod;
        public List<CardLastKnown> PresentCards = new List<CardLastKnown>();
        public CardRef BattleAttacker;
        public CardRef BattleTarget;
        public string DeclaredNameId = "";
    }

    public sealed class DuelCommand
    {
        public DuelCommandKind Kind;
        public int Player;
        public int CardId;
        public int TargetId;
        public int Slot;
        public CardPosition Position = CardPosition.FaceUpAttack;
        public string AbilityId = "";
        public long DecisionId;
        public string[] Options = Array.Empty<string>();
        public int[] Cards = Array.Empty<int>();
        public DuelPhase Phase;
        public string NameId = "";
    }

    public sealed class DuelAction
    {
        public string Id = "";
        public DuelCommandKind Kind;
        public CardRef Card;
        public string AbilityId = "";
        public DuelPhase Phase;
        public List<int> Slots = new List<int>();
        public List<CardRef> Targets = new List<CardRef>();
        public List<CardRef> SelectionCards = new List<CardRef>();
        public bool SelectionIsTarget;
        public int MinSelections;
        public int MaxSelections;
        public List<CardPosition> Positions = new List<CardPosition>();
        public bool CanAttackDirectly;
        public string[] ActivationOptions = Array.Empty<string>();
        public string Label = "";
        public bool RequiresNameDeclaration;
        public RuleCardKind DeclarationKind;
    }

    public sealed class DuelStepResult
    {
        public bool Accepted;
        public string Error = "";
        public long Revision;
        public List<DuelEvent> Events = new List<DuelEvent>();
    }

    public sealed class DuelStartRecord
    {
        public string[][] MainDecks = { Array.Empty<string>(), Array.Empty<string>() };
        public string[][] ExtraDecks = { Array.Empty<string>(), Array.Empty<string>() };
        public ulong Seed = 1;
        public int FirstPlayer;
        public bool Shuffle = true;
        public int OpeningHand = 5;
        public string RuleVersion = "ocg-2026-10-02-v1";
        public string CatalogHash = "";
        public int ProtocolVersion = 1;
        public string RulePackageHash = "";
        public string NameCatalogHash = "";
        public string BanlistHash = "";
    }

    public sealed class CardLastKnown
    {
        public bool Negated;
        public int HostInstanceId;
        public int MaterialCount;
        public CardRef Ref;
        public string DefinitionId = "";
        public int Owner;
        public int Controller;
        public DuelZone Zone;
        public CardPosition Position;
        public int Attack;
        public int? Defense;
        public int Level;
        public int Attribute;
        public int Race;
        public int Slot;
        public bool ProperlySummoned;
        public SummonMethod SummonMethod;
        public int VisibleToMask;
        public string NameId = "";
    }

    public sealed class PendingTrigger
    {
        public string AbilityId = "";
        public CardRef Source;
        public int Player;
        public bool Mandatory;
        public bool OptionalWhen;
        public long EventId;
        public long GroupId;
        public bool Public = true;
    }

    public enum DestructionStatus { Pending, Completed }
    public sealed class DuelDestructionOperation
    {
        public int Id;
        public List<CardRef> Targets = new List<CardRef>();
        public CardRef Source;
        public int EffectPlayer;
        public MoveCause Cause;
        public List<CardRef> Destroyed = new List<CardRef>();
        public List<CardLastKnown> DestroyedBefore = new List<CardLastKnown>();
        public List<int> ReplacementSeatsProcessed = new List<int>();
        public List<CardRef> Protected = new List<CardRef>();
        public bool Completed;
        public int ResumeStep;
        public int ChainNumber;
        public string Continuation = "chain";
    }
}
