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
        string insert = items.Find(i => i.Label == "function").Insert;
        StringAssert.Contains("${1:name}", insert);
        StringAssert.Contains("end", insert);
    }

    [Test]
    public void KeywordItems_If_IsBlockSnippet()
    {
        LuaPadKeyword kw = LuaPadTextUtil.KeywordItems("if").Find(i => i.Label == "if");
        StringAssert.Contains("${1:condition}", kw.Insert);
        StringAssert.Contains("then", kw.Insert);
        StringAssert.Contains("end", kw.Insert);
        Assert.AreEqual("if condition then .. end", kw.Detail);
    }

    [Test]
    public void KeywordItems_ControlStructures_AreBlockSnippets()
    {
        AssertSnippet("for", "${1:i}", "do", "end");
        AssertSnippet("while", "${1:condition}", "do", "end");
        AssertSnippet("repeat", "until", "${1:condition}", null);
        AssertSnippet("function", "${1:name}", "end", null);
    }

    static void AssertSnippet(string label, string a, string b, string c)
    {
        LuaPadKeyword kw = LuaPadTextUtil.KeywordItems(label).Find(i => i.Label == label);
        StringAssert.Contains(a, kw.Insert);
        StringAssert.Contains(b, kw.Insert);
        if (c != null)
        {
            StringAssert.Contains(c, kw.Insert);
        }
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
    public void NeedsLsp_AfterDotColonOrIdentPrefix()
    {
        Assert.IsTrue(LuaPadTextUtil.NeedsLsp("Log.", 4));
        Assert.IsTrue(LuaPadTextUtil.NeedsLsp("L", 1));
        Assert.IsTrue(LuaPadTextUtil.NeedsLsp("Log", 3));
        Assert.IsFalse(LuaPadTextUtil.NeedsLsp("", 0));
    }
}
