using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class XtraHEROInfernalDevicerCard : CardRules
    {
        public override string CardId => "19324993";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("19324993.1", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("19324993.2", CardRuleKind.ContinuousRule, "linked-monsters"),
            CardRuleRequirement.Done("19324993.summon", CardRuleKind.SummonProcedure, "summon-materials"),
            CardRuleRequirement.Done("19324993.restrictions", CardRuleKind.Restriction));
        public override bool HasSummonRecipe => true;
        public override bool MatchesSummonMaterials(CardDefinition target, IReadOnlyList<DuelCardState> materials, DuelCardCatalog catalog)
        {
            var cards = materials.Select(card => catalog.Get(card.DefinitionId)).ToArray();
            return cards.Length == 2 && cards.All(c => c.BelongsTo(0x8));
        }
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            yield return new TriggerProgramAbility(CardId, 1, new[] { DuelZone.Monster, DuelZone.ExtraMonster }, false, false,
                (context, fact) => HeroDeckTriggerAbility.SelfSummoned(context, fact) && fact.SummonMethod == SummonMethod.Link,
                context => !context.Engine.HasEffect(EffectRecordKind.PreventDeckToHand, context.Player) && FusionChoices(context).Any()
                    && !context.State.TurnFacts.Any(fact => fact.Kind == DuelEventKind.Summoned && fact.Player == context.Player
                        && fact.SummonMethod != SummonMethod.Normal && fact.SummonMethod != SummonMethod.Flip
                        && !context.Catalog.Get(fact.DefinitionId).BelongsTo(0x8)), Resolve)
                .Pay((context, command, link) => context.AddEffect(new DuelEffectRecord {
                    Kind = EffectRecordKind.OnlySpecialSummonSet, Player = context.Player, Value = 0x8, ExpiresTurn = context.State.Turn }))
                .Once(CardId + ".1").Category(EffectCategories.AddFromDeckToHand);
        }
        public override IEnumerable<ICardContinuousRule> CreateContinuousRules()
        {
            yield return new HeroContinuousRule(CardId + ".2", (context, output) => {
                foreach (var target in context.Engine.LinkedMonsters(context.Source).Where(card => card.CurrentRace == 8))
                {
                    output.Add(HeroContinuousRule.Record(context, target, EffectRecordKind.AddAttack, target.CurrentLevel * 100));
                    output.Add(HeroContinuousRule.Record(context, target, EffectRecordKind.AddDefense, target.CurrentLevel * 100));
                }
            });
        }
        static IEnumerable<DuelCardState> FusionChoices(EffectContext context) => context.State.Cards.Where(card =>
            card.Owner == context.Player && card.Zone == DuelZone.ExtraDeck && context.Catalog.Get(card.DefinitionId).BelongsTo(0x8)
            && context.Engine.Rules.Get(card.DefinitionId).NamedFusionMaterials.Count > 0 && NamedMaterials(context, card).Any());
        static IEnumerable<DuelCardState> NamedMaterials(EffectContext context, DuelCardState fusion) => context.Deck.Where(card =>
            context.Engine.Rules.Get(fusion.DefinitionId).NamedFusionMaterials.Contains(context.Catalog.Get(card.DefinitionId).OriginalNameId));
        static void Resolve(EffectContext context, DuelChainLink link)
        {
            if (link.Step == 0)
            {
                var choices = FusionChoices(context).ToArray(); link.Step = 1;
                if (choices.Length == 0) { link.Step = 3; return; }
                context.SelectCards(link, choices, "展示英雄融合怪兽"); return;
            }
            if (link.Step == 1)
            {
                var fusion = context.Card(link.Selected[0]); context.Reveal(fusion);
                var candidates = NamedMaterials(context, fusion).ToArray(); link.Step = 2;
                if (candidates.Length == 0) { link.Step = 3; return; }
                context.SelectCards(link, candidates, "选择不同卡名的记载素材", 1, System.Math.Min(2, candidates.Select(card =>
                    context.Catalog.Get(card.DefinitionId).OriginalNameId).Distinct().Count()));
                var groups = candidates.Select(card => new List<CardRef> { card.Ref }).ToList();
                for (int first = 0; first < candidates.Length; first++)
                    for (int second = first + 1; second < candidates.Length; second++)
                        if (context.Catalog.Get(candidates[first].DefinitionId).OriginalNameId != context.Catalog.Get(candidates[second].DefinitionId).OriginalNameId)
                            groups.Add(new List<CardRef> { candidates[first].Ref, candidates[second].Ref });
                context.State.PendingDecision.AllowedCardGroups = groups; return;
            }
            if (link.Step == 2)
            {
                foreach (int id in link.Selected) { var card = context.Card(id); context.Move(card, DuelZone.Hand); context.Reveal(card); }
                context.ShuffleDeck(); link.Step = 3;
            }
        }
    }
}
