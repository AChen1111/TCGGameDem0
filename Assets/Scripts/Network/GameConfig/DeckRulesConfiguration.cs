using System;
using System.Collections.Generic;
using System.Linq;

namespace AChen.Configuration
{
    public enum DeckSection { Main, Extra }

    /// <summary>不可变的组卡配置快照。服务端通用配置读取不调用此校验器。</summary>
    public sealed class DeckRulesConfiguration
    {
        public const string SectionsTable = "card-deck-sections";
        public const string BanlistTable = "card-banlist";
        readonly Dictionary<string, DeckSection> m_sections;
        readonly Dictionary<string, int> m_limits;
        readonly Dictionary<string, string> m_artIds;

        DeckRulesConfiguration(Dictionary<string, DeckSection> sections, Dictionary<string, int> limits, Dictionary<string, string> artIds)
        {
            m_sections = sections;
            m_limits = limits;
            m_artIds = artIds;
        }

        public int CardCount => m_sections.Count;
        public bool TryGetSection(string cardId, out DeckSection section)
        {
            section = default;
            return cardId != null && m_sections.TryGetValue(ResolveCardId(cardId), out section);
        }

        public string ResolveCardId(string cardId) => cardId != null && m_artIds.TryGetValue(cardId, out string canonical) ? canonical : cardId;

        public int GetMaxCopies(string cardId)
        {
            if (!TryGetSection(cardId, out _)) throw new ArgumentException("未知卡牌: " + cardId, nameof(cardId));
            return m_limits.TryGetValue(ResolveCardId(cardId), out int limit) ? limit : 3;
        }

        public static DeckRulesConfiguration Load(IReadOnlyDictionary<string, byte[]> files) =>
            Create(Table.CardRow.LoadBytes(Required(files, "Cards")).Select(x => x.CardId),
                Decode(files, SectionsTable), Decode(files, BanlistTable),
                files.TryGetValue("card-art-variants", out var artTable)
                    ? GameConfigTables.Map<CardArtVariant>(BinaryTable.Decode(artTable)) : Array.Empty<CardArtVariant>());

        public static DeckRulesConfiguration Create(IEnumerable<string> cardIds, BinaryTable sectionTable, BinaryTable banlist,
            IEnumerable<CardArtVariant> artVariants = null)
        {
            var ids = new HashSet<string>(cardIds, StringComparer.Ordinal);
            var sections = new Dictionary<string, DeckSection>(StringComparer.Ordinal);
            try
            {
                int idColumn = sectionTable.Column("CardId", "string");
                int sectionColumn = sectionTable.Column("Section", "string");
                foreach (var row in sectionTable.Rows)
                {
                    string id = (string)row[idColumn];
                    string value = (string)row[sectionColumn];
                    if (string.IsNullOrWhiteSpace(id) || !ids.Contains(id) || sections.ContainsKey(id)
                        || value != "Main" && value != "Extra")
                        throw new FormatException("卡牌分类无效或重复: " + id);
                    sections.Add(id, value == "Main" ? DeckSection.Main : DeckSection.Extra);
                }
                if (!ids.SetEquals(sections.Keys)) throw new FormatException("分类遗漏: " + string.Join(",", ids.Except(sections.Keys)));
            }
            catch (Exception ex) when (ex is FormatException || ex is ArgumentException || ex is InvalidCastException)
            { throw new FormatException(SectionsTable + ": " + ex.Message, ex); }

            var limits = new Dictionary<string, int>(StringComparer.Ordinal);
            try
            {
                int idColumn = banlist.Column("CardId", "string");
                int limitColumn = banlist.Column("MaxCopies", "int");
                foreach (var row in banlist.Rows)
                {
                    string id = (string)row[idColumn];
                    int limit = (int)row[limitColumn];
                    if (string.IsNullOrWhiteSpace(id) || !ids.Contains(id) || limits.ContainsKey(id) || limit < 0 || limit > 3)
                        throw new FormatException("卡牌限制无效或重复: " + id);
                    limits.Add(id, limit);
                }
            }
            catch (Exception ex) when (ex is FormatException || ex is ArgumentException || ex is InvalidCastException)
            { throw new FormatException(BanlistTable + ": " + ex.Message, ex); }
            var artIds = (artVariants ?? Array.Empty<CardArtVariant>()).ToDictionary(x => x.ArtId, x => x.CardId, StringComparer.Ordinal);
            if (artIds.Values.Any(x => !sections.ContainsKey(x))) throw new FormatException("异画关联不存在的规则卡");
            return new DeckRulesConfiguration(sections, limits, artIds);
        }

        static byte[] Required(IReadOnlyDictionary<string, byte[]> files, string name) =>
            files.TryGetValue(name, out var data) && data != null ? data : throw new FormatException("缺少组卡配置: " + name);

        static BinaryTable Decode(IReadOnlyDictionary<string, byte[]> files, string name)
        {
            try { return BinaryTable.Decode(Required(files, name)); }
            catch (Exception ex) when (ex is FormatException || ex is System.IO.IOException || ex is ArgumentException)
            { throw new FormatException(name + ": " + ex.Message, ex); }
        }
    }
}
