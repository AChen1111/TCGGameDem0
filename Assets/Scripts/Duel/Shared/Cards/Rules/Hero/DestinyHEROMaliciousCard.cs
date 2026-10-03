using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class DestinyHEROMaliciousCard : CardRules
    {
        public override string CardId => "09411399";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("09411399.1", CardRuleKind.ActivatedAbility));
        public override IEnumerable<IAbilityHandler> CreateAbilities() { yield return new MaliciousAbility(); }
    }

    sealed class MaliciousAbility : IAbilityHandler, IActivationSourcePolicy, IEffectCategoryProvider
    {
        public string CardId => "09411399";
        public string AbilityId => CardId + ".1";
        public int Speed => 1;
        public EffectCategories Categories => EffectCategories.SpecialSummonFromDeck;
        public bool AllowsSource(EffectContext context) => context.Source.Zone == DuelZone.Graveyard;
        IEnumerable<DuelCardState> Candidates(EffectContext context) => context.Deck.Where(card =>
            card.DefinitionId == CardId && context.CanSpecialSummon(card, context.Player));
        public bool CanActivate(EffectContext context) => AllowsSource(context) && Candidates(context).Any();
        public string ValidateActivation(EffectContext context, DuelCommand command) =>
            command.Cards.Length == 0 && command.TargetId == 0 ? "" : "NO_ACTIVATION_TARGET";
        public void PayCost(EffectContext context, DuelCommand command, DuelChainLink link)
        { link.Costs.Add(context.SourceRef); context.MoveAsCost(context.Source, DuelZone.Banished); }
        public void Resolve(EffectContext context, DuelChainLink link)
        {
            if (link.Step >= 5) return;
            if (link.Step == 0)
            {
                var candidates = Candidates(context).ToArray();
                link.Step = 1;
                if (candidates.Length == 0) { link.Step = 5; return; }
                context.SelectCards(link, candidates, "选择特殊召唤的魔性人");
                return;
            }
            if (link.Step == 1) link.Values["selected-card"] = link.Selected[0];
            if (EffectSummonFlow.Resume(context, link, context.Card(link.Values["selected-card"]), 1)
                && link.Step == 4) { context.ShuffleDeck(); link.Step = 5; }
        }
    }
}
