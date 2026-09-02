using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using AChen.Events;
using UnityEngine.Scripting;
using UnityEngine.Networking;

namespace AChen.Networking
{
    public sealed class AuthClient
    {
        static readonly JsonSerializerSettings s_jsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore
        };

        readonly BackendConfig m_config;
        string m_accessToken;
        string m_refreshToken;

        public bool IsAuthenticated => !string.IsNullOrEmpty(m_accessToken);
        public AuthUser CurrentUser { get; private set; }
        public PlayerData CurrentPlayer { get; private set; }

        public AuthClient(BackendConfig config = null)
        {
            m_config = config ?? new BackendConfig();
        }

        public async UniTask<AuthUser> RegisterAsync(
            string username,
            string password,
            CancellationToken cancellationToken = default)
        {
            AuthResponseDto response = await PostAuthAsync(
                "/api/auth/register",
                new RegisterRequest(username, password),
                cancellationToken);
            SetSession(response, GameEvent.PlayerRegistered);
            return CurrentUser;
        }

        public async UniTask<AuthUser> LoginAsync(
            string username,
            string password,
            CancellationToken cancellationToken = default)
        {
            AuthResponseDto response = await PostAuthAsync(
                "/api/auth/login",
                new LoginRequest(username, password),
                cancellationToken);
            SetSession(response, GameEvent.PlayerLoggedIn);
            return CurrentUser;
        }

        public async UniTask<AuthUser> GetCurrentUserAsync(CancellationToken cancellationToken = default)
        {
            CurrentUser = ToUser(await SendAuthenticatedAsync<UserDto>(
                UnityWebRequest.kHttpVerbGET,
                "/api/auth/me",
                null,
                cancellationToken));

            return CurrentUser;
        }

        public async UniTask<PlayerData> GetPlayerAsync(CancellationToken cancellationToken = default)
        {
            PlayerDto response = await SendAuthenticatedAsync<PlayerDto>(
                UnityWebRequest.kHttpVerbGET,
                "/api/player/bootstrap",
                null,
                cancellationToken);
            SetCurrentPlayer(ToPlayer(response));
            return CurrentPlayer;
        }

        public async UniTask<PlayerData> UpdatePlayerProfileAsync(
            string nickname,
            int? avatarId,
            long expectedRevision,
            CancellationToken cancellationToken = default)
        {
            PlayerDto response = await SendAuthenticatedAsync<PlayerDto>(
                "PATCH",
                "/api/player/profile",
                new UpdatePlayerProfileRequest(nickname, avatarId, expectedRevision),
                cancellationToken);
            SetCurrentPlayer(ToPlayer(response));
            return CurrentPlayer;
        }

        public async UniTask RefreshAsync(CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(m_refreshToken))
            {
                throw new InvalidOperationException("No refresh token is available.");
            }

            try
            {
                AuthResponseDto response = await PostAuthAsync(
                    "/api/auth/refresh",
                    new RefreshRequest(m_refreshToken),
                    cancellationToken);
                SetSession(response, GameEvent.PlayerTokenRefreshed);
            }
            catch (BackendApiException exception) when (exception.StatusCode == 401)
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
            try
            {
                await SendAsync(
                    UnityWebRequest.kHttpVerbPOST,
                    "/api/auth/logout",
                    new RefreshRequest(refreshToken),
                    null,
                    cancellationToken);
            }
            finally
            {
                ClearSession();
            }
        }

        public void ClearSession()
        {
            AuthUser previousUser = CurrentUser;
            m_accessToken = null;
            m_refreshToken = null;
            CurrentUser = null;
            CurrentPlayer = null;
            if (previousUser != null)
            {
                EventCenter.Dispatch(GameEvent.PlayerLoggedOut, previousUser);
            }
        }

        async UniTask<AuthResponseDto> PostAuthAsync(
            string path,
            object body,
            CancellationToken cancellationToken)
        {
            string json = await SendAsync(
                UnityWebRequest.kHttpVerbPOST,
                path,
                body,
                null,
                cancellationToken);
            return Deserialize<AuthResponseDto>(json);
        }

        async UniTask<T> SendAuthenticatedAsync<T>(
            string method,
            string path,
            object body,
            CancellationToken cancellationToken)
        {
            EnsureAuthenticated();
            try
            {
                return Deserialize<T>(await SendAsync(
                    method,
                    path,
                    body,
                    m_accessToken,
                    cancellationToken));
            }
            catch (BackendApiException exception) when (
                exception.StatusCode == 401 && !string.IsNullOrEmpty(m_refreshToken))
            {
                await RefreshAsync(cancellationToken);
                return Deserialize<T>(await SendAsync(
                    method,
                    path,
                    body,
                    m_accessToken,
                    cancellationToken));
            }
        }

        async UniTask<string> SendAsync(
            string method,
            string path,
            object body,
            string accessToken,
            CancellationToken cancellationToken)
        {
            using (var request = new UnityWebRequest(m_config.BaseUrl + path, method))
            {
                request.downloadHandler = new DownloadHandlerBuffer();
                request.timeout = m_config.TimeoutSeconds;
                request.SetRequestHeader("Accept", "application/json");

                if (body != null)
                {
                    byte[] payload = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(body, s_jsonSettings));
                    request.uploadHandler = new UploadHandlerRaw(payload);
                    request.SetRequestHeader("Content-Type", "application/json");
                }

                if (!string.IsNullOrEmpty(accessToken))
                {
                    request.SetRequestHeader("Authorization", "Bearer " + accessToken);
                }

                try
                {
                    await request.SendWebRequest().ToUniTask(
                        cancellationToken: cancellationToken,
                        cancelImmediately: true);
                }
                catch (UnityWebRequestException)
                {
                    throw CreateException(request);
                }

                if (request.result != UnityWebRequest.Result.Success)
                {
                    throw CreateException(request);
                }

                return request.downloadHandler.text;
            }
        }

        static T Deserialize<T>(string json)
        {
            T value = JsonConvert.DeserializeObject<T>(json, s_jsonSettings);
            if (value == null)
            {
                throw new BackendApiException(0, "INVALID_RESPONSE", "Backend returned an invalid response.");
            }

            return value;
        }

        static BackendApiException CreateException(UnityWebRequest request)
        {
            if (request.responseCode <= 0)
            {
                return new BackendApiException(0, "NETWORK_ERROR", request.error ?? "Could not reach backend.");
            }

            try
            {
                ProblemDetailsDto problem = JsonConvert.DeserializeObject<ProblemDetailsDto>(
                    request.downloadHandler.text,
                    s_jsonSettings);
                if (problem != null)
                {
                    return new BackendApiException(
                        request.responseCode,
                        string.IsNullOrEmpty(problem.Code) ? "HTTP_ERROR" : problem.Code,
                        string.IsNullOrEmpty(problem.Title) ? "Backend request failed." : problem.Title,
                        problem.Errors);
                }
            }
            catch (JsonException)
            {
                // Fall through to a response that does not expose an untrusted body.
            }

            return new BackendApiException(request.responseCode, "HTTP_ERROR", "Backend request failed.");
        }

        void SetSession(AuthResponseDto response, string eventName)
        {
            if (string.IsNullOrEmpty(response.AccessToken) ||
                string.IsNullOrEmpty(response.RefreshToken) ||
                response.User == null ||
                response.Player == null)
            {
                throw new BackendApiException(0, "INVALID_RESPONSE", "Backend returned an incomplete auth response.");
            }

            AuthUser user = ToUser(response.User);
            if (CurrentUser == null || CurrentUser.Id != user.Id)
            {
                CurrentPlayer = null;
            }

            m_accessToken = response.AccessToken;
            m_refreshToken = response.RefreshToken;
            CurrentUser = user;
            SetCurrentPlayer(ToPlayer(response.Player));
            EventCenter.Dispatch(eventName, CurrentUser, CurrentPlayer);
        }

        void SetCurrentPlayer(PlayerData player)
        {
            CurrentPlayer = player;
            EventCenter.Dispatch(GameEvent.PlayerDataChanged, player);
        }

        void EnsureAuthenticated()
        {
            if (!IsAuthenticated)
            {
                throw new InvalidOperationException("User is not authenticated.");
            }
        }

        static AuthUser ToUser(UserDto user) =>
            new AuthUser(user.Id, user.Username, user.CreatedAt);

        static PlayerData ToPlayer(PlayerDto player) =>
            new PlayerData(
                player.Id,
                player.Nickname,
                player.AvatarId,
                player.Gold,
                player.Revision,
                player.CreatedAt,
                player.UpdatedAt);

        sealed class RegisterRequest
        {
            public string Username { get; }
            public string Password { get; }

            public RegisterRequest(string username, string password)
            {
                Username = username;
                Password = password;
            }
        }

        sealed class LoginRequest
        {
            public string Username { get; }
            public string Password { get; }

            public LoginRequest(string username, string password)
            {
                Username = username;
                Password = password;
            }
        }

        sealed class RefreshRequest
        {
            public string RefreshToken { get; }

            public RefreshRequest(string refreshToken)
            {
                RefreshToken = refreshToken;
            }
        }

        sealed class UpdatePlayerProfileRequest
        {
            public string Nickname { get; }
            public int? AvatarId { get; }
            public long ExpectedRevision { get; }

            public UpdatePlayerProfileRequest(string nickname, int? avatarId, long expectedRevision)
            {
                Nickname = nickname;
                AvatarId = avatarId;
                ExpectedRevision = expectedRevision;
            }
        }

        [Preserve]
        sealed class AuthResponseDto
        {
            public AuthResponseDto() { }

            public string AccessToken { get; set; }
            public string RefreshToken { get; set; }
            public UserDto User { get; set; }
            public PlayerDto Player { get; set; }
        }

        [Preserve]
        sealed class UserDto
        {
            public UserDto() { }

            public Guid Id { get; set; }
            public string Username { get; set; }
            public DateTimeOffset CreatedAt { get; set; }
        }

        [Preserve]
        sealed class PlayerDto
        {
            public PlayerDto() { }

            public Guid Id { get; set; }
            public string Nickname { get; set; }
            public int? AvatarId { get; set; }
            public long Gold { get; set; }
            public long Revision { get; set; }
            public DateTimeOffset CreatedAt { get; set; }
            public DateTimeOffset UpdatedAt { get; set; }
        }

        [Preserve]
        sealed class ProblemDetailsDto
        {
            public ProblemDetailsDto() { }

            public string Title { get; set; }
            public string Code { get; set; }
            public Dictionary<string, string[]> Errors { get; set; }
        }
    }

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

        public UniTask<PlayerData> RefreshPlayerAsync(
            CancellationToken cancellationToken = default) =>
            m_authClient.GetPlayerAsync(cancellationToken);

        public UniTask<PlayerData> UpdatePlayerProfileAsync(
            string nickname,
            int? avatarId,
            long expectedRevision,
            CancellationToken cancellationToken = default) =>
            m_authClient.UpdatePlayerProfileAsync(
                nickname,
                avatarId,
                expectedRevision,
                cancellationToken);

        public UniTask LogoutAsync(CancellationToken cancellationToken = default) =>
            m_authClient.LogoutAsync(cancellationToken);

        public void ClearSession() => m_authClient.ClearSession();
    }
}
