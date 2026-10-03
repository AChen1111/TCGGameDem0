using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class TradeInCard : CardRules
    {
        public override string CardId => "38120068";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("38120068.1", CardRuleKind.ActivatedAbility));
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        { yield return new DrawSpellHandler(CardId, 2, new DiscardOneCost(card => card.Kind == RuleCardKind.Monster && card.Level == 8)); }
    }
}
