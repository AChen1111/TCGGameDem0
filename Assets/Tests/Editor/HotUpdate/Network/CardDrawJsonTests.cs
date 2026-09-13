using System;
using AChen.Networking;
using NUnit.Framework;

public sealed class CardDrawJsonTests
{
    const string PlayerJson =
        "{" +
        "\"id\":\"11111111-1111-1111-1111-111111111111\"," +
        "\"nickname\":\"LocalPlayer\"," +
        "\"avatarId\":0," +
        "\"ownedAvatarIds\":[0]," +
        "\"backgroundId\":1," +
        "\"ownedBackgroundIds\":[1]," +
        "\"ownedCards\":[" +
        "{\"cardId\":\"26077389\",\"rarity\":0,\"count\":2}," +
        "{\"cardId\":\"26077389\",\"rarity\":1,\"count\":1}" +
        "]," +
        "\"gold\":0," +
        "\"revision\":3," +
        "\"createdAt\":\"2026-09-13T00:00:00+00:00\"," +
        "\"updatedAt\":\"2026-09-13T00:00:00+00:00\"" +
        "}";

    [Test]
    public void Bootstrap_json_maps_owned_cards_with_string_id()
    {
        PlayerData player = AuthApi.ParsePlayerJson(PlayerJson);

        Assert.AreEqual(2, player.OwnedCards.Count);
        Assert.AreEqual(typeof(string), player.OwnedCards[0].CardId.GetType());
        Assert.AreEqual(typeof(int), player.OwnedCards[0].Rarity.GetType());
        Assert.AreEqual(typeof(int), player.OwnedCards[0].Count.GetType());
        Assert.AreEqual("26077389", player.OwnedCards[0].CardId);
        Assert.AreEqual(0, player.OwnedCards[0].Rarity);
        Assert.AreEqual(2, player.OwnedCards[0].Count);
        Assert.AreEqual("26077389", player.OwnedCards[1].CardId);
        Assert.AreEqual(1, player.OwnedCards[1].Rarity);
        Assert.AreEqual(1, player.OwnedCards[1].Count);
    }

    [Test]
    public void Draw_json_maps_results_and_player_owned_cards()
    {
        string json = "{\"results\":[{\"cardId\":\"08491308\",\"rarity\":1,\"sourcePool\":\"Card03\"}],\"player\":" + PlayerJson + "}";

        CardDrawResponse response = AuthApi.ParseDrawJson(json);

        Assert.AreEqual(1, response.Results.Count);
        Assert.AreEqual("08491308", response.Results[0].CardId);
        Assert.AreEqual(1, response.Results[0].Rarity);
        Assert.AreEqual("Card03", response.Results[0].SourcePool);
        Assert.AreEqual(typeof(string), response.Results[0].CardId.GetType());
        Assert.AreEqual(typeof(int), response.Results[0].Rarity.GetType());
        Assert.AreEqual(2, response.Player.OwnedCards.Count);
        Assert.AreEqual(new Guid("11111111-1111-1111-1111-111111111111"), response.Player.Id);
    }

    [Test]
    public void Pool_json_maps_cards_and_source_pool()
    {
        const string json =
            "{\"poolKey\":\"Card01\",\"cards\":[{\"cardId\":\"14558127\",\"sourcePool\":\"Card01\"},{\"cardId\":\"89631139\",\"sourcePool\":\"Card01\"}]}";

        GachaPoolData pool = AuthApi.ParseGachaPoolJson(json);

        Assert.AreEqual("Card01", pool.PoolKey);
        Assert.AreEqual(2, pool.Cards.Count);
        Assert.AreEqual("14558127", pool.Cards[0].CardId);
        Assert.AreEqual("Card01", pool.Cards[0].SourcePool);
        Assert.AreEqual("89631139", pool.Cards[1].CardId);
    }
}
