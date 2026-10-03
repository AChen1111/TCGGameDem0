using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class BlueEyesTwinBurstDragonCard : CardRules
    {
        public override string CardId => "02129638";
        public override bool HasSummonRecipe => true;
        public override int MinSummonMaterials => 2;
        public override int MaxSummonMaterials => 2;
        public override bool MatchesSummonMaterials(CardDefinition target, IReadOnlyList<DuelCardState> materials, DuelCardCatalog catalog) =>
            materials.Count == 2 && materials.All(c => c.CurrentNameId == "89631139");
        public override bool AllowsSummonMethod(DuelCardState card, int player, DuelState state, SummonMethod method) => method == SummonMethod.Fusion || method == SummonMethod.Contact;
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("02129638.1", CardRuleKind.ContinuousRule),
            CardRuleRequirement.Done("02129638.2", CardRuleKind.ContinuousRule),
            CardRuleRequirement.Done("02129638.3", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("02129638.summon", CardRuleKind.SummonProcedure),
            CardRuleRequirement.Done("02129638.restrictions", CardRuleKind.Restriction));
        public override IEnumerable<ICardContinuousRule> CreateContinuousRules()
        {
            yield return new ContinuousProgram(CardId + ".1", (c, output) =>
            {
                if ((c.Source.Zone == DuelZone.Monster || c.Source.Zone == DuelZone.ExtraMonster) && DuelEngine.IsPublic(c.Source) && !c.Source.Negated)
                    output.Add(new DuelEffectRecord { Kind = EffectRecordKind.BattleIndestructible, Target = c.SourceRef });
            });
            yield return new ContinuousProgram(CardId + ".2", (c, output) =>
            {
                if ((c.Source.Zone == DuelZone.Monster || c.Source.Zone == DuelZone.ExtraMonster) && DuelEngine.IsPublic(c.Source) && !c.Source.Negated)
                    output.Add(new DuelEffectRecord { Kind = EffectRecordKind.ExtraMonsterAttacks, Target = c.SourceRef, Value = 1 });
            });
        }
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            yield return new BlueDamageTriggerProgram(CardId, 3, new[] { DuelZone.Monster, DuelZone.ExtraMonster }, false,
                (c, fact) => fact.Kind == DuelEventKind.BattleStepChanged && fact.Amount == (int)BattleStep.DamageEnd
                    && fact.BattleAttacker.Equals(c.SourceRef) && c.State.Cards.Any(card => card.Ref.Equals(fact.BattleTarget)
                        && (card.Zone == DuelZone.Monster || card.Zone == DuelZone.ExtraMonster) && card.Controller != c.Player),
                c => true, (c, link) =>
                {
                    if (link.Step != 0) return;
                    var opponent = c.State.Cards.FirstOrDefault(card => card.Ref.Equals(link.TriggerEvent.BattleTarget)
                        && (card.Zone == DuelZone.Monster || card.Zone == DuelZone.ExtraMonster));
                    if (opponent != null && c.IsAffected(opponent)) c.Move(opponent, DuelZone.Banished);
                    link.Step = 1;
                }, step => step == BattleStep.DamageEnd);
        }
    }
}
