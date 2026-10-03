using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class TheWhiteStoneOfLegendCard : CardRules
    {
        public override string CardId => "79814787";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("79814787.1", CardRuleKind.ActivatedAbility, "trigger-checkpoint"));
        public override IEnumerable<IAbilityHandler> CreateAbilities() { yield return new WhiteStoneLegendAbility(); }
    }

    sealed class WhiteStoneLegendAbility : IAbilityHandler, ITriggeredAbility, IActivationSourcePolicy
    {
        public string CardId => "79814787";
        public string AbilityId => CardId + ".1";
        public int Speed => 1;
        public bool Mandatory => true;
        public bool OptionalWhen => false;
        public bool AllowsSource(EffectContext context) => context.Source.Zone == DuelZone.Graveyard;
        public bool CanActivate(EffectContext context) => AllowsSource(context);
        public bool IsTriggered(EffectContext context, DuelEvent fact) => fact.Kind == DuelEventKind.Moved
            && fact.Card.InstanceId == context.Source.InstanceId && fact.To == DuelZone.Graveyard;
        public string ValidateActivation(EffectContext context, DuelCommand command) => "";
        public void PayCost(EffectContext context, DuelCommand command, DuelChainLink link) { }
        public void Resolve(EffectContext context, DuelChainLink link)
        {
            if (link.Step == 0)
            {
                var candidates = context.Deck.Where(c => context.Catalog.Get(c.DefinitionId).OriginalNameId == "89631139").ToArray();
                link.Step = 1;
                if (candidates.Length > 0 && !context.Engine.HasEffect(EffectRecordKind.PreventDeckToHand, context.Player))
                    context.SelectCards(link, candidates, "加入青眼白龙");
                return;
            }
            if (link.Step != 1 || link.Selected.Count == 0) return;
            var card = context.Card(link.Selected[0]);
            if (context.TryMove(card, DuelZone.Hand)) { context.Reveal(card); context.ShuffleDeck(); }
            link.Step = 2;
        }
    }
}
