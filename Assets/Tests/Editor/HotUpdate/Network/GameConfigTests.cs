using System;
using System.Collections.Generic;
using System.IO;
using AChen.Networking;
using NUnit.Framework;

public sealed class GameConfigTests
{
    string m_TemporaryRoot;

    [SetUp]
    public void SetUp()
    {
        m_TemporaryRoot = Path.Combine(
            Path.GetTempPath(),
            "achen-game-config-tests",
            Guid.NewGuid().ToString("N"));
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(m_TemporaryRoot))
        {
            Directory.Delete(m_TemporaryRoot, true);
        }
    }

    [Test]
    public void Validator_rejects_duplicate_avatar_ids()
    {
        var snapshot = new GameConfigSnapshot(
            1,
            1,
            DateTimeOffset.UtcNow,
            new[]
            {
                new AvatarConfig(1, "A", "Avatar_A", 0, true),
                new AvatarConfig(1, "B", "Avatar_B", 1, true)
            },
            Array.Empty<CardPackConfig>());

        Assert.Throws<GameConfigDataException>(() => GameConfigSnapshotValidator.Validate(snapshot));
    }

    [Test]
    public void Store_replaces_snapshot_atomically_and_indexes_by_stable_id()
    {
        var store = new GameConfigStore();
        int notifications = 0;
        store.ConfigChanged += _ => notifications++;
        DateTimeOffset now = DateTimeOffset.UtcNow;

        store.Replace(CreateSnapshot(1, 1000), "\"game-config-1\"", now, now, false);
        Assert.IsTrue(store.TryGetAvatar(1, out AvatarConfig avatar));
        Assert.AreEqual("Avatar_Default", avatar.ResourceKey);
        Assert.IsTrue(store.TryGetCardPack(1001, out CardPackConfig cardPack));
        Assert.AreEqual(1000, cardPack.PriceGold);

        store.Replace(CreateSnapshot(2, 1200), "\"game-config-2\"", now, now, false);
        Assert.AreEqual(2, store.Snapshot.Revision);
        Assert.AreEqual(1200, store.CardPacks[1001].PriceGold);
        Assert.AreEqual(2, notifications);
    }

    [Test]
    public void Store_uses_server_time_for_card_pack_visibility()
    {
        DateTimeOffset serverNow = new DateTimeOffset(2026, 8, 23, 10, 0, 0, TimeSpan.Zero);
        var store = new GameConfigStore();
        store.Replace(CreateSnapshot(1, 1000), "\"game-config-1\"", serverNow, DateTimeOffset.UtcNow, false);

        Assert.IsTrue(store.IsCardPackVisible(new CardPackConfig(
            1,
            "Active",
            "Pack_Active",
            1,
            serverNow.AddMinutes(-1),
            serverNow.AddMinutes(1),
            0,
            true)));
        Assert.IsFalse(store.IsCardPackVisible(new CardPackConfig(
            2,
            "Future",
            "Pack_Future",
            1,
            serverNow.AddMinutes(1),
            null,
            0,
            true)));
        Assert.IsFalse(store.IsCardPackVisible(new CardPackConfig(
            3,
            "Disabled",
            "Pack_Disabled",
            1,
            null,
            null,
            0,
            false)));
    }

    [Test]
    public void Cache_isolated_by_backend_and_falls_back_to_last_good_backup()
    {
        var first = new GameConfigCache(new BackendConfig("https://one.example.test"), m_TemporaryRoot);
        var second = new GameConfigCache(new BackendConfig("https://two.example.test"), m_TemporaryRoot);
        Assert.AreNotEqual(first.CachePath, second.CachePath);

        DateTimeOffset now = DateTimeOffset.UtcNow;
        first.Save(CreateSnapshot(1, 1000), "\"game-config-1\"", now, now);
        first.Save(CreateSnapshot(2, 1200), "\"game-config-2\"", now, now);
        File.WriteAllText(first.CachePath, "not-json");

        Assert.IsTrue(first.TryLoad(out CachedGameConfig cached));
        Assert.AreEqual(1, cached.Snapshot.Revision);
        Assert.AreEqual("\"game-config-1\"", cached.ETag);
    }

    [Test]
    public void Player_avatar_id_contract_is_nullable_integer()
    {
        Assert.AreEqual(typeof(int?), typeof(PlayerData).GetProperty(nameof(PlayerData.AvatarId)).PropertyType);
        Assert.AreEqual(typeof(IReadOnlyList<int>), typeof(PlayerData).GetProperty(nameof(PlayerData.OwnedAvatarIds)).PropertyType);
        Assert.AreEqual(typeof(int?), typeof(PlayerData).GetProperty(nameof(PlayerData.BackgroundId)).PropertyType);
    }

    static GameConfigSnapshot CreateSnapshot(long revision, long priceGold)
    {
        DateTimeOffset publishedAt = new DateTimeOffset(2026, 8, 23, 9, 0, 0, TimeSpan.Zero);
        return new GameConfigSnapshot(
            1,
            revision,
            publishedAt,
            new[] { new AvatarConfig(1, "默认头像", "Avatar_Default", 0, true) },
            new[]
            {
                new CardPackConfig(
                    1001,
                    "基础卡包",
                    "CardPack_Default",
                    priceGold,
                    null,
                    null,
                    0,
                    true)
            });
    }
}
