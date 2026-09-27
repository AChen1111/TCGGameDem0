using System;
using System.Collections.Generic;

namespace AChen.Networking
{
    public static class InboxKinds
    {
        public const string FriendRequest = "friendRequest";
        public const string Gift = "gift";
    }

    public enum InboxRowAction
    {
        Accept,
        Reject,
        Claim
    }

    public static class FriendRowKeys
    {
        public const string Duel = "ui.friends.duel";
        public const string Add = "ui.friends.add";
        public const string Waiting = "ui.friends.waiting";

        public static string ActionKey(bool isFriend, bool isPending) =>
            isFriend ? Duel : isPending ? Waiting : Add;
    }

    public static class GiftTitleKeys
    {
        public const string Gold = "ui.gifts.pack_gold";
        public const string Card = "ui.gifts.pack_card";
        public const string Mixed = "ui.gifts.pack_mixed";

        public static string Resolve(string titleKey, long gold, IReadOnlyList<OwnedCardData> cards)
        {
            if (!string.IsNullOrWhiteSpace(titleKey))
            {
                return titleKey.Trim();
            }

            bool hasCard = cards != null && cards.Count > 0;
            if (gold > 0 && hasCard)
            {
                return Mixed;
            }

            return hasCard ? Card : Gold;
        }
    }

    public sealed class FriendSummaryData
    {
        public Guid Id { get; }
        public string Nickname { get; }
        public int? AvatarId { get; }
        public int AvatarFrameId { get; }

        internal FriendSummaryData(Guid id, string nickname, int? avatarId, int avatarFrameId)
        {
            Id = id;
            Nickname = nickname ?? string.Empty;
            AvatarId = avatarId;
            AvatarFrameId = avatarFrameId;
        }
    }

    public sealed class FriendSearchHitData
    {
        public Guid Id { get; }
        public string Nickname { get; }
        public int? AvatarId { get; }
        public int AvatarFrameId { get; }
        public bool IsFriend { get; }
        public bool IsPending { get; }

        internal FriendSearchHitData(Guid id, string nickname, int? avatarId, bool isFriend, bool isPending, int avatarFrameId)
        {
            Id = id;
            Nickname = nickname ?? string.Empty;
            AvatarId = avatarId;
            AvatarFrameId = avatarFrameId;
            IsFriend = isFriend;
            IsPending = isPending;
        }

        public FriendSearchHitData AsPending() =>
            new FriendSearchHitData(Id, Nickname, AvatarId, IsFriend, true, AvatarFrameId);

        public static FriendSearchHitData FromFriend(FriendSummaryData friend) =>
            new FriendSearchHitData(friend.Id, friend.Nickname, friend.AvatarId, true, false, friend.AvatarFrameId);
    }

    public sealed class InboxItemData
    {
        public string Kind { get; }
        public Guid Id { get; }
        public DateTimeOffset CreatedAt { get; }
        public Guid? PlayerId { get; }
        public string Nickname { get; }
        public int? AvatarId { get; }
        public int? AvatarFrameId { get; }
        public long Gold { get; }
        public IReadOnlyList<OwnedCardData> Cards { get; }
        public string TitleKey { get; }

        internal InboxItemData(
            string kind,
            Guid id,
            DateTimeOffset createdAt,
            Guid? playerId,
            string nickname,
            int? avatarId,
            long gold,
            IReadOnlyList<OwnedCardData> cards,
            string titleKey, int? avatarFrameId)
        {
            Kind = kind ?? string.Empty;
            Id = id;
            CreatedAt = createdAt;
            PlayerId = playerId;
            Nickname = nickname ?? string.Empty;
            AvatarId = avatarId;
            AvatarFrameId = avatarFrameId;
            Gold = gold;
            Cards = cards ?? Array.Empty<OwnedCardData>();
            TitleKey = titleKey ?? string.Empty;
        }

        public bool IsGift => string.Equals(Kind, InboxKinds.Gift, StringComparison.Ordinal);
    }

    public readonly struct GiftRewardSpec
    {
        public string Kind { get; }
        public long Count { get; }
        public string CardId { get; }
        public int Rarity { get; }

        public GiftRewardSpec(string kind, long count, string cardId = null, int rarity = 0)
        {
            Kind = kind ?? string.Empty;
            Count = count;
            CardId = cardId ?? string.Empty;
            Rarity = rarity;
        }
    }
}
