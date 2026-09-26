using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using NUnit.Framework;
using UnityEditor;

public class CodeUpdateTests
{
    [Test]
    public void ManifestUrl_PointsAtContentEndpoint()
    {
        Assert.AreEqual(
            "http://127.0.0.1:5080/api/content/latest/StandaloneWindows64",
            CodeUpdate.ManifestUrl("http://127.0.0.1:5080", "development", "StandaloneWindows64", "1.0.0"));
    }

    [Test]
    public void Sha256Of_IsStable()
    {
        byte[] data = { 1, 2, 3 };
        Assert.AreEqual(CodeUpdate.Sha256Of(data), CodeUpdate.Sha256Of(data));
        Assert.AreNotEqual(CodeUpdate.Sha256Of(data), CodeUpdate.Sha256Of(new byte[] { 1, 2, 4 }));
    }

    [Test]
    public void HasExpectedSha256_ComparesHashes()
    {
        byte[] data = { 1, 2, 3 };
        Assert.IsTrue(CodeUpdate.HasExpectedSha256(data, CodeUpdate.Sha256Of(data)));
        Assert.IsFalse(CodeUpdate.HasExpectedSha256(data, CodeUpdate.Sha256Of(new byte[] { 1, 2, 4 })));
    }

    [Test]
    public void IsComplete_DefaultsFalse()
    {
        Assert.IsFalse(CodeUpdate.IsComplete);
    }

}
