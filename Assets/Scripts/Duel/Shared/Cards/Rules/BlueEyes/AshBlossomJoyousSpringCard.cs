using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class AshBlossomJoyousSpringCard : CardRules
    {
        public override string CardId => "14558127";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("14558127.1", CardRuleKind.ActivatedAbility));
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        { yield return new HandChainNegateHandler(CardId, EffectCategories.AddFromDeckToHand | EffectCategories.SpecialSummonFromDeck | EffectCategories.SendDeckToGraveyard, false); }
    }
}
