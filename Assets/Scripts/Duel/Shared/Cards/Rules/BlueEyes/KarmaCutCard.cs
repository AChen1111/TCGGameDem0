using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class KarmaCutCard : CardRules
    {
        public override string CardId => "71587526";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("71587526.1", CardRuleKind.ActivatedAbility, "target-reference", "discard-cost"));
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            yield return new ProgramAbility(CardId, 1, 2, new[] { DuelZone.SpellTrap }, c => true, (c, link) =>
            {
                if (link.Step != 0) return;
                var target = c.State.Cards.FirstOrDefault(card => card.Ref.Equals(link.Targets[0])
                    && card.Controller != c.Player && (card.Zone == DuelZone.Monster || card.Zone == DuelZone.ExtraMonster)
                    && card.Position != CardPosition.FaceDownDefense);
                if (target != null && c.IsAffected(target))
                {
                    c.Move(target, DuelZone.Banished);
                    string name = c.Catalog.Get(target.DefinitionId).OriginalNameId;
                    foreach (var copy in c.State.Cards.Where(card => card.Owner != c.Player && card.Zone == DuelZone.Graveyard
                        && c.Catalog.Get(card.DefinitionId).OriginalNameId == name).ToArray())
                        if (c.IsAffected(copy)) c.Move(copy, DuelZone.Banished);
                }
                link.Step = 1;
            }).Target(c => c.State.Cards.Where(card => card.Controller != c.Player
                && (card.Zone == DuelZone.Monster || card.Zone == DuelZone.ExtraMonster)
                && card.Position != CardPosition.FaceDownDefense && c.CanTarget(card)))
                .Cost(1, 1, c => c.State.Cards.Where(card => card.Owner == c.Player && card.Zone == DuelZone.Hand),
                    (c, command, link) => { var card = c.Card(command.Cards[0]); link.Costs.Add(card.Ref); c.MoveAsCost(card, DuelZone.Graveyard); });
        }
    }
}
