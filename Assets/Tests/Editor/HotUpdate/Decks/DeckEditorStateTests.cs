using System;
using System.Linq;
using AChen.Decks;
using NUnit.Framework;

public sealed class DeckEditorStateTests
{
    static DeckData Empty() => new DeckData(Guid.NewGuid(), "卡组", Array.Empty<DeckCardEntry>(), Array.Empty<DeckCardEntry>(), 2);
    [Test]
    public void Adding_routes_extra_cards_and_reverting_changes_clears_dirty_state()
    {
        var state = new DeckEditorState(Empty());
        var rules = DeckValidatorTests.Rules();
        var inventory = new[] { new DeckCardEntry("E00", 1, 2) };
        Assert.IsTrue(state.TryChange("E00", 1, 1, rules, inventory).IsValid);
        Assert.IsTrue(state.IsDirty);
        Assert.AreEqual(1, state.Draft.ToData().ExtraDeck.Single().Count);
        Assert.IsEmpty(state.Draft.ToData().MainDeck);
        Assert.IsTrue(state.TryChange("E00", 1, -1, rules, inventory).IsValid);
        Assert.IsFalse(state.IsDirty);
    }
    [Test]
    public void Version_ownership_and_cross_version_limit_rejections_leave_the_draft_unchanged()
    {
        var state = new DeckEditorState(Empty());
        var rules = DeckValidatorTests.Rules();
        var inventory = new[] { new DeckCardEntry("M00", 0, 1), new DeckCardEntry("M00", 4, 3) };
        Assert.IsTrue(state.TryChange("M00", 0, 1, rules, inventory).IsValid);
        Assert.IsTrue(state.TryChange("M00", 0, 1, rules, inventory).Issues.Any(x => x.Code == DeckIssueCode.NotOwned));
        Assert.AreEqual(1, state.Count("M00", 0));
        Assert.IsTrue(state.TryChange("M00", 4, 2, rules, inventory).IsValid);
        Assert.IsTrue(state.TryChange("M00", 4, 1, rules, inventory).Issues.Any(x => x.Code == DeckIssueCode.CopyLimitExceeded));
        Assert.AreEqual(2, state.Count("M00", 4));
    }
    [Test]
    public void Accepting_server_result_updates_revision_and_rename_baseline()
    {
        var state = new DeckEditorState(Empty());
        state.Draft.Name = "新名字";
        Assert.IsTrue(state.IsDirty);
        var draft = state.Draft.ToData();
        state.AcceptSaved(new DeckData(draft.Id, draft.Name, draft.MainDeck, draft.ExtraDeck, 3));
        Assert.IsFalse(state.IsDirty);
        Assert.AreEqual(3, state.Draft.ToData().Revision);
        Assert.IsTrue(DeckValidator.Validate(state.Draft.ToData(), DeckValidatorTests.Rules(), Array.Empty<DeckCardEntry>()).IsValid);
    }
}
