using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class ChamberDragonmaidCard : CardRules
    {
        public override string CardId => "32600024";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("32600024.1", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("32600024.2", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("32600024.restrictions", CardRuleKind.Restriction));
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            yield return new TriggerProgramAbility(CardId, 1, DragonmaidFlow.Field, false, false, DragonmaidFlow.SummonedSelf,
                c => Spells(c).Any(),
                (c, link) => DragonmaidFlow.Pick(c, link, Spells(c), DuelZone.Hand, true))
                .Category(EffectCategories.AddFromDeckToHand).Once(CardId + ".1");
            yield return new TriggerProgramAbility(CardId, 2, DragonmaidFlow.Field, false, false,
                (c, fact) => DragonmaidFlow.Phase(fact, DuelPhase.Battle),
                c => High(c).Any(),
                (c, link) => DragonmaidFlow.ReturnAndSummon(c, link, High, false)).Once(CardId + ".2");
        }
        static IEnumerable<DuelCardState> Spells(EffectContext context) => context.Deck.Where(card =>
        {
            var printed = context.Catalog.Get(card.DefinitionId);
            return printed.BelongsTo(DragonmaidFlow.Maid) && printed.Kind != RuleCardKind.Monster;
        });
        static IEnumerable<DuelCardState> High(EffectContext context) =>
            DragonmaidFlow.Mine(context, DuelZone.Hand, DuelZone.Graveyard).Where(card =>
            {
                var printed = context.Catalog.Get(card.DefinitionId);
                return DragonmaidFlow.IsMaid(printed) && printed.Level >= 7 && context.CanSpecialSummon(card, context.Player);
            });
    }
}
