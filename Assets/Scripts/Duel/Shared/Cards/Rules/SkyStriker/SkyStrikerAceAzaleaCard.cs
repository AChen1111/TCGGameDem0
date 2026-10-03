using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class SkyStrikerAceAzaleaCard : CardRules
    {
        public override string CardId => "98462037";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("98462037.1", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("98462037.2", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("98462037.summon", CardRuleKind.SummonProcedure, "summon-materials"),
            CardRuleRequirement.Done("98462037.restrictions", CardRuleKind.Restriction));
        public override bool AllowsSummonMethod(DuelCardState card, int player, DuelState state, SummonMethod method) =>
            method == SummonMethod.Link && SkyFlow.OnceSpecialSummon(CardId, player, state);
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            yield return new TriggerProgramAbility(CardId, 1, SkyFlow.MonsterZones, false, false,
                (c, fact) => fact.Kind == DuelEventKind.Summoned && fact.Card.Equals(c.Source.Ref), c => true,
                (c, link) =>
                {
                    var operation = c.DestroyMany(link, c.State.Cards.Where(card => card.Ref.Equals(link.Targets[0]) && DuelEngine.OnField(card)));
                    if (!operation.Completed) return;
                    if (operation.Destroyed.Count > 0 && SkyFlow.GraveSpells(c) <= 3 && c.Source.Ref.Equals(link.Source)
                        && DuelEngine.OnField(c.Source)) c.Move(c.Source, DuelZone.Graveyard);
                }).Target(c => c.State.Cards.Where(card => DuelEngine.OnField(card) && c.CanTarget(card)));
            yield return new InstanceTriggerProgramAbility(CardId, 2, SkyFlow.MonsterZones, false, false,
                (c, fact) => fact.Kind == DuelEventKind.BattleStepChanged && fact.Amount == (int)BattleStep.DamageStart
                    && fact.BattleTarget.InstanceId != 0 && (fact.BattleAttacker.Equals(c.Source.Ref) || fact.BattleTarget.Equals(c.Source.Ref)),
                c => true, (c, link) => c.DestroyMany(link, c.State.Cards.Where(card =>
                    card.InstanceId == link.Values["battle-other"] && card.Generation == link.Values["battle-other-generation"]
                        && (card.Zone == DuelZone.Monster || card.Zone == DuelZone.ExtraMonster))))
                .Cost(1, 1, c => c.State.Cards.Where(card => card.Owner == c.Player && card.Zone == DuelZone.Graveyard
                    && c.Catalog.Get(card.DefinitionId).Kind == RuleCardKind.Spell),
                    (c, command, link) =>
                    {
                        var other = c.State.Attacker.Equals(c.Source.Ref) ? c.State.AttackTarget : c.State.Attacker;
                        link.Values["battle-other"] = other.InstanceId; link.Values["battle-other-generation"] = other.Generation;
                        var spell = c.Card(command.Cards[0]); link.Costs.Add(spell.Ref); c.MoveAsCost(spell, DuelZone.Banished);
                    });
        }
        public override bool HasSummonRecipe => true;
        public override bool MatchesSummonMaterials(CardDefinition target, IReadOnlyList<DuelCardState> materials, DuelCardCatalog catalog)
        {
            var cards = materials.Select(card => catalog.Get(card.DefinitionId)).ToArray();
            return cards.Length == 2 && materials.All(c => c.CurrentAttribute == 16 || c.CurrentAttribute == 32);
        }
    }
}
