using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using AChen.Configuration;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace AChen.Networking
{
    public static class LocalGameConfiguration
    {
        public static PublishedGameConfig Data { get; private set; }
        public static bool IsReady => Data != null;
        static AsyncOperationHandle<LocalizationSettings> s_settings;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetState() { Data = null; s_settings = default; }

        public static async UniTask InitializeAsync()
        {
            if (IsReady) return;
            var dataHandle = Addressables.LoadAssetAsync<TextAsset>("GameConfig/Data");
            s_settings = Addressables.LoadAssetAsync<LocalizationSettings>("GameConfig/LocalizationSettings");
            try
            {
                var asset = await dataHandle.Task;
                var settings = await s_settings.Task;
                if (asset == null || settings == null || settings.chineseFont == null || settings.englishFont == null)
                    throw new FormatException("统一配置资源或字体映射缺失");
                using (var sha = SHA256.Create())
                {
                    string hash = BitConverter.ToString(sha.ComputeHash(asset.bytes)).Replace("-", "").ToLowerInvariant();
                    if (ContentSession.UseLocalAssets)
                    {
                        ContentSession.ConfigHash = hash;
                    }
                    else if (hash != ContentSession.ConfigHash)
                    {
                        throw new FormatException("客户端配置与发布清单不一致");
                    }
                }
                var data = JsonConvert.DeserializeObject<PublishedGameConfig>(asset.text);
                if (data == null) throw new FormatException("统一配置为空");
                data.Validate();
                // 所有表先验证, 同步提交后才放行业务场景.
                var cards = Table.CardRow.LoadBytes(data.CardTable);
                var translations = Table.TranslationRow.LoadBytes(data.TranslationTable);
                CardCatalog.Install(cards);
                LocalizationService.Install(translations, settings);
                Data = data;
                ALog.Log("统一配置加载完成. Release=" + ContentSession.ReleaseId, ALogCategories.Net);
            }
            catch
            {
                if (s_settings.IsValid()) Addressables.Release(s_settings);
                throw;
            }
            finally { if (dataHandle.IsValid()) Addressables.Release(dataHandle); }
        }

        public static async UniTask CheckVersionAsync(CancellationToken token = default)
        {
            if (ContentSession.UseLocalAssets)
            {
                return;
            }

            RequireCurrent();
            string path = "/api/content/manifests/latest?channel=" + Uri.EscapeDataString(ContentSession.Channel)
                + "&platform=" + Uri.EscapeDataString(ContentSession.Platform) + "&appVersion=" + Uri.EscapeDataString(ContentSession.AppVersion);
            var latest = await new BackendHttpClient().SendAsync<VersionResponse>("GET", path, cancellationToken: token);
            if (latest.SchemaVersion != 2 || latest.ReleaseId != ContentSession.ReleaseId)
            {
                ContentSession.RestartRequired = true;
                ALog.LogWarning("检测到内容更新, 等待重启. Current=" + ContentSession.ReleaseId + "; Latest=" + latest.ReleaseId, ALogCategories.Net);
                RequireCurrent();
            }
            ContentSession.ServerTime = latest.ServerTime;
            ContentSession.ServerTimeReceivedAt = DateTimeOffset.UtcNow;
            ALog.Log("登录内容版本检查完成. Release=" + latest.ReleaseId, ALogCategories.Net);
        }

        public static void RequireCurrent()
        {
            if (ContentSession.UseLocalAssets)
            {
                if (!IsReady)
                    throw new BackendApiException(503, "CONTENT_NOT_READY", "游戏配置未就绪，请重试");
                return;
            }

            if (ContentSession.RestartRequired)
                throw new BackendApiException(409, "CONTENT_UPDATE_REQUIRED", "游戏内容已更新，请重启游戏");
            if (!IsReady || string.IsNullOrEmpty(ContentSession.ReleaseId))
                throw new BackendApiException(503, "CONTENT_NOT_READY", "游戏配置未就绪，请重试");
        }

        public static GachaPoolData GetPool(string key)
        {
            RequireCurrent();
            var cards = key == "CardAll"
                ? Data.AllCards.Select(x => new GachaPoolCard(x.CardId, x.SourcePool))
                : Data.PoolEntries.Where(x => x.PoolKey == key).Select(x => new GachaPoolCard(x.CardId, key));
            var result = cards.OrderBy(x => x.CardId).ToArray();
            if (result.Length == 0) throw new BackendApiException(422, "GACHA_POOL_NOT_FOUND", "卡池不存在");
            return new GachaPoolData(key, result);
        }

        [UnityEngine.Scripting.Preserve]
        sealed class VersionResponse
        {
            public int SchemaVersion { get; set; }
            public string ReleaseId { get; set; }
            public DateTimeOffset ServerTime { get; set; }
        }
    }
}
