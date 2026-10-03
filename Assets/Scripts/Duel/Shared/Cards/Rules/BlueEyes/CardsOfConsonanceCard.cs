using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class CardsOfConsonanceCard : CardRules
    {
        public override string CardId => "39701395";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("39701395.1", CardRuleKind.ActivatedAbility));
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        { yield return new DrawSpellHandler(CardId, 2, new DiscardOneCost(card => card.Kind == RuleCardKind.Monster && card.Race == 8192 && card.IsTuner && card.Attack <= 1000)); }
    }
}
