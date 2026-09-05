using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AChen.Networking
{
    public sealed class GameConfigManager : PersistentMonoSingleton<GameConfigManager>
    {
        static readonly TimeSpan s_refreshInterval = TimeSpan.FromMinutes(5);

        readonly GameConfigStore m_store = new GameConfigStore();
        GameConfigClient m_client;
        GameConfigCache m_cache;
        BackendConfig m_config;
        DateTimeOffset m_lastCheckUtc;
        bool m_initializing;
        bool m_syncing;

        public GameConfigStore Store => m_store;
        public bool IsReady => m_store.HasSnapshot;
        public string LastError { get; private set; }

        public async UniTask InitializeAsync(
            BackendConfig config = null,
            CancellationToken cancellationToken = default)
        {
            if (m_initializing)
            {
                await UniTask.WaitUntil(() => !m_initializing, cancellationToken: cancellationToken);
                if (!IsReady)
                {
                    throw new GameConfigDataException(LastError ?? "游戏配置初始化失败");
                }
                return;
            }

            if (m_client != null && IsReady)
            {
                await EnsureFreshAsync(false, cancellationToken);
                return;
            }

            m_initializing = true;
            try
            {
                m_config = config ?? new BackendConfig();
                m_client = new GameConfigClient(m_config);
                m_cache = new GameConfigCache(m_config);
                if (m_cache.TryLoad(out CachedGameConfig cached))
                {
                    m_store.Replace(
                        cached.Snapshot,
                        cached.ETag,
                        cached.ServerTime,
                        cached.CheckedAtUtc,
                        true);
                }

                try
                {
                    await EnsureFreshAsync(true, cancellationToken);
                }
                catch (Exception exception) when (!(exception is OperationCanceledException))
                {
                    LastError = exception.Message;
                    m_store.MarkStale();
                    if (!m_store.HasSnapshot)
                    {
                        throw;
                    }

                    ALog.LogWarning("后端配置同步失败，继续使用本地最后可用版本：" + exception.Message, ALogCategories.Net);
                }
            }
            finally
            {
                m_initializing = false;
                IsDone = IsReady;
            }
        }

        public async UniTask EnsureFreshAsync(
            bool force = false,
            CancellationToken cancellationToken = default)
        {
            if (m_client == null)
            {
                await InitializeAsync(cancellationToken: cancellationToken);
                return;
            }

            if (!force && DateTimeOffset.UtcNow - m_lastCheckUtc < s_refreshInterval)
            {
                return;
            }

            if (m_syncing)
            {
                await UniTask.WaitUntil(() => !m_syncing, cancellationToken: cancellationToken);
                return;
            }

            m_syncing = true;
            try
            {
                m_lastCheckUtc = DateTimeOffset.UtcNow;
                GameConfigFetchResult result = await m_client.FetchAsync(m_store.ETag, cancellationToken);
                DateTimeOffset checkedAtUtc = DateTimeOffset.UtcNow;
                if (result.NotModified)
                {
                    if (!m_store.HasSnapshot)
                    {
                        throw new GameConfigDataException("服务器未返回游戏配置，且本地没有可用缓存");
                    }

                    m_store.MarkChecked(result.ServerTime, checkedAtUtc);
                }
                else
                {
                    m_cache.Save(result.Snapshot, result.ETag, result.ServerTime, checkedAtUtc);
                    m_store.Replace(
                        result.Snapshot,
                        result.ETag,
                        result.ServerTime,
                        checkedAtUtc,
                        false);
                }

                m_lastCheckUtc = checkedAtUtc;
                LastError = null;
            }
            finally
            {
                m_syncing = false;
            }
        }

        protected override void OnInit()
        {
            InitializeFromSingletonAsync().Forget();
        }

        async UniTaskVoid InitializeFromSingletonAsync()
        {
            try
            {
                await InitializeAsync(cancellationToken: this.GetCancellationTokenOnDestroy());
            }
            catch (Exception exception)
            {
                LastError = exception.Message;
                ALog.LogError("后端配置初始化失败：" + exception.Message, ALogCategories.Net);
                IsDone = true;
            }
        }

        void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus && IsReady)
            {
                RefreshAfterFocusAsync().Forget();
            }
        }

        async UniTaskVoid RefreshAfterFocusAsync()
        {
            try
            {
                await EnsureFreshAsync(false, this.GetCancellationTokenOnDestroy());
            }
            catch (Exception exception) when (!(exception is OperationCanceledException))
            {
                LastError = exception.Message;
                m_store.MarkStale();
                ALog.LogWarning("恢复前台时配置同步失败：" + exception.Message, ALogCategories.Net);
            }
        }
    }
}
