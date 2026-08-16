using NUnit.Framework;

public class LuaPadTextUtilTests
{
    [Test]
    public void LineChar_AfterDotOnFirstLine()
    {
        LuaPadTextUtil.LineChar("Log.", 4, out int line, out int character);
        Assert.AreEqual(0, line);
        Assert.AreEqual(4, character);
    }

    [Test]
    public void EffectiveCursor_FallsBackToEndWhenCaretUnreliable()
    {
        Assert.AreEqual(4, LuaPadTextUtil.EffectiveCursor("Log.", 0));
        Assert.AreEqual(4, LuaPadTextUtil.EffectiveCursor("Log.", 4));
        Assert.AreEqual(1, LuaPadTextUtil.EffectiveCursor("p", 0));
    }

    [Test]
    public void ShouldComplete_AfterDotOrIdentPrefix()
    {
        Assert.IsTrue(LuaPadTextUtil.ShouldComplete("Log.", 4));
        Assert.IsTrue(LuaPadTextUtil.ShouldComplete("p", 1));
        Assert.IsTrue(LuaPadTextUtil.ShouldComplete("f", 1));
        Assert.IsFalse(LuaPadTextUtil.ShouldComplete("", 0));
    }

    [Test]
    public void PrefixAt_ReadsIdentBeforeCursor()
    {
        Assert.AreEqual("p", LuaPadTextUtil.PrefixAt("p", 1));
        Assert.AreEqual("f", LuaPadTextUtil.PrefixAt("f", 1));
        Assert.AreEqual("", LuaPadTextUtil.PrefixAt("Log.", 4));
        Assert.AreEqual("In", LuaPadTextUtil.PrefixAt("Log.In", 6));
    }

    [Test]
    public void KeywordItems_P_IncludesPrintWithParens()
    {
        var items = LuaPadTextUtil.KeywordItems("p");
        CollectionAssert.Contains(items.ConvertAll(i => i.Label), "print");
        Assert.AreEqual("print()", items.Find(i => i.Label == "print").Insert);
    }

    [Test]
    public void KeywordItems_F_IncludesFunction()
    {
        var items = LuaPadTextUtil.KeywordItems("f");
        CollectionAssert.Contains(items.ConvertAll(i => i.Label), "function");
        Assert.AreEqual("function", items.Find(i => i.Label == "function").Insert);
    }

    [Test]
    public void ApplyCompletion_PrintReplacesPrefix()
    {
        Assert.AreEqual("print()", LuaPadTextUtil.ApplyCompletion("p", 1, "print()"));
    }

    [Test]
    public void CaretAfterInsert_PrintLandsInsideParens()
    {
        Assert.AreEqual(6, LuaPadTextUtil.CaretAfterInsert(0, "print()"));
    }

    [Test]
    public void ApplyCompletion_InsertsAfterDot()
    {
        Assert.AreEqual("Log.Info", LuaPadTextUtil.ApplyCompletion("Log.", 4, "Info"));
    }

    [Test]
    public void ApplyCompletion_ReplacesPartialIdent()
    {
        Assert.AreEqual("Log.Info", LuaPadTextUtil.ApplyCompletion("Log.In", 6, "Info"));
    }

    [Test]
    public void NeedsLsp_OnlyAfterDotOrColon()
    {
        Assert.IsTrue(LuaPadTextUtil.NeedsLsp("Log.", 4));
        Assert.IsFalse(LuaPadTextUtil.NeedsLsp("p", 1));
    }
}
