using Newtonsoft.Json.Linq;
using NUnit.Framework;

public class LuaPadCompletionTests
{
    [Test]
    public void FromLsp_PutsParameterHintInDetail()
    {
        JObject page = LuaPadCompletion.FromLsp(JObject.Parse(
            "{\"label\":\"Error\",\"detail\":\"(strMessage, strCategory) -> nil\",\"insertText\":\"Error\",\"kind\":3}"));
        Assert.AreEqual("Error", (string)page["label"]);
        Assert.AreEqual("(strMessage, strCategory) -> nil", (string)page["detail"]);
        Assert.AreEqual("Error", (string)page["insertText"]);
        Assert.AreEqual(1, (int)page["kind"]);
    }

    [Test]
    public void FromLsp_UsesLabelDetailsWhenDetailMissing()
    {
        JObject page = LuaPadCompletion.FromLsp(JObject.Parse(
            "{\"label\":\"Info\",\"labelDetails\":{\"detail\":\"(strMessage, strCategory) -> nil\"},\"insertText\":\"Info\"}"));
        Assert.AreEqual("(strMessage, strCategory) -> nil", (string)page["detail"]);
        Assert.AreEqual("Info", (string)page["insertText"]);
    }

    [Test]
    public void FromLsp_ForwardsDocumentationValue()
    {
        JObject page = LuaPadCompletion.FromLsp(JObject.Parse(
            "{\"label\":\"Warn\",\"insertText\":\"Warn\",\"documentation\":{\"kind\":\"markdown\",\"value\":\"警告日志\"}}"));
        Assert.AreEqual("警告日志", (string)page["documentation"]);
    }

    [Test]
    public void FromLsp_EmptyLabel_ReturnsNull()
    {
        Assert.IsNull(LuaPadCompletion.FromLsp(new JObject()));
    }

    [Test]
    public void ToMonacoKind_MapsLspFunctionAndSnippet()
    {
        Assert.AreEqual(1, LuaPadCompletion.ToMonacoKind(3));
        Assert.AreEqual(27, LuaPadCompletion.ToMonacoKind(15));
        Assert.AreEqual(17, LuaPadCompletion.ToMonacoKind(14));
    }

    [Test]
    public void FromKeyword_SnippetSetsMonacoSnippetKind()
    {
        var kw = new LuaPadKeyword("if", "if ${1:condition} then\n\t$0\nend", "if condition then .. end");
        JObject page = LuaPadCompletion.FromKeyword(kw);
        Assert.AreEqual("if", (string)page["label"]);
        Assert.AreEqual("if condition then .. end", (string)page["detail"]);
        Assert.AreEqual(27, (int)page["kind"]);
        StringAssert.Contains("${1:condition}", (string)page["insertText"]);
    }

    [Test]
    public void BuildItems_DedupsLspAgainstKeywordLabel()
    {
        JArray items = LuaPadCompletion.BuildItems("p", new JArray
        {
            new JObject { ["label"] = "print", ["insertText"] = "print" },
        });
        int prints = 0;
        foreach (JToken it in items)
        {
            if ((string)it["label"] == "print")
            {
                prints++;
            }
        }
        Assert.AreEqual(1, prints);
    }
}
