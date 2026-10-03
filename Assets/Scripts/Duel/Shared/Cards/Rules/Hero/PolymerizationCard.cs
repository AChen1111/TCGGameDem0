using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class PolymerizationCard : CardRules
    {
        public override string CardId => "24094653";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("24094653.1", CardRuleKind.ActivatedAbility));
        public override IEnumerable<IAbilityHandler> CreateAbilities() { yield return new HeroFusionSpellAbility(CardId); }
    }
}
