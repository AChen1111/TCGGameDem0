using System;
using System.Collections.Generic;

namespace AChen.Networking
{
    public sealed class AuthUser
    {
        public Guid Id { get; }
        public string Username { get; }
        public string Email { get; }
        public DateTimeOffset CreatedAt { get; }

        internal AuthUser(Guid id, string username, string email, DateTimeOffset createdAt)
        {
            Id = id;
            Username = username;
            Email = email;
            CreatedAt = createdAt;
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
