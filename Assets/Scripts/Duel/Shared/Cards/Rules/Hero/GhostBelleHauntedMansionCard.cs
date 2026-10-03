using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class GhostBelleHauntedMansionCard : CardRules
    {
        public override string CardId => "73642296";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("73642296.1", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("73642296.restrictions", CardRuleKind.Restriction, "usage-limit"));
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            yield return new HandChainNegateHandler(CardId, EffectCategories.AddFromGraveyardToHandDeckExtra
                | EffectCategories.SpecialSummonFromGraveyard | EffectCategories.BanishFromGraveyard, true);
        }
    }
}
