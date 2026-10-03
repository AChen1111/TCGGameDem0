using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class ElementalHEROShiningNeosWingmanCard : CardRules
    {
        public override string CardId => "56733747";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("56733747.1", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("56733747.2", CardRuleKind.ContinuousRule),
            CardRuleRequirement.Done("56733747.3", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("56733747.summon", CardRuleKind.SummonProcedure, "summon-materials"),
            CardRuleRequirement.Done("56733747.restrictions", CardRuleKind.Restriction));
        public override bool HasSummonRecipe => true;
        public override IReadOnlyList<string> NamedFusionMaterials => new[] { "89943723" };
        public override int MinSummonMaterials => 2;
        public override int MaxSummonMaterials => 2;
        public override bool MatchesSummonMaterials(CardDefinition target, IReadOnlyList<DuelCardState> materials, DuelCardCatalog catalog) =>
            materials.Count == 2 && materials.Any(card => card.CurrentNameId == "89943723") && materials.Any(card => catalog.Get(card.DefinitionId).MonsterType == RuleMonsterType.Fusion && catalog.Get(card.DefinitionId).BelongsTo(0x184));
        public override bool AllowsSummonMethod(DuelCardState card, int player, DuelState state, SummonMethod method) =>
            method == SummonMethod.Fusion;
        public override IEnumerable<ICardContinuousRule> CreateContinuousRules()
        {
            yield return new HeroContinuousRule(CardId + ".2", (context, output) => {
                int boost = context.State.Cards.Count(card => card.Owner == context.Player && card.Zone == DuelZone.Graveyard
                    && context.Catalog.Get(card.DefinitionId).Kind == RuleCardKind.Monster) * 300;
                output.Add(HeroContinuousRule.Record(context, context.Source, EffectRecordKind.AddAttack, boost));
                output.Add(HeroContinuousRule.Record(context, context.Source, EffectRecordKind.EffectIndestructible));
            });
        }
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            yield return new TriggerProgramAbility(CardId, 1, new[] { DuelZone.Monster, DuelZone.ExtraMonster }, false, false,
                (context, fact) => HeroDeckTriggerAbility.SelfSummoned(context, fact) && fact.SummonMethod != SummonMethod.Normal,
                context => OpponentField(context).Any(), (context, link) => {
                    if (link.Step == 0)
                    {
                        var candidates = OpponentField(context).ToArray();
                        int maximum = System.Math.Min(candidates.Length, HeroContinuousRule.AttributeCount(
                            HeroContinuousRule.Monsters(context, 0).Concat(HeroContinuousRule.Monsters(context, 1)))); link.Step = 1;
                        if (maximum == 0) { link.Step = 2; return; }
                        context.SelectCards(link, candidates, "选择破坏的对方卡", 1, maximum); return;
                    }
                    if (link.Step == 1)
                    {
                        if (!link.Values.ContainsKey("destroy-count"))
                        { link.Values["destroy-count"] = link.Selected.Count; for (int index = 0; index < link.Selected.Count; index++) link.Values["destroy-" + index] = link.Selected[index]; }
                        var operation = context.DestroyMany(link, Enumerable.Range(0, link.Values["destroy-count"]).Select(index => context.Card(link.Values["destroy-" + index])));
                        if (!operation.Completed) return;
                        link.Step = 2;
                    }
                }).Once(CardId + ".1");
            yield return new TriggerProgramAbility(CardId, 3,
                new[] { DuelZone.Monster, DuelZone.ExtraMonster, DuelZone.Graveyard, DuelZone.Banished }, true, false,
                (context, fact) => fact.Kind == DuelEventKind.Destroyed && fact.Cause == MoveCause.Battle
                    && fact.Card.InstanceId != context.Source.InstanceId
                    && (fact.BattleAttacker.InstanceId == context.Source.InstanceId && fact.BattleTarget.InstanceId == fact.Card.InstanceId
                        || fact.BattleTarget.InstanceId == context.Source.InstanceId && fact.BattleAttacker.InstanceId == fact.Card.InstanceId),
                context => true, (context, link) => {
                    if (link.Step != 0) return;
                    context.Engine.Damage(1 - context.Player, System.Math.Max(0, context.Catalog.Get(link.TriggerEvent.DefinitionId).Attack));
                    link.Step = 1;
                });
        }
        static IEnumerable<DuelCardState> OpponentField(EffectContext context) => context.State.Cards.Where(card =>
            card.Controller == 1 - context.Player && DuelEngine.OnField(card));
    }
}
