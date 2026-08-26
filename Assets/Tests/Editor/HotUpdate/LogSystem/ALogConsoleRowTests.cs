using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.UIElements;

public class ALogConsoleRowTests
{
    [Test]
    public void BindEntryRow_Error_UsesRedMessageAndErrorBackground() {
        VisualElement row = ALogConsoleWindow.MakeEntryRow();
        ALogConsoleWindow.BindEntryRow(row, Entry(ALogLevel.Error));

        Assert.IsTrue(row.ClassListContains("entry-row--error"));
        Assert.IsTrue(MessageLabel(row).ClassListContains("level--error"));
    }

    [Test]
    public void BindEntryRow_Warning_UsesYellowMessageAndWarningBackground() {
        VisualElement row = ALogConsoleWindow.MakeEntryRow();
        ALogConsoleWindow.BindEntryRow(row, Entry(ALogLevel.Warning));

        Assert.IsTrue(row.ClassListContains("entry-row--warning"));
        Assert.IsTrue(MessageLabel(row).ClassListContains("level--warning"));
    }

    [Test]
    public void BindEntryRow_Log_HasNoLevelBackground() {
        VisualElement row = ALogConsoleWindow.MakeEntryRow();
        ALogConsoleWindow.BindEntryRow(row, Entry(ALogLevel.Log));

        Assert.IsFalse(row.ClassListContains("entry-row--error"));
        Assert.IsFalse(row.ClassListContains("entry-row--warning"));
        Assert.IsTrue(MessageLabel(row).ClassListContains("level--log"));
    }

    [Test]
    public void BindEntryRow_RecyclingClearsPreviousLevelStyle() {
        VisualElement row = ALogConsoleWindow.MakeEntryRow();
        ALogConsoleWindow.BindEntryRow(row, Entry(ALogLevel.Error));
        ALogConsoleWindow.BindEntryRow(row, Entry(ALogLevel.Log));

        Assert.IsFalse(row.ClassListContains("entry-row--error"));
        Assert.IsFalse(MessageLabel(row).ClassListContains("level--error"));
    }

    [Test]
    public void MakeEntryRow_StartsWithCheckToggle() {
        VisualElement row = ALogConsoleWindow.MakeEntryRow();
        Assert.IsInstanceOf<Toggle>(row[0]);
        Assert.IsTrue(row[0].ClassListContains("entry-row__check"));
    }

    [Test]
    public void BindEntryRow_SetsCheckState() {
        VisualElement row = ALogConsoleWindow.MakeEntryRow();
        ALogConsoleWindow.BindEntryRow(row, Entry(ALogLevel.Log), true);
        Assert.IsTrue(((Toggle)row[0]).value);
        ALogConsoleWindow.BindEntryRow(row, Entry(ALogLevel.Log), false);
        Assert.IsFalse(((Toggle)row[0]).value);
    }

    [Test]
    public void FormatCopyText_IncludesCategoryMessageAndFrames() {
        var entry = new ALogEntry {
            Category = "Unity_Native",
            Message = "SocketException: reset",
            Frames = new List<ALogFrame> {
                new ALogFrame { Signature = "Socket.Send", FilePath = null, Line = 0 },
                new ALogFrame { Signature = "Foo.Bar", FilePath = "Foo.cs", Line = 12 },
            },
        };

        string text = ALogConsoleWindow.FormatCopyText(entry);

        Assert.That(text, Does.Contain("[Unity_Native] SocketException: reset"));
        Assert.That(text, Does.Contain("Socket.Send    <no source>"));
        Assert.That(text, Does.Contain("Foo.Bar    Foo.cs:12"));
    }

    [Test]
    public void FormatCopyText_NullEntry_IsEmpty() {
        Assert.AreEqual(string.Empty, ALogConsoleWindow.FormatCopyText((ALogEntry)null));
    }

    [Test]
    public void FormatCopyText_MultipleEntries_JoinsWithBlankLine() {
        var a = new ALogEntry { Category = "Network", Message = "one" };
        var b = new ALogEntry { Category = "UI", Message = "two" };

        string text = ALogConsoleWindow.FormatCopyText(new[] { a, b });

        Assert.That(text, Does.Contain("[Network] one"));
        Assert.That(text, Does.Contain("[UI] two"));
        Assert.That(text, Does.Contain("\n\n"));
    }

    [Test]
    public void FormatCopyText_EmptyList_IsEmpty() {
        Assert.AreEqual(string.Empty, ALogConsoleWindow.FormatCopyText(new ALogEntry[0]));
    }

    [Test]
    public void ApplySelectAll_AddsVisibleEntryIds() {
        var ids = new HashSet<int> { 9 };
        var entries = new[] {
            new ALogEntry { Id = 1 },
            new ALogEntry { Id = 2 },
        };

        ALogConsoleWindow.ApplySelectAll(ids, entries);

        Assert.IsTrue(ids.Contains(1));
        Assert.IsTrue(ids.Contains(2));
        Assert.IsTrue(ids.Contains(9));
    }

    [Test]
    public void ApplySelectNone_ClearsIds() {
        var ids = new HashSet<int> { 1, 2 };
        ALogConsoleWindow.ApplySelectNone(ids);
        Assert.AreEqual(0, ids.Count);
    }

    static Label MessageLabel(VisualElement row) {
        return (Label)row[3];
    }

    static ALogEntry Entry(ALogLevel level) {
        return new ALogEntry {
            Level = level,
            Category = "Network",
            Message = "msg",
        };
    }
}
