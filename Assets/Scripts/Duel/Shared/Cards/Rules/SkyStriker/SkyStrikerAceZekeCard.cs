using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class SkyStrikerAceZekeCard : CardRules
    {
        public override string CardId => "75147529";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("75147529.1", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("75147529.2", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("75147529.summon", CardRuleKind.SummonProcedure, "summon-materials"),
            CardRuleRequirement.Done("75147529.restrictions", CardRuleKind.Restriction));
        public override bool AllowsSummonMethod(DuelCardState card, int player, DuelState state, SummonMethod method) =>
            method == SummonMethod.Link && SkyFlow.OnceSpecialSummon(CardId, player, state);
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            yield return new TriggerProgramAbility(CardId, 1, SkyFlow.MonsterZones, false, false,
                (c, fact) => fact.Kind == DuelEventKind.Summoned && fact.Card.Equals(c.Source.Ref) && fact.SummonMethod == SummonMethod.Link,
                c => true, (c, link) =>
                {
                    foreach (var card in c.State.Cards.Where(card => card.Ref.Equals(link.Targets[0])
                        && (card.Zone == DuelZone.Monster || card.Zone == DuelZone.ExtraMonster) && DuelEngine.IsPublic(card) && c.IsAffected(card)))
                    {
                        int expiry = c.State.Turn + (c.State.TurnPlayer == c.Player ? 1 : 0);
                        int controller = card.Controller; int slot = card.Slot;
                        var position = card.Position;
                        var controlReturn = c.State.Effects.FirstOrDefault(record => record.Kind == EffectRecordKind.ReturnControl
                            && record.Target.Equals(card.Ref) && record.ExpiresTurn <= expiry);
                        if (controlReturn != null) controller = controlReturn.Player;
                        if (c.TryMove(card, DuelZone.Banished))
                            c.AddEffect(new DuelEffectRecord { Kind = EffectRecordKind.DelayedReturn, Target = card.Ref,
                                Player = controller, ExpiresTurn = expiry, ReturnZone = DuelZone.Monster,
                                ReturnSlot = slot, ReturnPosition = position });
                    }
                }).Target(c => c.State.Cards.Where(card => (card.Zone == DuelZone.Monster || card.Zone == DuelZone.ExtraMonster)
                    && DuelEngine.IsPublic(card) && c.CanTarget(card)));
            yield return new InstanceProgramAbility(CardId, 2, 1, SkyFlow.MonsterZones, c => true,
                (c, link) =>
                {
                    if (!c.Source.Ref.Equals(link.Source) || !SkyFlow.Active(c)) return;
                    c.AddEffect(new DuelEffectRecord { Kind = EffectRecordKind.AddAttack, Target = c.Source.Ref,
                        Value = 1000, ResetIfSourceNegated = true });
                    foreach (var card in c.State.Cards.Where(card => card.Ref.Equals(link.Targets[0]) && DuelEngine.OnField(card) && c.IsAffected(card)))
                        c.Move(card, DuelZone.Graveyard);
                }).Target(c => c.State.Cards.Where(card => card.Controller == c.Player && DuelEngine.OnField(card)
                    && card.InstanceId != c.Source.InstanceId && c.CanTarget(card)));
        }
        public override bool HasSummonRecipe => true;
        public override bool MatchesSummonMaterials(CardDefinition target, IReadOnlyList<DuelCardState> materials, DuelCardCatalog catalog)
        {
            var cards = materials.Select(card => catalog.Get(card.DefinitionId)).ToArray();
            return cards.Length == 2 && cards.Any(c => c.BelongsTo(0x1115));
        }
    }
}
