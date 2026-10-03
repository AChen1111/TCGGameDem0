using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class CombinedManeuverEngageZeroCard : CardRules
    {
        public override string CardId => "17217034";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("17217034.1", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("17217034.2", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("17217034.summon", CardRuleKind.SummonProcedure, "summon-materials"),
            CardRuleRequirement.Done("17217034.restrictions", CardRuleKind.Restriction));
        public override bool AllowsSummonMethod(DuelCardState card, int player, DuelState state, SummonMethod method) =>
            SkyFlow.OnceSpecialSummon(CardId, player, state);
        public override bool CanUseAsMaterial(CardDefinition target, DuelCardState material, DuelState state) =>
            target.MonsterType != RuleMonsterType.Link;
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            yield return new TriggerProgramAbility(CardId, 1, SkyFlow.MonsterZones, false, false,
                (c, fact) => fact.Kind == DuelEventKind.Summoned && fact.Card.Equals(c.Source.Ref), c => true,
                (c, link) =>
                {
                    foreach (var card in c.State.Cards.Where(card => card.Ref.Equals(link.Targets[0])
                        && (card.Zone == DuelZone.Monster || card.Zone == DuelZone.ExtraMonster) && c.IsAffected(card)))
                        c.AddEffect(new DuelEffectRecord { Kind = EffectRecordKind.TargetNegate, Target = card.Ref, ExpiresTurn = c.State.Turn });
                }).Target(c => c.State.Cards.Where(card => (card.Zone == DuelZone.Monster || card.Zone == DuelZone.ExtraMonster)
                    && DuelEngine.IsPublic(card) && card.CurrentAtk >= 2500 && c.CanTarget(card)));
            yield return new TriggerProgramAbility(CardId, 2, SkyFlow.MonsterZones, false, false,
                (c, fact) => fact.Kind == DuelEventKind.BattleStepChanged && fact.Amount == (int)BattleStep.DamageStart
                    && fact.BattleAttacker.Equals(c.Source.Ref) && fact.BattleTarget.InstanceId != 0,
                c => new[] { "26077387", "37351133" }.All(name => c.State.Cards.Any(card => card.Owner == c.Player
                    && card.Zone == DuelZone.Graveyard && card.CurrentNameId == name)),
                (c, link) => c.DestroyMany(link, c.State.Cards.Where(card => card.Controller == 1 - c.Player
                    && (card.Zone == DuelZone.Monster || card.Zone == DuelZone.ExtraMonster))));
        }
        public override bool HasSummonRecipe => true;
        public override bool MatchesSummonMaterials(CardDefinition target, IReadOnlyList<DuelCardState> materials, DuelCardCatalog catalog)
        {
            var cards = materials.Select(card => catalog.Get(card.DefinitionId)).ToArray();
            return cards.Length == 2 && materials.All(c => c.CurrentAttribute == 16 || c.CurrentAttribute == 32);
        }
    }
}
