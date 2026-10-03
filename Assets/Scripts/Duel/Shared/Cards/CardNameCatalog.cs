using System;
using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class CardNameDefinition
    {
        public string NameId { get; }
        public RuleCardKind Kind { get; }
        public string Japanese { get; }
        public string Chinese { get; }
        public string English { get; }
        public CardNameDefinition(string nameId, RuleCardKind kind, string japanese, string chinese, string english)
        { NameId = nameId; Kind = kind; Japanese = japanese; Chinese = chinese; English = english; }
    }

    /// <summary>宣言名称目录与可构筑卡池分离；无 token、异画和规则同名别名。</summary>
    public sealed class CardNameCatalog
    {
        readonly Dictionary<string, CardNameDefinition> m_names;
        public IReadOnlyList<CardNameDefinition> Names { get; }
        public string Fingerprint => CardNameCatalogData.Fingerprint;
        CardNameCatalog(IEnumerable<CardNameDefinition> names)
        {
            Names = Array.AsReadOnly(names.ToArray());
            m_names = Names.ToDictionary(name => name.NameId, StringComparer.Ordinal);
        }
        public CardNameDefinition Get(string nameId) => m_names[nameId];
        public bool Contains(string nameId) => m_names.ContainsKey(nameId);
        public IEnumerable<CardNameDefinition> ForKind(RuleCardKind kind) => Names.Where(name => name.Kind == kind);
        public static CardNameCatalog CreateDefault() => new CardNameCatalog(CardNameCatalogData.CreateNames());
    }
}
