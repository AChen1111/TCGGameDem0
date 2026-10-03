using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class TheWhiteStoneOfAncientsCard : CardRules
    {
        public override string CardId => "71039903";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("71039903.1", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("71039903.2", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("71039903.restrictions", CardRuleKind.Restriction));
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            yield return new AncientStoneEndAbility();
            yield return new ProgramAbility(CardId, 2, 1, new[] { DuelZone.Graveyard }, c => true, (c, link) =>
            {
                if (link.Step != 0) return;
                var target = c.State.Cards.FirstOrDefault(card => card.Ref.Equals(link.Targets[0]) && card.Zone == DuelZone.Graveyard);
                if (target != null) { c.Move(target, DuelZone.Hand, CardPosition.FaceDown); c.Reveal(target); }
                link.Step = 1;
            }).Target(c => c.State.Cards.Where(card => card.Owner == c.Player && card.Zone == DuelZone.Graveyard
                && c.Catalog.Get(card.DefinitionId).BelongsTo(0xdd)))
                .Pay((c, command, link) => { link.Costs.Add(c.SourceRef); c.MoveAsCost(c.Source, DuelZone.Banished); })
                .Once(CardId + ".2").Category(EffectCategories.AddFromGraveyardToHandDeckExtra);
        }
    }

    sealed class AncientStoneEndAbility : IAbilityHandler, ITriggeredAbility, IActivationSourcePolicy,
        IActivationInstanceUsageLimit, IEffectCategoryProvider
    {
        public string CardId => "71039903";
        public string AbilityId => CardId + ".1";
        public int Speed => 1;
        public bool Mandatory => false;
        public bool OptionalWhen => false;
        public string UsageKey => AbilityId;
        public int Limit => 1;
        public bool CountNegatedActivation => true;
        public EffectCategories Categories => EffectCategories.SpecialSummonFromDeck;
        public bool AllowsSource(EffectContext c) => c.Source.Zone == DuelZone.Graveyard;
        static IEnumerable<DuelCardState> Candidates(EffectContext c) => c.Deck.Where(card => c.Catalog.Get(card.DefinitionId).BelongsTo(0xdd)
            && c.CanSpecialSummon(card, c.Player));
        public bool CanActivate(EffectContext c) => AllowsSource(c) && c.State.Phase == DuelPhase.End && Candidates(c).Any();
        public bool IsTriggered(EffectContext c, DuelEvent fact) => c.State.Phase == DuelPhase.End
            && (fact.Kind == DuelEventKind.PhaseChanged || fact.Kind == DuelEventKind.Moved && fact.Card.Equals(c.SourceRef) && fact.To == DuelZone.Graveyard)
            && c.State.TurnFacts.Any(e => e.Kind == DuelEventKind.Moved && e.To == DuelZone.Graveyard && e.Card.Equals(c.SourceRef));
        public string ValidateActivation(EffectContext c, DuelCommand command) => command.Cards.Length == 0 && command.TargetId == 0 ? "" : "NO_ACTIVATION_TARGET";
        public void PayCost(EffectContext c, DuelCommand command, DuelChainLink link) { }
        public void Resolve(EffectContext c, DuelChainLink link)
        {
            if (link.Step >= 5) return;
            if (link.Step == 0)
            {
                var candidates = Candidates(c).ToArray(); link.Step = 1;
                if (candidates.Length == 0) { link.Step = 5; return; }
                c.SelectCards(link, candidates, "特殊召唤青眼怪兽"); return;
            }
            if (link.Step == 1) link.Values["ancient-blue"] = link.Selected[0];
            if (EffectSummonFlow.Resume(c, link, c.Card(link.Values["ancient-blue"]), 1) && link.Step == 4)
            { c.ShuffleDeck(); link.Step = 5; }
        }
    }
}
