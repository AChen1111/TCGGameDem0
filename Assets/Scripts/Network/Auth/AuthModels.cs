using System;
using System.Collections.Generic;

namespace AChen.Networking
{
    public sealed class AuthUser
    {
        public Guid Id { get; }
        public string Username { get; }
        public DateTimeOffset CreatedAt { get; }

        internal AuthUser(Guid id, string username, DateTimeOffset createdAt)
        {
            Id = id;
            Username = username;
            CreatedAt = createdAt;
        }
    }

    public sealed class OwnedCardData
    {
        public string CardId { get; }
        public int Rarity { get; }
        public int Count { get; }

        internal OwnedCardData(string cardId, int rarity, int count)
        {
            CardId = cardId ?? string.Empty;
            Rarity = rarity;
            Count = count;
        }
    }

    public sealed class CardDrawResult
    {
        public string CardId { get; }//卡牌ID
        public int Rarity { get; }//卡牌稀有度
        public string SourcePool { get; }//卡牌来源池

        internal CardDrawResult(string cardId, int rarity, string sourcePool)
        {
            CardId = cardId ?? string.Empty;
            Rarity = rarity;
            SourcePool = sourcePool ?? string.Empty;
        }
    }

    public sealed class CardDrawResponse
    {
        public IReadOnlyList<CardDrawResult> Results { get; }
        public PlayerData Player { get; }

        internal CardDrawResponse(IReadOnlyList<CardDrawResult> results, PlayerData player)
        {
            Results = results ?? Array.Empty<CardDrawResult>();
            Player = player;
        }
    }

    public sealed class PlayerData
    {
        public Guid Id { get; }
        public string Nickname { get; }
        public int? AvatarId { get; }
        public IReadOnlyList<int> OwnedAvatarIds { get; }
        public int? BackgroundId { get; }
        public IReadOnlyList<int> OwnedBackgroundIds { get; }
        public IReadOnlyList<OwnedCardData> OwnedCards { get; }
        public long Gold { get; }
        public long Revision { get; }
        public DateTimeOffset CreatedAt { get; }
        public DateTimeOffset UpdatedAt { get; }

        internal PlayerData(
            Guid id,
            string nickname,
            int? avatarId,
            IReadOnlyList<int> ownedAvatarIds,
            int? backgroundId,
            IReadOnlyList<int> ownedBackgroundIds,
            IReadOnlyList<OwnedCardData> ownedCards,
            long gold,
            long revision,
            DateTimeOffset createdAt,
            DateTimeOffset updatedAt)
        {
            Id = id;
            Nickname = nickname;
            AvatarId = avatarId;
            OwnedAvatarIds = ownedAvatarIds ?? Array.Empty<int>();
            BackgroundId = backgroundId;
            OwnedBackgroundIds = ownedBackgroundIds ?? Array.Empty<int>();
            OwnedCards = ownedCards ?? Array.Empty<OwnedCardData>();
            Gold = gold;
            Revision = revision;
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
        }
    }

    public sealed class BackendApiException : Exception
    {
        public long StatusCode { get; }
        public string Code { get; }
        public IReadOnlyDictionary<string, string[]> Errors { get; }

        internal BackendApiException(
            long statusCode,
            string code,
            string message,
            IReadOnlyDictionary<string, string[]> errors = null)
            : base(message)
        {
            StatusCode = statusCode;
            Code = code;
            Errors = errors ?? new Dictionary<string, string[]>();
        }
    }
}
