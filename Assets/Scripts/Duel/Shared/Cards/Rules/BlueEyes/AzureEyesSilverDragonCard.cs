using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class AzureEyesSilverDragonCard : CardRules
    {
        public override string CardId => "40908371";
        public override bool HasSummonRecipe => true;
        public override bool MatchesSummonMaterials(CardDefinition target, IReadOnlyList<DuelCardState> materials, DuelCardCatalog catalog) =>
            materials.Count >= 2 && materials.Count(c => catalog.Get(c.DefinitionId).IsTuner) == 1 && materials.All(c => c.CurrentLevel > 0) && materials.Sum(c => c.CurrentLevel) == target.Level && materials.Where(c => !catalog.Get(c.DefinitionId).IsTuner).All(c => c.CurrentNormal);
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("40908371.1", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("40908371.2", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("40908371.summon", CardRuleKind.SummonProcedure),
            CardRuleRequirement.Done("40908371.restrictions", CardRuleKind.Restriction));
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            yield return new InstanceTriggerProgramAbility(CardId, 2, new[] { DuelZone.Monster, DuelZone.ExtraMonster }, false, false,
                (c, fact) => fact.Kind == DuelEventKind.PhaseChanged && c.State.Phase == DuelPhase.Standby && c.State.TurnPlayer == c.Player,
                c => true, (c, link) =>
                {
                    if (link.Step >= 3) return;
                    var target = c.State.Cards.FirstOrDefault(card => card.Ref.Equals(link.Targets[0]) && card.Zone == DuelZone.Graveyard);
                    if (target == null || !c.CanSpecialSummon(target, c.Player)) { link.Step = 3; return; }
                    EffectSummonFlow.Resume(c, link, target, 0);
                }).Target(c => c.State.Cards.Where(card => card.Owner == c.Player && card.Zone == DuelZone.Graveyard
                    && card.CurrentNormal && c.CanSpecialSummon(card, c.Player))).Category(EffectCategories.SpecialSummonFromGraveyard);
            yield return new TriggerProgramAbility(CardId, 1, new[] { DuelZone.Monster, DuelZone.ExtraMonster }, true, false,
                (c, fact) => fact.Kind == DuelEventKind.Summoned && fact.Card.Equals(c.SourceRef)
                    && fact.SummonMethod != SummonMethod.Normal && fact.SummonMethod != SummonMethod.Flip,
                c => true, (c, link) =>
                {
                    if (link.Step != 0) return;
                    foreach (var dragon in c.State.Cards.Where(card => card.Controller == c.Player
                        && (card.Zone == DuelZone.Monster || card.Zone == DuelZone.ExtraMonster) && DuelEngine.IsPublic(card)
                        && card.CurrentRace == 8192 && c.IsAffected(card)).ToArray())
                    {
                        c.AddEffect(new DuelEffectRecord { Kind = EffectRecordKind.EffectIndestructible, Target = dragon.Ref, ExpiresTurn = c.State.Turn + 1 });
                        c.AddEffect(new DuelEffectRecord { Kind = EffectRecordKind.Untargetable, Target = dragon.Ref, ExpiresTurn = c.State.Turn + 1 });
                    }
                    link.Step = 1;
                });
        }
    }
}
