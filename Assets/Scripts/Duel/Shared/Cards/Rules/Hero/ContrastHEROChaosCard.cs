using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class ContrastHEROChaosCard : CardRules
    {
        public override string CardId => "23204029";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("23204029.1", CardRuleKind.ContinuousRule, "continuous-effects"),
            CardRuleRequirement.Done("23204029.2", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("23204029.summon", CardRuleKind.SummonProcedure, "summon-materials"),
            CardRuleRequirement.Done("23204029.restrictions", CardRuleKind.Restriction));
        public override bool HasSummonRecipe => true;
        public override int MinSummonMaterials => 2;
        public override int MaxSummonMaterials => 2;
        public override bool MatchesSummonMaterials(CardDefinition target, IReadOnlyList<DuelCardState> materials, DuelCardCatalog catalog) =>
            materials.Count == 2 && materials.All(card => catalog.Get(card.DefinitionId).BelongsTo(0xa008));
        public override bool AllowsSummonMethod(DuelCardState card, int player, DuelState state, SummonMethod method) =>
            method == SummonMethod.Fusion;
        public override IEnumerable<ICardContinuousRule> CreateContinuousRules()
        {
            yield return new HeroContinuousRule(CardId + ".1", (context, output) =>
                output.Add(HeroContinuousRule.Record(context, context.Source, EffectRecordKind.SetAttribute, 48)));
        }
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            yield return new InstanceProgramAbility(CardId, 2, 2, new[] { DuelZone.Monster, DuelZone.ExtraMonster },
                context => true, (context, link) => {
                    if (link.Step != 0) return; link.Step = 1;
                    var target = context.State.Cards.FirstOrDefault(card => card.Ref.Equals(link.Targets[0])
                        && DuelEngine.OnField(card) && DuelEngine.IsPublic(card));
                    if (target != null && context.IsAffected(target)) context.AddEffect(new DuelEffectRecord {
                        Kind = EffectRecordKind.TargetNegate, Target = target.Ref, ExpiresTurn = context.State.Turn });
                }).Target(context => context.State.Cards.Where(card => DuelEngine.OnField(card) && DuelEngine.IsPublic(card) && context.CanTarget(card)));
        }
    }
}
