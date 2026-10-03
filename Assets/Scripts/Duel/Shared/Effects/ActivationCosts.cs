using System;
using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public interface IActivationCostSelection
    {
        IEnumerable<DuelCardState> CostCandidates(EffectContext context);
        int MinCosts { get; }
        int MaxCosts { get; }
    }

    /// <summary>Cards carries selected activation targets; PayCost records refs and pays no card cost.</summary>
    public interface IActivationSelectedTargets : IActivationCostSelection { }

    public interface IActivationCost : IActivationCostSelection
    {
        bool CanPay(EffectContext context);
        string Validate(EffectContext context, DuelCommand command);
        void Pay(EffectContext context, DuelCommand command, DuelChainLink link);
        int DeckCardsConsumed { get; }
    }

    public sealed class NoActivationCost : IActivationCost
    {
        public int MinCosts => 0;
        public int MaxCosts => 0;
        public int DeckCardsConsumed => 0;
        public IEnumerable<DuelCardState> CostCandidates(EffectContext context) => Array.Empty<DuelCardState>();
        public bool CanPay(EffectContext context) => true;
        public string Validate(EffectContext context, DuelCommand command) => command.Cards.Length == 0 ? "" : "NO_ACTIVATION_COST";
        public void Pay(EffectContext context, DuelCommand command, DuelChainLink link) { }
    }

    public sealed class DiscardOneCost : IActivationCost
    {
        readonly Func<CardDefinition, bool> m_filter;
        public DiscardOneCost(Func<CardDefinition, bool> filter) { m_filter = filter; }
        public int MinCosts => 1;
        public int MaxCosts => 1;
        public int DeckCardsConsumed => 0;
        public IEnumerable<DuelCardState> CostCandidates(EffectContext context) => context.State.Cards.Where(card =>
            card.Owner == context.Player && card.Zone == DuelZone.Hand && card.InstanceId != context.Source.InstanceId
            && m_filter(context.Catalog.Get(card.DefinitionId)));
        public bool CanPay(EffectContext context) => CostCandidates(context).Any();
        public string Validate(EffectContext context, DuelCommand command) => command.Cards.Length == 1
            && CostCandidates(context).Any(card => card.InstanceId == command.Cards[0]) ? "" : "INVALID_DISCARD_COST";
        public void Pay(EffectContext context, DuelCommand command, DuelChainLink link)
        {
            var card = context.Card(command.Cards[0]);
            link.Costs.Add(card.Ref);
            context.MoveAsCost(card, DuelZone.Graveyard);
        }
    }

    public sealed class BanishTopCardsCost : IActivationCost
    {
        readonly int m_count;
        public BanishTopCardsCost(int count) { m_count = count; }
        public int MinCosts => 0;
        public int MaxCosts => 0;
        public int DeckCardsConsumed => m_count;
        public IEnumerable<DuelCardState> CostCandidates(EffectContext context) => Array.Empty<DuelCardState>();
        public bool CanPay(EffectContext context) => context.State.Players[context.Player].Deck.Count >= m_count;
        public string Validate(EffectContext context, DuelCommand command) => command.Cards.Length == 0 ? "" : "COST_USES_DECK_TOP";
        public void Pay(EffectContext context, DuelCommand command, DuelChainLink link)
        {
            foreach (var card in context.Deck.Take(m_count).ToArray())
            {
                link.Costs.Add(card.Ref);
                context.MoveAsCost(card, DuelZone.Banished, CardPosition.FaceDown);
            }
        }
    }
}
