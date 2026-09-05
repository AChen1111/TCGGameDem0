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

    public sealed class PlayerData
    {
        public Guid Id { get; }
        public string Nickname { get; }
        public int? AvatarId { get; }
        public IReadOnlyList<int> OwnedAvatarIds { get; }
        public int? BackgroundId { get; }
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
