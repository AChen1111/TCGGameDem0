using System.Collections.Generic;

namespace AChen.Duel.Core
{
    /// <summary>一张规则卡的全部行为入口；不持有任何对局可变状态。</summary>
    public abstract class CardRules
    {
        public abstract string CardId { get; }
        public abstract CardRuleSupport Support { get; }
        public virtual IEnumerable<IAbilityHandler> CreateAbilities() { yield break; }
        public virtual IEnumerable<ICardContinuousRule> CreateContinuousRules() { yield break; }
        public virtual IEnumerable<ICardSummonProcedure> CreateSummonProcedures() { yield break; }
        public virtual bool SuppressSummonModifiersWhenNegated => false;
        public virtual void ResolveReplacement(EffectContext context, DuelChainLink link, string program) =>
            throw new System.InvalidOperationException("Unknown replacement program for " + CardId + ": " + program);
        public virtual bool HasSummonRecipe => false;
        public virtual int MinSummonMaterials => 0;
        public virtual int MaxSummonMaterials => 0;
        public virtual DuelZone ContactMaterialDestination => DuelZone.Graveyard;
        public virtual IReadOnlyList<string> NamedFusionMaterials => System.Array.Empty<string>();
        public virtual bool MatchesSummonMaterials(CardDefinition target, IReadOnlyList<DuelCardState> materials,
            DuelCardCatalog catalog) => false;
        public virtual bool CanUseAsMaterial(CardDefinition target, DuelCardState material, DuelState state) => true;
        public virtual bool AllowsEffectSpecialSummon(DuelCardState card, int player, DuelState state) => true;
        public virtual bool AllowsSummonMethod(DuelCardState card, int player, DuelState state, SummonMethod method) =>
            method == SummonMethod.Effect ? AllowsEffectSpecialSummon(card, player, state) : true;
        public virtual bool AllowsMonsterZoneEntry(DuelCardState card, int controller, DuelState state, DuelCardCatalog catalog,
            IReadOnlyList<DuelCardState> leaving = null) => true;
    }
}
