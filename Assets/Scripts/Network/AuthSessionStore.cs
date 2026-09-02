using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace AChen.Networking
{
    public interface IAuthSessionStore
    {
        bool TryLoad(out string refreshToken);
        void Save(string refreshToken);
        void Clear();
    }

    public sealed class PlayerPrefsAuthSessionStore : IAuthSessionStore
    {
        const string KeyPrefix = "AChen.Auth.RefreshToken.";

        readonly string m_key;

        public PlayerPrefsAuthSessionStore(BackendConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            m_key = KeyPrefix + CreateServerKey(config.BaseUrl);
        }

        public bool TryLoad(out string refreshToken)
        {
            refreshToken = PlayerPrefs.GetString(m_key, string.Empty);
            return !string.IsNullOrWhiteSpace(refreshToken);
        }

        public void Save(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                throw new ArgumentException("Refresh token cannot be empty.", nameof(refreshToken));
            }

            PlayerPrefs.SetString(m_key, refreshToken);
            PlayerPrefs.Save();
        }

        public void Clear()
        {
            if (!PlayerPrefs.HasKey(m_key))
            {
                return;
            }

            PlayerPrefs.DeleteKey(m_key);
            PlayerPrefs.Save();
        }

        static string CreateServerKey(string baseUrl)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(baseUrl));
                return BitConverter.ToString(hash, 0, 8).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}
