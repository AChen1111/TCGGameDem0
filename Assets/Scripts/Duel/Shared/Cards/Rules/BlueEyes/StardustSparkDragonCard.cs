using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class StardustSparkDragonCard : CardRules
    {
        public override string CardId => "83994433";
        public override bool HasSummonRecipe => true;
        public override bool MatchesSummonMaterials(CardDefinition target, IReadOnlyList<DuelCardState> materials, DuelCardCatalog catalog) =>
            materials.Count >= 2 && materials.Count(c => catalog.Get(c.DefinitionId).IsTuner) == 1 && materials.All(c => c.CurrentLevel > 0) && materials.Sum(c => c.CurrentLevel) == target.Level;
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("83994433.1", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("83994433.summon", CardRuleKind.SummonProcedure),
            CardRuleRequirement.Done("83994433.restrictions", CardRuleKind.Restriction));
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            yield return new InstanceProgramAbility(CardId, 1, 2, new[] { DuelZone.Monster, DuelZone.ExtraMonster }, c => true, (c, link) =>
            {
                if (link.Step != 0) return;
                var target = c.State.Cards.FirstOrDefault(card => card.Ref.Equals(link.Targets[0]) && DuelEngine.OnField(card));
                if (target != null && DuelEngine.IsPublic(target) && c.IsAffected(target))
                    c.AddEffect(new DuelEffectRecord { Kind = EffectRecordKind.DestroyReplacement, Target = target.Ref,
                        Remaining = 1, ExpiresTurn = c.State.Turn });
                link.Step = 1;
            }).Target(c => c.State.Cards.Where(card => card.Controller == c.Player && DuelEngine.OnField(card)
                && DuelEngine.IsPublic(card) && c.CanTarget(card)));
        }
    }
}
