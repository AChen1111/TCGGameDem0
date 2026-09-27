using System;
using System.Collections.Generic;
using System.Linq;
using AChen.Configuration;

namespace AChen.Decks
{
    public sealed class DeckDraft
    {
        readonly DeckData m_source;
        readonly List<DeckCardEntry> m_main;
        readonly List<DeckCardEntry> m_extra;
        public string Name { get; set; }
        public DeckDraft(DeckData source)
        {
            m_source = source ?? throw new ArgumentNullException(nameof(source));
            Name = source.Name;
            m_main = source.MainDeck.ToList();
            m_extra = source.ExtraDeck.ToList();
        }
        /// <summary>设置该版本的总张数；0 可移除已下架卡，便于修复旧卡组。保存时统一校验规则。</summary>
        public void SetCardCount(string cardId, int rarity, int count, DeckRulesConfiguration rules)
        {
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
            DeckSection section = default;
            if (count > 0)
            {
                if (rarity < 0 || rarity > 4) throw new ArgumentOutOfRangeException(nameof(rarity));
                if (rules == null) throw new InvalidOperationException("组卡配置未就绪");
                if (!rules.TryGetSection(cardId, out section)) throw new ArgumentException("未知卡牌: " + cardId, nameof(cardId));
            }
            m_main.RemoveAll(x => x != null && x.CardId == cardId && x.Rarity == rarity);
            m_extra.RemoveAll(x => x != null && x.CardId == cardId && x.Rarity == rarity);
            if (count > 0) (section == DeckSection.Main ? m_main : m_extra).Add(new DeckCardEntry(cardId, rarity, count));
        }

        public DeckData ToData() => new DeckData(m_source.Id, Name, m_main, m_extra,
            m_source.Revision, m_source.CreatedAt, m_source.UpdatedAt);
    }
}
