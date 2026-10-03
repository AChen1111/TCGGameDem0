using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class ElementalHEROShadowMistCard : CardRules
    {
        public override string CardId => "50720316";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("50720316.1", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("50720316.2", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("50720316.restrictions", CardRuleKind.Restriction, "shared-usage-limit"));
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            yield return new HeroDeckTriggerAbility(CardId, 1, context => context.Deck.Where(card =>
                context.Catalog.Get(card.DefinitionId).SpellTrapType == RuleSpellTrapType.QuickPlay
                && context.Catalog.Get(card.DefinitionId).BelongsTo(0xa5)), DuelZone.Hand,
                (context, fact) => HeroDeckTriggerAbility.SelfSummoned(context, fact) && fact.SummonMethod != SummonMethod.Normal,
                HeroDeckTriggerAbility.FaceUpMonster, EffectCategories.AddFromDeckToHand, CardId);
            yield return new HeroDeckTriggerAbility(CardId, 2, context => context.Deck.Where(card =>
                card.DefinitionId != CardId && context.Catalog.Get(card.DefinitionId).Kind == RuleCardKind.Monster
                && context.Catalog.Get(card.DefinitionId).BelongsTo(0x8)), DuelZone.Hand,
                (context, fact) => fact.Kind == DuelEventKind.Moved && fact.To == DuelZone.Graveyard && fact.Card.Equals(context.Source.Ref),
                context => context.Source.Zone == DuelZone.Graveyard, EffectCategories.AddFromDeckToHand, CardId);
        }
    }
}
