using System;
using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class DuelCardCatalog
    {
        readonly Dictionary<string, CardDefinition> m_cards = new Dictionary<string, CardDefinition>(StringComparer.Ordinal);
        readonly Dictionary<string, string> m_artIds;
        public IReadOnlyList<CardDefinition> Cards { get; }
        public string Fingerprint { get; }
        public string BanlistFingerprint => CardCatalogData.BanlistFingerprint;
        DuelCardCatalog(IEnumerable<CardDefinition> cards)
        {
            Cards = Array.AsReadOnly(cards.ToArray());
            foreach (var card in Cards) m_cards.Add(card.CardId, card);
            m_artIds = CardCatalogData.CreateArtAliases();
            Fingerprint = DuelCardDataDigest.Compute(Cards, m_artIds, CardCatalogData.SourceFingerprint);
        }
        public string ResolveCardId(string cardId) => m_artIds.TryGetValue(cardId, out string ruleId) ? ruleId : cardId;
        public CardDefinition Get(string cardId) => m_cards[ResolveCardId(cardId)];
        public static DuelCardCatalog CreateDefault() => new DuelCardCatalog(CardCatalogData.CreateCards());
    }
}
