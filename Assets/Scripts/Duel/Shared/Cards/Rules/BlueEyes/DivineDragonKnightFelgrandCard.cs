using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class DivineDragonKnightFelgrandCard : CardRules
    {
        public override string CardId => "01639384";
        public override bool HasSummonRecipe => true;
        public override bool MatchesSummonMaterials(CardDefinition target, IReadOnlyList<DuelCardState> materials, DuelCardCatalog catalog) =>
            materials.Count == 2 && materials.All(c => c.CurrentLevel == 8);
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("01639384.1", CardRuleKind.ActivatedAbility, "temporary-negation", "immunity"),
            CardRuleRequirement.Done("01639384.summon", CardRuleKind.SummonProcedure, "xyz-materials"),
            CardRuleRequirement.Done("01639384.restrictions", CardRuleKind.Restriction, "instance-usage-limit"));
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            yield return new InstanceProgramAbility(CardId, 1, 2, new[] { DuelZone.Monster, DuelZone.ExtraMonster }, c => true, (c, link) =>
            {
                if (link.Step != 0) return;
                var target = c.State.Cards.FirstOrDefault(card => card.Ref.Equals(link.Targets[0]) && DuelEngine.OnField(card));
                if (target != null && c.IsAffected(target))
                {
                    c.AddEffect(new DuelEffectRecord { Kind = EffectRecordKind.TargetNegate, Target = target.Ref, ExpiresTurn = c.State.Turn });
                    c.AddEffect(new DuelEffectRecord { Kind = EffectRecordKind.Unaffected, Target = target.Ref, ExpiresTurn = c.State.Turn });
                }
                link.Step = 1;
            }).Target(c => c.State.Cards.Where(card => (card.Zone == DuelZone.Monster || card.Zone == DuelZone.ExtraMonster)
                && card.Position != CardPosition.FaceDownDefense && c.CanTarget(card)))
                .Cost(1, 1, c => c.Source.Materials.Select(c.Card), (c, command, link) =>
                { var material = c.Card(command.Cards[0]); link.Costs.Add(material.Ref); c.MoveAsCost(material, DuelZone.Graveyard); });
        }
    }
}
