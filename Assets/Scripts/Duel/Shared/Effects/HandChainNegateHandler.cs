using System.Linq;

namespace AChen.Duel.Core
{
    /// <summary>丢弃自身并直接响应链顶指定类别；无效对象是连锁项，而非源卡实体。</summary>
    public sealed class HandChainNegateHandler : IAbilityHandler, IActivationSourcePolicy, IActivationUsageLimit, IDamageStepAbility
    {
        readonly EffectCategories m_categories;
        readonly bool m_negateActivation;
        public string CardId { get; }
        public string AbilityId => CardId + ".1";
        public int Speed => 2;
        public string UsageKey => AbilityId;
        public int Limit => 1;
        public bool CountNegatedActivation => true;
        public bool AllowsDamageStep(BattleStep step) => m_negateActivation;
        public HandChainNegateHandler(string cardId, EffectCategories categories, bool negateActivation)
        { CardId = cardId; m_categories = categories; m_negateActivation = negateActivation; }
        public bool AllowsSource(EffectContext context) => context.Source.Zone == DuelZone.Hand;
        public bool CanActivate(EffectContext context)
        {
            if (!AllowsSource(context) || context.State.Chain.Count == 0) return false;
            var previous = context.State.Chain[context.State.Chain.Count - 1];
            return (previous.Categories & m_categories) != 0;
        }
        public string ValidateActivation(EffectContext context, DuelCommand command) =>
            command.Cards.Length == 0 && command.TargetId == 0 ? "" : "NO_ACTIVATION_TARGET";
        public void PayCost(EffectContext context, DuelCommand command, DuelChainLink link)
        {
            link.Values.Add("responded-link", context.State.Chain[context.State.Chain.Count - 1].Number);
            link.Costs.Add(context.Source.Ref);
            context.MoveAsCost(context.Source, DuelZone.Graveyard);
        }
        public void Resolve(EffectContext context, DuelChainLink link)
        {
            if (link.Step != 0) return;
            var previous = context.State.Chain.First(item => item.Number == link.Values["responded-link"]);
            if (m_negateActivation) previous.ActivationNegated = true;
            else context.Engine.TryNegateEffect(previous, context.SourceRef);
            link.Step = 1;
        }
    }
}
