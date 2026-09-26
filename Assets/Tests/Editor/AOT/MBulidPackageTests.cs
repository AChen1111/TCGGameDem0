using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using NUnit.Framework;
using AChen.Configuration;
using UnityEngine;

public class MBulidPackageTests
{
    string root;
    MBulidPackage package;
    const string Platform = "Android";
    const string Version = "1.0.1";
    string DirectoryPath => package.DirectoryFor(Platform, Version);
    [SetUp] public void Setup()
    {
        root = Path.Combine(Path.GetTempPath(), "mBulid-tests-" + Guid.NewGuid().ToString("N"));
        package = new MBulidPackage(root);
        package.ResetVersion(Platform, Version);
    }
    [TearDown] public void Cleanup()
    {
        if (Directory.Exists(root)) Directory.Delete(root, true);
    }
    [TestCase("../escape")]
    [TestCase("a/b")]
    [TestCase("a\\b")]
    [TestCase("a:b")]
    [TestCase("a*")]
    [TestCase("a.")]
    [TestCase("")]
    public void Invalid_version_is_rejected(string version) => Assert.Throws<FormatException>(() => package.ResetVersion(Platform, version));

    [Test] public void Reset_recreates_only_same_platform_and_version()
    {
        string win = package.DirectoryFor("StandaloneWindows64", Version);
        package.ResetVersion("StandaloneWindows64", Version);
        package.ResetVersion(Platform, "1.0.2");
        File.WriteAllText(Path.Combine(DirectoryPath, "old.txt"), "old");
        File.WriteAllText(Path.Combine(win, "keep.txt"), "keep");
        package.ResetVersion(Platform, Version);
        Assert.AreEqual("1.0.1_安卓", Path.GetFileName(DirectoryPath));
        Assert.AreEqual("1.0.1_win", Path.GetFileName(win));
        Assert.IsFalse(File.Exists(Path.Combine(DirectoryPath, "old.txt")));
        Assert.IsTrue(File.Exists(Path.Combine(win, "keep.txt")));
        Assert.IsTrue(Directory.Exists(package.DirectoryFor(Platform, "1.0.2")));
    }
    void Code()
    {
        // 仅用于文件验证单元测试, 正式构建使用 CompileDllCommand 的 Player 输出.
        var dll = AppDomain.CurrentDomain.GetAssemblies().Single(x => x.GetName().Name == "HotUpdate").Location;
        package.BeginCode(Platform, Version);
        package.CompleteCode(Platform, Version, dll);
    }
    void Resources()
    {
        package.BeginResources(Platform, Version);
        string remote = Path.Combine(DirectoryPath, "Addressables");
        Directory.CreateDirectory(remote);
        foreach (string name in new[] { "catalog_1.0.1.bin", "catalog_1.0.1.hash", "config.bundle" })
            File.WriteAllBytes(Path.Combine(remote, name), new byte[] { 1, 2, 3 });
        package.CompleteResources(Platform, Version, Directory.GetFiles(remote), Path.Combine(Application.dataPath, "GameConfiguration"));
    }
    [Test] public void Archive_contains_raw_dll_version_and_verified_files()
    {
        Code(); Resources();
        string archive = package.CreateArchive(Platform, Version, out var manifest);
        Assert.AreEqual(DevelopmentProtocol.Version, manifest.schemaVersion);
        Assert.AreEqual(Version, manifest.contentVersion);
        Assert.AreEqual("HybridCLR/HotUpdate.dll", manifest.hotUpdatePath);
        using (var zip = ZipFile.OpenRead(archive))
        {
            Assert.AreEqual(manifest.files.Length + 1, zip.Entries.Count);
            Assert.IsNotNull(zip.GetEntry("manifest.json"));
            Assert.IsNotNull(zip.GetEntry("HybridCLR/HotUpdate.dll.sha256"));
        }
    }
    [Test] public void Resource_rebuild_preserves_dll_but_invalidates_publication()
    {
        Code(); Resources();
        var hash = package.Read(Platform, Version).dll.sha256;
        package.BeginResources(Platform, Version);
        Assert.AreEqual(hash, MBulidPackage.HashFile(Path.Combine(DirectoryPath, "HybridCLR", "HotUpdate.dll")));
        Assert.Throws<InvalidOperationException>(() => package.Validate(Platform, Version));
        Assert.IsNull(package.Read(Platform, Version).resources);
        Resources();
        Assert.DoesNotThrow(() => package.Validate(Platform, Version));
    }
    [Test] public void Failed_code_rebuild_cannot_reuse_previous_dll()
    {
        Code(); Resources(); package.BeginCode(Platform, Version);
        Assert.Throws<InvalidOperationException>(() => package.CompleteCode(Platform, Version, Path.Combine(root, "missing.dll")));
        Assert.IsNull(package.Read(Platform, Version).dll);
        Assert.Throws<InvalidOperationException>(() => package.Validate(Platform, Version));
        Assert.IsNotNull(package.Read(Platform, Version).resources);
    }
    [TestCase("HybridCLR/HotUpdate.dll")]
    [TestCase("HybridCLR/HotUpdate.dll.sha256")]
    [TestCase("Addressables/config.bundle")]
    [TestCase("GameConfig/Cards.bytes")]
    public void Tampered_artifact_cannot_be_published(string relative)
    {
        Code(); Resources(); File.AppendAllText(MBulidPackage.SafePath(DirectoryPath, relative), "changed");
        Assert.Throws<IOException>(() => package.Validate(Platform, Version));
    }
    [Test] public void Wrong_assembly_is_rejected()
    {
        Assert.Throws<InvalidOperationException>(() => package.CompleteCode(Platform, Version, typeof(MBulidPackageTests).Assembly.Location));
        Assert.IsNull(package.Read(Platform, Version).dll);
    }
}
