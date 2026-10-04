using System.Collections.Generic;
using System.IO;
using System.Linq;
using AChen.Configuration;
using NUnit.Framework;

public sealed class DeckPackagedConfigTests
{
    [Test]
    public void Deck_rules_are_in_the_hot_update_assembly()
    {
        Assert.AreEqual("HotUpdate", typeof(DeckRulesConfiguration).Assembly.GetName().Name);
        Assert.AreEqual("HotUpdate", typeof(DeckSection).Assembly.GetName().Name);
        Assert.IsNull(typeof(StartupContext).Assembly.GetType("AChen.Configuration.DeckRulesConfiguration"));
    }

    [Test]
    public void Packaged_tables_decode_into_the_same_rules_snapshot()
    {
        var files = new Dictionary<string, byte[]>();
        foreach (string name in new[] { "Cards", DeckRulesConfiguration.SectionsTable, DeckRulesConfiguration.BanlistTable })
        {
            files.Add(name, File.ReadAllBytes("Assets/GameConfiguration/" + name + ".bytes"));
        }
        var rules = DeckRulesConfiguration.Load(files);
        Assert.AreEqual(118, rules.CardCount);
        Assert.IsTrue(rules.TryGetSection("01639384", out var section));
        Assert.AreEqual(DeckSection.Extra, section);
        Assert.AreEqual(3, rules.GetMaxCopies("01639384"));
    }

    [Test]
    public void Published_catalog_has_four_disjoint_pools_and_two_alternate_arts()
    {
        var files = Directory.GetFiles("Assets/GameConfiguration", "*.bytes")
            .ToDictionary(path => Path.GetFileNameWithoutExtension(path), File.ReadAllBytes);
        var data = GameConfigTables.Assemble(files);
        CollectionAssert.AreEquivalent(new[] { "Card01", "Card02", "Card03", "Card04", "CardGeneric" },
            data.PoolEntries.Select(x => x.PoolKey).Distinct());
        CollectionAssert.AreEquivalent(data.AllCards.Select(x => x.CardId), data.PoolEntries.Select(x => x.CardId));
        Assert.AreEqual(data.AllCards.Length, data.PoolEntries.Select(x => x.CardId).Distinct().Count());
        Assert.AreEqual("青眼白龙到来", data.Catalog.CardPacks.Single(x => x.CoverResourceKey == "c_01").Title);
        Assert.AreEqual("龙女仆", data.Catalog.CardPacks.Single(x => x.CoverResourceKey == "c_04").Title);
        Assert.AreEqual("Card04", data.Catalog.CardPacks.Single(x => x.CoverResourceKey == "c_04").PoolKey);
        Assert.AreEqual("Card03", data.Catalog.CardPacks.Single(x => x.CoverResourceKey == "c_05").PoolKey);
        Assert.AreEqual("Card02", data.Catalog.CardPacks.Single(x => x.CoverResourceKey == "c_10").PoolKey);
        Assert.IsTrue(data.Catalog.CardPacks.Where(x => x.CoverResourceKey != "c_01" && x.CoverResourceKey != "c_04"
            && x.CoverResourceKey != "c_05" && x.CoverResourceKey != "c_10")
            .All(x => x.Title == "泛用卡池" && x.PoolKey == "CardGeneric"));
        Assert.AreEqual("41232647", data.ResolveCardId("41232648"));
        Assert.AreEqual("14558127", data.ResolveCardId("14558128"));
        Assert.AreEqual("94145021", data.ResolveCardId("94145022"));
        Assert.AreEqual(8, data.SpecialMaterials.Length);
        CollectionAssert.AreEquivalent(new[] { 0, 1, 3 }, data.RarityWeights.Select(x => x.Rarity));
    }
}
