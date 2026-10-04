using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class ParlorDragonmaidCard : CardRules
    {
        public override string CardId => "88453933";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("88453933.1", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("88453933.2", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("88453933.restrictions", CardRuleKind.Restriction));
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            yield return new TriggerProgramAbility(CardId, 1, DragonmaidFlow.Field, false, false, DragonmaidFlow.SummonedSelf,
                c => contextDeck(c).Any(),
                (c, link) => DragonmaidFlow.Pick(c, link, contextDeck(c), DuelZone.Graveyard, true))
                .Category(EffectCategories.SendDeckToGraveyard).Once(CardId + ".1");
            yield return new TriggerProgramAbility(CardId, 2, DragonmaidFlow.Field, false, false,
                (c, fact) => DragonmaidFlow.Phase(fact, DuelPhase.Battle),
                c => LevelEight(c).Any(),
                (c, link) => DragonmaidFlow.ReturnAndSummon(c, link, LevelEight, false)).Once(CardId + ".2");
        }
        static IEnumerable<DuelCardState> contextDeck(EffectContext context) => context.Deck.Where(card =>
            context.Catalog.Get(card.DefinitionId).BelongsTo(DragonmaidFlow.Maid) && card.DefinitionId != "88453933");
        static IEnumerable<DuelCardState> LevelEight(EffectContext context) =>
            DragonmaidFlow.Mine(context, DuelZone.Hand, DuelZone.Graveyard).Where(card =>
                DragonmaidFlow.IsMaid(context.Catalog.Get(card.DefinitionId)) && context.Catalog.Get(card.DefinitionId).Level == 8
                && context.CanSpecialSummon(card, context.Player));
    }
}
