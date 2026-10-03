using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class FoolishBurialCard : CardRules
    {
        public override string CardId => "81439173";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("81439173.1", CardRuleKind.ActivatedAbility));
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        { yield return new DeckSelectionHandler(CardId, card => card.Kind == RuleCardKind.Monster, DuelZone.Graveyard); }
    }
}
