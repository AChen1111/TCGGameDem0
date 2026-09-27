using System;
using System.Threading;
using AChen.Networking;
using Cysharp.Threading.Tasks;

namespace AChen.Player
{
    public sealed partial class PlayerSession
    {
        public UniTask<CardWorkshopResult> CraftCardAsync(string cardId, long expectedUrAmount, CancellationToken ct = default) =>
            ExecuteLockedAsync("CraftCard", cardId, async (player, token) =>
            {
                var result = await SendAuthenticatedCallAsync((access, retry) =>
                    Api.CraftCardAsync(access, cardId, player.Revision, expectedUrAmount, retry), token);
                SetCurrentPlayer(result.Player);
                return result;
            }, result => $"Revision={result.Player.Revision}; UR={result.Player.Ur}", ct);

        public UniTask<CardWorkshopResult> DismantleCardAsync(string cardId, int rarity, int count, long expectedUrAmount, CancellationToken ct = default) =>
            ExecuteLockedAsync("DismantleCard", cardId, async (player, token) =>
            {
                var result = await SendAuthenticatedCallAsync((access, retry) =>
                    Api.DismantleCardAsync(access, cardId, rarity, count, player.Revision, expectedUrAmount, retry), token);
                SetCurrentPlayer(result.Player);
                return result;
            }, result => $"Revision={result.Player.Revision}; UR={result.Player.Ur}", ct);

        public UniTask<GiftClaimResult> ClaimGiftAsync(Guid giftId, CancellationToken ct = default) =>
            ExecuteLockedAsync("ClaimGift", giftId.ToString("D"), async (player, token) =>
            {
                var result = await SendAuthenticatedCallAsync((access, retry) =>
                    Api.ClaimGiftAsync(access, giftId, player.Revision, retry), token);
                SetCurrentPlayer(result.Player);
                return result;
            }, result => $"Revision={result.Player.Revision}; UR={result.Player.Ur}", ct);
    }
}
