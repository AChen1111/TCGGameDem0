using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class DrawbreadCard : CardRules
    {
        public override string CardId => "83838727";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("83838727.1", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("83838727.restrictions", CardRuleKind.Restriction));
        public override IEnumerable<IAbilityHandler> CreateAbilities() { yield return new DrawbreadAbility(); }
    }

    sealed class DrawbreadAbility : IAbilityHandler, IActivationUsageLimit, IEffectCategoryProvider
    {
        public string CardId => "83838727";
        public string AbilityId => CardId + ".1";
        public int Speed => 1;
        public string UsageKey => CardId;
        public int Limit => 1;
        public bool CountNegatedActivation => false;
        public EffectCategories Categories => EffectCategories.AddFromDeckToHand;
        public bool CanActivate(EffectContext context) => context.State.Players[context.Player].LifePoints > 200
            && context.State.Players[context.Player].Deck.Count > 0 && !context.Engine.HasEffect(EffectRecordKind.PreventDeckToHand, context.Player)
            && context.State.Cards.Any(card => card.Owner == context.Player && card.Zone == DuelZone.Graveyard
                && context.Catalog.Get(card.DefinitionId).Kind == RuleCardKind.Monster);
        public string ValidateActivation(EffectContext context, DuelCommand command) =>
            command.Cards.Length == 0 && command.TargetId == 0 ? "" : "NO_ACTIVATION_TARGET";
        public void PayCost(EffectContext context, DuelCommand command, DuelChainLink link) => context.State.Players[context.Player].LifePoints -= 200;
        public void Resolve(EffectContext context, DuelChainLink link)
        {
            if (link.Step == 0)
            {
                if (context.Engine.HasEffect(EffectRecordKind.PreventDeckToHand, context.Player)) { link.Step = 2; return; }
                if (context.State.Players[context.Player].Deck.Count == 0) { context.Engine.Draw(context.Player, 1); link.Step = 2; return; }
                var drawn = context.Card(context.State.Players[context.Player].Deck[0]);
                context.Engine.Draw(context.Player, 1); context.Reveal(drawn);
                if (context.Catalog.Get(drawn.DefinitionId).Kind != RuleCardKind.Monster) { link.Step = 2; return; }
                bool present = context.State.Cards.Any(card => card.Owner == context.Player && card.Zone == DuelZone.Graveyard
                    && context.Catalog.Get(card.DefinitionId).Kind == RuleCardKind.Monster && (card.CurrentAttribute & drawn.CurrentAttribute) != 0);
                if (!present) { context.Engine.Draw(context.Player, 1); link.Step = 2; return; }
                link.Step = 1;
                context.SelectCards(link, context.State.Cards.Where(card => card.Owner == context.Player && card.Zone == DuelZone.Hand), "选择丢弃的手卡"); return;
            }
            if (link.Step == 1) { context.Move(context.Card(link.Selected[0]), DuelZone.Graveyard); link.Step = 2; }
        }
    }
}
