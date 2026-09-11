using System;
using System.Collections.Generic;
using System.Reflection;
using AChen.Events;
using AChen.Networking;
using AChen.Player;
using NUnit.Framework;

public sealed class PlayerStateEventTests
{
    [Test]
    public void Gold_change_does_not_notify_profile_or_inventory_views()
    {
        Guid id = Guid.NewGuid();
        using (var recorder = new Recorder())
        {
            Publish(CreatePlayer(id, 100), CreatePlayer(id, 200));
            CollectionAssert.AreEqual(new[] { "gold" }, recorder.Events);
        }
    }

    [Test]
    public void Avatar_purchase_does_not_notify_wallpaper_or_avatar_display()
    {
        Guid id = Guid.NewGuid();
        using (var recorder = new Recorder())
        {
            Publish(CreatePlayer(id, 100), CreatePlayer(id, 80, new[] { 1, 2 }));
            CollectionAssert.AreEqual(new[] { "gold", "ownedAvatars" }, recorder.Events);
        }
    }

    [Test]
    public void Account_switch_replays_equal_values_and_logout_clears_all_views()
    {
        PlayerData first = CreatePlayer(Guid.NewGuid(), 100);
        PlayerData second = CreatePlayer(Guid.NewGuid(), 100);
        string[] expected = { "gold", "nickname", "avatar", "background", "ownedAvatars", "ownedWallpapers" };
        using (var recorder = new Recorder())
        {
            Publish(first, second);
            CollectionAssert.AreEqual(expected, recorder.Events);
            recorder.Events.Clear();
            Publish(second, null);
            CollectionAssert.AreEqual(expected, recorder.Events);
        }
    }

    [Test]
    public void Unchanged_snapshot_does_not_refresh_field_subscribers()
    {
        Guid id = Guid.NewGuid();
        using (var recorder = new Recorder())
        {
            Publish(CreatePlayer(id, 100), CreatePlayer(id, 100));
            Assert.IsEmpty(recorder.Events);
        }
    }

    static PlayerData CreatePlayer(Guid id, long gold, int[] avatars = null)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        return (PlayerData)Activator.CreateInstance(typeof(PlayerData), BindingFlags.Instance | BindingFlags.NonPublic,
            null, new object[] { id, "player", (int?)1, avatars ?? new[] { 1 }, (int?)10, new[] { 10 }, gold, 1L, now, now }, null);
    }

    static void Publish(PlayerData previous, PlayerData current) =>
        PlayerChangePublisher.Publish(previous, current);

    sealed class Recorder : IDisposable
    {
        public readonly List<string> Events = new List<string>();

        public Recorder()
        {
            EventCenter.AddListener(GameEvent.PlayerGoldChanged, Gold);
            EventCenter.AddListener(GameEvent.PlayerNicknameChanged, Nickname);
            EventCenter.AddListener(GameEvent.PlayerAvatarChanged, Avatar);
            EventCenter.AddListener(GameEvent.PlayerBackgroundChanged, Background);
            EventCenter.AddListener(GameEvent.PlayerOwnedAvatarsChanged, Avatars);
            EventCenter.AddListener(GameEvent.PlayerOwnedWallpapersChanged, Wallpapers);
        }

        public void Dispose()
        {
            EventCenter.RemoveListener(GameEvent.PlayerGoldChanged, Gold);
            EventCenter.RemoveListener(GameEvent.PlayerNicknameChanged, Nickname);
            EventCenter.RemoveListener(GameEvent.PlayerAvatarChanged, Avatar);
            EventCenter.RemoveListener(GameEvent.PlayerBackgroundChanged, Background);
            EventCenter.RemoveListener(GameEvent.PlayerOwnedAvatarsChanged, Avatars);
            EventCenter.RemoveListener(GameEvent.PlayerOwnedWallpapersChanged, Wallpapers);
        }

        void Gold(long? value) => Events.Add("gold");
        void Nickname(Guid? id, string value) => Events.Add("nickname");
        void Avatar(int? value) => Events.Add("avatar");
        void Background(int? value) => Events.Add("background");
        void Avatars(PlayerData value) => Events.Add("ownedAvatars");
        void Wallpapers(PlayerData value) => Events.Add("ownedWallpapers");
    }
}
