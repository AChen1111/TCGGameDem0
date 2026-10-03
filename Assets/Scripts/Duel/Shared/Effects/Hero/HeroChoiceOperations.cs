using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    internal static class HeroChoiceOperations
    {
        public static bool SelectAndMove(EffectContext context, DuelChainLink link,
            IEnumerable<DuelCardState> candidates, DuelZone destination, int firstStep = 0)
        {
            if (link.Step == firstStep)
            {
                var options = candidates.Where(card => destination != DuelZone.Hand || card.Zone != DuelZone.Deck
                    || !context.Engine.HasEffect(EffectRecordKind.PreventDeckToHand, context.Player)).ToArray(); link.Step++;
                if (options.Length == 0) { link.Step++; return true; }
                context.SelectCards(link, options, destination == DuelZone.Hand ? "选择加入手卡的卡" : "选择卡片"); return false;
            }
            if (link.Step == firstStep + 1)
            {
                var selected = context.Card(link.Selected[0]); bool fromDeck = selected.Zone == DuelZone.Deck;
                context.Move(selected, destination, destination == DuelZone.Hand ? CardPosition.FaceDown : CardPosition.FaceUp);
                if (destination == DuelZone.Hand && selected.Zone == DuelZone.Hand) context.Reveal(selected);
                if (fromDeck) context.ShuffleDeck(); link.Step++;
            }
            return true;
        }
        public static IEnumerable<DuelCardState> HeroGrave(EffectContext context) => context.State.Cards.Where(card =>
            card.Owner == context.Player && card.Zone == DuelZone.Graveyard
            && context.Catalog.Get(card.DefinitionId).Kind == RuleCardKind.Monster && context.Catalog.Get(card.DefinitionId).BelongsTo(0x8));
        public static void BanishCost(EffectContext context, DuelCommand command, DuelChainLink link)
        {
            foreach (int id in command.Cards)
            { var card = context.Card(id); link.Costs.Add(card.Ref); context.MoveAsCost(card, DuelZone.Banished); }
        }
    }
}
