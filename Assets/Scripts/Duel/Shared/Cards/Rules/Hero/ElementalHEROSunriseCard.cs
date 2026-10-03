using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class ElementalHEROSunriseCard : CardRules
    {
        public override string CardId => "22908820";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("22908820.1", CardRuleKind.ActivatedAbility, "trigger-checkpoints"),
            CardRuleRequirement.Done("22908820.2", CardRuleKind.ContinuousRule, "continuous-effects"),
            CardRuleRequirement.Done("22908820.3", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("22908820.summon", CardRuleKind.SummonProcedure, "summon-materials"),
            CardRuleRequirement.Done("22908820.restrictions", CardRuleKind.Restriction));
        public override bool HasSummonRecipe => true;
        public override int MinSummonMaterials => 2;
        public override int MaxSummonMaterials => 2;
        public override bool MatchesSummonMaterials(CardDefinition target, IReadOnlyList<DuelCardState> materials, DuelCardCatalog catalog) =>
            materials.Count == 2 && materials.All(card => catalog.Get(card.DefinitionId).BelongsTo(0x8))
            && materials[0].CurrentAttribute != materials[1].CurrentAttribute;
        public override bool AllowsSummonMethod(DuelCardState card, int player, DuelState state, SummonMethod method) =>
            method == SummonMethod.Fusion;
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            yield return new HeroDeckTriggerAbility(CardId, 1, context => context.Deck.Where(card => card.DefinitionId == "45906428"),
                DuelZone.Hand, (context, fact) => HeroDeckTriggerAbility.SelfSummoned(context, fact)
                    && fact.SummonMethod != SummonMethod.Normal && fact.SummonMethod != SummonMethod.Flip,
                HeroDeckTriggerAbility.FaceUpMonster, EffectCategories.AddFromDeckToHand);
            yield return new TriggerProgramAbility(CardId, 3, new[] { DuelZone.Monster, DuelZone.ExtraMonster }, false, false,
                (context, fact) => fact.Kind == DuelEventKind.AttackDeclared && fact.PresentCards.Any(card => card.Controller == context.Player
                    && card.Ref.InstanceId != context.Source.InstanceId && (card.Ref.Equals(fact.BattleAttacker) || card.Ref.Equals(fact.BattleTarget))
                    && context.Catalog.Get(card.DefinitionId).BelongsTo(0x8)), context => true,
                (context, link) => {
                    if (link.Step != 0) return;
                    var victim = context.State.Cards.FirstOrDefault(card => card.Ref.Equals(link.Targets[0]) && DuelEngine.OnField(card));
                    if (victim == null) { link.Step = 1; return; }
                    var operation = context.DestroyMany(link, new[] { victim });
                    if (!operation.Completed) return;
                    link.Step = 1;
                }).Target(context => context.State.Cards.Where(card => DuelEngine.OnField(card) && context.CanTarget(card))).Once(CardId + ".3");
        }
        public override IEnumerable<ICardContinuousRule> CreateContinuousRules()
        {
            yield return new HeroContinuousRule(CardId + ".2", (context, output) => {
                var monsters = HeroContinuousRule.Monsters(context, context.Player).ToArray();
                int boost = HeroContinuousRule.AttributeCount(monsters) * 200;
                foreach (var monster in monsters) output.Add(HeroContinuousRule.Record(context, monster, EffectRecordKind.AddAttack, boost));
            });
        }
    }
}
