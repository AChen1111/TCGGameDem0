#if UNITY_EDITOR
using NUnit.Framework;

public sealed class GameConfigCsvEditorParserTests
{
    const string Header = "Table,Id,Name,ResourceKey,PriceGold,StartsAt,EndsAt,SortOrder,IsEnabled\r\n";

    [Test]
    public void Parser_maps_all_three_tables_and_sorts_rows()
    {
        EditorGameConfigDocument document = GameConfigCsvEditorParser.Parse(Header +
            "Avatar,0,默认头像,a_00,200,,,2,True\r\n" +
            "Wallpaper,1,默认壁纸,c_01,500,,,1,True\r\n" +
            "CardPack,2,卡包,c_02,100,2026-09-01T00:00:00Z,2026-10-01T00:00:00Z,0,True\r\n");

        Assert.AreEqual(0, document.avatars[0].id);
        Assert.AreEqual(200, document.avatars[0].priceGold);
        Assert.AreEqual("c_01", document.wallpapers[0].resourceKey);
        Assert.AreEqual("卡包", document.cardPacks[0].title);
    }

    [Test]
    public void Parser_reports_source_line_for_table_specific_date_error()
    {
        var exception = Assert.Throws<System.FormatException>(() =>
            GameConfigCsvEditorParser.Parse(Header +
                "Avatar,0,默认头像,a_00,200,2026-09-01T00:00:00Z,,0,True\r\n"));
        StringAssert.Contains("第 2 行", exception.Message);
    }

    [Test]
    public void Parser_rejects_duplicate_resource_keys_case_insensitively()
    {
        Assert.Throws<System.FormatException>(() => GameConfigCsvEditorParser.Parse(Header +
            "Wallpaper,0,A,wallpaper,100,,,0,True\r\n" +
            "Wallpaper,1,B,WALLPAPER,100,,,1,True\r\n"));
    }
}
#endif
