using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class EEmergencyCallCard : CardRules
    {
        public override string CardId => "00213326";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("00213326.1", CardRuleKind.ActivatedAbility));
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        { yield return new DeckSelectionHandler(CardId, card => card.Kind == RuleCardKind.Monster && card.BelongsTo(0x3008), DuelZone.Hand); }
    }
}
