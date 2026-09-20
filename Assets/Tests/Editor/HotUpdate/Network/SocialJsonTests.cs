using System.Collections.Generic;
using AChen.Networking;
using NUnit.Framework;

public sealed class SocialJsonTests
{
    const string FriendJson =
        "[" +
        "{\"id\":\"11111111-1111-1111-1111-111111111111\",\"nickname\":\"Alice\",\"avatarId\":2}," +
        "{\"id\":\"22222222-2222-2222-2222-222222222222\",\"nickname\":\"Bob\",\"avatarId\":0}" +
        "]";

    const string SearchJson =
        "[" +
        "{\"id\":\"22222222-2222-2222-2222-222222222222\",\"nickname\":\"Bob\",\"avatarId\":0,\"isFriend\":true}," +
        "{\"id\":\"33333333-3333-3333-3333-333333333333\",\"nickname\":\"Bobby\",\"avatarId\":1,\"isFriend\":false,\"isPending\":true}" +
        "]";

    const string InboxJson =
        "[" +
        "{\"kind\":\"friendRequest\",\"id\":\"aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa\"," +
        "\"createdAt\":\"2026-09-20T00:00:00+00:00\",\"playerId\":\"11111111-1111-1111-1111-111111111111\"," +
        "\"nickname\":\"Alice\",\"avatarId\":2,\"gold\":0,\"cards\":[]}," +
        "{\"kind\":\"gift\",\"id\":\"bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb\"," +
        "\"createdAt\":\"2026-09-20T01:00:00+00:00\",\"nickname\":null,\"avatarId\":null," +
        "\"gold\":250,\"titleKey\":\"ui.gifts.pack_mixed\"," +
        "\"cards\":[{\"cardId\":\"26077389\",\"rarity\":1,\"count\":2}]}" +
        "]";

    [Test]
    public void Friends_json_maps_id_and_nickname()
    {
        IReadOnlyList<FriendSummaryData> friends = AuthApi.ParseFriendsJson(FriendJson);

        Assert.AreEqual(2, friends.Count);
        Assert.AreEqual("Alice", friends[0].Nickname);
        Assert.AreEqual(2, friends[0].AvatarId);
        Assert.AreEqual("Bob", friends[1].Nickname);
    }

    [Test]
    public void Search_json_maps_friend_flag()
    {
        IReadOnlyList<FriendSearchHitData> hits = AuthApi.ParseFriendSearchJson(SearchJson);

        Assert.AreEqual(2, hits.Count);
        Assert.IsTrue(hits[0].IsFriend);
        Assert.IsFalse(hits[0].IsPending);
        Assert.IsFalse(hits[1].IsFriend);
        Assert.IsTrue(hits[1].IsPending);
        Assert.AreEqual("Bobby", hits[1].Nickname);
    }

    [Test]
    public void Inbox_json_maps_request_and_gift_rewards()
    {
        IReadOnlyList<InboxItemData> items = AuthApi.ParseInboxJson(InboxJson);

        Assert.AreEqual(2, items.Count);
        Assert.AreEqual(InboxKinds.FriendRequest, items[0].Kind);
        Assert.AreEqual("Alice", items[0].Nickname);
        Assert.IsTrue(items[1].IsGift);
        Assert.AreEqual("ui.gifts.pack_mixed", items[1].TitleKey);
        Assert.AreEqual(250, items[1].Gold);
        Assert.AreEqual("26077389", items[1].Cards[0].CardId);
        Assert.AreEqual(1, items[1].Cards[0].Rarity);
        Assert.AreEqual(2, items[1].Cards[0].Count);
    }

    [Test]
    public void Factory_builds_gold_and_card_specs()
    {
        IReadOnlyList<InboxItemData> items = AuthApi.ParseInboxJson(InboxJson);
        List<GiftRewardSpec> specs = GiftRewardFactory.BuildSpecs(items[1].Gold, items[1].Cards);

        Assert.AreEqual(2, specs.Count);
        Assert.AreEqual(GiftRewardFactory.GoldKind, specs[0].Kind);
        Assert.AreEqual(250, specs[0].Count);
        Assert.AreEqual(GiftRewardFactory.CardKind, specs[1].Kind);
        Assert.AreEqual("26077389", specs[1].CardId);
        Assert.AreEqual(1, specs[1].Rarity);
        Assert.AreEqual(2, specs[1].Count);
    }

    [Test]
    public void Factory_skips_empty_rewards()
    {
        List<GiftRewardSpec> specs = GiftRewardFactory.BuildSpecs(0, null);
        Assert.AreEqual(0, specs.Count);
    }

    [Test]
    public void Row_button_key_switches_duel_and_add()
    {
        Assert.AreEqual("ui.friends.duel", FriendRowKeys.ActionKey(true, false));
        Assert.AreEqual("ui.friends.add", FriendRowKeys.ActionKey(false, false));
        Assert.AreEqual("ui.friends.waiting", FriendRowKeys.ActionKey(false, true));
    }

    [Test]
    public void Friend_error_codes_map_to_copy_keys()
    {
        Assert.AreEqual("err.friend_not_found", LocalizationService.ErrorKey("FRIEND_NOT_FOUND"));
        Assert.AreEqual("err.friend_self", LocalizationService.ErrorKey("FRIEND_SELF"));
        Assert.AreEqual("err.friend_already", LocalizationService.ErrorKey("FRIEND_ALREADY"));
        Assert.AreEqual("err.friend_pending", LocalizationService.ErrorKey("FRIEND_PENDING"));
        Assert.AreEqual("err.friend_request_failed", LocalizationService.ErrorKey("FRIEND_REQUEST_FAILED"));
        Assert.AreEqual("err.friend_accept_failed", LocalizationService.ErrorKey("FRIEND_ACCEPT_FAILED"));
        Assert.AreEqual("err.friend_reject_failed", LocalizationService.ErrorKey("FRIEND_REJECT_FAILED"));
        Assert.AreEqual("err.gift_claim_failed", LocalizationService.ErrorKey("GIFT_CLAIM_FAILED"));
    }
}
