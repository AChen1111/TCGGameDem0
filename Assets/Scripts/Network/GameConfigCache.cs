using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;
using UnityEngine.Scripting;

namespace AChen.Networking
{
    public sealed class CachedGameConfig
    {
        public GameConfigSnapshot Snapshot { get; }
        public string ETag { get; }
        public DateTimeOffset ServerTime { get; }
        public DateTimeOffset CheckedAtUtc { get; }

        internal CachedGameConfig(
            GameConfigSnapshot snapshot,
            string etag,
            DateTimeOffset serverTime,
            DateTimeOffset checkedAtUtc)
        {
            Snapshot = snapshot;
            ETag = etag;
            ServerTime = serverTime;
            CheckedAtUtc = checkedAtUtc;
        }
    }

    public sealed class GameConfigCache
    {
        static readonly JsonSerializerSettings s_jsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Include
        };

        readonly string m_cachePath;
        readonly string m_backupPath;

        public string CachePath => m_cachePath;

        public GameConfigCache(BackendConfig config, string rootPath = null)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            string root = string.IsNullOrWhiteSpace(rootPath)
                ? Path.Combine(Application.persistentDataPath, "GameConfig")
                : rootPath;
            string serverKey = CreateServerKey(config.BaseUrl);
            string directory = Path.Combine(root, serverKey);
            m_cachePath = Path.Combine(directory, "bootstrap-v1.json");
            m_backupPath = m_cachePath + ".bak";
        }

        public bool TryLoad(out CachedGameConfig cached)
        {
            if (TryLoadFile(m_cachePath, out cached))
            {
                return true;
            }

            return TryLoadFile(m_backupPath, out cached);
        }

        public void Save(
            GameConfigSnapshot snapshot,
            string etag,
            DateTimeOffset serverTime,
            DateTimeOffset checkedAtUtc)
        {
            GameConfigSnapshotValidator.Validate(snapshot);
            var envelope = new CacheEnvelope
            {
                Snapshot = snapshot,
                ETag = etag,
                ServerTime = serverTime,
                CheckedAtUtc = checkedAtUtc
            };
            string directory = Path.GetDirectoryName(m_cachePath);
            Directory.CreateDirectory(directory);
            string temporaryPath = m_cachePath + ".tmp";
            File.WriteAllText(temporaryPath, JsonConvert.SerializeObject(envelope, s_jsonSettings), Encoding.UTF8);

            if (!File.Exists(m_cachePath))
            {
                File.Move(temporaryPath, m_cachePath);
                return;
            }

            try
            {
                File.Replace(temporaryPath, m_cachePath, m_backupPath);
            }
            catch (PlatformNotSupportedException)
            {
                ReplaceWithBackup(temporaryPath);
            }
            catch (IOException)
            {
                ReplaceWithBackup(temporaryPath);
            }
        }

        bool TryLoadFile(string path, out CachedGameConfig cached)
        {
            cached = null;
            if (!File.Exists(path))
            {
                return false;
            }

            try
            {
                CacheEnvelope envelope = JsonConvert.DeserializeObject<CacheEnvelope>(
                    File.ReadAllText(path, Encoding.UTF8),
                    s_jsonSettings);
                if (envelope == null || string.IsNullOrWhiteSpace(envelope.ETag) || envelope.CheckedAtUtc == default)
                {
                    return false;
                }

                GameConfigSnapshotValidator.Validate(envelope.Snapshot);
                cached = new CachedGameConfig(
                    envelope.Snapshot,
                    envelope.ETag,
                    envelope.ServerTime,
                    envelope.CheckedAtUtc);
                return true;
            }
            catch (Exception exception) when (
                exception is IOException ||
                exception is UnauthorizedAccessException ||
                exception is JsonException ||
                exception is GameConfigDataException)
            {
                return false;
            }
        }

        void ReplaceWithBackup(string temporaryPath)
        {
            File.Copy(m_cachePath, m_backupPath, true);
            File.Delete(m_cachePath);
            File.Move(temporaryPath, m_cachePath);
        }

        static string CreateServerKey(string baseUrl)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(baseUrl));
                return BitConverter.ToString(hash, 0, 8).Replace("-", "").ToLowerInvariant();
            }
        }

        [Preserve]
        sealed class CacheEnvelope
        {
            public CacheEnvelope() { }
            public GameConfigSnapshot Snapshot { get; set; }
            public string ETag { get; set; }
            public DateTimeOffset ServerTime { get; set; }
            public DateTimeOffset CheckedAtUtc { get; set; }
        }
    }
}
