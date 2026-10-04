using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class BystialDruiswurmCard : CardRules
    {
        public override string CardId => "06637331";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("06637331.1", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("06637331.2", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("06637331.restrictions", CardRuleKind.Restriction));
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            yield return new ProgramAbility(CardId, 1, 2, new[] { DuelZone.Hand },
                c => GraveLights(c).Any() && (c.State.TurnPlayer == c.Player || DragonmaidFlow.FieldMonsters(c, 1 - c.Player).Any())
                    && c.Engine.GetSpecialSummonDestinations(c.Source, c.Player).Count > 0,
                (c, link) =>
                {
                    if (link.Step == 0)
                    {
                        var target = c.State.Cards.FirstOrDefault(card => card.Ref.Equals(link.Targets[0]) && card.Zone == DuelZone.Graveyard);
                        if (target != null) c.Move(target, DuelZone.Banished);
                        link.Values["picked"] = c.Source.InstanceId;
                        link.Step = 1;
                    }
                    DragonmaidFlow.ResumeSummon(c, link, 1, false, SummonMethod.Effect);
                }).Target(GraveLights).Once(CardId + ".1");
            yield return new TriggerProgramAbility(CardId, 2, new[] { DuelZone.Graveyard }, false, false,
                DragonmaidFlow.LeftFieldToGrave,
                c => c.State.Cards.Any(card => card.Controller != c.Player && DuelEngine.OnField(card) && card.SummonMethod != SummonMethod.Normal && c.CanTarget(card)),
                (c, link) =>
                {
                    if (link.Step != 0) return;
                    var target = c.State.Cards.FirstOrDefault(card => card.Ref.Equals(link.Targets[0]) && DuelEngine.OnField(card));
                    if (target != null && c.IsAffected(target)) c.Move(target, DuelZone.Graveyard);
                    link.Step = 1;
                }).Target(c => c.State.Cards.Where(card => card.Controller != c.Player && DuelEngine.OnField(card)
                    && card.SummonMethod != SummonMethod.Normal && c.CanTarget(card))).Once(CardId + ".2");
        }
        static IEnumerable<DuelCardState> GraveLights(EffectContext context) => context.State.Cards.Where(card =>
            card.Zone == DuelZone.Graveyard && context.Catalog.Get(card.DefinitionId).Kind == RuleCardKind.Monster
            && (DragonmaidFlow.Attr(card, DragonmaidFlow.Light) || DragonmaidFlow.Attr(card, DragonmaidFlow.Dark)) && context.CanTarget(card));
    }
}
