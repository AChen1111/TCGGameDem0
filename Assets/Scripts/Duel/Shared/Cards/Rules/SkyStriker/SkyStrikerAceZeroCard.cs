using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class SkyStrikerAceZeroCard : CardRules
    {
        public override string CardId => "76072561";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("76072561.1", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("76072561.2", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("76072561.summon", CardRuleKind.SummonProcedure, "summon-materials"),
            CardRuleRequirement.Done("76072561.restrictions", CardRuleKind.Restriction));
        public override bool AllowsSummonMethod(DuelCardState card, int player, DuelState state, SummonMethod method) =>
            SkyFlow.OnceSpecialSummon(CardId, player, state);
        public override bool CanUseAsMaterial(CardDefinition target, DuelCardState material, DuelState state) => target.MonsterType != RuleMonsterType.Link;
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            IEnumerable<DuelCardState> Spells(EffectContext c) => c.State.Cards.Where(card => card.Owner == c.Player
                && (card.Zone == DuelZone.Deck && !c.Engine.HasEffect(EffectRecordKind.PreventDeckToHand, c.Player) || card.Zone == DuelZone.Graveyard)
                && c.Catalog.Get(card.DefinitionId).Kind == RuleCardKind.Spell && SkyFlow.Striker(c, card));
            yield return new TriggerProgramAbility(CardId, 1, SkyFlow.MonsterZones, false, false,
                (c, fact) => fact.Kind == DuelEventKind.Summoned && fact.Card.Equals(c.Source.Ref), c => Spells(c).Any(),
                (c, link) => SkyFlow.Search(c, link, Spells(c))).Once(CardId + ".shared")
                .Category(EffectCategories.AddFromDeckToHand | EffectCategories.AddFromGraveyardToHandDeckExtra);
            IEnumerable<DuelCardState[]> Pairs(EffectContext c)
            {
                var candidates = c.State.Cards.Where(card => card.Owner == c.Player
                    && (card.Zone == DuelZone.Deck || card.Zone == DuelZone.Graveyard)
                    && c.Engine.CanSpecialSummonConditions(card, c.Player)).ToArray();
                foreach (var raye in candidates.Where(card => card.CurrentNameId == "26077387"))
                foreach (var roze in candidates.Where(card => card.CurrentNameId == "37351133")) yield return new[] { raye, roze };
            }
            yield return new ProgramAbility(CardId, 2, 2, SkyFlow.MonsterZones,
                c => Pairs(c).Any() && 5 - c.State.Cards.Count(card => card.Controller == c.Player && card.Zone == DuelZone.Monster)
                        + (c.Source.Zone == DuelZone.Monster ? 1 : 0) >= 2
                    && !c.Engine.ApplicableEffects().Any(record => record.Kind == EffectRecordKind.SummonGroupLimit
                        && (record.Player < 0 || record.Player == c.Player) && record.Value < 2),
                (c, link) =>
                {
                    if (link.Step == 0)
                    {
                        var pairs = Pairs(c).Where(pair => c.Engine.CanSpecialSummonGroup(pair, c.Player)).ToArray(); link.Step = 1;
                        if (pairs.Length == 0) { link.Step = 10; return; }
                        c.SelectCards(link, pairs.SelectMany(pair => pair).Distinct(), "选择同时召唤的零衣与露世", 2, 2);
                        c.State.PendingDecision.AllowedCardGroups = pairs.Select(pair => pair.Select(card => card.Ref).ToList()).ToList(); return;
                    }
                    if (link.Step == 1)
                    {
                        var cards = link.Selected.Select(c.Card).ToArray();
                        link.Values["zero-raye"] = cards.Single(card => card.CurrentNameId == "26077387").InstanceId;
                        link.Values["zero-roze"] = cards.Single(card => card.CurrentNameId == "37351133").InstanceId;
                    }
                    if (link.Step < 4 && !EffectSummonFlow.Resume(c, link, c.Card(link.Values["zero-raye"]), 1)) return;
                    if (link.Step < 7 && !EffectSummonFlow.Resume(c, link, c.Card(link.Values["zero-roze"]), 4)) return;
                    if (link.Step == 7)
                    {
                        if (!new[] { link.Values["zero-raye"], link.Values["zero-roze"] }.All(id =>
                            c.Card(id).Zone == DuelZone.Monster && c.Card(id).Controller == c.Player)) { link.Step = 10; return; }
                        link.Step = 8;
                        c.OpenDecision(link, DecisionKind.YesNo, new[] {
                            new DecisionOption { Id = "yes", Value = "yes", Label = "破坏一张场上卡" },
                            new DecisionOption { Id = "no", Value = "no", Label = "不破坏" } }, "是否破坏一张场上卡？"); return;
                    }
                    if (link.Step == 8)
                    {
                        if (link.Answers[0] == "no") { link.Step = 10; return; }
                        link.Step = 9; c.SelectCards(link, c.State.Cards.Where(DuelEngine.OnField), "选择破坏的场上卡"); return;
                    }
                    if (link.Step == 9)
                    {
                        var operation = c.DestroyMany(link, new[] { c.Card(link.Selected[0]) });
                        if (operation.Completed) link.Step = 10;
                    }
                }).Pay((c, command, link) => { link.Costs.Add(c.Source.Ref); c.MoveAsCost(c.Source, DuelZone.Graveyard); })
                .Once(CardId + ".shared").Category(EffectCategories.SpecialSummonFromDeck | EffectCategories.SpecialSummonFromGraveyard);
        }
        public override bool HasSummonRecipe => true;
        public override bool MatchesSummonMaterials(CardDefinition target, IReadOnlyList<DuelCardState> materials, DuelCardCatalog catalog)
        {
            var cards = materials.Select(card => catalog.Get(card.DefinitionId)).ToArray();
            return cards.Length == 2 && cards.All(c => c.BelongsTo(0x1115));
        }
    }
}
