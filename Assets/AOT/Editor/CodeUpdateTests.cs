using NUnit.Framework;

public class CodeUpdateTests
{
    [Test]
    public void DllUrl_PointsAtLocalCdn()
    {
        Assert.AreEqual("http://127.0.0.1:8000/HybridCLR/HotUpdate.dll.bytes", CodeUpdate.DllUrl);
        Assert.AreEqual(CodeUpdate.DllUrl + ".hash", CodeUpdate.HashUrl);
    }

    [Test]
    public void HashOf_IsStable()
    {
        byte[] data = { 1, 2, 3 };
        Assert.AreEqual(CodeUpdate.HashOf(data), CodeUpdate.HashOf(data));
        Assert.AreNotEqual(CodeUpdate.HashOf(data), CodeUpdate.HashOf(new byte[] { 1, 2, 4 }));
    }

    [Test]
    public void IsCurrent_ComparesHashes()
    {
        Assert.IsTrue(CodeUpdate.IsCurrent("abc", "abc"));
        Assert.IsFalse(CodeUpdate.IsCurrent("abc", "abd"));
    }

    [Test]
    public void IsComplete_DefaultsFalse()
    {
        Assert.IsFalse(CodeUpdate.IsComplete);
    }
}
