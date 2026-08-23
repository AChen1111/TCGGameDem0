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
            "http://127.0.0.1:5080/api/content/manifests/latest?channel=development&platform=StandaloneWindows64&appVersion=1.0.0",
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

    [TestCase(BuildTarget.StandaloneWindows64, "StandaloneWindows64")]
    [TestCase(BuildTarget.Android, "Android")]
    [TestCase(BuildTarget.iOS, "iOS")]
    public void PlatformName_MapsSupportedBuildTargets(BuildTarget target, string expected)
    {
        Assert.AreEqual(expected, ContentReleasePackageBuilder.PlatformName(target));
    }

    [TestCase("0.1.0")]
    [TestCase("1.0.0-beta.1+build.9")]
    public void IsStrictSemVer_AcceptsValidVersions(string value)
    {
        Assert.IsTrue(ContentReleasePackageBuilder.IsStrictSemVer(value));
    }

    [TestCase("1.0")]
    [TestCase("01.0.0")]
    [TestCase("1.0.0-01")]
    public void IsStrictSemVer_RejectsInvalidVersions(string value)
    {
        Assert.IsFalse(ContentReleasePackageBuilder.IsStrictSemVer(value));
    }

    [Test]
    public void ResolveContentUrl_StaysOnConfiguredBackend()
    {
        Assert.AreEqual(
            "http://127.0.0.1:5080/content/releases/id/file.bundle",
            CodeUpdate.ResolveContentUrl(
                "http://127.0.0.1:5080",
                "/content/releases/id/file.bundle"));
        Assert.Throws<InvalidDataException>(() => CodeUpdate.ResolveContentUrl(
            "http://127.0.0.1:5080",
            "https://example.com/file.bundle"));
    }

    [Test]
    public void BuildFromFiles_CreatesCompleteHashedReleasePackage()
    {
        string root = Path.Combine(Path.GetTempPath(), "AChenContentPackageTests", Guid.NewGuid().ToString("N"));
        string addressables = Path.Combine(root, "addressables");
        string output = Path.Combine(root, "output");
        string dll = Path.Combine(root, "HotUpdate.dll.bytes");
        Directory.CreateDirectory(addressables);
        File.WriteAllBytes(dll, new byte[] { 1, 2, 3 });
        File.WriteAllBytes(Path.Combine(addressables, "catalog_0.1.0.bin"), new byte[] { 4, 5 });
        File.WriteAllText(Path.Combine(addressables, "catalog_0.1.0.hash"), "catalog-hash");
        File.WriteAllBytes(Path.Combine(addressables, "game.bundle"), new byte[] { 6, 7, 8, 9 });

        try
        {
            ContentReleasePackage package = ContentReleasePackageBuilder.BuildFromFiles(
                "StandaloneWindows64",
                "0.1.0",
                "0.1.1",
                addressables,
                dll,
                output);

            Assert.AreEqual(ContentReleasePackageBuilder.Sha256OfFile(package.ZipPath), package.ArchiveSha256);
            using (ZipArchive archive = ZipFile.OpenRead(package.ZipPath))
            {
                string[] entries = archive.Entries.Select(value => value.FullName).ToArray();
                CollectionAssert.Contains(entries, "release-manifest.json");
                CollectionAssert.Contains(entries, "HybridCLR/HotUpdate.dll.bytes");
                CollectionAssert.Contains(entries, "Addressables/catalog_0.1.0.bin");
                CollectionAssert.Contains(entries, "Addressables/catalog_0.1.0.hash");
                CollectionAssert.Contains(entries, "Addressables/game.bundle");

                ZipArchiveEntry manifestEntry = archive.GetEntry("release-manifest.json");
                using (var reader = new StreamReader(manifestEntry.Open()))
                {
                    string json = reader.ReadToEnd();
                    StringAssert.Contains("\"schemaVersion\": 1", json);
                    StringAssert.Contains("\"contentVersion\": \"0.1.1\"", json);
                    StringAssert.Contains(ContentReleasePackageBuilder.Sha256OfFile(dll), json);
                }
            }
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, true);
            }
        }
    }
}
