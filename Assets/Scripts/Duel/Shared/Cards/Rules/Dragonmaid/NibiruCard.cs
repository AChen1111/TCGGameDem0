using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class NibiruCard : CardRules
    {
        public override string CardId => "27204311";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("27204311.1", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("27204311.restrictions", CardRuleKind.Restriction));
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            yield return new ProgramAbility(CardId, 1, 2, new[] { DuelZone.Hand },
                c => (c.State.Phase == DuelPhase.Main1 || c.State.Phase == DuelPhase.Main2)
                    && c.State.TurnFacts.Count(fact => fact.Kind == DuelEventKind.Summoned && fact.Detail != "flip" && fact.Player != c.Player) >= 5
                    && (c.Engine.GetSpecialSummonDestinations(c.Source, c.Player).Count > 0 || Tributable(c).Any(card => card.Controller == c.Player)),
                (c, link) =>
                {
                    if (link.Step == 0)
                    {
                        foreach (var card in Tributable(c).ToArray()) c.Move(card, DuelZone.Graveyard, cause: MoveCause.Tribute);
                        link.Step = 1;
                    }
                    DragonmaidFlow.ResumeSummon(c, link, 1, false, SummonMethod.Effect);
                }).Pay((c, command, link) => link.Values["picked"] = c.Source.InstanceId).Once(CardId);
        }
        static IEnumerable<DuelCardState> Tributable(EffectContext context) =>
            context.State.Cards.Where(card => DuelEngine.OnField(card) && DuelEngine.IsPublic(card)
                && context.Catalog.Get(card.DefinitionId).Kind == RuleCardKind.Monster && context.IsAffected(card));
    }
}
