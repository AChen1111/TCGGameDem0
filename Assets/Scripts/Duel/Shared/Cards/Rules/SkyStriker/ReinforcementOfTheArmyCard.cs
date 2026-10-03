using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class ReinforcementOfTheArmyCard : CardRules
    {
        public override string CardId => "32807848";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("32807848.1", CardRuleKind.ActivatedAbility));
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        { yield return new DeckSelectionHandler(CardId, card => card.Kind == RuleCardKind.Monster && card.Race == 1 && card.Level > 0 && card.Level <= 4, DuelZone.Hand); }
    }
}
