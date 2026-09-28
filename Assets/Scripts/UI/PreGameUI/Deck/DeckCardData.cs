public sealed class DeckCardData
{
    public string CardId { get; }
    public string SourcePool { get; }
    public string ArtId { get; }
    public int Rarity { get; }
    public int Owned { get; }
    public bool InDeck { get; }
    public DeckEditWindow Window { get; }
    public DeckCardData(string id, string pool, int rarity, int owned, bool inDeck, DeckEditWindow window, string artId = null)
    { CardId = id; SourcePool = pool; ArtId = artId ?? id; Rarity = rarity; Owned = owned; InDeck = inDeck; Window = window; }
}
