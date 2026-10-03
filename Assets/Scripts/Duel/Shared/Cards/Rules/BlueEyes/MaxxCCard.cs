using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class MaxxCCard : CardRules
    {
        public override string CardId => "23434538";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("23434538.1", CardRuleKind.ActivatedAbility, "lingering-draw"),
            CardRuleRequirement.Done("23434538.restrictions", CardRuleKind.Restriction, "usage-limit"));
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            yield return new ProgramAbility(CardId, 1, 2, new[] { DuelZone.Hand }, c => true, (c, link) =>
            {
                if (link.Step != 0) return;
                c.AddEffect(new DuelEffectRecord { Kind = EffectRecordKind.DrawOnOpponentSpecialSummon, Player = c.Player,
                    ExpiresTurn = c.State.Turn });
                link.Step = 1;
            }).Pay((c, command, link) => { link.Costs.Add(c.SourceRef); c.MoveAsCost(c.Source, DuelZone.Graveyard); })
                .Once(CardId + ".1");
        }
    }
}
