using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class AHeroLivesCard : CardRules
    {
        public override string CardId => "08949584";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("08949584.1", CardRuleKind.ActivatedAbility));
        public override IEnumerable<IAbilityHandler> CreateAbilities() { yield return new AHeroLivesAbility(); }
    }

    sealed class AHeroLivesAbility : IAbilityHandler, IEffectCategoryProvider
    {
        public string CardId => "08949584";
        public string AbilityId => CardId + ".1";
        public int Speed => 1;
        public EffectCategories Categories => EffectCategories.SpecialSummonFromDeck;
        static IEnumerable<DuelCardState> Candidates(EffectContext context) => context.Deck.Where(card =>
            context.Catalog.Get(card.DefinitionId).BelongsTo(0x3008) && context.Catalog.Get(card.DefinitionId).Level <= 4
            && context.CanSpecialSummon(card, context.Player));
        public bool CanActivate(EffectContext context) => context.State.Players[context.Player].LifePoints > 1
            && !context.State.Cards.Any(card => card.Controller == context.Player
                && (card.Zone == DuelZone.Monster || card.Zone == DuelZone.ExtraMonster)
                && DuelEngine.IsPublic(card)) && Candidates(context).Any();
        public string ValidateActivation(EffectContext context, DuelCommand command) =>
            command.Cards.Length == 0 && command.TargetId == 0 ? "" : "NO_ACTIVATION_TARGET";
        public void PayCost(EffectContext context, DuelCommand command, DuelChainLink link)
        { context.State.Players[context.Player].LifePoints = (context.State.Players[context.Player].LifePoints + 1) / 2; }
        public void Resolve(EffectContext context, DuelChainLink link)
        {
            if (link.Step >= 5) return;
            if (link.Step == 0)
            {
                var candidates = Candidates(context).ToArray();
                link.Step = 1;
                if (candidates.Length == 0) { link.Step = 5; return; }
                context.SelectCards(link, candidates, "选择特殊召唤的元素英雄");
                return;
            }
            if (link.Step == 1) link.Values["selected-card"] = link.Selected[0];
            if (EffectSummonFlow.Resume(context, link, context.Card(link.Values["selected-card"]), 1)
                && link.Step == 4) { context.ShuffleDeck(); link.Step = 5; }
        }
    }
}
