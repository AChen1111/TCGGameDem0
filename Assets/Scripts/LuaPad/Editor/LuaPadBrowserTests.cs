using NUnit.Framework;

public class LuaPadBrowserTests
{
    [Test]
    public void BuildStartArguments_PassesPortAndUrlOnly()
    {
        Assert.AreEqual(
            "--port 123 --url http://127.0.0.1:4567/",
            LuaPadBrowser.BuildStartArguments("http://127.0.0.1:4567", 123));
    }
}
