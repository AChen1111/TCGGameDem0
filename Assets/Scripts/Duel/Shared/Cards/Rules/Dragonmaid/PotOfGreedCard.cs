using System.Collections.Generic;

namespace AChen.Duel.Core
{
    public sealed class PotOfGreedCard : CardRules
    {
        public override string CardId => "55144522";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("55144522.1", CardRuleKind.ActivatedAbility));
        public override IEnumerable<IAbilityHandler> CreateAbilities() { yield return new DrawSpellHandler(CardId, 2); }
    }
}
