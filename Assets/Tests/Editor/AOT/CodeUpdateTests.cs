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

    [Test]
    public void EditorLocalSession_DoesNotDependOnWorkbenchReceipt()
    {
        var fields = typeof(AChen.Configuration.ContentSession).GetFields(
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
        var previous = fields.Select(field => field.GetValue(null)).ToArray();
        bool wasComplete = CodeUpdate.IsComplete;
        try
        {
            AChen.Configuration.ContentSession.ConfigHash = "previous-remote-config";
            AChen.Configuration.ContentSession.CatalogUrl = "https://example.invalid/catalog.bin";
            CodeUpdate.BindEditorLocalSession("http://127.0.0.1:5080", "development", "Editor", "1.0.0");
            AChen.Configuration.ContentSession.Bind(CodeUpdate.Context);

            Assert.AreEqual("Editor", AChen.Configuration.ContentSession.Target);
            Assert.AreEqual(CodeUpdate.EditorLocalReleaseId, AChen.Configuration.ContentSession.ReleaseId);
            Assert.IsTrue(AChen.Configuration.ContentSession.UseLocalAssets);
            Assert.IsNull(AChen.Configuration.ContentSession.ConfigHash);
            Assert.IsNull(AChen.Configuration.ContentSession.Configs);
            Assert.IsNull(AChen.Configuration.ContentSession.CatalogUrl);
            Assert.IsTrue(CodeUpdate.IsComplete);
        }
        finally
        {
            for (int i = 0; i < fields.Length; i++) fields[i].SetValue(null, previous[i]);
            typeof(CodeUpdate).GetProperty(nameof(CodeUpdate.IsComplete)).SetValue(null, wasComplete);
        }
    }

}
