using System;
using System.Linq;
using AChen.Configuration;
using AChen.Decks;
using NUnit.Framework;

public sealed class DeckValidatorTests
{
    [TestCase(0, 0, DeckValidationMode.Draft, true)]
    [TestCase(0, 0, DeckValidationMode.Playable, false)]
    [TestCase(39, 0, DeckValidationMode.Playable, false)]
    [TestCase(39, 0, DeckValidationMode.Draft, true)]
    [TestCase(40, 0, DeckValidationMode.Playable, true)]
    [TestCase(60, 15, DeckValidationMode.Playable, true)]
    [TestCase(61, 0, DeckValidationMode.Draft, false)]
    [TestCase(40, 16, DeckValidationMode.Draft, false)]
    public void Counts_distinguish_saveable_drafts_from_playable_decks(int main, int extra, DeckValidationMode mode, bool valid)
    {
        var deck = new DeckData(Guid.NewGuid(), "Deck", Entries("M", main), Entries("E", extra));
        Assert.AreEqual(valid, DeckValidator.Validate(deck, Rules(), Inventory(), mode).IsValid);
    }

    [TestCase(0, 0, true)]
    [TestCase(0, 1, false)]
    [TestCase(1, 1, true)]
    [TestCase(1, 2, false)]
    [TestCase(2, 2, true)]
    [TestCase(2, 3, false)]
    [TestCase(3, 3, true)]
    [TestCase(3, 4, false)]
    public void Ban_limits_count_duplicate_rows_and_different_rarities_together(int limit, int count, bool valid)
    {
        var cards = Enumerable.Range(0, count).Select(i => new DeckCardEntry("M00", i % 2, 1));
        var result = DeckValidator.Validate(new DeckData(Guid.NewGuid(), "Deck", cards, Array.Empty<DeckCardEntry>()), Rules(limit), Inventory());
        Assert.AreEqual(valid, result.IsValid);
        if (!valid)
        {
            var issue = result.Issues.Single(x => x.Code == DeckIssueCode.CopyLimitExceeded);
            Assert.AreEqual("M00", issue.CardId);
            Assert.AreEqual(count, issue.Actual);
            Assert.AreEqual(limit, issue.Allowed);
        }
    }

    [Test]
    public void Three_copy_default_also_counts_cards_in_the_wrong_section()
    {
        var deck = new DeckData(Guid.NewGuid(), "Deck", new[] { new DeckCardEntry("M01", 0, 2) },
            new[] { new DeckCardEntry("M01", 1, 2) });
        var issue = DeckValidator.Validate(deck, Rules(), Inventory()).Issues.Single(x => x.Code == DeckIssueCode.CopyLimitExceeded);
        Assert.AreEqual(4, issue.Actual);
        Assert.AreEqual(3, issue.Allowed);
    }

    [TestCase("M00", 0, 1, true, DeckIssueCode.WrongSection)]
    [TestCase("E00", 0, 1, false, DeckIssueCode.WrongSection)]
    [TestCase("unknown", 0, 1, false, DeckIssueCode.UnknownCard)]
    [TestCase("", 0, 1, false, DeckIssueCode.InvalidEntry)]
    [TestCase("M00", -1, 1, false, DeckIssueCode.InvalidEntry)]
    [TestCase("M00", 5, 1, false, DeckIssueCode.InvalidEntry)]
    [TestCase("M00", 0, 0, false, DeckIssueCode.InvalidEntry)]
    [TestCase("M00", 0, -1, false, DeckIssueCode.InvalidEntry)]
    public void Invalid_card_entries_report_structured_errors(string id, int rarity, int count, bool extra, DeckIssueCode code)
    {
        var cards = new[] { new DeckCardEntry(id, rarity, count) };
        var deck = new DeckData(Guid.NewGuid(), "Deck", extra ? Array.Empty<DeckCardEntry>() : cards,
            extra ? cards : Array.Empty<DeckCardEntry>());
        Assert.IsTrue(DeckValidator.Validate(deck, Rules(), Inventory()).Issues.Any(x => x.Code == code));
    }

    [Test]
    public void Ownership_counts_duplicate_rows_per_rarity_without_consuming_inventory()
    {
        var owned = new[] { new DeckCardEntry("M00", 0, 1), new DeckCardEntry("M00", 1, 2) };
        var deck = new DeckData(Guid.NewGuid(), "Deck", new[] { new DeckCardEntry("M00", 0, 1), new DeckCardEntry("M00", 0, 1) }, Array.Empty<DeckCardEntry>());
        var issue = DeckValidator.Validate(deck, Rules(), owned).Issues.Single(x => x.Code == DeckIssueCode.NotOwned);
        Assert.AreEqual(0, issue.Rarity);
        Assert.AreEqual(2, issue.Actual);
        Assert.AreEqual(1, issue.Allowed);
        var valid = new DeckData(Guid.NewGuid(), "Deck", new[] { new DeckCardEntry("M00", 0, 1), new DeckCardEntry("M00", 1, 2) }, Array.Empty<DeckCardEntry>());
        Assert.IsTrue(DeckValidator.Validate(valid, Rules(), owned).IsValid);
        Assert.IsTrue(DeckValidator.Validate(valid, Rules(), owned).IsValid);
        Assert.AreEqual(1, owned[0].Count);
    }

    [Test]
    public void Unready_configuration_null_entries_and_invalid_names_cannot_pass_validation()
    {
        var empty = new DeckData(Guid.NewGuid(), "Deck", Array.Empty<DeckCardEntry>(), Array.Empty<DeckCardEntry>());
        Assert.IsTrue(DeckValidator.Validate(empty, null, Inventory()).Issues.Any(x => x.Code == DeckIssueCode.ConfigNotReady));
        var malformed = new DeckData(Guid.NewGuid(), " ", new DeckCardEntry[] { null }, Array.Empty<DeckCardEntry>());
        var issues = DeckValidator.Validate(malformed, Rules(), Inventory()).Issues;
        Assert.IsTrue(issues.Any(x => x.Code == DeckIssueCode.InvalidName));
        Assert.IsTrue(issues.Any(x => x.Code == DeckIssueCode.InvalidEntry));
    }

    internal static DeckCardEntry[] Entries(string prefix, int count) => Enumerable.Range(0, (count + 2) / 3)
        .Select(i => new DeckCardEntry(prefix + i.ToString("D2"), 0, Math.Min(3, count - i * 3))).ToArray();

    internal static DeckRulesConfiguration Rules(int maxCopies = 3)
    {
        var ids = Enumerable.Range(0, 30).Select(i => "M" + i.ToString("D2"))
            .Concat(Enumerable.Range(0, 10).Select(i => "E" + i.ToString("D2"))).ToArray();
        return DeckRulesConfiguration.Create(ids,
            new BinaryTable { Names = new[] { "CardId", "Section" }, Types = new[] { "string", "string" },
                Rows = ids.Select(id => new object[] { id, id.StartsWith("M") ? "Main" : "Extra" }).ToArray() },
            new BinaryTable { Names = new[] { "CardId", "MaxCopies" }, Types = new[] { "string", "int" },
                Rows = new[] { new object[] { "M00", maxCopies } } });
    }

    internal static DeckCardEntry[] Inventory() => Entries("M", 90).Concat(Entries("E", 30))
        .SelectMany(x => new[] { new DeckCardEntry(x.CardId, 0, 10), new DeckCardEntry(x.CardId, 1, 10) }).ToArray();
}
