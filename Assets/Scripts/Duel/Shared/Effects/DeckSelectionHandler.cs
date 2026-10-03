using System;
using System.Linq;

namespace AChen.Duel.Core
{
    /// <summary>处理时选择牌库中的卡。不可把发动时客户端传来的实体当作已锁定的检索结果。</summary>
    public sealed class DeckSelectionHandler : IAbilityHandler, IEffectCategoryProvider
    {
        readonly Func<CardDefinition, bool> m_filter;
        readonly DuelZone m_destination;
        public string CardId { get; }
        public string AbilityId => CardId + ".1";
        public int Speed => 1;
        public EffectCategories Categories => m_destination == DuelZone.Hand
            ? EffectCategories.AddFromDeckToHand : EffectCategories.SendDeckToGraveyard;
        public DeckSelectionHandler(string cardId, Func<CardDefinition, bool> filter, DuelZone destination)
        { CardId = cardId; m_filter = filter; m_destination = destination; }
        public bool CanActivate(EffectContext context) => (m_destination != DuelZone.Hand
            || !context.Engine.HasEffect(EffectRecordKind.PreventDeckToHand, context.Player))
            && context.Deck.Any(card => m_filter(context.Catalog.Get(card.DefinitionId)));
        public string ValidateActivation(EffectContext context, DuelCommand command) =>
            command.Cards.Length == 0 && command.TargetId == 0 ? "" : "NO_ACTIVATION_TARGET";
        public void PayCost(EffectContext context, DuelCommand command, DuelChainLink link) { }

        public void Resolve(EffectContext context, DuelChainLink link)
        {
            if (link.Step == 0)
            {
                var candidates = m_destination == DuelZone.Hand && context.Engine.HasEffect(EffectRecordKind.PreventDeckToHand, context.Player)
                    ? System.Array.Empty<DuelCardState>() : context.Deck.Where(card => m_filter(context.Catalog.Get(card.DefinitionId))).ToArray();
                link.Step = 1;
                if (candidates.Length == 0) { link.Step = 2; return; }
                context.SelectCards(link, candidates, "选择牌库中的卡");
                return;
            }
            if (link.Step == 1)
            {
                var card = context.Card(link.Selected[0]);
                if (context.TryMove(card, m_destination, m_destination == DuelZone.Hand ? CardPosition.FaceDown : CardPosition.FaceUp))
                {
                    if (m_destination == DuelZone.Hand) context.Reveal(card);
                    context.ShuffleDeck();
                }
                link.Step = 2;
            }
        }
    }
}
