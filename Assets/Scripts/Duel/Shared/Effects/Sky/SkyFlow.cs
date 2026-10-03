using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    internal static class SkyFlow
    {
        internal static bool MainEmpty(EffectContext context) => !context.State.Cards.Any(c =>
            c.Controller == context.Player && c.Zone == DuelZone.Monster);
        internal static int GraveSpells(EffectContext context) => context.State.Cards.Count(c =>
            c.Owner == context.Player && c.Zone == DuelZone.Graveyard && context.Catalog.Get(c.DefinitionId).Kind == RuleCardKind.Spell);
        internal static bool Active(EffectContext context) => DuelEngine.OnField(context.Source)
            && DuelEngine.IsPublic(context.Source) && !context.Source.Negated;
        internal static DuelZone[] MonsterZones => new[] { DuelZone.Monster, DuelZone.ExtraMonster };
        internal static DuelZone[] SpellZones => new[] { DuelZone.Hand, DuelZone.SpellTrap };
        internal static bool OnceSpecialSummon(string cardId, int player, DuelState state) => !state.TurnFacts.Any(f =>
            f.Kind == DuelEventKind.Summoned && f.Player == player && f.DefinitionId == cardId
                && f.SummonMethod != SummonMethod.Normal && f.SummonMethod != SummonMethod.Flip);
        internal static bool Striker(EffectContext context, DuelCardState card) => context.Catalog.Get(card.DefinitionId).BelongsTo(0x115);
        internal static bool Search(EffectContext context, DuelChainLink link, IEnumerable<DuelCardState> candidates)
        {
            if (link.Step == 0)
            {
                link.Values["search-succeeded"] = 0;
                var cards = candidates.ToArray(); link.Step = 1;
                if (cards.Any(card => card.Zone == DuelZone.Deck) && context.Engine.HasEffect(EffectRecordKind.PreventDeckToHand, context.Player))
                    cards = cards.Where(card => card.Zone != DuelZone.Deck).ToArray();
                if (cards.Length == 0) { link.Step = 2; return true; }
                context.SelectCards(link, cards, "选择加入手卡的卡"); return false;
            }
            if (link.Step == 1)
            {
                var card = context.Card(link.Selected[0]); bool deck = card.Zone == DuelZone.Deck;
                if (context.TryMove(card, DuelZone.Hand))
                { context.Reveal(card); link.Values["search-succeeded"] = 1; }
                if (deck) context.ShuffleDeck(); link.Step = 2;
            }
            return true;
        }
        internal static void OptionalDraw(EffectContext context, DuelChainLink link, int step)
        {
            if (link.Step == step)
            {
                link.Step++;
                context.OpenDecision(link, DecisionKind.YesNo, new[] {
                    new DecisionOption { Id = "yes", Value = "yes", Label = "抽一张卡" },
                    new DecisionOption { Id = "no", Value = "no", Label = "不抽卡" } }, "是否抽一张卡？");
                return;
            }
            if (link.Step == step + 1)
            { if (link.Answers[0] == "yes") context.Engine.Draw(context.Player, 1); link.Step++; }
        }
    }
}
