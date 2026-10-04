using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class SolemnJudgmentCard : CardRules
    {
        public override string CardId => "41420027";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("41420027.1", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("41420027.restrictions", CardRuleKind.Restriction));
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            yield return new ProgramAbility(CardId, 1, 3, new[] { DuelZone.SpellTrap },
                c => c.Source.SetTurn < c.State.Turn && c.State.Players[c.Player].LifePoints >= 2 && c.State.Chain.Count > 0
                    && c.State.Chain.Last().IsCardActivation
                    && (c.State.Chain.Last().ActivationKind == RuleCardKind.Spell || c.State.Chain.Last().ActivationKind == RuleCardKind.Trap),
                (c, link) =>
                {
                    if (link.Step != 0) return;
                    c.State.Chain.Single(item => item.Number == link.Values["solemn-link"]).ActivationNegated = true;
                    link.Step = 1;
                }).Pay((c, command, link) =>
                {
                    link.Values["solemn-link"] = c.State.Chain.Last().Number;
                    c.State.Players[c.Player].LifePoints /= 2;
                });
        }
    }
}
