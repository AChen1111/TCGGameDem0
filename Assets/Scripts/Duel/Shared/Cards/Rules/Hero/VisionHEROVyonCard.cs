using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class VisionHEROVyonCard : CardRules
    {
        public override string CardId => "27780618";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("27780618.1", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("27780618.2", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("27780618.restrictions", CardRuleKind.Restriction));
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            yield return new HeroDeckTriggerAbility(CardId, 1,
                context => context.Deck.Where(card => context.Catalog.Get(card.DefinitionId).Kind == RuleCardKind.Monster
                    && context.Catalog.Get(card.DefinitionId).BelongsTo(0x8)), DuelZone.Graveyard,
                HeroDeckTriggerAbility.SelfSummoned, HeroDeckTriggerAbility.FaceUpMonster, EffectCategories.SendDeckToGraveyard);
            yield return new InstanceProgramAbility(CardId, 2, 1, new[] { DuelZone.Monster, DuelZone.ExtraMonster },
                context => !context.Engine.HasEffect(EffectRecordKind.PreventDeckToHand, context.Player)
                    && context.Deck.Any(card => card.DefinitionId == "24094653"),
                (context, link) => HeroChoiceOperations.SelectAndMove(context, link,
                    context.Deck.Where(card => card.DefinitionId == "24094653"), DuelZone.Hand))
                .Cost(1, 1, HeroChoiceOperations.HeroGrave, HeroChoiceOperations.BanishCost)
                .Category(EffectCategories.AddFromDeckToHand);
        }
    }
}
