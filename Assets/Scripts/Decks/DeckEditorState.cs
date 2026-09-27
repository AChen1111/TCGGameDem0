using System;
using System.Collections.Generic;
using System.Linq;
using AChen.Configuration;

namespace AChen.Decks
{
    /// <summary>编辑草稿与已保存快照；失败的增减不会修改草稿。</summary>
    public sealed class DeckEditorState
    {
        DeckData m_saved;
        public DeckDraft Draft { get; private set; }
        public DeckEditorState(DeckData saved) { AcceptSaved(saved); }
        public void AcceptSaved(DeckData saved) { m_saved = saved; Draft = new DeckDraft(saved); }
        public bool IsDirty => Draft.Name != m_saved.Name || !Same(Draft.ToData().MainDeck, m_saved.MainDeck)
            || !Same(Draft.ToData().ExtraDeck, m_saved.ExtraDeck);
        static bool Same(IReadOnlyList<DeckCardEntry> a, IReadOnlyList<DeckCardEntry> b) =>
            a.OrderBy(x => x.CardId, StringComparer.Ordinal).ThenBy(x => x.Rarity)
                .Select(x => (x.CardId, x.Rarity, x.Count)).SequenceEqual(
                    b.OrderBy(x => x.CardId, StringComparer.Ordinal).ThenBy(x => x.Rarity).Select(x => (x.CardId, x.Rarity, x.Count)));
        public int Count(string id, int rarity) => Draft.ToData().MainDeck.Concat(Draft.ToData().ExtraDeck)
            .Where(x => x.CardId == id && x.Rarity == rarity).Sum(x => x.Count);
        public DeckValidationResult TryChange(string id, int rarity, int delta,
            DeckRulesConfiguration rules, IReadOnlyList<DeckCardEntry> inventory)
        {
            int count = Count(id, rarity) + delta;
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(delta));
            var next = new DeckDraft(Draft.ToData());
            next.SetCardCount(id, rarity, count, rules);
            var result = DeckValidator.Validate(next.ToData(), rules, inventory, DeckValidationMode.Draft);
            if (result.IsValid) Draft = next;
            return result;
        }
    }
}
