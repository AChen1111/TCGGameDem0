using System;
using System.Security.Cryptography;
using System.Text;
#if UNITY_EDITOR
using System.IO;
#endif
using UnityEngine;

namespace AChen.Networking
{
    public interface IAuthSessionStore
    {
        bool TryLoad(out string refreshToken);
        void Save(string refreshToken);
        void Clear();
    }

    public sealed class PlatformAuthSessionStore : IAuthSessionStore
    {
        const string KeyPrefix = "AChen.Auth.RefreshToken.";

#if UNITY_EDITOR
        readonly string m_filePath;
#else
        readonly string m_key;
#endif

        public PlatformAuthSessionStore(BackendConfig config)
#if UNITY_EDITOR
            : this(config, Application.streamingAssetsPath)
        {
        }

        public PlatformAuthSessionStore(BackendConfig config, string editorStorageRoot)
#endif
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            string serverKey = CreateServerKey(config.BaseUrl);
#if UNITY_EDITOR
            m_filePath = Path.Combine(
                editorStorageRoot,
                "AuthSessions",
                KeyPrefix + serverKey + ".txt");
#else
            m_key = KeyPrefix + serverKey;
#endif
        }

        public bool TryLoad(out string refreshToken)
        {
#if UNITY_EDITOR
            refreshToken = File.Exists(m_filePath)
                ? File.ReadAllText(m_filePath, Encoding.UTF8)
                : string.Empty;
#else
            refreshToken = PlayerPrefs.GetString(m_key, string.Empty);
#endif
            return !string.IsNullOrWhiteSpace(refreshToken);
        }

        public void Save(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                throw new ArgumentException("Refresh token cannot be empty.", nameof(refreshToken));
            }

#if UNITY_EDITOR
            Directory.CreateDirectory(Path.GetDirectoryName(m_filePath));
            File.WriteAllText(m_filePath, refreshToken, new UTF8Encoding(false));
#else
            PlayerPrefs.SetString(m_key, refreshToken);
            PlayerPrefs.Save();
#endif
        }

        public void Clear()
        {
#if UNITY_EDITOR
            if (File.Exists(m_filePath))
            {
                File.Delete(m_filePath);
            }
#else
            if (!PlayerPrefs.HasKey(m_key))
            {
                return;
            }

            PlayerPrefs.DeleteKey(m_key);
            PlayerPrefs.Save();
#endif
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
