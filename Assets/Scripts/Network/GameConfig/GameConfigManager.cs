using System;
using System.Linq;
using System.Threading;
using AChen.Configuration;
using Cysharp.Threading.Tasks;

namespace AChen.Networking
{
    public sealed class GameConfigManager : PersistentMonoSingleton<GameConfigManager>
    {
        readonly GameConfigStore m_store = new GameConfigStore();
        public GameConfigStore Store => m_store;
        public bool IsReady => m_store.HasSnapshot;
        public string LastError { get; private set; }
        public UniTask InitializeAsync(BackendConfig config = null, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            LocalGameConfiguration.RequireCurrent();
            if (!IsReady)
            {
                var data = LocalGameConfiguration.Data.Catalog;
                var snapshot = new GameConfigSnapshot(3, 1, ContentSession.ServerTime,
                    data.Avatars.Select(x => new AvatarConfig(x.Id, x.Name, x.ResourceKey, x.PriceGold, x.SortOrder, x.IsEnabled, x.StartsAt, x.EndsAt)),
                    data.Wallpapers.Select(x => new WallpaperConfig(x.Id, x.Name, x.ResourceKey, x.PriceGold, x.SortOrder, x.IsEnabled, x.StartsAt, x.EndsAt)),
                    data.CardPacks.Select(x => new CardPackConfig(x.Id, x.Title, x.CoverResourceKey, x.PoolKey, x.PriceGold, x.StartsAt, x.EndsAt, x.SortOrder, x.IsEnabled)));
                m_store.Replace(snapshot, ContentSession.ReleaseId, ContentSession.ServerTime, ContentSession.ServerTimeReceivedAt, false);
            }
            else m_store.MarkChecked(ContentSession.ServerTime, ContentSession.ServerTimeReceivedAt);
            IsDone = true;
            return UniTask.CompletedTask;
        }
        public UniTask EnsureFreshAsync(bool force = false, CancellationToken cancellationToken = default) => InitializeAsync(cancellationToken: cancellationToken);
        protected override void OnInit()
        {
            try { InitializeAsync().GetAwaiter().GetResult(); }
            catch (Exception exception)
            {
                LastError = exception.Message;
                IsDone = true;
                ALog.LogError("配置初始化失败: " + exception.Message, ALogCategories.Net);
            }
        }
    }
}
