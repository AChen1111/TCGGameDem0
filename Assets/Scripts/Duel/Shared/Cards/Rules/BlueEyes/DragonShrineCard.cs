using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class DragonShrineCard : CardRules
    {
        public override string CardId => "41620959";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("41620959.1", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("41620959.restrictions", CardRuleKind.Restriction));
        public override IEnumerable<IAbilityHandler> CreateAbilities() { yield return new DragonShrineAbility(); }
    }

    sealed class DragonShrineAbility : IAbilityHandler, IActivationUsageLimit, IEffectCategoryProvider
    {
        public string CardId => "41620959";
        public string AbilityId => CardId + ".1";
        public int Speed => 1;
        public string UsageKey => AbilityId;
        public int Limit => 1;
        public bool CountNegatedActivation => false;
        public EffectCategories Categories => EffectCategories.SendDeckToGraveyard;
        static IEnumerable<DuelCardState> Candidates(EffectContext context) => context.Deck.Where(c =>
            context.Catalog.Get(c.DefinitionId).Kind == RuleCardKind.Monster && context.Catalog.Get(c.DefinitionId).Race == 8192);
        public bool CanActivate(EffectContext context) => Candidates(context).Any();
        public string ValidateActivation(EffectContext context, DuelCommand command) =>
            command.TargetId == 0 && command.Cards.Length == 0 ? "" : "NO_ACTIVATION_TARGET";
        public void PayCost(EffectContext context, DuelCommand command, DuelChainLink link) { }
        public void Resolve(EffectContext context, DuelChainLink link)
        {
            if (link.Step == 0)
            {
                link.Step = 1;
                var candidates = Candidates(context).ToArray();
                if (candidates.Length == 0) { link.Step = 3; return; }
                context.SelectCards(link, candidates, "选择送去墓地的龙族怪兽");
                return;
            }
            if (link.Step == 1)
            {
                var first = context.Card(link.Selected.Single());
                context.Move(first, DuelZone.Graveyard);
                var definition = context.Catalog.Get(first.DefinitionId);
                var candidates = Candidates(context).ToArray();
                if (first.Zone == DuelZone.Graveyard && (definition.IsNormal
                    || context.Engine.HasEffect(EffectRecordKind.TreatAsNormal, context.Player, first)) && candidates.Length > 0)
                {
                    link.Step = 2;
                    context.SelectCards(link, candidates, "可以再送一只龙族怪兽去墓地", 0, 1);
                    return;
                }
                context.ShuffleDeck(); link.Step = 3;
            }
            if (link.Step == 2)
            {
                foreach (int id in link.Selected) context.Move(context.Card(id), DuelZone.Graveyard);
                context.ShuffleDeck(); link.Step = 3;
            }
        }
    }
}
