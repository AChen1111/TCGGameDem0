using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class TheMelodyOfAwakeningDragonCard : CardRules
    {
        public override string CardId => "48800175";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("48800175.1", CardRuleKind.ActivatedAbility));
        public override IEnumerable<IAbilityHandler> CreateAbilities() { yield return new MelodyOfAwakeningDragonAbility(); }
    }

    sealed class MelodyOfAwakeningDragonAbility : IAbilityHandler, IActivationCostSelection, IEffectCategoryProvider
    {
        readonly DiscardOneCost m_cost = new DiscardOneCost(card => true);
        public string CardId => "48800175";
        public string AbilityId => CardId + ".1";
        public int Speed => 1;
        public int MinCosts => 1;
        public int MaxCosts => 1;
        public EffectCategories Categories => EffectCategories.AddFromDeckToHand;
        public IEnumerable<DuelCardState> CostCandidates(EffectContext context) => m_cost.CostCandidates(context);
        static IEnumerable<DuelCardState> Candidates(EffectContext context) => context.Deck.Where(c => {
            var card = context.Catalog.Get(c.DefinitionId);
            return card.Kind == RuleCardKind.Monster && card.Race == 8192 && card.Attack >= 3000
                && card.Defense.HasValue && card.Defense.Value <= 2500; });
        public bool CanActivate(EffectContext context) => m_cost.CanPay(context) && Candidates(context).Any()
            && !context.Engine.HasEffect(EffectRecordKind.PreventDeckToHand, context.Player);
        public string ValidateActivation(EffectContext context, DuelCommand command) =>
            command.TargetId == 0 ? m_cost.Validate(context, command) : "NO_ACTIVATION_TARGET";
        public void PayCost(EffectContext context, DuelCommand command, DuelChainLink link) => m_cost.Pay(context, command, link);
        public void Resolve(EffectContext context, DuelChainLink link)
        {
            if (link.Step == 0)
            {
                link.Step = 1;
                if (context.Engine.HasEffect(EffectRecordKind.PreventDeckToHand, context.Player)) { link.Step = 2; return; }
                var candidates = Candidates(context).ToArray();
                if (candidates.Length == 0) { link.Step = 2; return; }
                context.SelectCards(link, candidates, "选择最多两只龙族怪兽加入手牌", 1, System.Math.Min(2, candidates.Length));
                return;
            }
            if (link.Step == 1)
            {
                foreach (int id in link.Selected)
                { var card = context.Card(id); context.Move(card, DuelZone.Hand, CardPosition.FaceDown); context.Reveal(card); }
                context.ShuffleDeck(); link.Step = 2;
            }
        }
    }
}
