#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using AChen.Configuration;
using NUnit.Framework;

public sealed class GameConfigCsvEditorParserTests
{
    string m_Root;

    [SetUp]
    public void SetUp()
    {
        m_Root = Path.Combine(Path.GetTempPath(), "achen-tabledata-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(m_Root);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(m_Root)) Directory.Delete(m_Root, true);
    }

    [Test]
    public void CompileDirectory_builds_bytes_and_keeps_leading_zeros()
    {
        var files = PublishedConfigBuilder.CompileDirectory("TableData");
        Assert.AreEqual("bytes", GameConfigTables.Format("avatars"));
        Assert.AreEqual("bytes", GameConfigTables.Format("Cards"));
        Assert.AreEqual(0x32425441, BitConverter.ToInt32(files["avatars"], 0));
        Assert.AreEqual(0x32425441, BitConverter.ToInt32(files["Cards"], 0));
        var data = GameConfigTables.Assemble(files);
        Assert.IsTrue(data.AllCards.Any(x => x.CardId == "00213326"));
        Assert.IsTrue(Table.CardRow.LoadBytes(data.CardTable).Any(x => x.CardId == "00213326"));
        Assert.AreEqual(3, data.WallpaperOffsets[0].Sprite.Length);
        Assert.IsFalse(data.Extra.ContainsKey("Cards"));
        Assert.IsTrue(data.Extra.ContainsKey("avatars"));
    }

    [Test]
    public void CompileDirectory_adds_unknown_table_without_business_behavior()
    {
        CopyRequired("TableData");
        File.WriteAllText(Path.Combine(m_Root, "tips.csv"), "Id,Text\nint,string\n1,hello\n");
        var files = PublishedConfigBuilder.CompileDirectory(m_Root);
        var data = GameConfigTables.Assemble(files);
        Assert.AreEqual("hello", data.Extra["tips"][0]["Text"]);
        Assert.AreEqual(1L, Convert.ToInt64(data.Extra["tips"][0]["Id"]));
    }

    [Test]
    public void CompileDirectory_rejects_subdirectory_csv_and_keeps_existing_artifacts()
    {
        string existing = Path.Combine(PublishedConfigBuilder.Root, "avatars.bytes");
        byte[] before = File.ReadAllBytes(existing);
        CopyRequired("TableData");
        Directory.CreateDirectory(Path.Combine(m_Root, "nested"));
        File.WriteAllText(Path.Combine(m_Root, "nested", "extra.csv"), "Id\nint\n1\n");
        var error = Assert.Throws<FormatException>(() => PublishedConfigBuilder.CompileDirectory(m_Root));
        StringAssert.Contains("源表必须平铺", error.Message);
        CollectionAssert.AreEqual(before, File.ReadAllBytes(existing));
    }

    [Test]
    public void CompileDirectory_reports_type_and_duplicate_errors()
    {
        CopyRequired("TableData");
        File.WriteAllText(Path.Combine(m_Root, "avatars.csv"), "Id,Name\nguess,string\n1,A\n");
        var typeError = Assert.Throws<FormatException>(() => PublishedConfigBuilder.CompileDirectory(m_Root));
        StringAssert.Contains("avatars.csv", typeError.Message);
        StringAssert.Contains("第 2 行", typeError.Message);

        CopyRequired("TableData");
        File.WriteAllText(Path.Combine(m_Root, "1bad.csv"), "Id\nint\n1\n");
        var nameError = Assert.Throws<FormatException>(() => PublishedConfigBuilder.CompileDirectory(m_Root));
        StringAssert.Contains("表名无效", nameError.Message);
    }

    [Test]
    public void CompileDirectory_requires_all_known_tables()
    {
        CopyRequired("TableData");
        File.Delete(Path.Combine(m_Root, "wallpapers.csv"));
        var error = Assert.Throws<FormatException>(() => PublishedConfigBuilder.CompileDirectory(m_Root));
        StringAssert.Contains("缺少必需配置: wallpapers", error.Message);
    }

    [Test]
    public void ReadFields_keeps_quoted_commas_and_newlines()
    {
        var rows = GameConfigCsvEditorParser.ReadFields("A,B\nstring,string\n\"hello, world\",\"line1\nline2\"\n");
        Assert.AreEqual("hello, world", rows[2][0]);
        Assert.AreEqual("line1\nline2", rows[2][1]);
    }

    [Test]
    public void Generated_bytes_meta_keeps_existing_guid()
    {
        string meta = File.ReadAllText("Assets/GameConfiguration/avatars.bytes.meta");
        StringAssert.Contains("guid: 926a35f00a1b47744b8f79918673dc81", meta);
    }

    [Test]
    public void ConfigArtifacts_require_format()
    {
        var files = PublishedConfigBuilder.CompileDirectory("TableData");
        var artifacts = files.Select(pair => new ConfigArtifact
        {
            category = pair.Key,
            address = GameConfigTables.Address(pair.Key),
            format = GameConfigTables.Format(pair.Key),
            path = GameConfigTables.PackagePath(pair.Key),
            size = pair.Value.LongLength,
            sha256 = "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef"
        }).ToArray();
        ConfigArtifacts.Validate(artifacts, "");
        artifacts[0].format = null;
        Assert.Throws<FormatException>(() => ConfigArtifacts.Validate(artifacts, ""));
    }

    void CopyRequired(string source)
    {
        foreach (string path in Directory.GetFiles(source, "*.csv"))
            File.Copy(path, Path.Combine(m_Root, Path.GetFileName(path)), true);
    }
}
#endif
