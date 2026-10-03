using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class GalaxyEyesCipherBladeDragonCard : CardRules
    {
        public override string CardId => "02530830";
        public override bool HasSummonRecipe => true;
        public override bool MatchesSummonMaterials(CardDefinition target, IReadOnlyList<DuelCardState> materials, DuelCardCatalog catalog) =>
            materials.Count == 3 && materials.All(c => c.CurrentLevel == 9) || materials.Count == 1 && catalog.Get(materials[0].DefinitionId).MonsterType == RuleMonsterType.Xyz && catalog.Get(materials[0].DefinitionId).Rank == 8 && catalog.Get(materials[0].DefinitionId).BelongsTo(0x107b);
        public override bool CanUseAsMaterial(CardDefinition target, DuelCardState material, DuelState state) => target.MonsterType != RuleMonsterType.Xyz;
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("02530830.1", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("02530830.2", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("02530830.summon", CardRuleKind.SummonProcedure),
            CardRuleRequirement.Done("02530830.restrictions", CardRuleKind.Restriction));
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            yield return new TriggerProgramAbility(CardId, 2, new[] { DuelZone.Graveyard }, false, false,
                (c, fact) => fact.Kind == DuelEventKind.Destroyed && fact.Card.Equals(c.SourceRef) && fact.Before != null
                    && fact.Before.SummonMethod == SummonMethod.Xyz && fact.To == DuelZone.Graveyard
                    && (fact.Cause == MoveCause.Effect || fact.Cause == MoveCause.Battle) && fact.EffectPlayer != c.Player,
                c => true, (c, link) =>
                {
                    if (link.Step >= 3) return;
                    var target = c.State.Cards.FirstOrDefault(card => card.Ref.Equals(link.Targets[0]) && card.Zone == DuelZone.Graveyard);
                    if (target == null || !c.CanSpecialSummon(target, c.Player)) { link.Step = 3; return; }
                    EffectSummonFlow.Resume(c, link, target, 0);
                }).Target(c => c.State.Cards.Where(card => card.Owner == c.Player && card.Zone == DuelZone.Graveyard
                    && card.CurrentNameId == "18963306" && c.CanSpecialSummon(card, c.Player))).Category(EffectCategories.SpecialSummonFromGraveyard);
            yield return new InstanceProgramAbility(CardId, 1, 1, new[] { DuelZone.Monster, DuelZone.ExtraMonster }, c => true, (c, link) =>
            {
                if (link.Step != 0) return;
                var target = c.State.Cards.FirstOrDefault(card => card.Ref.Equals(link.Targets[0]) && DuelEngine.OnField(card));
                if (c.DestroyMany(link, target == null ? System.Array.Empty<DuelCardState>() : new[] { target }).Completed) link.Step = 1;
            }).Target(c => c.State.Cards.Where(card => DuelEngine.OnField(card) && c.CanTarget(card)))
                .Cost(1, 1, c => c.Source.Materials.Select(c.Card), (c, command, link) =>
                { var material = c.Card(command.Cards[0]); link.Costs.Add(material.Ref); c.MoveAsCost(material, DuelZone.Graveyard); });
        }
    }
}
