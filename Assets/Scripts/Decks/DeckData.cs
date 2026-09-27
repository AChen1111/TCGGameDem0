using System;
using System.Collections.Generic;
using System.Linq;

namespace AChen.Decks
{
    public sealed class DeckCardEntry
    {
        public string CardId { get; }
        public int Rarity { get; }
        public int Count { get; }
        public DeckCardEntry(string cardId, int rarity, int count)
        { CardId = cardId; Rarity = rarity; Count = count; }
    }

    /// <summary>保存版本的只读快照。编辑时创建 DeckDraft，不修改该对象。</summary>
    public sealed class DeckData
    {
        public Guid Id { get; }
        public string Name { get; }
        public IReadOnlyList<DeckCardEntry> MainDeck { get; }
        public IReadOnlyList<DeckCardEntry> ExtraDeck { get; }
        public long Revision { get; }
        public DateTimeOffset CreatedAt { get; }
        public DateTimeOffset UpdatedAt { get; }

        public DeckData(Guid id, string name, IEnumerable<DeckCardEntry> mainDeck, IEnumerable<DeckCardEntry> extraDeck,
            long revision = 0, DateTimeOffset createdAt = default, DateTimeOffset updatedAt = default)
        {
            Id = id; Name = name; Revision = revision; CreatedAt = createdAt; UpdatedAt = updatedAt;
            MainDeck = Array.AsReadOnly((mainDeck ?? throw new ArgumentNullException(nameof(mainDeck))).ToArray());
            ExtraDeck = Array.AsReadOnly((extraDeck ?? throw new ArgumentNullException(nameof(extraDeck))).ToArray());
        }
    }
}
