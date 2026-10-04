using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class PredaplantDragostapeliaCard : CardRules
    {
        public override string CardId => "69946549";
        public override bool HasSummonRecipe => true;
        public override int MinSummonMaterials => 2;
        public override int MaxSummonMaterials => 2;
        public override bool AllowsSummonMethod(DuelCardState card, int player, DuelState state, SummonMethod method) =>
            method == SummonMethod.Fusion;
        public override bool MatchesSummonMaterials(CardDefinition target, IReadOnlyList<DuelCardState> materials, DuelCardCatalog catalog)
        {
            var fusions = materials.Where(card => catalog.Get(card.DefinitionId).MonsterType == RuleMonsterType.Fusion).ToArray();
            var dark = materials.Where(card => DragonmaidFlow.Attr(card, DragonmaidFlow.Dark)).ToArray();
            return materials.Count == 2 && fusions.Length > 0 && dark.Length > 0
                && fusions.Concat(dark).Select(card => card.InstanceId).Distinct().Count() == 2;
        }
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("69946549.1", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("69946549.2", CardRuleKind.ContinuousRule),
            CardRuleRequirement.Done("69946549.summon", CardRuleKind.SummonProcedure),
            CardRuleRequirement.Done("69946549.restrictions", CardRuleKind.Restriction));
        public override IEnumerable<ICardContinuousRule> CreateContinuousRules()
        {
            yield return new ContinuousProgram(CardId + ".2", (c, output) =>
            {
                if (!DuelEngine.OnField(c.Source) || !DuelEngine.IsPublic(c.Source) || c.Source.Negated) return;
                foreach (var card in DragonmaidFlow.FieldMonsters(c, 1 - c.Player).Where(card =>
                    card.Counters.TryGetValue("predator", out int count) && count > 0))
                    output.Add(new DuelEffectRecord { Kind = EffectRecordKind.TargetNegate, Target = card.Ref, RequiresSource = true });
            });
        }
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            yield return new ProgramAbility(CardId, 1, 2, DragonmaidFlow.Field, c => Targets(c).Any(), (c, link) =>
            {
                if (link.Step != 0) return;
                var target = c.State.Cards.FirstOrDefault(card => card.Ref.Equals(link.Targets[0]) && DuelEngine.OnField(card) && DuelEngine.IsPublic(card));
                if (target != null && c.IsAffected(target))
                {
                    target.Counters["predator"] = target.Counters.TryGetValue("predator", out int count) ? count + 1 : 1;
                    if (target.CurrentLevel >= 2)
                        c.AddEffect(new DuelEffectRecord { Kind = EffectRecordKind.SetLevel, Target = target.Ref, Value = 1 });
                }
                link.Step = 1;
            }).Target(Targets).Once(CardId + ".1");
        }
        static IEnumerable<DuelCardState> Targets(EffectContext context) =>
            DragonmaidFlow.FieldMonsters(context, 1 - context.Player).Where(context.CanTarget);
    }
}
