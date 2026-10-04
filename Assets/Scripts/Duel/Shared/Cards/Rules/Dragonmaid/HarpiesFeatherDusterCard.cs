using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class HarpiesFeatherDusterCard : CardRules
    {
        public override string CardId => "18144507";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("18144507.1", CardRuleKind.ActivatedAbility));
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            yield return new ProgramAbility(CardId, 1, 1, new[] { DuelZone.Hand },
                c => Backrow(c).Any(), (c, link) =>
                {
                    var operation = c.DestroyMany(link, Backrow(c));
                    if (operation.Completed) link.Step = 1;
                });
        }
        static IEnumerable<DuelCardState> Backrow(EffectContext context) => context.State.Cards.Where(card =>
            card.Controller != context.Player && (card.Zone == DuelZone.SpellTrap || card.Zone == DuelZone.Field) && context.IsAffected(card));
    }
}
