namespace AChen.Duel.Core
{
    public class DrawSpellHandler : IAbilityHandler, IActivationCostSelection, IEffectCategoryProvider
    {
        readonly int m_count;
        readonly int m_opponentRecovery;
        readonly IActivationCost m_cost;
        public string CardId { get; }
        public string AbilityId => CardId + ".1";
        public int Speed => 1;
        public EffectCategories Categories => EffectCategories.AddFromDeckToHand;
        public DrawSpellHandler(string cardId, int count, int opponentRecovery = 0)
            : this(cardId, count, new NoActivationCost(), opponentRecovery) { }
        public DrawSpellHandler(string cardId, int count, IActivationCost cost, int opponentRecovery = 0)
        { CardId = cardId; m_count = count; m_cost = cost; m_opponentRecovery = opponentRecovery; }
        public int MinCosts => m_cost.MinCosts;
        public int MaxCosts => m_cost.MaxCosts;
        public System.Collections.Generic.IEnumerable<DuelCardState> CostCandidates(EffectContext context) => m_cost.CostCandidates(context);
        public bool CanActivate(EffectContext context) => !context.Engine.HasEffect(EffectRecordKind.PreventDeckToHand, context.Player)
            && context.State.Players[context.Player].Deck.Count >= m_count + m_cost.DeckCardsConsumed
            && m_cost.CanPay(context);
        public string ValidateActivation(EffectContext context, DuelCommand command) =>
            command.TargetId == 0 ? m_cost.Validate(context, command) : "NO_ACTIVATION_TARGET";
        public void PayCost(EffectContext context, DuelCommand command, DuelChainLink link) => m_cost.Pay(context, command, link);
        public void Resolve(EffectContext context, DuelChainLink link)
        {
            if (link.Step != 0) return;
            context.Engine.Draw(context.Player, m_count);
            if (!context.State.Finished && m_opponentRecovery > 0)
                context.Recover(1 - context.Player, m_opponentRecovery);
            link.Step = 1;
        }
    }

    public sealed class OncePerTurnDrawSpellHandler : DrawSpellHandler, IActivationUsageLimit
    {
        public string UsageKey => CardId;
        public int Limit => 1;
        public bool CountNegatedActivation => false;
        public OncePerTurnDrawSpellHandler(string cardId, int count, IActivationCost cost)
            : base(cardId, count, cost) { }
    }
}
