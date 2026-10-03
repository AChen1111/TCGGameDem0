using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class PotOfDesiresCard : CardRules
    {
        public override string CardId => "35261759";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("35261759.1", CardRuleKind.ActivatedAbility));
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        { yield return new OncePerTurnDrawSpellHandler(CardId, 2, new BanishTopCardsCost(10)); }
    }
}
