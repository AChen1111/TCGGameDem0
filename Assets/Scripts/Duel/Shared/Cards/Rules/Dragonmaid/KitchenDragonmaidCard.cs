using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class KitchenDragonmaidCard : CardRules
    {
        public override string CardId => "16960120";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("16960120.1", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("16960120.2", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("16960120.restrictions", CardRuleKind.Restriction));
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            yield return new TriggerProgramAbility(CardId, 1, DragonmaidFlow.Field, false, false, DragonmaidFlow.SummonedSelf,
                c => DeckMaids(c).Any(), (c, link) =>
                {
                    if (link.Step == 0)
                    {
                        var list = DeckMaids(c).ToArray();
                        link.Step = 1;
                        if (list.Length == 0) { link.Step = 3; return; }
                        c.SelectCards(link, list, "选择加入手卡的半龙女仆");
                        return;
                    }
                    if (link.Step == 1)
                    {
                        var card = c.Card(link.Selected[0]);
                        if (c.TryMove(card, DuelZone.Hand, CardPosition.FaceDown)) { c.Reveal(card); c.ShuffleDeck(); }
                        var hand = HandMaids(c).ToArray();
                        link.Step = 2;
                        if (hand.Length == 0) { link.Step = 3; return; }
                        c.SelectCards(link, hand, "选择送去墓地的半龙女仆");
                        return;
                    }
                    if (link.Step == 2) { c.Move(c.Card(link.Selected[0]), DuelZone.Graveyard); link.Step = 3; }
                }).Category(EffectCategories.AddFromDeckToHand).Once(CardId + ".1");
            yield return new TriggerProgramAbility(CardId, 2, DragonmaidFlow.Field, false, false,
                (c, fact) => DragonmaidFlow.Phase(fact, DuelPhase.Battle),
                c => LevelEight(c).Any(),
                (c, link) => DragonmaidFlow.ReturnAndSummon(c, link, LevelEight, false)).Once(CardId + ".2");
        }
        static IEnumerable<DuelCardState> DeckMaids(EffectContext context) => context.Deck.Where(card =>
            DragonmaidFlow.IsMaid(context.Catalog.Get(card.DefinitionId)) && card.DefinitionId != "16960120");
        static IEnumerable<DuelCardState> HandMaids(EffectContext context) =>
            DragonmaidFlow.Mine(context, DuelZone.Hand).Where(card => DragonmaidFlow.IsMaid(context.Catalog.Get(card.DefinitionId)));
        static IEnumerable<DuelCardState> LevelEight(EffectContext context) =>
            DragonmaidFlow.Mine(context, DuelZone.Hand, DuelZone.Graveyard).Where(card =>
                DragonmaidFlow.IsMaid(context.Catalog.Get(card.DefinitionId)) && context.Catalog.Get(card.DefinitionId).Level == 8
                && context.CanSpecialSummon(card, context.Player));
    }
}
