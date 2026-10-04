using System.Collections.Generic;

namespace AChen.Duel.Core
{
    /// <summary>一张规则卡的全部行为入口；不持有任何对局可变状态。</summary>
    public abstract class CardRules
    {
        /// <summary>返回卡牌的固定编号，用于关联印刷资料和规则定义。</summary>
        public abstract string CardId { get; }

        /// <summary>返回此卡规则的支持状态，以及尚未实现的规则项。</summary>
        public abstract CardRuleSupport Support { get; }

        /// <summary>创建此卡可发动的能力；没有独立发动能力时返回空序列。</summary>
        public virtual IEnumerable<IAbilityHandler> CreateAbilities() { yield break; }

        /// <summary>创建此卡在场上持续适用的规则。</summary>
        public virtual IEnumerable<ICardContinuousRule> CreateContinuousRules() { yield break; }

        /// <summary>创建此卡专属的特殊召唤手续；手续通过公共引擎执行。</summary>
        public virtual IEnumerable<ICardSummonProcedure> CreateSummonProcedures() { yield break; }

        /// <summary>效果无效后，是否永久失去本次召唤赋予的攻击或攻击次数修正。</summary>
        public virtual bool SuppressSummonModifiersWhenNegated => false;

        /// <summary>处理此卡规则替换的连锁效果；只有声明替换程序的卡需要覆写。</summary>
        public virtual void ResolveReplacement(EffectContext context, DuelChainLink link, string program) =>
            throw new System.InvalidOperationException("Unknown replacement program for " + CardId + ": " + program);

        /// <summary>召唤应对结束后，这张卡仍在场上时调用。</summary>
        public virtual void AfterSummonConfirmed(EffectContext context) { }

        /// <summary>此卡是否使用自定义融合素材配方，而不是通用素材数量规则。</summary>
        public virtual bool HasSummonRecipe => false;

        /// <summary>此卡自定义融合配方所需素材数的下限。</summary>
        public virtual int MinSummonMaterials => 0;

        /// <summary>此卡自定义融合配方允许的素材数上限。</summary>
        public virtual int MaxSummonMaterials => 0;

        /// <summary>接触融合成功后素材送往的区域。</summary>
        public virtual DuelZone ContactMaterialDestination => DuelZone.Graveyard;

        /// <summary>此融合怪兽可以指定名称作为素材的卡牌编号。</summary>
        public virtual IReadOnlyList<string> NamedFusionMaterials => System.Array.Empty<string>();

        /// <summary>判断所选素材整体是否满足此卡的融合配方。</summary>
        /// <param name="target">尝试融合召唤的卡牌定义。</param>
        /// <param name="materials">准备使用的素材。</param>
        /// <param name="catalog">提供素材卡牌的印刷定义。</param>
        /// <returns>素材组合符合此卡配方时返回 <see langword="true"/>。</returns>
        public virtual bool MatchesSummonMaterials(CardDefinition target, IReadOnlyList<DuelCardState> materials,
            DuelCardCatalog catalog) => false;

        /// <summary>判断单张卡能否作为指定召唤的素材。</summary>
        /// <param name="target">召唤目标的卡牌定义。</param>
        /// <param name="material">候选素材。</param>
        /// <param name="state">当前决斗状态。</param>
        /// <returns>此卡允许该素材时返回 <see langword="true"/>。</returns>
        public virtual bool CanUseAsMaterial(CardDefinition target, DuelCardState material, DuelState state) => true;

        /// <summary>判断此卡能否由效果特殊召唤。</summary>
        /// <param name="card">候选卡牌。</param>
        /// <param name="player">执行召唤的玩家。</param>
        /// <param name="state">当前决斗状态。</param>
        /// <returns>没有额外限制，或规则允许召唤时返回 <see langword="true"/>。</returns>
        public virtual bool AllowsEffectSpecialSummon(DuelCardState card, int player, DuelState state) => true;

        /// <summary>判断此卡能否通过指定召唤方式召唤；效果召唤默认遵循效果特召限制。</summary>
        /// <param name="card">候选卡牌。</param>
        /// <param name="player">执行召唤的玩家。</param>
        /// <param name="state">当前决斗状态。</param>
        /// <param name="method">本次召唤方式。</param>
        /// <returns>此卡允许该召唤方式时返回 <see langword="true"/>。</returns>
        public virtual bool AllowsSummonMethod(DuelCardState card, int player, DuelState state, SummonMethod method) =>
            method == SummonMethod.Effect ? AllowsEffectSpecialSummon(card, player, state) : true;

        /// <summary>判断召唤此卡后，该玩家的怪兽区域是否仍满足此卡的规则。</summary>
        /// <param name="card">候选入场卡牌。</param>
        /// <param name="controller">召唤后控制此卡的玩家。</param>
        /// <param name="state">当前决斗状态。</param>
        /// <param name="catalog">用于读取场上卡牌定义。</param>
        /// <param name="leaving">本次操作中即将离开怪兽区域的卡牌。</param>
        /// <returns>区域条件允许入场时返回 <see langword="true"/>。</returns>
        public virtual bool AllowsMonsterZoneEntry(DuelCardState card, int controller, DuelState state, DuelCardCatalog catalog,
            IReadOnlyList<DuelCardState> leaving = null) => true;
    }
}
