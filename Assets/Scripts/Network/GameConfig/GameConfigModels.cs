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
        public IReadOnlyList<WallpaperConfig> Wallpapers { get; }
        public IReadOnlyList<CardPackConfig> CardPacks { get; }

        [JsonConstructor]
        public GameConfigSnapshot(
            int schemaVersion,
            long revision,
            DateTimeOffset publishedAt,
            IEnumerable<AvatarConfig> avatars,
            IEnumerable<WallpaperConfig> wallpapers,
            IEnumerable<CardPackConfig> cardPacks)
        {
            SchemaVersion = schemaVersion;
            Revision = revision;
            PublishedAt = publishedAt;
            Avatars = (avatars ?? Array.Empty<AvatarConfig>()).ToArray();
            Wallpapers = (wallpapers ?? Array.Empty<WallpaperConfig>()).ToArray();
            CardPacks = (cardPacks ?? Array.Empty<CardPackConfig>()).ToArray();
        }
    }

    /// <summary>头像、壁纸等外观类商品的公共配置: 同一套字段、同一套校验规则.</summary>
    [Preserve]
    public abstract class CosmeticConfig
    {
        public int Id { get; }
        public string Name { get; }
        public string ResourceKey { get; }
        public long PriceGold { get; }
        public int SortOrder { get; }
        public bool IsEnabled { get; }

        public DateTimeOffset? StartsAt { get; }
        public DateTimeOffset? EndsAt { get; }

        protected CosmeticConfig(int id, string name, string resourceKey, long priceGold, int sortOrder, bool isEnabled, DateTimeOffset? startsAt, DateTimeOffset? endsAt)
        {
            Id = id;
            Name = name;
            ResourceKey = resourceKey;
            PriceGold = priceGold;
            SortOrder = sortOrder;
            IsEnabled = isEnabled;
            StartsAt = startsAt;
            EndsAt = endsAt;
        }
    }

    [Preserve]
    public sealed class AvatarConfig : CosmeticConfig
    {
        [JsonConstructor]
        public AvatarConfig(int id, string name, string resourceKey, long priceGold, int sortOrder, bool isEnabled, DateTimeOffset? startsAt = null, DateTimeOffset? endsAt = null)
            : base(id, name, resourceKey, priceGold, sortOrder, isEnabled, startsAt, endsAt)
        {
        }
    }

    [Preserve]
    public sealed class WallpaperConfig : CosmeticConfig
    {
        [JsonConstructor]
        public WallpaperConfig(int id, string name, string resourceKey, long priceGold, int sortOrder, bool isEnabled, DateTimeOffset? startsAt = null, DateTimeOffset? endsAt = null)
            : base(id, name, resourceKey, priceGold, sortOrder, isEnabled, startsAt, endsAt)
        {
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
        public const int SupportedSchemaVersion = 2;

        public static void Validate(GameConfigSnapshot snapshot)
        {
            if (snapshot == null)
            {
                throw new GameConfigDataException("缺少游戏配置");
            }

            if (snapshot.SchemaVersion != SupportedSchemaVersion)
            {
                throw new GameConfigDataException(
                    $"不支持的游戏配置结构版本：{snapshot.SchemaVersion}");
            }

            if (snapshot.Revision <= 0 || snapshot.PublishedAt == default)
            {
                throw new GameConfigDataException("游戏配置版本无效");
            }

            ValidateCosmetics(snapshot.Avatars, "头像");
            ValidateCosmetics(snapshot.Wallpapers, "壁纸");
            ValidateCardPacks(snapshot.CardPacks);
        }

        static void ValidateCosmetics<T>(IReadOnlyList<T> items, string label) where T : CosmeticConfig
        {
            var ids = new HashSet<int>();
            var resourceKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (T item in items ?? Array.Empty<T>())
            {
                if (item == null || item.Id < 0 || item.PriceGold < 0 ||
                    string.IsNullOrWhiteSpace(item.Name) || item.Name.Length > 64 ||
                    string.IsNullOrWhiteSpace(item.ResourceKey) || item.ResourceKey.Length > 128 ||
                    item.StartsAt.HasValue && item.EndsAt.HasValue && item.EndsAt <= item.StartsAt)
                {
                    throw new GameConfigDataException(label + "配置包含无效项目");
                }

                if (!ids.Add(item.Id) || !resourceKeys.Add(item.ResourceKey))
                {
                    throw new GameConfigDataException(label + "配置包含重复的 ID 或资源键");
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
                    throw new GameConfigDataException("卡包配置包含无效项目");
                }

                if (!ids.Add(cardPack.Id))
                {
                    throw new GameConfigDataException("卡包配置包含重复 ID");
                }
            }
        }
    }
}
