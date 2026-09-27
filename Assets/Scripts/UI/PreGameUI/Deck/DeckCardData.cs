public sealed class DeckCardData
{
    public string CardId { get; }
    public string SourcePool { get; }
    public int Rarity { get; }
    public int Owned { get; }
    public bool InDeck { get; }
    public DeckEditWindow Window { get; }
    public DeckCardData(string id, string pool, int rarity, int owned, bool inDeck, DeckEditWindow window)
    { CardId = id; SourcePool = pool; Rarity = rarity; Owned = owned; InDeck = inDeck; Window = window; }
}
