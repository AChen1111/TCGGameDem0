using System;
using System.Linq;
using AChen.Decks;
using NUnit.Framework;

public sealed class DeckDraftTests
{
    [Test]
    public void Save_uses_current_banlist_normalizes_rows_and_preserves_draft_on_failure()
    {
        var source = new DeckData(Guid.NewGuid(), "Before", new[] { new DeckCardEntry("M00", 0, 1), new DeckCardEntry("M00", 0, 1) }, Array.Empty<DeckCardEntry>(), 4);
        var draft = new DeckDraft(source) { Name = "  After  " };
        var prepared = DeckValidator.PrepareSave(draft.ToData(), DeckValidatorTests.Rules(), DeckValidatorTests.Inventory());
        Assert.AreEqual("After", prepared.Name);
        Assert.AreEqual(2, prepared.MainDeck.Single().Count);
        var error = Assert.Throws<DeckValidationException>(() => DeckValidator.PrepareSave(draft.ToData(), DeckValidatorTests.Rules(0), DeckValidatorTests.Inventory()));
        Assert.IsTrue(error.Result.Issues.Any(x => x.Code == DeckIssueCode.CopyLimitExceeded && x.Allowed == 0));
        Assert.AreEqual(2, draft.ToData().MainDeck.Sum(x => x.Count));
        Assert.AreEqual("Before", source.Name);
        Assert.AreEqual(4, source.Revision);
        draft.SetCardCount("M00", 0, 0, null);
        Assert.IsTrue(DeckValidator.Validate(draft.ToData(), DeckValidatorTests.Rules(0), DeckValidatorTests.Inventory()).IsValid);
    }

    [Test]
    public void Edit_renames_routes_cards_merges_copies_and_removes_without_changing_saved_snapshot()
    {
        var source = new DeckData(Guid.NewGuid(), "Before", new[] { new DeckCardEntry("M00", 0, 1), new DeckCardEntry("M00", 0, 1) },
            Array.Empty<DeckCardEntry>(), 7);
        var draft = new DeckDraft(source) { Name = "After" };
        var rules = DeckValidatorTests.Rules();
        draft.SetCardCount("M00", 0, 3, rules);
        draft.SetCardCount("E00", 1, 1, rules);
        var edited = draft.ToData();
        Assert.AreEqual("After", edited.Name);
        Assert.AreEqual(source.Id, edited.Id);
        Assert.AreEqual(7, edited.Revision);
        Assert.AreEqual(3, edited.MainDeck.Single().Count);
        Assert.AreEqual("E00", edited.ExtraDeck.Single().CardId);
        draft.SetCardCount("M00", 0, 0, rules);
        Assert.IsEmpty(draft.ToData().MainDeck);
        Assert.AreEqual(3, edited.MainDeck.Single().Count);
        Assert.AreEqual("Before", source.Name);
        Assert.AreEqual(2, source.MainDeck.Count);
        Assert.Throws<ArgumentOutOfRangeException>(() => draft.SetCardCount("E00", 1, -1, rules));
        Assert.AreEqual(1, draft.ToData().ExtraDeck.Single().Count);
    }
}
