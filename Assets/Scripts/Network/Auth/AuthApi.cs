using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;
using UnityEngine.Scripting;

namespace AChen.Networking
{
    /// <summary>登录/注册/刷新成功后服务器返回的完整会话.</summary>
    public sealed class AuthSession
    {
        public string AccessToken { get; }
        public string RefreshToken { get; }
        public AuthUser User { get; }
        public PlayerData Player { get; }

        internal AuthSession(string accessToken, string refreshToken, AuthUser user, PlayerData player)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            User = user;
            Player = player;
        }
    }

    /// <summary>账号与玩家资料相关的后端接口. 无状态, 令牌由调用方(PlayerSession)持有并传入.</summary>
    public sealed partial class AuthApi
    {
        readonly BackendHttpClient m_http;

        public AuthApi(BackendHttpClient http)
        {
            m_http = http ?? throw new ArgumentNullException(nameof(http));
        }

        public UniTask<AuthSession> RegisterAsync(string username, string password, CancellationToken cancellationToken) =>
            PostAuthAsync("/api/auth/register", new CredentialsRequest(username, password), cancellationToken);

        public UniTask<AuthSession> LoginAsync(string username, string password, CancellationToken cancellationToken) =>
            PostAuthAsync("/api/auth/login", new CredentialsRequest(username, password), cancellationToken);

        public UniTask<AuthSession> RefreshAsync(string refreshToken, CancellationToken cancellationToken) =>
            PostAuthAsync("/api/auth/refresh", new RefreshRequest(refreshToken), cancellationToken);

        public UniTask LogoutAsync(string refreshToken, CancellationToken cancellationToken) =>
            m_http.SendAsync(UnityWebRequest.kHttpVerbPOST, "/api/auth/logout", new RefreshRequest(refreshToken), null, cancellationToken);

        public async UniTask<PlayerData> GetPlayerAsync(string accessToken, CancellationToken cancellationToken)
        {
            PlayerDto dto = await m_http.SendAsync<PlayerDto>(
                UnityWebRequest.kHttpVerbGET, "/api/player/bootstrap", null, accessToken, cancellationToken);
            return ToPlayer(dto);
        }

        public async UniTask<PlayerData> UpdateProfileAsync(
            string accessToken,
            string nickname,
            int? avatarId,
            int? backgroundId,
            int avatarFrameId,
            long expectedRevision,
            CancellationToken cancellationToken)
        {
            PlayerDto dto = await m_http.SendAsync<PlayerDto>(
                "PATCH",
                "/api/player/profile",
                new UpdatePlayerProfileRequest(nickname, avatarId, backgroundId, expectedRevision, avatarFrameId),
                accessToken,
                cancellationToken);
            return ToPlayer(dto);
        }

        public async UniTask<PlayerData> PurchaseShopItemAsync(
            string accessToken,
            string catalogType,
            int itemId,
            long expectedRevision,
            CancellationToken cancellationToken)
        {
            PlayerDto dto = await m_http.SendAsync<PlayerDto>(
                UnityWebRequest.kHttpVerbPOST,
                "/api/player/purchase",
                new PurchaseShopItemRequest(catalogType, itemId, expectedRevision),
                accessToken,
                cancellationToken);
            return ToPlayer(dto);
        }

        public async UniTask<CardDrawResponse> DrawCardsAsync(
            string accessToken,
            int packId,
            string poolKey,
            int count,
            long expectedRevision,
            CancellationToken cancellationToken)
        {
            CardDrawResponseDto dto = await m_http.SendAsync<CardDrawResponseDto>(
                UnityWebRequest.kHttpVerbPOST,
                "/api/player/card-draws",
                new DrawCardsRequest(packId, poolKey, count, expectedRevision),
                accessToken,
                cancellationToken);
            return ToDraw(dto);
        }

        public async UniTask<GachaPoolData> GetGachaPoolAsync(
            string accessToken,
            string poolKey,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await UniTask.FromResult(LocalGameConfiguration.GetPool(poolKey ?? string.Empty));
        }

        public static PlayerData ParsePlayerJson(string json) =>
            ToPlayer(BackendJson.DeserializeResponse<PlayerDto>(json));

        public static CardDrawResponse ParseDrawJson(string json) =>
            ToDraw(BackendJson.DeserializeResponse<CardDrawResponseDto>(json));

        public static GachaPoolData ParseGachaPoolJson(string json) =>
            ToPool(BackendJson.DeserializeResponse<GachaPoolDto>(json));

        public static IReadOnlyList<FriendSummaryData> ParseFriendsJson(string json) =>
            MapFriends(BackendJson.DeserializeResponse<FriendSummaryDto[]>(json));

        public static IReadOnlyList<FriendSearchHitData> ParseFriendSearchJson(string json) =>
            MapSearch(BackendJson.DeserializeResponse<FriendSearchHitDto[]>(json));

        public static IReadOnlyList<InboxItemData> ParseInboxJson(string json) =>
            MapInbox(BackendJson.DeserializeResponse<InboxItemDto[]>(json));

        public async UniTask<IReadOnlyList<FriendSummaryData>> GetFriendsAsync(
            string accessToken,
            CancellationToken cancellationToken)
        {
            FriendSummaryDto[] dto = await m_http.SendAsync<FriendSummaryDto[]>(
                UnityWebRequest.kHttpVerbGET, "/api/friends", null, accessToken, cancellationToken);
            return MapFriends(dto);
        }

        public async UniTask<IReadOnlyList<FriendSearchHitData>> SearchFriendsAsync(
            string accessToken,
            string nickname,
            CancellationToken cancellationToken)
        {
            string path = "/api/friends/search?nickname=" + Uri.EscapeDataString(nickname ?? string.Empty);
            FriendSearchHitDto[] dto = await m_http.SendAsync<FriendSearchHitDto[]>(
                UnityWebRequest.kHttpVerbGET, path, null, accessToken, cancellationToken);
            return MapSearch(dto);
        }

        public UniTask SendFriendRequestAsync(
            string accessToken,
            Guid targetPlayerId,
            CancellationToken cancellationToken) =>
            m_http.SendAsync(
                UnityWebRequest.kHttpVerbPOST,
                "/api/friends/requests",
                new TargetPlayerRequest(targetPlayerId),
                accessToken,
                cancellationToken);

        public UniTask AcceptFriendRequestAsync(
            string accessToken,
            Guid requestId,
            CancellationToken cancellationToken) =>
            m_http.SendAsync(
                UnityWebRequest.kHttpVerbPOST,
                "/api/friends/requests/" + requestId.ToString("D") + "/accept",
                null,
                accessToken,
                cancellationToken);

        public UniTask RejectFriendRequestAsync(
            string accessToken,
            Guid requestId,
            CancellationToken cancellationToken) =>
            m_http.SendAsync(
                UnityWebRequest.kHttpVerbPOST,
                "/api/friends/requests/" + requestId.ToString("D") + "/reject",
                null,
                accessToken,
                cancellationToken);

        public async UniTask<IReadOnlyList<InboxItemData>> GetInboxAsync(
            string accessToken,
            CancellationToken cancellationToken)
        {
            InboxItemDto[] dto = await m_http.SendAsync<InboxItemDto[]>(
                UnityWebRequest.kHttpVerbGET, "/api/inbox", null, accessToken, cancellationToken);
            return MapInbox(dto);
        }

        public async UniTask<GiftClaimResult> ClaimGiftAsync(string accessToken, Guid giftId, long expectedRevision, CancellationToken cancellationToken)
        {
            var dto = await m_http.SendAsync<GiftClaimDto>("POST", "/api/gifts/" + giftId.ToString("D") + "/claim",
                new ClaimGiftRequest(expectedRevision), accessToken, cancellationToken);
            return new GiftClaimResult(ToPlayer(dto.Player), dto.UrGained);
        }

        async UniTask<AuthSession> PostAuthAsync(string path, object body, CancellationToken cancellationToken)
        {
            AuthResponseDto response = await m_http.SendAsync<AuthResponseDto>(
                UnityWebRequest.kHttpVerbPOST, path, body, null, cancellationToken);
            if (string.IsNullOrEmpty(response.AccessToken) ||
                string.IsNullOrEmpty(response.RefreshToken) ||
                response.User == null ||
                response.Player == null)
            {
                throw new BackendApiException(0, "INVALID_RESPONSE", "服务器返回的登录数据不完整", localizationKey: "err.invalid_response.login_incomplete");
            }

            AuthUser user = ToUser(response.User);
            PlayerData player = ToPlayer(response.Player);
            if (player.Id != user.Id)
            {
                throw new BackendApiException(0, "INVALID_RESPONSE", "服务器返回的玩家身份不一致", localizationKey: "err.invalid_response.player_mismatch");
            }

            return new AuthSession(response.AccessToken, response.RefreshToken, user, player);
        }

        static AuthUser ToUser(UserDto user) =>
            new AuthUser(user.Id, user.Username, user.CreatedAt);

        static PlayerData ToPlayer(PlayerDto player) =>
            new PlayerData(
                player.Id,
                player.Nickname,
                player.AvatarId,
                player.OwnedAvatarIds,
                player.BackgroundId,
                player.OwnedBackgroundIds,
                ToOwnedCards(player.OwnedCards),
                player.Gold,
                player.Revision,
                player.CreatedAt,
                player.UpdatedAt, player.AvatarFrameId, player.OwnedAvatarFrameIds, player.Ur, player.OwnedArtIds);

        static CardDrawResponse ToDraw(CardDrawResponseDto dto)
        {
            if (dto == null || dto.Player == null)
            {
                throw new BackendApiException(0, "INVALID_RESPONSE", "服务器返回的抽卡数据不完整", localizationKey: "err.invalid_response.draw_incomplete");
            }

            CardDrawResultDto[] results = dto.Results ?? Array.Empty<CardDrawResultDto>();
            var mapped = new CardDrawResult[results.Length];
            for (int i = 0; i < results.Length; i++)
            {
                CardDrawResultDto result = results[i];
                mapped[i] = new CardDrawResult(
                    result != null ? result.CardId : null,
                    result != null ? result.Rarity : 0,
                    result != null ? result.SourcePool : null, result.IsOverflow, result.UrGained,
                    result.ArtId);
            }

            return new CardDrawResponse(mapped, ToPlayer(dto.Player));
        }

        static GachaPoolData ToPool(GachaPoolDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.PoolKey))
            {
                throw new BackendApiException(0, "INVALID_RESPONSE", "服务器返回的卡池数据不完整", localizationKey: "err.invalid_response.pool_incomplete");
            }

            GachaPoolCardDto[] cards = dto.Cards ?? Array.Empty<GachaPoolCardDto>();
            var mapped = new GachaPoolCard[cards.Length];
            for (int i = 0; i < cards.Length; i++)
            {
                GachaPoolCardDto card = cards[i];
                mapped[i] = new GachaPoolCard(
                    card != null ? card.CardId : null,
                    card != null ? card.SourcePool : null);
            }

            return new GachaPoolData(dto.PoolKey, mapped);
        }

        static IReadOnlyList<FriendSummaryData> MapFriends(FriendSummaryDto[] dto)
        {
            FriendSummaryDto[] items = dto ?? Array.Empty<FriendSummaryDto>();
            var mapped = new FriendSummaryData[items.Length];
            for (int i = 0; i < items.Length; i++)
            {
                FriendSummaryDto item = items[i];
                mapped[i] = new FriendSummaryData(
                    item != null ? item.Id : Guid.Empty,
                    item != null ? item.Nickname : null,
                    item != null ? item.AvatarId : null, item.AvatarFrameId);
            }

            return mapped;
        }

        static IReadOnlyList<FriendSearchHitData> MapSearch(FriendSearchHitDto[] dto)
        {
            FriendSearchHitDto[] items = dto ?? Array.Empty<FriendSearchHitDto>();
            var mapped = new FriendSearchHitData[items.Length];
            for (int i = 0; i < items.Length; i++)
            {
                FriendSearchHitDto item = items[i];
                mapped[i] = new FriendSearchHitData(
                    item != null ? item.Id : Guid.Empty,
                    item != null ? item.Nickname : null,
                    item != null ? item.AvatarId : null,
                    item != null && item.IsFriend,
                    item != null && item.IsPending, item.AvatarFrameId);
            }

            return mapped;
        }

        static IReadOnlyList<InboxItemData> MapInbox(InboxItemDto[] dto)
        {
            InboxItemDto[] items = dto ?? Array.Empty<InboxItemDto>();
            var mapped = new InboxItemData[items.Length];
            for (int i = 0; i < items.Length; i++)
            {
                InboxItemDto item = items[i];
                mapped[i] = new InboxItemData(
                    item != null ? item.Kind : null,
                    item != null ? item.Id : Guid.Empty,
                    item != null ? item.CreatedAt : default,
                    item != null ? item.PlayerId : null,
                    item != null ? item.Nickname : null,
                    item != null ? item.AvatarId : null,
                    item != null ? item.Gold : 0,
                    ToOwnedCards(item != null ? item.Cards : null),
                    item != null ? item.TitleKey : null, item.AvatarFrameId);
            }

            return mapped;
        }

        static OwnedCardData[] ToOwnedCards(OwnedCardDto[] cards)
        {
            if (cards == null || cards.Length == 0)
            {
                return Array.Empty<OwnedCardData>();
            }

            var mapped = new OwnedCardData[cards.Length];
            for (int i = 0; i < cards.Length; i++)
            {
                OwnedCardDto card = cards[i];
                mapped[i] = new OwnedCardData(
                    card != null ? card.CardId : null,
                    card != null ? card.Rarity : 0,
                    card != null ? card.Count : 0);
            }

            return mapped;
        }

        sealed class CredentialsRequest
        {
            public string Username { get; }
            public string Password { get; }

            public CredentialsRequest(string username, string password)
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
            public int? BackgroundId { get; }
            public long ExpectedRevision { get; }

            public int AvatarFrameId { get; }

            public UpdatePlayerProfileRequest(string nickname, int? avatarId, int? backgroundId, long expectedRevision, int avatarFrameId)
            {
                Nickname = nickname;
                AvatarFrameId = avatarFrameId;
                AvatarId = avatarId;
                BackgroundId = backgroundId;
                ExpectedRevision = expectedRevision;
            }
        }

        sealed class PurchaseShopItemRequest
        {
            public string CatalogType { get; }
            public int ItemId { get; }
            public long ExpectedRevision { get; }

            public PurchaseShopItemRequest(string catalogType, int itemId, long expectedRevision)
            {
                CatalogType = catalogType;
                ItemId = itemId;
                ExpectedRevision = expectedRevision;
            }
        }

        sealed class TargetPlayerRequest
        {
            public Guid TargetPlayerId { get; }

            public TargetPlayerRequest(Guid targetPlayerId)
            {
                TargetPlayerId = targetPlayerId;
            }
        }

        sealed class ClaimGiftRequest
        {
            public long ExpectedRevision { get; }

            public ClaimGiftRequest(long expectedRevision)
            {
                ExpectedRevision = expectedRevision;
            }
        }

        sealed class DrawCardsRequest
        {
            public int PackId { get; }
            public string PoolKey { get; }
            public int Count { get; }
            public long ExpectedRevision { get; }

            public DrawCardsRequest(int packId, string poolKey, int count, long expectedRevision)
            {
                PackId = packId;
                PoolKey = poolKey;
                Count = count;
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
            public int[] OwnedAvatarIds { get; set; }
            public int AvatarFrameId { get; set; }
            public int[] OwnedAvatarFrameIds { get; set; }
            public int? BackgroundId { get; set; }
            public int[] OwnedBackgroundIds { get; set; }
            public OwnedCardDto[] OwnedCards { get; set; }
            public string[] OwnedArtIds { get; set; }
            public long Gold { get; set; }
            public long Ur { get; set; }
            public long Revision { get; set; }
            public DateTimeOffset CreatedAt { get; set; }
            public DateTimeOffset UpdatedAt { get; set; }
        }

        [Preserve]
        sealed class OwnedCardDto
        {
            public OwnedCardDto() { }

            public string CardId { get; set; }
            public int Rarity { get; set; }
            public int Count { get; set; }
        }

        [Preserve]
        sealed class CardDrawResultDto
        {
            public CardDrawResultDto() { }

            public string CardId { get; set; }
            public int Rarity { get; set; }
            public string SourcePool { get; set; }
            public string ArtId { get; set; }
            public bool IsOverflow { get; set; }
            public long UrGained { get; set; }
        }

        [Preserve]
        sealed class CardDrawResponseDto
        {
            public CardDrawResponseDto() { }

            public CardDrawResultDto[] Results { get; set; }
            public PlayerDto Player { get; set; }
        }

        [Preserve]
        sealed class GachaPoolCardDto
        {
            public GachaPoolCardDto() { }

            public string CardId { get; set; }
            public string SourcePool { get; set; }
        }

        [Preserve]
        sealed class GachaPoolDto
        {
            public GachaPoolDto() { }

            public string PoolKey { get; set; }
            public GachaPoolCardDto[] Cards { get; set; }
        }

        [Preserve]
        sealed class FriendSummaryDto
        {
            public FriendSummaryDto() { }

            public Guid Id { get; set; }
            public string Nickname { get; set; }
            public int? AvatarId { get; set; }
            public int AvatarFrameId { get; set; }
        }

        [Preserve]
        sealed class FriendSearchHitDto
        {
            public FriendSearchHitDto() { }

            public Guid Id { get; set; }
            public string Nickname { get; set; }
            public int? AvatarId { get; set; }
            public int AvatarFrameId { get; set; }
            public bool IsFriend { get; set; }
            public bool IsPending { get; set; }
        }

        [Preserve]
        sealed class InboxItemDto
        {
            public InboxItemDto() { }

            public string Kind { get; set; }
            public Guid Id { get; set; }
            public DateTimeOffset CreatedAt { get; set; }
            public Guid? PlayerId { get; set; }
            public string Nickname { get; set; }
            public int? AvatarId { get; set; }
            public int? AvatarFrameId { get; set; }
            public long Gold { get; set; }
            public long Ur { get; set; }
            public OwnedCardDto[] Cards { get; set; }
            public string TitleKey { get; set; }
        }
    }
}
