using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class DragonmaidCehrmbaCard : CardRules
    {
        public override string CardId => "12163590";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("12163590.1", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("12163590.2", CardRuleKind.ContinuousRule),
            CardRuleRequirement.Done("12163590.3", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("12163590.restrictions", CardRuleKind.Restriction));
        public override IEnumerable<ICardContinuousRule> CreateContinuousRules()
        {
            yield return new ContinuousProgram(CardId + ".2", (c, output) =>
            {
                if (!DuelEngine.OnField(c.Source) || !DuelEngine.IsPublic(c.Source) || c.Source.Negated) return;
                foreach (var card in DragonmaidFlow.FieldMonsters(c, c.Player).Where(card =>
                    DragonmaidFlow.IsDragon(c.Catalog.Get(card.DefinitionId)) && c.Catalog.Get(card.DefinitionId).MonsterType == RuleMonsterType.Fusion))
                    output.Add(new DuelEffectRecord { Kind = EffectRecordKind.EffectIndestructible, Target = card.Ref,
                        Source = c.Source.Ref, RequiresSource = true, Player = card.Controller });
            });
        }
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            yield return new ProgramAbility(CardId, 1, 1, new[] { DuelZone.Hand }, c => Revived(c).Any(), (c, link) =>
            {
                if (link.Step == 0)
                {
                    c.MoveAsCost(c.Source, DuelZone.Graveyard);
                    var list = Revived(c).ToArray();
                    link.Step = 1;
                    if (list.Length == 0) { link.Step = 8; return; }
                    c.SelectCards(link, list, "选择特殊召唤的半龙女仆");
                    return;
                }
                if (link.Step == 1) { link.Values["picked"] = link.Selected[0]; link.Step = 2; }
                DragonmaidFlow.ResumeSummon(c, link, 2, false, SummonMethod.Effect);
            }).Once(CardId + ".1");
            yield return new TriggerProgramAbility(CardId, 3, DragonmaidFlow.Field, false, false,
                (c, fact) => DragonmaidFlow.Phase(fact, DuelPhase.Main2),
                c => Small(c).Any(),
                (c, link) => DragonmaidFlow.ReturnAndSummon(c, link, Small, false)).Once(CardId + ".3");
        }
        static IEnumerable<DuelCardState> Revived(EffectContext context) =>
            DragonmaidFlow.Mine(context, DuelZone.Graveyard, DuelZone.Banished).Where(card =>
                card.DefinitionId != "12163590" && DragonmaidFlow.IsMaid(context.Catalog.Get(card.DefinitionId))
                && context.CanSpecialSummon(card, context.Player));
        static IEnumerable<DuelCardState> Small(EffectContext context) =>
            DragonmaidFlow.Mine(context, DuelZone.Hand).Where(card =>
            {
                var printed = context.Catalog.Get(card.DefinitionId);
                return DragonmaidFlow.IsMaid(printed) && printed.Level > 0 && printed.Level <= 4 && context.CanSpecialSummon(card, context.Player);
            });
    }
}
