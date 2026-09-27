using System;
using System.Collections.Generic;
using System.Linq;
using AChen.Configuration;

namespace AChen.Decks
{
    public enum DeckValidationMode { Draft, Playable }
    public enum DeckIssueCode
    {
        MainTooSmall, MainTooLarge, ExtraTooLarge, CopyLimitExceeded,
        InvalidName, InvalidEntry, UnknownCard, WrongSection, NotOwned, ConfigNotReady, InventoryNotReady
    }

    public sealed class DeckValidationIssue
    {
        public DeckIssueCode Code { get; }
        public string CardId { get; }
        public int? Rarity { get; }
        public DeckSection? Section { get; }
        public long Actual { get; }
        public long Allowed { get; }
        public DeckValidationIssue(DeckIssueCode code, long actual = 0, long allowed = 0,
            string cardId = null, int? rarity = null, DeckSection? section = null)
        { Code = code; Actual = actual; Allowed = allowed; CardId = cardId; Rarity = rarity; Section = section; }
    }

    public sealed class DeckValidationResult
    {
        public IReadOnlyList<DeckValidationIssue> Issues { get; }
        public bool IsValid => Issues.Count == 0;
        internal DeckValidationResult(List<DeckValidationIssue> issues) { Issues = issues.AsReadOnly(); }
    }

    public static class DeckValidator
    {
        public static DeckData PrepareSave(DeckData deck, DeckRulesConfiguration rules, IReadOnlyList<DeckCardEntry> ownedCards)
        {
            var result = Validate(deck, rules, ownedCards);
            if (!result.IsValid) throw new DeckValidationException(result);
            return new DeckData(deck.Id, deck.Name.Trim(), Normalize(deck.MainDeck), Normalize(deck.ExtraDeck),
                deck.Revision, deck.CreatedAt, deck.UpdatedAt);
        }

        static IEnumerable<DeckCardEntry> Normalize(IReadOnlyList<DeckCardEntry> cards) => cards
            .GroupBy(x => (x.CardId, x.Rarity)).Select(x => new DeckCardEntry(x.Key.CardId, x.Key.Rarity, x.Sum(card => card.Count)));

        public static DeckValidationResult Validate(DeckData deck, DeckRulesConfiguration rules,
            IReadOnlyList<DeckCardEntry> ownedCards, DeckValidationMode mode = DeckValidationMode.Draft)
        {
            if (deck == null) throw new ArgumentNullException(nameof(deck));
            if (mode != DeckValidationMode.Draft && mode != DeckValidationMode.Playable)
                throw new ArgumentOutOfRangeException(nameof(mode));
            var issues = new List<DeckValidationIssue>();
            if (!IsValidName(deck.Name)) issues.Add(new DeckValidationIssue(DeckIssueCode.InvalidName));
            if (rules == null) issues.Add(new DeckValidationIssue(DeckIssueCode.ConfigNotReady));
            if (ownedCards == null) issues.Add(new DeckValidationIssue(DeckIssueCode.InventoryNotReady));
            Inspect(deck.MainDeck, DeckSection.Main, rules, issues);
            Inspect(deck.ExtraDeck, DeckSection.Extra, rules, issues);
            long main = deck.MainDeck.Where(ValidEntry).Sum(x => (long)x.Count);
            long extra = deck.ExtraDeck.Where(ValidEntry).Sum(x => (long)x.Count);
            if (mode == DeckValidationMode.Playable && main < 40)
                issues.Add(new DeckValidationIssue(DeckIssueCode.MainTooSmall, main, 40, section: DeckSection.Main));
            if (main > 60) issues.Add(new DeckValidationIssue(DeckIssueCode.MainTooLarge, main, 60, section: DeckSection.Main));
            if (extra > 15) issues.Add(new DeckValidationIssue(DeckIssueCode.ExtraTooLarge, extra, 15, section: DeckSection.Extra));
            var cards = deck.MainDeck.Concat(deck.ExtraDeck).Where(ValidEntry).ToArray();
            foreach (var group in cards.GroupBy(x => x.CardId, StringComparer.Ordinal))
            {
                if (rules == null || !rules.TryGetSection(group.Key, out _)) continue;
                long count = group.Sum(x => (long)x.Count);
                int allowed = rules.GetMaxCopies(group.Key);
                if (count > allowed)
                    issues.Add(new DeckValidationIssue(DeckIssueCode.CopyLimitExceeded, count, allowed, group.Key));
            }
            if (ownedCards != null)
            {
                var inventory = ownedCards.Where(ValidEntry).GroupBy(x => (x.CardId, x.Rarity))
                    .ToDictionary(x => x.Key, x => x.Sum(card => (long)card.Count));
                foreach (var group in cards.GroupBy(x => (x.CardId, x.Rarity)))
                {
                    inventory.TryGetValue(group.Key, out long available);
                    long used = group.Sum(x => (long)x.Count);
                    if (used > available)
                        issues.Add(new DeckValidationIssue(DeckIssueCode.NotOwned, used, available, group.Key.CardId, group.Key.Rarity));
                }
            }
            return new DeckValidationResult(issues);
        }

        public static bool IsValidName(string name) => !string.IsNullOrWhiteSpace(name)
            && name.Trim().Length <= 64 && !name.Any(char.IsControl);

        static bool ValidEntry(DeckCardEntry entry) => entry != null && !string.IsNullOrWhiteSpace(entry.CardId)
            && entry.CardId.Length <= 128 && !entry.CardId.Any(char.IsControl)
            && entry.Rarity >= 0 && entry.Rarity <= 4 && entry.Count > 0;

        static void Inspect(IReadOnlyList<DeckCardEntry> cards, DeckSection section, DeckRulesConfiguration rules,
            List<DeckValidationIssue> issues)
        {
            foreach (var card in cards)
            {
                if (!ValidEntry(card))
                    issues.Add(new DeckValidationIssue(DeckIssueCode.InvalidEntry, cardId: card?.CardId, rarity: card?.Rarity, section: section));
                else if (rules != null)
                {
                    if (!rules.TryGetSection(card.CardId, out var expected))
                        issues.Add(new DeckValidationIssue(DeckIssueCode.UnknownCard, cardId: card.CardId, rarity: card.Rarity, section: section));
                    else if (expected != section)
                        issues.Add(new DeckValidationIssue(DeckIssueCode.WrongSection, cardId: card.CardId, rarity: card.Rarity, section: section));
                }
            }
        }
    }

    public sealed class DeckValidationException : Exception
    {
        public DeckValidationResult Result { get; }
        public DeckValidationException(DeckValidationResult result) : base("卡组未通过保存校验") { Result = result; }
    }
}
