using System.Threading;
using AChen.Networking;
using Cysharp.Threading.Tasks;

namespace AChen.Player
{
    /// <summary>
    /// 跨场景保留当前登录会话及服务端下发的玩家数据。
    /// </summary>
    public sealed class PlayerSession : PersistentMonoSingleton<PlayerSession>
    {
        readonly AuthClient m_authClient = new();

        public bool IsAuthenticated => m_authClient.IsAuthenticated;
        public AuthUser CurrentUser => m_authClient.CurrentUser;
        public PlayerData CurrentPlayer => m_authClient.CurrentPlayer;

        public UniTask<AuthUser> LoginAsync(
            string username,
            string password,
            CancellationToken cancellationToken = default) =>
            m_authClient.LoginAsync(username, password, cancellationToken);

        public UniTask<AuthUser> RegisterAsync(
            string username,
            string password,
            CancellationToken cancellationToken = default) =>
            m_authClient.RegisterAsync(username, password, cancellationToken);

        public UniTask<bool> TryRestoreSessionAsync(
            CancellationToken cancellationToken = default) =>
            m_authClient.TryRestoreSessionAsync(cancellationToken);

        public UniTask<PlayerData> RefreshPlayerAsync(
            CancellationToken cancellationToken = default) =>
            m_authClient.GetPlayerAsync(cancellationToken);

        public UniTask<PlayerData> UpdatePlayerProfileAsync(
            string nickname,
            int? avatarId,
            int? backgroundId,
            long expectedRevision,
            CancellationToken cancellationToken = default) =>
            m_authClient.UpdatePlayerProfileAsync(
                nickname,
                avatarId,
                backgroundId,
                expectedRevision,
                cancellationToken);

        public UniTask LogoutAsync(CancellationToken cancellationToken = default) =>
            m_authClient.LogoutAsync(cancellationToken);

        public void ClearSession() => m_authClient.ClearSession();
    }
}
