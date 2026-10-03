using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class GalaxyEyesCipherDragonCard : CardRules
    {
        public override string CardId => "18963306";
        public override bool HasSummonRecipe => true;
        public override bool MatchesSummonMaterials(CardDefinition target, IReadOnlyList<DuelCardState> materials, DuelCardCatalog catalog) =>
            materials.Count == 2 && materials.All(c => c.CurrentLevel == 8);
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("18963306.1", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("18963306.summon", CardRuleKind.SummonProcedure),
            CardRuleRequirement.Done("18963306.restrictions", CardRuleKind.Restriction));
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            yield return new InstanceProgramAbility(CardId, 1, 1, new[] { DuelZone.Monster, DuelZone.ExtraMonster },
                c => FreeSlots(c).Any(), (c, link) =>
                {
                    if (link.Step >= 2) return;
                    var target = c.State.Cards.FirstOrDefault(card => card.Ref.Equals(link.Targets[0])
                        && (card.Zone == DuelZone.Monster || card.Zone == DuelZone.ExtraMonster) && DuelEngine.IsPublic(card));
                    if (target == null || !c.IsAffected(target)) { link.Step = 2; return; }
                    if (link.Step == 0)
                    {
                        var slots = FreeSlots(c).ToArray(); link.Step = 1;
                        if (slots.Length == 0 || !c.Engine.Rules.Get(target.DefinitionId).AllowsMonsterZoneEntry(target, c.Player, c.State, c.Catalog)) { link.Step = 2; return; }
                        c.OpenDecision(link, DecisionKind.ChooseZone, slots.Select(slot => new DecisionOption {
                            Id = slot.ToString(System.Globalization.CultureInfo.InvariantCulture), Value = slot.ToString(System.Globalization.CultureInfo.InvariantCulture), Label = "区域 " + (slot + 1) }), "选择取得控制权的区域");
                        return;
                    }
                    int previous = target.Controller;
                    if (c.Engine.TryMove(target, DuelZone.Monster, c.Player, int.Parse(link.Answers[0], System.Globalization.CultureInfo.InvariantCulture),
                        target.Position, MoveCause.Effect, c.SourceRef, c.Player))
                    {
                        c.AddEffect(new DuelEffectRecord { Kind = EffectRecordKind.TargetNegate, Target = target.Ref, ExpiresTurn = c.State.Turn });
                        c.AddEffect(new DuelEffectRecord { Kind = EffectRecordKind.SetAttack, Target = target.Ref, Value = 3000, ExpiresTurn = c.State.Turn });
                        c.AddEffect(new DuelEffectRecord { Kind = EffectRecordKind.SetName, Target = target.Ref, NameId = "18963306", ExpiresTurn = c.State.Turn });
                        c.AddEffect(new DuelEffectRecord { Kind = EffectRecordKind.ReturnControl, Target = target.Ref, Player = previous, ExpiresTurn = c.State.Turn });
                    }
                    link.Step = 2;
                }).Target(c => c.State.Cards.Where(card => card.Controller != c.Player
                    && (card.Zone == DuelZone.Monster || card.Zone == DuelZone.ExtraMonster) && DuelEngine.IsPublic(card) && c.CanTarget(card)
                    && c.Engine.Rules.Get(card.DefinitionId).AllowsMonsterZoneEntry(card, c.Player, c.State, c.Catalog)))
                    .Cost(1, 1, c => c.Source.Materials.Select(c.Card), (c, command, link) =>
                    {
                        var material = c.Card(command.Cards[0]); link.Costs.Add(material.Ref); c.MoveAsCost(material, DuelZone.Graveyard);
                        c.AddEffect(new DuelEffectRecord { Kind = EffectRecordKind.CannotDirectAttack, Player = c.Player,
                            Cards = new List<CardRef> { c.SourceRef }, ExpiresTurn = c.State.Turn });
                    });
        }
        static IEnumerable<int> FreeSlots(EffectContext c) => Enumerable.Range(0, 5).Where(slot => !c.State.Cards.Any(card =>
            card.Controller == c.Player && card.Zone == DuelZone.Monster && card.Slot == slot));
    }
}
