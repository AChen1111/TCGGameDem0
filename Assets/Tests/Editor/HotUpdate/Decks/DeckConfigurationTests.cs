#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using AChen.Configuration;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.AddressableAssets;

public sealed class DeckConfigurationTests
{
    string m_root;

    [SetUp]
    public void SetUp()
    {
        m_root = Path.Combine(Path.GetTempPath(), "achen-deck-tables-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(m_root);
        foreach (string file in Directory.GetFiles("TableData", "*.csv"))
            File.Copy(file, Path.Combine(m_root, Path.GetFileName(file)));
    }

    [TearDown]
    public void TearDown() => Directory.Delete(m_root, true);

    [Test]
    public void Source_tables_classify_every_card_and_keep_forbidden_limited_and_semi_limited_rules()
    {
        var sections = BinaryTableCsv.Load("TableData/card-deck-sections.csv");
        var cards = BinaryTableCsv.Load("TableData/Cards.csv");
        int id = sections.Column("CardId", "string");
        int section = sections.Column("Section", "string");
        CollectionAssert.AreEquivalent(cards.Rows.Select(x => x[0]), sections.Rows.Select(x => x[id]));
        Assert.AreEqual(92, sections.Rows.Length);
        Assert.AreEqual(55, sections.Rows.Count(x => (string)x[section] == "Main"));
        Assert.AreEqual(37, sections.Rows.Count(x => (string)x[section] == "Extra"));
        Assert.IsTrue(sections.Rows.Any(x => (string)x[id] == "01639384" && (string)x[section] == "Extra"));
        var bans = BinaryTable.Decode(BinaryTableCsv.Compile("TableData/card-banlist.csv"));
        bans.Column("CardId", "string");
        bans.Column("MaxCopies", "int");
        Assert.AreEqual(3, bans.Rows.Length);
        Assert.AreEqual(0, bans.Rows.Single(x => (string)x[0] == "23434538")[1]);
        Assert.AreEqual(1, bans.Rows.Single(x => (string)x[0] == "14558127")[1]);
        Assert.AreEqual(2, bans.Rows.Single(x => (string)x[0] == "21143940")[1]);
    }

    [TestCase(null)]
    [TestCase("CardId,MaxCopies\nstring,int\n01639384,-1\n")]
    [TestCase("CardId,MaxCopies\nstring,int\n01639384,4\n")]
    [TestCase("CardId,MaxCopies\nstring,int\nunknown,1\n")]
    [TestCase("CardId,MaxCopies\nstring,int\n,1\n")]
    [TestCase("CardId,MaxCopies\nstring,int\n01639384,1\n01639384,2\n")]
    [TestCase("CardId,WrongColumn\nstring,int\n")]
    public void Compile_rejects_missing_or_invalid_banlist(string csv)
    {
        string path = Path.Combine(m_root, "card-banlist.csv");
        if (csv == null) File.Delete(path); else File.WriteAllText(path, csv);
        var error = Assert.Throws<FormatException>(() => PublishedConfigBuilder.CompileDirectory(m_root));
        StringAssert.Contains("card-banlist", error.Message);
    }
    [TestCase("missing")]
    [TestCase("incomplete")]
    [TestCase("duplicate")]
    [TestCase("unknown")]
    [TestCase("invalid-section")]
    public void Compile_requires_exactly_one_valid_section_for_each_card(string failure)
    {
        string path = Path.Combine(m_root, "card-deck-sections.csv");
        var lines = File.ReadAllLines(path).ToList();
        switch (failure)
        {
            case "missing": File.Delete(path); break;
            case "incomplete": lines.RemoveAt(2); break;
            case "duplicate": lines.Add(lines[2]); break;
            case "unknown": lines.Add("unknown,Main"); break;
            case "invalid-section": lines[2] = lines[2].Split(',')[0] + ",Side"; break;
        }
        if (failure != "missing") File.WriteAllLines(path, lines);
        StringAssert.Contains("card-deck-sections", Assert.Throws<FormatException>(() => PublishedConfigBuilder.CompileDirectory(m_root)).Message);
    }

    [Test]
    public void Client_loads_both_tables_but_legacy_server_configuration_still_accepts_old_packages()
    {
        File.WriteAllText(Path.Combine(m_root, "card-banlist.csv"), "CardId,MaxCopies\nstring,int\n01639384,1\n");
        var files = PublishedConfigBuilder.CompileDirectory(m_root);
        var rules = DeckRulesConfiguration.Load(files);
        Assert.AreEqual(92, rules.CardCount);
        Assert.AreEqual(1, rules.GetMaxCopies("01639384"));
        Assert.AreEqual(3, rules.GetMaxCopies("00213326"));
        Assert.IsTrue(rules.TryGetSection("01639384", out var section));
        Assert.AreEqual(DeckSection.Extra, section);
        files.Remove("card-banlist");
        files.Remove("card-deck-sections");
        Assert.DoesNotThrow(() => GameConfigTables.Assemble(files));
        Assert.Throws<FormatException>(() => DeckRulesConfiguration.Load(files));
        Assert.AreEqual(1, rules.GetMaxCopies("01639384"));
    }

    [Test]
    public void Generated_deck_tables_match_source_and_have_packaged_addresses()
    {
        foreach (string name in new[] { DeckRulesConfiguration.SectionsTable, DeckRulesConfiguration.BanlistTable })
        {
            string assetPath = PublishedConfigBuilder.Root + "/" + name + ".bytes";
            CollectionAssert.AreEqual(BinaryTableCsv.Compile("TableData/" + name + ".csv"), File.ReadAllBytes(assetPath));
            var entry = AddressableAssetSettingsDefaultObject.Settings.FindAssetEntry(AssetDatabase.AssetPathToGUID(assetPath));
            Assert.NotNull(entry);
            Assert.AreEqual("Remote_GameConfig", entry.parentGroup.Name);
            Assert.AreEqual("GameConfig/" + name, entry.address);
            Assert.IsTrue(entry.labels.Contains(GameConfigTables.Label));
            Assert.IsTrue(entry.labels.Contains(GameConfigTables.GeneratedLabel));
        }
    }
}
#endif
