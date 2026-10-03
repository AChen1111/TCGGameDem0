using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class CrystalWingSynchroDragonCard : CardRules
    {
        public override string CardId => "50954680";
        public override bool HasSummonRecipe => true;
        public override bool MatchesSummonMaterials(CardDefinition target, IReadOnlyList<DuelCardState> materials, DuelCardCatalog catalog) =>
            materials.Count >= 2 && materials.Count(c => catalog.Get(c.DefinitionId).IsTuner) == 1 && materials.All(c => c.CurrentLevel > 0) && materials.Sum(c => c.CurrentLevel) == target.Level && materials.Where(c => !catalog.Get(c.DefinitionId).IsTuner).All(c => catalog.Get(c.DefinitionId).MonsterType == RuleMonsterType.Synchro);
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("50954680.1", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("50954680.2", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("50954680.summon", CardRuleKind.SummonProcedure),
            CardRuleRequirement.Done("50954680.restrictions", CardRuleKind.Restriction));
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            yield return new BlueDamageTriggerProgram(CardId, 2, new[] { DuelZone.Monster, DuelZone.ExtraMonster }, true,
                (c, fact) => fact.Kind == DuelEventKind.BattleStepChanged && fact.Amount == (int)BattleStep.Calculation
                    && (fact.BattleAttacker.Equals(c.SourceRef) || fact.BattleTarget.Equals(c.SourceRef))
                    && BattleOpponent(c, fact) != null && BattleOpponent(c, fact).CurrentLevel >= 5,
                c => true, (c, link) =>
                {
                    if (link.Step != 0) return;
                    var opponent = BattleOpponent(c, link.TriggerEvent);
                    if (opponent != null && c.Source.Ref.Equals(c.SourceRef))
                        c.AddEffect(new DuelEffectRecord { Kind = EffectRecordKind.AddAttack, Target = c.SourceRef,
                            Value = opponent.CurrentAtk, CalculationOnly = true });
                    link.Step = 1;
                }, step => step == BattleStep.Calculation);
            yield return new BlueInstanceDamageProgram(CardId, 1, 2, new[] { DuelZone.Monster, DuelZone.ExtraMonster },
                c => c.State.Chain.Count > 0 && c.State.Chain.Last().ActivationKind == RuleCardKind.Monster
                    && c.State.Chain.Last().Source.InstanceId != c.Source.InstanceId, (c, link) =>
                {
                    if (link.Step != 0) return;
                    var previous = c.State.Chain.Single(item => item.Number == link.Values["crystal-counter-link"]);
                    previous.ActivationNegated = true;
                    var monster = c.State.Cards.FirstOrDefault(card => card.Ref.Equals(previous.Source));
                    var destroyed = c.DestroyMany(link, monster == null ? System.Array.Empty<DuelCardState>() : new[] { monster });
                    if (!destroyed.Completed) return;
                    if (destroyed.DestroyedBefore.Count > 0 && c.Source.Ref.Equals(c.SourceRef) && DuelEngine.OnField(c.Source))
                        c.AddEffect(new DuelEffectRecord { Kind = EffectRecordKind.AddAttack, Target = c.SourceRef,
                            Value = c.Catalog.Get(destroyed.DestroyedBefore[0].DefinitionId).Attack, ExpiresTurn = c.State.Turn, ResetIfSourceNegated = true });
                    link.Step = 1;
                }, step => true).Pay((c, command, link) => link.Values["crystal-counter-link"] = c.State.Chain.Last().Number);
        }
        static DuelCardState BattleOpponent(EffectContext c, DuelEvent fact)
        {
            var reference = fact.BattleAttacker.Equals(c.SourceRef) ? fact.BattleTarget : fact.BattleAttacker;
            return c.State.Cards.FirstOrDefault(card => card.Ref.Equals(reference)
                && (card.Zone == DuelZone.Monster || card.Zone == DuelZone.ExtraMonster) && card.Controller != c.Player);
        }
    }
}
