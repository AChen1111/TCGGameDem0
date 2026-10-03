using System.Collections.Generic;

namespace AChen.Duel.Core
{
    /// <summary>Material policy belongs to each individual card rule.</summary>
    public static class SummonRecipes
    {
        public static bool MatchesLink(CardDefinition target, IReadOnlyList<DuelCardState> materials, DuelCardCatalog catalog) =>
            CardRuleCatalog.CreateDefault(catalog).Get(target.CardId).MatchesSummonMaterials(target, materials, catalog);
    }
}
