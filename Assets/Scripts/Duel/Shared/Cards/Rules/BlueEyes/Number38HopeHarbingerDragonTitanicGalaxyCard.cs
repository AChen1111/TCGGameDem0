using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class Number38HopeHarbingerDragonTitanicGalaxyCard : CardRules
    {
        public override string CardId => "63767246";
        public override bool HasSummonRecipe => true;
        public override bool MatchesSummonMaterials(CardDefinition target, IReadOnlyList<DuelCardState> materials, DuelCardCatalog catalog) =>
            materials.Count == 2 && materials.All(c => c.CurrentLevel == 8);
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("63767246.1", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("63767246.2", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("63767246.3", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("63767246.summon", CardRuleKind.SummonProcedure),
            CardRuleRequirement.Done("63767246.restrictions", CardRuleKind.Restriction));
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            yield return new TriggerProgramAbility(CardId, 3, new[] { DuelZone.Monster, DuelZone.ExtraMonster }, false, false,
                (c, fact) => DestroyedOwnXyz(c, fact), c => true, (c, link) =>
                {
                    if (link.Step >= 2) return;
                    if (link.Step == 0)
                    {
                        var choices = c.State.LastCheckpointEvents.Where(f => f.GroupId == link.TriggerEvent.GroupId && DestroyedOwnXyz(c, f))
                            .Select(f => new DecisionOption { Id = f.Id.ToString(System.Globalization.CultureInfo.InvariantCulture),
                                Value = c.Catalog.Get(f.Before.DefinitionId).Attack.ToString(System.Globalization.CultureInfo.InvariantCulture),
                                Label = c.Catalog.Get(f.Before.DefinitionId).Name }).ToArray();
                        if (choices.Length > 1) { link.Step = 1; c.OpenDecision(link, DecisionKind.ChooseMode, choices, "选择原本攻击力"); return; }
                        link.Values["harbinger-gain"] = c.Catalog.Get(link.TriggerEvent.Before.DefinitionId).Attack;
                    }
                    else link.Values["harbinger-gain"] = int.Parse(link.Answers[0], System.Globalization.CultureInfo.InvariantCulture);
                    var target = c.State.Cards.FirstOrDefault(card => card.Ref.Equals(link.Targets[0])
                        && (card.Zone == DuelZone.Monster || card.Zone == DuelZone.ExtraMonster));
                    if (target != null && c.IsAffected(target)) c.AddEffect(new DuelEffectRecord { Kind = EffectRecordKind.AddAttack,
                        Target = target.Ref, Value = link.Values["harbinger-gain"] });
                    link.Step = 2;
                }).Target(c => c.State.Cards.Where(card => card.Controller == c.Player
                    && (card.Zone == DuelZone.Monster || card.Zone == DuelZone.ExtraMonster) && DuelEngine.IsPublic(card)
                    && c.Catalog.Get(card.DefinitionId).MonsterType == RuleMonsterType.Xyz && c.CanTarget(card)));
            yield return new ProgramAbility(CardId, 2, 2, new[] { DuelZone.Monster, DuelZone.ExtraMonster },
                c => c.State.BattleStep == BattleStep.Declaration && c.State.TurnPlayer != c.Player
                    && c.State.Cards.Any(card => card.Ref.Equals(c.State.Attacker) && card.Controller != c.Player),
                (c, link) =>
                {
                    if (link.Step != 0) return;
                    if (c.Source.Ref.Equals(c.SourceRef)) c.Engine.RedirectAttack(c.Source, true);
                    link.Step = 1;
                }).Cost(1, 1, c => c.Source.Materials.Select(c.Card), (c, command, link) =>
                { var material = c.Card(command.Cards[0]); link.Costs.Add(material.Ref); c.MoveAsCost(material, DuelZone.Graveyard); });
            yield return new InstanceProgramAbility(CardId, 1, 2, new[] { DuelZone.Monster, DuelZone.ExtraMonster },
                c => c.State.Chain.Count > 0 && c.State.Chain.Last().ActivationKind == RuleCardKind.Spell
                    && (c.State.Chain.Last().ActivationZone == DuelZone.SpellTrap || c.State.Chain.Last().ActivationZone == DuelZone.Field),
                (c, link) =>
                {
                    if (link.Step != 0) return;
                    var previous = c.State.Chain.Single(item => item.Number == link.Values["harbinger-counter-link"]);
                    if (!c.Engine.TryNegateEffect(previous, c.SourceRef)) { link.Step = 1; return; }
                    var spell = c.State.Cards.FirstOrDefault(card => card.Ref.Equals(previous.Source) && DuelEngine.OnField(card));
                    if (spell != null && c.Source.Ref.Equals(c.SourceRef) && (c.Source.Zone == DuelZone.Monster || c.Source.Zone == DuelZone.ExtraMonster)
                        && c.IsAffected(spell))
                    {
                        c.Move(spell, DuelZone.Material);
                        spell.HostInstanceId = c.Source.InstanceId; c.Source.Materials.Add(spell.InstanceId);
                    }
                    link.Step = 1;
                }).Pay((c, command, link) => link.Values["harbinger-counter-link"] = c.State.Chain.Last().Number);
        }
        static bool DestroyedOwnXyz(EffectContext c, DuelEvent fact) => fact.Kind == DuelEventKind.Destroyed
            && (fact.Cause == MoveCause.Battle || fact.Cause == MoveCause.Effect) && fact.Before != null
            && fact.Before.Controller == c.Player && fact.Before.Ref.InstanceId != c.Source.InstanceId
            && (fact.Before.Position == CardPosition.FaceUpAttack || fact.Before.Position == CardPosition.FaceUpDefense)
            && c.Catalog.Get(fact.Before.DefinitionId).MonsterType == RuleMonsterType.Xyz;
    }
}
