using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class TerraformingCard : CardRules
    {
        public override string CardId => "73628505";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("73628505.1", CardRuleKind.ActivatedAbility));
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        { yield return new DeckSelectionHandler(CardId, card => card.Kind == RuleCardKind.Spell
            && card.SpellTrapType == RuleSpellTrapType.Field, DuelZone.Hand); }
    }
}
