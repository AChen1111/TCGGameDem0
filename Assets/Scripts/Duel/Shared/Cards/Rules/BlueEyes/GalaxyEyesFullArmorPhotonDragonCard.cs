using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class GalaxyEyesFullArmorPhotonDragonCard : CardRules
    {
        public override string CardId => "39030163";
        public override bool HasSummonRecipe => true;
        public override bool MatchesSummonMaterials(CardDefinition target, IReadOnlyList<DuelCardState> materials, DuelCardCatalog catalog) =>
            materials.Count == 3 && materials.All(c => c.CurrentLevel == 8) || materials.Count == 1 && materials[0].DefinitionId != CardId && catalog.Get(materials[0].DefinitionId).MonsterType == RuleMonsterType.Xyz && catalog.Get(materials[0].DefinitionId).BelongsTo(0x107b);
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("39030163.1", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("39030163.2", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("39030163.summon", CardRuleKind.SummonProcedure),
            CardRuleRequirement.Done("39030163.restrictions", CardRuleKind.Restriction));
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            yield return new FullArmorEquipAbility();
            yield return new InstanceProgramAbility(CardId, 2, 1, new[] { DuelZone.Monster, DuelZone.ExtraMonster }, c => true, (c, link) =>
            {
                if (link.Step != 0) return;
                var target = c.State.Cards.FirstOrDefault(card => card.Ref.Equals(link.Targets[0]) && DuelEngine.OnField(card) && DuelEngine.IsPublic(card));
                if (c.DestroyMany(link, target == null ? System.Array.Empty<DuelCardState>() : new[] { target }).Completed) link.Step = 1;
            }).Target(c => c.State.Cards.Where(card => card.Controller != c.Player && DuelEngine.OnField(card)
                && DuelEngine.IsPublic(card) && c.CanTarget(card)))
                .Cost(1, 1, c => c.Source.Materials.Select(c.Card), (c, command, link) =>
                { var material = c.Card(command.Cards[0]); link.Costs.Add(material.Ref); c.MoveAsCost(material, DuelZone.Graveyard); });
        }
    }

    sealed class FullArmorEquipAbility : IAbilityHandler, IActivationSourcePolicy, IActivationInstanceUsageLimit, IActivationSelectedTargets
    {
        public string CardId => "39030163";
        public string AbilityId => CardId + ".1";
        public int Speed => 1;
        public string UsageKey => AbilityId;
        public int Limit => 1;
        public bool CountNegatedActivation => false;
        public int MinCosts => 1;
        public int MaxCosts => 2;
        public bool AllowsSource(EffectContext c) => (c.Source.Zone == DuelZone.Monster || c.Source.Zone == DuelZone.ExtraMonster) && DuelEngine.IsPublic(c.Source);
        public IEnumerable<DuelCardState> CostCandidates(EffectContext c) => c.State.Cards.Where(card => card.Zone == DuelZone.SpellTrap
            && card.EquipTarget.Equals(c.SourceRef) && c.CanTarget(card));
        public bool CanActivate(EffectContext c) => AllowsSource(c) && CostCandidates(c).Any();
        public string ValidateActivation(EffectContext c, DuelCommand command) => command.TargetId == 0 && command.Cards.Length >= 1
            && command.Cards.Length <= 2 && command.Cards.Distinct().Count() == command.Cards.Length
            && command.Cards.All(id => CostCandidates(c).Any(card => card.InstanceId == id)) ? "" : "INVALID_EQUIP_TARGETS";
        public void PayCost(EffectContext c, DuelCommand command, DuelChainLink link) => link.Targets.AddRange(command.Cards.Select(id => c.Card(id).Ref));
        public void Resolve(EffectContext c, DuelChainLink link)
        {
            if (link.Step != 0) return;
            if (c.Source.Ref.Equals(c.SourceRef) && (c.Source.Zone == DuelZone.Monster || c.Source.Zone == DuelZone.ExtraMonster))
            foreach (var equip in c.State.Cards.Where(card => link.Targets.Contains(card.Ref) && card.Zone == DuelZone.SpellTrap
                && card.EquipTarget.Equals(c.SourceRef) && c.IsAffected(card)).ToArray())
            { c.Move(equip, DuelZone.Material); equip.HostInstanceId = c.Source.InstanceId; c.Source.Materials.Add(equip.InstanceId); }
            link.Step = 1;
        }
    }
}
