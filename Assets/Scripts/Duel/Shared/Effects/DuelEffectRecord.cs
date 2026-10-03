using System.Collections.Generic;

namespace AChen.Duel.Core
{
    public enum EffectRecordKind
    {
        TargetNegate, NameNegate, SetAttack, SetDefense, AddAttack, AddDefense, SetLevel, SetAttribute, SetName,
        DrawOnOpponentSpecialSummon, PreventDeckToHand, BanishOpponentGraveyard, CannotAttack,
        BattleIndestructible, EffectIndestructible, Untargetable, Unaffected, CannotBeMaterial,
        CannotSpecialSummon, OnlySpecialSummonSet, CannotActivateName, CannotSummonName,
        ReturnControl, DelayedSummon, DelayedDestroy, SkipPhase, Piercing, ExtraAttacks,
        PreventResponses, SummonGroupLimit, SpellResponsesBlocked, EndShuffleHand,
        DestroyReplacement, CannotUseAsXyzMaterial, CannotUseAsLinkMaterial, DirectAttack, ExtraMonsterAttacks, TreatAsNormal, OnlySummonMethod, RecoverOnSpellActivated, OnlyExtraDeckRace, CannotReturnToExtra, EachMonsterOnce, CannotDirectAttack, DelayedReturn, CannotActivateCard, OnlyExtraDeckSet, BanishWhenLeavesField
    }

    /// <summary>确定性适用记录；保存原始引用和期限，不持有委托、连接或动画。</summary>
    public sealed class DuelEffectRecord
    {
        public long Id;
        public EffectRecordKind Kind;
        public CardRef Source;
        public CardRef Target;
        public int Player = -1;
        public string NameId = "";
        public int Value;
        public int ExpiresTurn;
        public DuelPhase ExpiresPhase = DuelPhase.End;
        public bool RequiresSource;
        public int Remaining;
        public int ActivationPlayer = -1;
        public List<CardRef> Cards = new List<CardRef>();
        public List<string> Names = new List<string>();
        public List<long> ProcessedEventGroups = new List<long>();
        public string SourceDefinitionId = "";
        public long ChainId;
        public int ResponseToLink;
        public long CreatedEventId;
        public int SetCode;
        public bool CalculationOnly;
        public bool RemoveIfActivationNegated;
        public bool ResetIfSourceNegated;
        public DuelZone ReturnZone = DuelZone.Monster;
        public int ReturnSlot;
        public CardPosition ReturnPosition = CardPosition.FaceUpAttack;
    }

    public interface ICardContinuousRule
    {
        string RuleId { get; }
        void Collect(EffectContext context, IList<DuelEffectRecord> output);
    }

    public interface ITriggeredAbility
    {
        bool Mandatory { get; }
        bool OptionalWhen { get; }
        bool IsTriggered(EffectContext context, DuelEvent fact);
    }

    public interface IDamageStepAbility
    {
        bool AllowsDamageStep(BattleStep step);
    }

    public interface IPhaseAbility
    {
        bool AllowsPhase(EffectContext context);
    }
}
