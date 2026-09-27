using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using AChen.Configuration;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace AChen.Networking
{
    public static class LocalGameConfiguration
    {
        public static PublishedGameConfig Data { get; private set; }
        public static DeckRulesConfiguration DeckRules { get; private set; }
        public static bool IsReady => Data != null && DeckRules != null;
        static AsyncOperationHandle<LocalizationSettings> s_settings;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetState()
        {
            if (s_settings.IsValid()) Addressables.Release(s_settings);
            Data = null; DeckRules = null; s_settings = default;
            LocalizationService.Uninstall();
            CardCatalog.Uninstall();
            Application.quitting -= ResetState; Application.quitting += ResetState;
        }

        public static async UniTask InitializeAsync(Action<float> onProgress = null)
        {
            if (IsReady) { onProgress?.Invoke(1f); return; }
            var locations = Addressables.LoadResourceLocationsAsync(GameConfigTables.Label, typeof(TextAsset));
            var handles = new System.Collections.Generic.List<AsyncOperationHandle<TextAsset>>();
            s_settings = Addressables.LoadAssetAsync<LocalizationSettings>("GameConfig/LocalizationSettings");
            try
            {
                var found = await locations.Task;
                if (locations.Status != AsyncOperationStatus.Succeeded) throw new FormatException("配置目录加载失败");
                var ordered = found.OrderBy(x => x.PrimaryKey, StringComparer.Ordinal).ToArray();
                foreach (var location in ordered) handles.Add(Addressables.LoadAssetAsync<TextAsset>(location));
                var files = new System.Collections.Generic.Dictionary<string, byte[]>(StringComparer.Ordinal);
                for (int i = 0; i < handles.Count; i++)
                {
                    string address = ordered[i].PrimaryKey;
                    TextAsset asset;
                    try { asset = await handles[i].Task; }
                    catch (Exception ex) { throw new FormatException("配置加载失败: " + address, ex); }
                    if (asset == null || !address.StartsWith("GameConfig/", StringComparison.Ordinal)) throw new FormatException("配置地址或资产无效: " + address);
                    string name = address.Substring("GameConfig/".Length);
                    if (files.ContainsKey(name)) throw new FormatException("配置地址重复: " + name);
                    files.Add(name, asset.bytes);
                    onProgress?.Invoke((i + 1f) / (handles.Count + 2f));
                }
                var settings = await s_settings.Task;
                if (settings == null || settings.chineseFont == null || settings.englishFont == null) throw new FormatException("LocalizationSettings: 字体映射缺失");
                if (!ContentSession.UseLocalAssets) ConfigArtifacts.Verify(files, ContentSession.Configs);
                var data = GameConfigTables.Assemble(files);
                var deckRules = DeckRulesConfiguration.Load(files);
                if (ContentSession.UseLocalAssets)
                {
                    var configs = files.Select(pair =>
                    {
                        using (var hash = SHA256.Create())
                            return new ConfigArtifact
                            {
                                category = pair.Key, address = GameConfigTables.Address(pair.Key),
                                format = GameConfigTables.Format(pair.Key), path = GameConfigTables.PackagePath(pair.Key),
                                size = pair.Value.LongLength,
                                sha256 = BitConverter.ToString(hash.ComputeHash(pair.Value)).Replace("-", "").ToLowerInvariant()
                            };
                    }).ToArray();
                    ConfigArtifacts.Validate(configs, "");
                    ContentSession.Configs = configs;
                    ContentSession.ConfigHash = DevelopmentProtocol.ConfigHash(configs);
                }
                var cards = Table.CardRow.LoadBytes(data.CardTable);
                var translations = Table.TranslationRow.LoadBytes(data.TranslationTable);
                try
                {
                    LocalizationService.Install(translations, settings);
                    CardCatalog.Install(cards);
                    DeckRules = deckRules;
                    Data = data;
                }
                catch
                {
                    Data = null; DeckRules = null;
                    LocalizationService.Uninstall();
                    CardCatalog.Uninstall();
                    throw;
                }
                onProgress?.Invoke(1f);
                ALog.Log("全部配置加载完成. Release=" + ContentSession.ReleaseId + "; Tables=" + files.Count, ALogCategories.Net);
            }
            catch (Exception ex)
            {
                ALog.LogError("配置加载或校验失败. Error=" + ex.Message, ALogCategories.Net);
                if (s_settings.IsValid()) Addressables.Release(s_settings);
                s_settings = default; throw;
            }
            finally
            {
                foreach (var handle in handles) if (handle.IsValid()) Addressables.Release(handle);
                if (locations.IsValid()) Addressables.Release(locations);
            }
        }

        public static async UniTask CheckVersionAsync(CancellationToken token = default)
        {
            if (ContentSession.UseLocalAssets)
            {
                return;
            }

            RequireCurrent();
            token.ThrowIfCancellationRequested();
            await UniTask.CompletedTask;

        }

        public static void RequireCurrent()
        {
            if (!IsReady)
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
