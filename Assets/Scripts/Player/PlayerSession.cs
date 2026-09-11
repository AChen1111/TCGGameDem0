using System;
using System.Linq;
using System.Threading;
using AChen.Events;
using AChen.Networking;
using Cysharp.Threading.Tasks;

namespace AChen.Player
{
    /// <summary>
    /// 玩家会话的唯一真相源: 持有令牌与当前玩家资料, 负责登录/恢复/登出, 串行化资料修改与资产操作, 并派发变更事件.
    /// 网络细节交给 AuthApi, 令牌持久化交给 IAuthSessionStore.
    /// </summary>
    public sealed class PlayerSession : PersistentMonoSingleton<PlayerSession>
    {
        readonly BackendConfig m_config = new BackendConfig();
        readonly SemaphoreSlim m_mutationLock = new(1, 1);
        AuthApi m_api;
        IAuthSessionStore m_sessionStore;
        string m_accessToken;
        string m_refreshToken;

        AuthApi Api => m_api ??= new AuthApi(new BackendHttpClient(m_config));
        IAuthSessionStore SessionStore => m_sessionStore ??= new PlatformAuthSessionStore(m_config);

        public bool IsAuthenticated => !string.IsNullOrEmpty(m_accessToken);
        public AuthUser CurrentUser { get; private set; }
        public PlayerData CurrentPlayer { get; private set; }

        /// <summary>每次建立或清理会话都递增; 跨越会话边界的异步操作据此拒绝提交结果.</summary>
        internal long SessionVersion { get; private set; }

        // ---- 会话生命周期 ----

        public async UniTask<AuthUser> RegisterAsync(string username, string password, CancellationToken cancellationToken = default)
        {
            long sessionVersion = ++SessionVersion;
            AuthSession session = await Api.RegisterAsync(username, password, cancellationToken);
            EnsureSessionVersion(sessionVersion);
            CommitSession(session, GameEvent.PlayerRegistered);
            return CurrentUser;
        }

        public async UniTask<AuthUser> LoginAsync(string username, string password, CancellationToken cancellationToken = default)
        {
            long sessionVersion = ++SessionVersion;
            AuthSession session = await Api.LoginAsync(username, password, cancellationToken);
            EnsureSessionVersion(sessionVersion);
            CommitSession(session, GameEvent.PlayerLoggedIn);
            return CurrentUser;
        }

        public async UniTask<bool> TryRestoreSessionAsync(CancellationToken cancellationToken = default)
        {
            if (IsAuthenticated)
            {
                return true;
            }

            if (!SessionStore.TryLoad(out m_refreshToken))
            {
                return false;
            }

            await RefreshAsync(cancellationToken);
            return true;
        }

        public async UniTask RefreshAsync(CancellationToken cancellationToken = default)
        {
            long sessionVersion = SessionVersion;
            if (string.IsNullOrEmpty(m_refreshToken))
            {
                throw new InvalidOperationException("No refresh token is available.");
            }

            try
            {
                AuthSession session = await Api.RefreshAsync(m_refreshToken, cancellationToken);
                EnsureSessionVersion(sessionVersion);
                CommitSession(session, null);
            }
            catch (BackendApiException exception) when (
                exception.StatusCode == 401 && SessionVersion == sessionVersion)
            {
                ClearSession();
                throw;
            }
        }

        public async UniTask LogoutAsync(CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(m_refreshToken))
            {
                ClearSession();
                return;
            }

            string refreshToken = m_refreshToken;
            long sessionVersion = SessionVersion;
            try
            {
                await Api.LogoutAsync(refreshToken, cancellationToken);
            }
            finally
            {
                if (SessionVersion == sessionVersion)
                {
                    ClearSession();
                }
            }
        }

        public void ClearSession()
        {
            AuthUser previousUser = CurrentUser;
            PlayerData previousPlayer = CurrentPlayer;
            SessionVersion++;
            m_accessToken = null;
            m_refreshToken = null;
            CurrentUser = null;
            CurrentPlayer = null;
            try
            {
                SessionStore.Clear();
                ALog.Log($"清理玩家会话完成. Player={previousUser?.Id}", ALogCategories.Net);
            }
            finally
            {
                PlayerChangePublisher.Publish(previousPlayer, null);
                if (previousUser != null)
                {
                    EventCenter.Dispatch(GameEvent.PlayerLoggedOut, previousUser);
                }
            }
        }

        // ---- 玩家资料操作 ----

        public UniTask<PlayerData> RefreshPlayerAsync(CancellationToken cancellationToken = default) =>
            ExecuteMutationAsync("RefreshPlayer", "Current", (_, token) => SendAuthenticatedAsync(Api.GetPlayerAsync, token), cancellationToken);

        public UniTask<PlayerData> RenameAsync(string nickname, CancellationToken cancellationToken = default) =>
            ExecuteMutationAsync("Rename", "Profile", (player, token) =>
            {
                string value = nickname?.Trim() ?? string.Empty;
                if (value.Length is < 2 or > 24 || value.Any(char.IsControl))
                {
                    throw new BackendApiException(422, "VALIDATION_ERROR", "昵称需为 2-24 个字符且不能包含控制字符");
                }

                return value == player.Nickname
                    ? UniTask.FromResult(player)
                    : UpdateProfileAsync(value, player.AvatarId, player.BackgroundId, player.Revision, token);
            }, cancellationToken);

        public UniTask<PlayerData> SetAvatarAsync(int avatarId, CancellationToken cancellationToken = default) =>
            ExecuteMutationAsync("SetAvatar", avatarId.ToString(), (player, token) =>
            {
                if (!player.OwnedAvatarIds.Contains(avatarId))
                {
                    throw new BackendApiException(422, "AVATAR_NOT_OWNED", "尚未拥有该头像");
                }

                return player.AvatarId == avatarId
                    ? UniTask.FromResult(player)
                    : UpdateProfileAsync(player.Nickname, avatarId, player.BackgroundId, player.Revision, token);
            }, cancellationToken);

        public UniTask<PlayerData> SetBackgroundAsync(int backgroundId, CancellationToken cancellationToken = default) =>
            ExecuteMutationAsync("SetBackground", backgroundId.ToString(), (player, token) =>
            {
                if (!player.OwnedBackgroundIds.Contains(backgroundId))
                {
                    throw new BackendApiException(422, "WALLPAPER_NOT_OWNED", "尚未拥有该壁纸");
                }

                return UpdateBackgroundAsync(player, backgroundId, token);
            }, cancellationToken);

        public UniTask<PlayerData> SetNextBackgroundAsync(CancellationToken cancellationToken = default) =>
            ExecuteMutationAsync("NextBackground", "Profile", (player, token) =>
            {
                int[] owned = player.OwnedBackgroundIds.OrderBy(value => value).ToArray();
                if (owned.Length == 0)
                {
                    throw new BackendApiException(422, "WALLPAPER_NOT_OWNED", "没有已拥有壁纸");
                }

                int index = player.BackgroundId is int id ? Array.IndexOf(owned, id) : -1;
                return UpdateBackgroundAsync(player, owned[(index + 1) % owned.Length], token);
            }, cancellationToken);

        public UniTask<PlayerData> PurchaseShopItemAsync(string catalogType, int itemId, CancellationToken cancellationToken = default) =>
            ExecuteMutationAsync("Purchase", $"{catalogType}/{itemId}", (player, token) =>
                SendAuthenticatedAsync(
                    (accessToken, ct) => Api.PurchaseShopItemAsync(accessToken, catalogType, itemId, player.Revision, ct),
                    token), cancellationToken);

        UniTask<PlayerData> UpdateBackgroundAsync(PlayerData player, int backgroundId, CancellationToken token) =>
            player.BackgroundId == backgroundId
                ? UniTask.FromResult(player)
                : UpdateProfileAsync(player.Nickname, player.AvatarId, backgroundId, player.Revision, token);

        UniTask<PlayerData> UpdateProfileAsync(string nickname, int? avatarId, int? backgroundId, long revision, CancellationToken token) =>
            SendAuthenticatedAsync(
                (accessToken, ct) => Api.UpdateProfileAsync(accessToken, nickname, avatarId, backgroundId, revision, ct),
                token);

        async UniTask<PlayerData> ExecuteMutationAsync(
            string operation,
            string target,
            Func<PlayerData, CancellationToken, UniTask<PlayerData>> execute,
            CancellationToken cancellationToken)
        {
            long sessionVersion = SessionVersion;
            await m_mutationLock.WaitAsync(cancellationToken);
            try
            {
                // 排队结束后再取最新资料与版本,并拒绝来自上一会话的操作.
                cancellationToken.ThrowIfCancellationRequested();
                EnsureSessionVersion(sessionVersion);
                PlayerData player = CurrentPlayer;
                if (!IsAuthenticated || player == null)
                {
                    throw new BackendApiException(401, "INVALID_ACCESS_TOKEN", "登录状态已失效，请重新登录");
                }

                PlayerData updated = await execute(player, cancellationToken);
                ALog.Log($"玩家操作完成. Operation={operation}; Target={target}; Player={updated.Id}; Revision={updated.Revision}", ALogCategories.Net);
                return updated;
            }
            catch (BackendApiException exception)
            {
                if (exception.Code == "PLAYER_DATA_CHANGED" && SessionVersion == sessionVersion)
                {
                    // 冲突后同步最新状态,不自动重放购买等有副作用的操作.
                    try
                    {
                        await SendAuthenticatedAsync(Api.GetPlayerAsync, cancellationToken);
                    }
                    catch (OperationCanceledException) { throw; }
                    catch (Exception refreshException)
                    {
                        ALog.LogWarning($"冲突后刷新玩家失败. Operation={operation}; Error={refreshException.Message}", ALogCategories.Net);
                    }
                }

                ALog.LogWarning($"玩家操作失败. Operation={operation}; Target={target}; Code={exception.Code}; Status={exception.StatusCode}", ALogCategories.Net);
                throw;
            }
            finally
            {
                m_mutationLock.Release();
            }
        }

        // ---- 鉴权请求与状态提交 ----

        /// <summary>带访问令牌调用接口; 401 时刷新令牌重试一次, 成功后把返回的玩家资料提交为当前状态.</summary>
        async UniTask<PlayerData> SendAuthenticatedAsync(
            Func<string, CancellationToken, UniTask<PlayerData>> call,
            CancellationToken cancellationToken)
        {
            if (!IsAuthenticated)
            {
                throw new BackendApiException(401, "INVALID_ACCESS_TOKEN", "登录状态已失效，请重新登录");
            }

            long sessionVersion = SessionVersion;
            PlayerData player;
            try
            {
                player = await call(m_accessToken, cancellationToken);
                EnsureSessionVersion(sessionVersion);
            }
            catch (BackendApiException exception) when (
                exception.StatusCode == 401 && SessionVersion == sessionVersion &&
                !string.IsNullOrEmpty(m_refreshToken))
            {
                await RefreshAsync(cancellationToken);
                EnsureSessionVersion(sessionVersion);
                player = await call(m_accessToken, cancellationToken);
                EnsureSessionVersion(sessionVersion);
            }

            SetCurrentPlayer(player);
            return CurrentPlayer;
        }

        /// <summary>提交服务器返回的会话; evt 为 null 表示令牌刷新, 只更新状态不发登录事件.</summary>
        void CommitSession(AuthSession session, EventId<AuthUser, PlayerData>? evt)
        {
            PlayerData previousPlayer = CurrentPlayer;
            PlayerData player = session.Player;
            if (previousPlayer != null && previousPlayer.Id == player.Id && previousPlayer.Revision > player.Revision)
            {
                player = previousPlayer;
            }

            // 先持久化并提交完整会话, 订阅者读取时才能看到一致状态.
            SessionStore.Save(session.RefreshToken);
            m_accessToken = session.AccessToken;
            m_refreshToken = session.RefreshToken;
            CurrentUser = session.User;
            CurrentPlayer = player;
            ALog.Log($"提交玩家会话完成. Event={(evt.HasValue ? evt.Value.Name : "TokenRefreshed")}; Player={session.User.Id}; Revision={player.Revision}", ALogCategories.Net);
            PlayerChangePublisher.Publish(previousPlayer, player);
            if (evt.HasValue)
            {
                EventCenter.Dispatch(evt.Value, CurrentUser, CurrentPlayer);
            }
        }

        void SetCurrentPlayer(PlayerData player)
        {
            if (CurrentUser == null || player.Id != CurrentUser.Id)
            {
                throw new BackendApiException(401, "SESSION_CHANGED", "登录状态已变更，请重试");
            }

            PlayerData previousPlayer = CurrentPlayer;
            if (previousPlayer != null && previousPlayer.Id == player.Id && previousPlayer.Revision > player.Revision)
            {
                return;
            }

            CurrentPlayer = player;
            PlayerChangePublisher.Publish(previousPlayer, player);
        }

        void EnsureSessionVersion(long sessionVersion)
        {
            if (SessionVersion != sessionVersion)
            {
                throw new BackendApiException(401, "SESSION_CHANGED", "登录状态已变更，请重试");
            }
        }
    }
}
