using System;
using System.Collections.Generic;

namespace AChen.Networking
{
    public sealed class GameConfigStore
    {
        IReadOnlyDictionary<int, AvatarConfig> m_avatars = new Dictionary<int, AvatarConfig>();
        IReadOnlyDictionary<int, CardPackConfig> m_cardPacks = new Dictionary<int, CardPackConfig>();
        DateTimeOffset m_serverTime;
        DateTimeOffset m_serverTimeReceivedAtUtc;

        public event Action<GameConfigSnapshot> ConfigChanged;

        public GameConfigSnapshot Snapshot { get; private set; }
        public string ETag { get; private set; }
        public bool HasSnapshot => Snapshot != null;
        public bool IsStale { get; private set; }
        public IReadOnlyDictionary<int, AvatarConfig> Avatars => m_avatars;
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
                throw new GameConfigDataException("Game configuration ETag is missing.");
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

            m_avatars = avatars;
            m_cardPacks = cardPacks;
            Snapshot = snapshot;
            ETag = etag;
            m_serverTime = serverTime;
            m_serverTimeReceivedAtUtc = receivedAtUtc;
            IsStale = isStale;
            ConfigChanged?.Invoke(snapshot);
        }

        public void MarkChecked(DateTimeOffset serverTime, DateTimeOffset receivedAtUtc)
        {
            if (!HasSnapshot)
            {
                throw new InvalidOperationException("Cannot validate an empty game configuration store.");
            }

            m_serverTime = serverTime;
            m_serverTimeReceivedAtUtc = receivedAtUtc;
            IsStale = false;
        }

        public void MarkStale()
        {
            if (HasSnapshot)
            {
                IsStale = true;
            }
        }

        public bool TryGetAvatar(int id, out AvatarConfig avatar) => m_avatars.TryGetValue(id, out avatar);

        public bool TryGetCardPack(int id, out CardPackConfig cardPack) => m_cardPacks.TryGetValue(id, out cardPack);

        public bool IsCardPackVisible(CardPackConfig cardPack)
        {
            DateTimeOffset now = ServerNow;
            return cardPack != null &&
                   cardPack.IsEnabled &&
                   (!cardPack.StartsAt.HasValue || cardPack.StartsAt <= now) &&
                   (!cardPack.EndsAt.HasValue || cardPack.EndsAt > now);
        }
    }
}
