using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class BlackRoseMoonlightDragonCard : CardRules
    {
        public override string CardId => "33698022";
        public override bool HasSummonRecipe => true;
        public override bool MatchesSummonMaterials(CardDefinition target, IReadOnlyList<DuelCardState> materials, DuelCardCatalog catalog) =>
            materials.Count >= 2 && materials.Count(c => catalog.Get(c.DefinitionId).IsTuner) == 1 && materials.All(c => c.CurrentLevel > 0) && materials.Sum(c => c.CurrentLevel) == target.Level;
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("33698022.1", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("33698022.summon", CardRuleKind.SummonProcedure),
            CardRuleRequirement.Done("33698022.restrictions", CardRuleKind.Restriction));
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            yield return new TriggerProgramAbility(CardId, 1, new[] { DuelZone.Monster, DuelZone.ExtraMonster }, true, false,
                (c, fact) => fact.Kind == DuelEventKind.Summoned && fact.SummonMethod != SummonMethod.Normal && fact.SummonMethod != SummonMethod.Flip
                    && (fact.Card.Equals(c.SourceRef) || fact.After != null && fact.After.Controller != c.Player && fact.After.Level >= 5),
                c => true, (c, link) =>
                {
                    if (link.Step != 0) return;
                    var target = c.State.Cards.FirstOrDefault(card => card.Ref.Equals(link.Targets[0])
                        && (card.Zone == DuelZone.Monster || card.Zone == DuelZone.ExtraMonster));
                    if (target != null && c.IsAffected(target)) c.Move(target, DuelZone.Hand, CardPosition.FaceDown);
                    link.Step = 1;
                }).Target(c => c.State.Cards.Where(card => card.Controller != c.Player
                    && (card.Zone == DuelZone.Monster || card.Zone == DuelZone.ExtraMonster) && DuelEngine.IsPublic(card)
                    && card.SummonedTurn > 0 && card.SummonMethod != SummonMethod.Normal && card.SummonMethod != SummonMethod.Flip && c.CanTarget(card)))
                    .Once(CardId + ".1");
        }
    }
}
