using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class DragonmaidLorparCard : CardRules
    {
        public override string CardId => "15848542";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("15848542.1", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("15848542.2", CardRuleKind.ContinuousRule),
            CardRuleRequirement.Done("15848542.3", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("15848542.restrictions", CardRuleKind.Restriction));
        public override IEnumerable<ICardContinuousRule> CreateContinuousRules()
        {
            yield return new ContinuousProgram(CardId + ".2", (c, output) =>
            {
                if (!DuelEngine.OnField(c.Source) || !DuelEngine.IsPublic(c.Source) || c.Source.Negated) return;
                if (c.State.Cards.Any(card => card.Controller == c.Player && DuelEngine.OnField(card)
                    && c.Catalog.Get(card.DefinitionId).MonsterType == RuleMonsterType.Fusion))
                    output.Add(new DuelEffectRecord { Kind = EffectRecordKind.EffectIndestructible, Target = c.Source.Ref,
                        Source = c.Source.Ref, RequiresSource = true, Player = c.Player });
            });
        }
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            yield return new ProgramAbility(CardId, 1, 1, new[] { DuelZone.Hand }, c => Faces(c).Any(), (c, link) =>
            {
                if (link.Step != 0) return;
                c.MoveAsCost(c.Source, DuelZone.Graveyard);
                var target = c.State.Cards.FirstOrDefault(card => card.Ref.Equals(link.Targets[0]) && DuelEngine.OnField(card));
                if (target != null && c.IsAffected(target))
                    c.AddEffect(new DuelEffectRecord { Kind = EffectRecordKind.CannotActivateCard, Target = target.Ref, Player = -1, ExpiresTurn = c.State.Turn });
                link.Step = 1;
            }).Target(Faces).Once(CardId + ".1");
            yield return new TriggerProgramAbility(CardId, 3, DragonmaidFlow.Field, false, false,
                (c, fact) => DragonmaidFlow.Phase(fact, DuelPhase.Main2),
                c => LevelThree(c).Any(),
                (c, link) => DragonmaidFlow.ReturnAndSummon(c, link, LevelThree, false)).Once(CardId + ".3");
        }
        static IEnumerable<DuelCardState> Faces(EffectContext context) => context.State.Cards.Where(card =>
            DuelEngine.OnField(card) && DuelEngine.IsPublic(card) && context.CanTarget(card));
        static IEnumerable<DuelCardState> LevelThree(EffectContext context) =>
            DragonmaidFlow.Mine(context, DuelZone.Hand).Where(card =>
                DragonmaidFlow.IsMaid(context.Catalog.Get(card.DefinitionId)) && context.Catalog.Get(card.DefinitionId).Level == 3
                && context.CanSpecialSummon(card, context.Player));
    }
}
