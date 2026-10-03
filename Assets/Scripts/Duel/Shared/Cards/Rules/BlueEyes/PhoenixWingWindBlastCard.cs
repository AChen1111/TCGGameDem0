using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class PhoenixWingWindBlastCard : CardRules
    {
        public override string CardId => "63356631";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("63356631.1", CardRuleKind.ActivatedAbility, "target-reference", "discard-cost"));
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            yield return new ProgramAbility(CardId, 1, 2, new[] { DuelZone.SpellTrap }, c => true, (c, link) =>
            {
                if (link.Step != 0) return;
                var target = c.State.Cards.FirstOrDefault(card => card.Ref.Equals(link.Targets[0])
                    && card.Controller != c.Player && DuelEngine.OnField(card));
                if (target != null && c.IsAffected(target))
                {
                    var definition = c.Catalog.Get(target.DefinitionId);
                    c.Move(target, definition.IsExtra ? DuelZone.ExtraDeck : DuelZone.Deck, CardPosition.FaceDown);
                    if (!definition.IsExtra)
                    { c.State.Players[target.Owner].Deck.Remove(target.InstanceId); c.State.Players[target.Owner].Deck.Insert(0, target.InstanceId); }
                }
                link.Step = 1;
            }).Target(c => c.State.Cards.Where(card => card.Controller != c.Player && DuelEngine.OnField(card) && c.CanTarget(card)))
                .Cost(1, 1, c => c.State.Cards.Where(card => card.Owner == c.Player && card.Zone == DuelZone.Hand),
                    (c, command, link) => { var card = c.Card(command.Cards[0]); link.Costs.Add(card.Ref); c.MoveAsCost(card, DuelZone.Graveyard); });
        }
    }
}
