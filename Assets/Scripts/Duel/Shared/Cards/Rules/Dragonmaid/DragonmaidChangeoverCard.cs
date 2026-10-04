using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class DragonmaidChangeoverCard : CardRules
    {
        public override string CardId => "40110009";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("40110009.1", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("40110009.2", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("40110009.restrictions", CardRuleKind.Restriction));
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            yield return new DragonFusionSpell(CardId, card => card.MonsterType == RuleMonsterType.Fusion && card.Race == DragonmaidFlow.Dragon);
            yield return new ProgramAbility(CardId, 2, 1, new[] { DuelZone.Graveyard }, c => Maids(c).Any(), (c, link) =>
            {
                if (link.Step != 0) return;
                var target = c.State.Cards.FirstOrDefault(card => card.Ref.Equals(link.Targets[0]) && DuelEngine.OnField(card));
                if (c.TryMove(c.Source, DuelZone.Hand, CardPosition.FaceDown) && target != null && c.IsAffected(target))
                    c.Move(target, DuelZone.Hand);
                link.Step = 1;
            }).Target(Maids).Once(CardId + ".2");
        }
        static IEnumerable<DuelCardState> Maids(EffectContext context) => DragonmaidFlow.FieldMonsters(context, context.Player)
            .Where(card => DragonmaidFlow.IsMaid(context.Catalog.Get(card.DefinitionId)) && context.CanTarget(card));
    }
}
