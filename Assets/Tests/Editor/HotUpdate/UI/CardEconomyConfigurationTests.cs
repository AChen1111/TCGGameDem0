using System;
using System.Collections.Generic;
using System.IO;
using AChen.Configuration;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.AddressableAssets;

public sealed class CardEconomyConfigurationTests
{
    static Dictionary<string,byte[]> Files()=>new Dictionary<string,byte[]> {
        ["card-crafting"]=File.ReadAllBytes("Assets/GameConfiguration/card-crafting.bytes"),
        ["card-recycling"]=File.ReadAllBytes("Assets/GameConfiguration/card-recycling.bytes") };
    [Test]
    public void Tables_prices_addresses_and_hot_update_assembly_are_consistent()
    {
        Assert.AreEqual("HotUpdate",typeof(CardEconomyConfiguration).Assembly.GetName().Name);
        var rules=CardEconomyConfiguration.Load(Files());Assert.AreEqual(30,rules.CraftCostUr);
        for(int r=0;r<5;r++){Assert.AreEqual(10+5*r,rules.DismantleUr(r));Assert.AreEqual(10+5*r,rules.OverflowUr(r));}
        foreach(var pair in Files())
        {
            CollectionAssert.AreEqual(BinaryTableCsv.Compile("TableData/"+pair.Key+".csv"),pair.Value);
            var entry=AddressableAssetSettingsDefaultObject.Settings.FindAssetEntry(AssetDatabase.AssetPathToGUID("Assets/GameConfiguration/"+pair.Key+".bytes"));
            Assert.AreEqual("GameConfig/"+pair.Key,entry.address);Assert.AreEqual("Remote_GameConfig",entry.parentGroup.Name);
            Assert.IsTrue(entry.labels.Contains(GameConfigTables.Label));Assert.IsTrue(entry.labels.Contains(GameConfigTables.GeneratedLabel));
        }
    }
    [TestCase("card-crafting")][TestCase("card-recycling")]
    public void Missing_table_never_becomes_free(string name){var f=Files();f.Remove(name);Assert.Throws<FormatException>(()=>CardEconomyConfiguration.Load(f));}
    [TestCase("craft-zero")][TestCase("craft-duplicate")][TestCase("craft-rarity")]
    [TestCase("recycle-missing")][TestCase("recycle-duplicate")][TestCase("recycle-negative")][TestCase("wrong-type")]
    public void Runtime_rejects_invalid_configurations(string failure)
    {
        var f=Files();string name=failure.StartsWith("craft")?"card-crafting":"card-recycling";
        var table=BinaryTable.Decode(f[name]);
        switch(failure)
        {
            case "craft-zero":table.Rows[0][1]=0L;break;
            case "craft-duplicate":table.Rows=new[]{table.Rows[0],table.Rows[0]};break;
            case "craft-rarity":table.Rows[0][0]=1;break;
            case "recycle-missing":table.Rows=new[]{table.Rows[0]};break;
            case "recycle-duplicate":table.Rows[1][0]=0;break;
            case "recycle-negative":table.Rows[4][2]=-1L;break;
            case "wrong-type":f[name]=f["card-crafting"];Assert.Throws<FormatException>(()=>CardEconomyConfiguration.Load(f));return;
        }
        f[name]=table.Encode();Assert.Throws<FormatException>(()=>CardEconomyConfiguration.Load(f));
    }
    [Test]
    public void Grant_preserves_legacy_copies_and_only_converts_new_overflow()
    {
        var rules=CardEconomyConfiguration.Load(Files());var a=rules.Grant(5,2,0);Assert.AreEqual(0,a.Kept);Assert.AreEqual(20,a.Ur);
        var b=rules.Grant(2,3,4);Assert.AreEqual(1,b.Kept);Assert.AreEqual(60,b.Ur);
    }
}
