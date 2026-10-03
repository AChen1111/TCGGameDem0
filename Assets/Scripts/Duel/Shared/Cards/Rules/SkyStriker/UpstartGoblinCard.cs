using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class UpstartGoblinCard : CardRules
    {
        public override string CardId => "70368879";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("70368879.1", CardRuleKind.ActivatedAbility));
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        { yield return new DrawSpellHandler(CardId, 1, 1000); }
    }
}
