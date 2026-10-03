using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class SkyStrikerAirspaceAreaZeroCard : CardRules
    {
        public override string CardId => "50005218";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("50005218.0", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("50005218.1", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("50005218.2", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("50005218.restrictions", CardRuleKind.Restriction));
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            yield return new ProgramAbility(CardId, 0, 1, new[] { DuelZone.Hand, DuelZone.Field },
                c => c.Source.Zone == DuelZone.Hand || c.Source.Position == CardPosition.FaceDown, (c, link) => { });
            yield return new ProgramAbility(CardId, 1, 1, new[] { DuelZone.Field },
                c => DuelEngine.IsPublic(c.Source) && c.Deck.Count() >= 3, ResolveExcavation)
                .Target(c => c.State.Cards.Where(card => card.Controller == c.Player && card.InstanceId != c.Source.InstanceId
                    && DuelEngine.OnField(card) && c.CanTarget(card)))
                .Once(CardId + ".1").Category(EffectCategories.AddFromDeckToHand);
            yield return new TriggerProgramAbility(CardId, 2, new[] { DuelZone.Graveyard }, false, false,
                (c, fact) => fact.Kind == DuelEventKind.Moved && fact.Card.Equals(c.SourceRef)
                    && fact.To == DuelZone.Graveyard && fact.Before.Zone == DuelZone.Field && fact.Cause == MoveCause.Effect,
                c => SummonCandidates(c).Any(), ResolveSummon)
                .Once(CardId + ".2").Category(EffectCategories.SpecialSummonFromDeck);
        }

        static void ResolveExcavation(EffectContext context, DuelChainLink link)
        {
            if (link.Step == 0)
            {
                var revealed = context.Deck.Take(3).ToArray();
                link.Values["excavate-count"] = revealed.Length;
                for (int i = 0; i < revealed.Length; i++)
                {
                    link.Values["excavate." + i] = revealed[i].InstanceId;
                    revealed[i].RevealedToMask = 3; context.Reveal(revealed[i]);
                }
                var strikers = revealed.Where(card => context.Catalog.Get(card.DefinitionId).BelongsTo(0x115)).ToArray();
                link.Values["revealed-striker"] = strikers.Length == 0 ? 0 : 1;
                link.Step = 1;
                if (strikers.Length > 0 && !context.Engine.HasEffect(EffectRecordKind.PreventDeckToHand, context.Player))
                { context.SelectCards(link, strikers, "可以将翻开的闪刀卡加入手卡", 0, 1); return; }
            }
            if (link.Step == 1)
            {
                foreach (int id in link.Selected)
                { var chosen = context.Card(id); if (context.TryMove(chosen, DuelZone.Hand)) context.Reveal(chosen); }
                for (int i = 0; i < link.Values["excavate-count"]; i++)
                    context.Card(link.Values["excavate." + i]).RevealedToMask = 0;
                if (link.Values["excavate-count"] > 0) context.ShuffleDeck();
                var target = context.State.Cards.FirstOrDefault(card => card.Ref.Equals(link.Targets[0]) && DuelEngine.OnField(card));
                if (link.Values["revealed-striker"] != 0 && target != null && context.IsAffected(target))
                    context.Move(target, DuelZone.Graveyard);
                link.Step = 2;
            }
        }

        static IEnumerable<DuelCardState> SummonCandidates(EffectContext context) => context.Deck.Where(card =>
            context.Catalog.Get(card.DefinitionId).Kind == RuleCardKind.Monster
                && context.Catalog.Get(card.DefinitionId).BelongsTo(0x1115) && context.CanSpecialSummon(card, context.Player));

        static void ResolveSummon(EffectContext context, DuelChainLink link)
        {
            if (link.Step == 0)
            {
                var candidates = SummonCandidates(context).ToArray(); link.Step = 1;
                if (candidates.Length == 0) { link.Step = 5; return; }
                context.SelectCards(link, candidates, "选择卡组的闪刀姬特殊召唤"); return;
            }
            if (link.Step == 1) link.Values["summon-card"] = link.Selected[0];
            if (link.Step >= 1 && link.Step <= 3
                && EffectSummonFlow.Resume(context, link, context.Card(link.Values["summon-card"]), 1))
            { context.ShuffleDeck(); link.Step = 5; }
        }
    }
}
