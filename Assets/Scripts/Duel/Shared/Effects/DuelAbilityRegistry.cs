using System;
using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class DuelAbilityRegistry
    {
        readonly Dictionary<string, IAbilityHandler> m_handlers;
        public IReadOnlyList<IAbilityHandler> Handlers { get; }
        public DuelAbilityRegistry(IEnumerable<IAbilityHandler> handlers)
        {
            Handlers = Array.AsReadOnly(handlers.ToArray());
            m_handlers = Handlers.ToDictionary(handler => handler.AbilityId, StringComparer.Ordinal);
        }
        public IAbilityHandler Get(string abilityId) => m_handlers[abilityId];
        public bool TryGet(string abilityId, out IAbilityHandler handler) => m_handlers.TryGetValue(abilityId, out handler);
        public static DuelAbilityRegistry Create(CardRuleCatalog rules) =>
            new DuelAbilityRegistry(rules.Cards.SelectMany(card => card.CreateAbilities()));
        public static DuelAbilityRegistry CreateDefault() => Create(CardRuleCatalog.CreateDefault(DuelCardCatalog.CreateDefault()));
    }
}
