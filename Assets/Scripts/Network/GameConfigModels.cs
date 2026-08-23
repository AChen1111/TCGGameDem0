using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace AChen.Networking
{
    [Preserve]
    public sealed class GameConfigSnapshot
    {
        public int SchemaVersion { get; }
        public long Revision { get; }
        public DateTimeOffset PublishedAt { get; }
        public IReadOnlyList<AvatarConfig> Avatars { get; }
        public IReadOnlyList<CardPackConfig> CardPacks { get; }

        [JsonConstructor]
        public GameConfigSnapshot(
            int schemaVersion,
            long revision,
            DateTimeOffset publishedAt,
            IEnumerable<AvatarConfig> avatars,
            IEnumerable<CardPackConfig> cardPacks)
        {
            SchemaVersion = schemaVersion;
            Revision = revision;
            PublishedAt = publishedAt;
            Avatars = (avatars ?? Array.Empty<AvatarConfig>()).ToArray();
            CardPacks = (cardPacks ?? Array.Empty<CardPackConfig>()).ToArray();
        }
    }

    [Preserve]
    public sealed class AvatarConfig
    {
        public int Id { get; }
        public string Name { get; }
        public string ResourceKey { get; }
        public int SortOrder { get; }
        public bool IsEnabled { get; }

        [JsonConstructor]
        public AvatarConfig(int id, string name, string resourceKey, int sortOrder, bool isEnabled)
        {
            Id = id;
            Name = name;
            ResourceKey = resourceKey;
            SortOrder = sortOrder;
            IsEnabled = isEnabled;
        }
    }

    [Preserve]
    public sealed class CardPackConfig
    {
        public int Id { get; }
        public string Title { get; }
        public string CoverResourceKey { get; }
        public long PriceGold { get; }
        public DateTimeOffset? StartsAt { get; }
        public DateTimeOffset? EndsAt { get; }
        public int SortOrder { get; }
        public bool IsEnabled { get; }

        [JsonConstructor]
        public CardPackConfig(
            int id,
            string title,
            string coverResourceKey,
            long priceGold,
            DateTimeOffset? startsAt,
            DateTimeOffset? endsAt,
            int sortOrder,
            bool isEnabled)
        {
            Id = id;
            Title = title;
            CoverResourceKey = coverResourceKey;
            PriceGold = priceGold;
            StartsAt = startsAt;
            EndsAt = endsAt;
            SortOrder = sortOrder;
            IsEnabled = isEnabled;
        }
    }

    public sealed class GameConfigDataException : Exception
    {
        public GameConfigDataException(string message) : base(message) { }
    }

    public static class GameConfigSnapshotValidator
    {
        public const int SupportedSchemaVersion = 1;

        public static void Validate(GameConfigSnapshot snapshot)
        {
            if (snapshot == null)
            {
                throw new GameConfigDataException("Game configuration is missing.");
            }

            if (snapshot.SchemaVersion != SupportedSchemaVersion)
            {
                throw new GameConfigDataException(
                    $"Unsupported game configuration schema: {snapshot.SchemaVersion}.");
            }

            if (snapshot.Revision <= 0 || snapshot.PublishedAt == default)
            {
                throw new GameConfigDataException("Game configuration version is invalid.");
            }

            ValidateAvatars(snapshot.Avatars);
            ValidateCardPacks(snapshot.CardPacks);
        }

        static void ValidateAvatars(IReadOnlyList<AvatarConfig> avatars)
        {
            var ids = new HashSet<int>();
            var resourceKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (AvatarConfig avatar in avatars ?? Array.Empty<AvatarConfig>())
            {
                if (avatar == null || avatar.Id <= 0 ||
                    string.IsNullOrWhiteSpace(avatar.Name) || avatar.Name.Length > 64 ||
                    string.IsNullOrWhiteSpace(avatar.ResourceKey) || avatar.ResourceKey.Length > 128)
                {
                    throw new GameConfigDataException("Avatar configuration contains an invalid item.");
                }

                if (!ids.Add(avatar.Id) || !resourceKeys.Add(avatar.ResourceKey))
                {
                    throw new GameConfigDataException("Avatar configuration contains duplicate IDs or resource keys.");
                }
            }
        }

        static void ValidateCardPacks(IReadOnlyList<CardPackConfig> cardPacks)
        {
            var ids = new HashSet<int>();
            foreach (CardPackConfig cardPack in cardPacks ?? Array.Empty<CardPackConfig>())
            {
                if (cardPack == null || cardPack.Id <= 0 || cardPack.PriceGold < 0 ||
                    string.IsNullOrWhiteSpace(cardPack.Title) || cardPack.Title.Length > 64 ||
                    string.IsNullOrWhiteSpace(cardPack.CoverResourceKey) || cardPack.CoverResourceKey.Length > 128 ||
                    cardPack.StartsAt.HasValue && cardPack.EndsAt.HasValue && cardPack.EndsAt <= cardPack.StartsAt)
                {
                    throw new GameConfigDataException("Card pack configuration contains an invalid item.");
                }

                if (!ids.Add(cardPack.Id))
                {
                    throw new GameConfigDataException("Card pack configuration contains duplicate IDs.");
                }
            }
        }
    }
}
