using System.Collections.Generic;
using System.IO;
using AChen.Configuration;
using NUnit.Framework;

public sealed class DeckPackagedConfigTests
{
    [Test]
    public void Packaged_tables_decode_into_the_same_rules_snapshot()
    {
        var files = new Dictionary<string, byte[]>();
        foreach (string name in new[] { "Cards", DeckRulesConfiguration.SectionsTable, DeckRulesConfiguration.BanlistTable })
        {
            files.Add(name, File.ReadAllBytes("Assets/GameConfiguration/" + name + ".bytes"));
        }
        var rules = DeckRulesConfiguration.Load(files);
        Assert.AreEqual(94, rules.CardCount);
        Assert.IsTrue(rules.TryGetSection("01639384", out var section));
        Assert.AreEqual(DeckSection.Extra, section);
        Assert.AreEqual(3, rules.GetMaxCopies("01639384"));
    }
}
