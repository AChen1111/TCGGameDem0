using System;
using System.Collections.Generic;
using AChen.Events;

namespace AChen.Networking
{
    public sealed class GameConfigStore
    {
        IReadOnlyDictionary<int, AvatarConfig> m_avatars = new Dictionary<int, AvatarConfig>();
        IReadOnlyDictionary<int, WallpaperConfig> m_wallpapers = new Dictionary<int, WallpaperConfig>();
        IReadOnlyDictionary<int, CardPackConfig> m_cardPacks = new Dictionary<int, CardPackConfig>();
        DateTimeOffset m_serverTime;
        DateTimeOffset m_serverTimeReceivedAtUtc;

        public GameConfigSnapshot Snapshot { get; private set; }
        public string ETag { get; private set; }
        public bool HasSnapshot => Snapshot != null;
        public bool IsStale { get; private set; }
        public IReadOnlyDictionary<int, AvatarConfig> Avatars => m_avatars;
        public IReadOnlyDictionary<int, WallpaperConfig> Wallpapers => m_wallpapers;
        public IReadOnlyDictionary<int, CardPackConfig> CardPacks => m_cardPacks;
        public DateTimeOffset ServerNow =>
            m_serverTime + (DateTimeOffset.UtcNow - m_serverTimeReceivedAtUtc);

        public void Replace(
            GameConfigSnapshot snapshot,
            string etag,
            DateTimeOffset serverTime,
            DateTimeOffset receivedAtUtc,
            bool isStale)
        {
            GameConfigSnapshotValidator.Validate(snapshot);
            if (string.IsNullOrWhiteSpace(etag))
            {
                throw new GameConfigDataException("缺少游戏配置版本标识");
            }

            var avatars = new Dictionary<int, AvatarConfig>(snapshot.Avatars.Count);
            foreach (AvatarConfig avatar in snapshot.Avatars)
            {
                avatars.Add(avatar.Id, avatar);
            }

            var cardPacks = new Dictionary<int, CardPackConfig>(snapshot.CardPacks.Count);
            foreach (CardPackConfig cardPack in snapshot.CardPacks)
            {
                cardPacks.Add(cardPack.Id, cardPack);
            }

            var wallpapers = new Dictionary<int, WallpaperConfig>(snapshot.Wallpapers.Count);
            foreach (WallpaperConfig wallpaper in snapshot.Wallpapers)
            {
                wallpapers.Add(wallpaper.Id, wallpaper);
            }

            m_avatars = avatars;
            m_wallpapers = wallpapers;
            m_cardPacks = cardPacks;
            Snapshot = snapshot;
            ETag = etag;
            m_serverTime = serverTime;
            m_serverTimeReceivedAtUtc = receivedAtUtc;
            IsStale = isStale;
            EventCenter.Dispatch(GameEvent.GameConfigChanged, snapshot, isStale);
        }

        public void MarkChecked(DateTimeOffset serverTime, DateTimeOffset receivedAtUtc)
        {
            if (!HasSnapshot)
            {
                throw new InvalidOperationException("Cannot validate an empty game configuration store.");
            }

            m_serverTime = serverTime;
            m_serverTimeReceivedAtUtc = receivedAtUtc;
            SetStale(false);
        }

        public void MarkStale()
        {
            if (HasSnapshot)
            {
                SetStale(true);
            }
        }

        void SetStale(bool isStale)
        {
            if (IsStale == isStale) return;

            IsStale = isStale;
            EventCenter.Dispatch(GameEvent.GameConfigChanged, Snapshot, isStale);
        }

        public bool TryGetAvatar(int id, out AvatarConfig avatar) => m_avatars.TryGetValue(id, out avatar);

        public bool TryGetWallpaper(int id, out WallpaperConfig wallpaper) => m_wallpapers.TryGetValue(id, out wallpaper);

        public bool TryGetCardPack(int id, out CardPackConfig cardPack) => m_cardPacks.TryGetValue(id, out cardPack);

        public bool IsCardPackVisible(CardPackConfig cardPack) =>
            IsScheduleVisible(cardPack != null && cardPack.IsEnabled, cardPack?.StartsAt, cardPack?.EndsAt);

        public bool IsCosmeticVisible(CosmeticConfig cosmetic) =>
            IsScheduleVisible(cosmetic != null && cosmetic.IsEnabled, cosmetic?.StartsAt, cosmetic?.EndsAt);

        bool IsScheduleVisible(bool enabled, DateTimeOffset? startsAt, DateTimeOffset? endsAt)
        {
            if (!enabled) return false;
            DateTimeOffset now = ServerNow;
            return (!startsAt.HasValue || startsAt <= now) &&
                   (!endsAt.HasValue || endsAt > now);
        }
    }
}
