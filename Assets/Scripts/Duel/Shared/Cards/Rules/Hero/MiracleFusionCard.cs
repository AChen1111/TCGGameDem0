using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class MiracleFusionCard : CardRules
    {
        public override string CardId => "45906428";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("45906428.1", CardRuleKind.ActivatedAbility));
        public override IEnumerable<IAbilityHandler> CreateAbilities() { yield return new HeroFusionSpellAbility(CardId, true); }
    }
}
