using NUnit.Framework;

public class LuaPadViewTests
{
    [Test]
    public void Placeholder_IsPrintHello()
    {
        Assert.AreEqual("print('hello from Lua Pad')", LuaPadView.Placeholder);
    }
}
