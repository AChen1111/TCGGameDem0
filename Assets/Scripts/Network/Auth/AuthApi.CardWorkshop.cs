using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.Scripting;

namespace AChen.Networking
{
    public sealed class CardWorkshopResult
    {
        public PlayerData Player { get; }
        public long UrAmount { get; }
        internal CardWorkshopResult(PlayerData player, long amount) { Player = player; UrAmount = amount; }
    }
    public sealed class GiftClaimResult
    {
        public PlayerData Player { get; }
        public long UrGained { get; }
        internal GiftClaimResult(PlayerData player, long gained) { Player = player; UrGained = gained; }
    }

    public sealed partial class AuthApi
    {
        public async UniTask<CardWorkshopResult> CraftCardAsync(string access, string cardId, long revision, long quote, CancellationToken ct)
        {
            var dto = await m_http.SendAsync<WorkshopDto>("POST", "/api/player/cards/craft",
                new CraftBody { CardId = cardId, ExpectedRevision = revision, ExpectedUrAmount = quote }, access, ct);
            return new CardWorkshopResult(ToPlayer(dto.Player), dto.UrAmount);
        }
        public async UniTask<CardWorkshopResult> DismantleCardAsync(string access, string cardId, int rarity, int count, long revision, long quote, CancellationToken ct)
        {
            var dto = await m_http.SendAsync<WorkshopDto>("POST", "/api/player/cards/dismantle",
                new DismantleBody { CardId = cardId, Rarity = rarity, Count = count, ExpectedRevision = revision, ExpectedUrAmount = quote }, access, ct);
            return new CardWorkshopResult(ToPlayer(dto.Player), dto.UrAmount);
        }
        [Preserve] class CraftBody
        {
            public string CardId { get; set; }
            public long ExpectedRevision { get; set; }
            public long ExpectedUrAmount { get; set; }
        }
        [Preserve] sealed class DismantleBody : CraftBody
        {
            public int Rarity { get; set; }
            public int Count { get; set; }
        }
        [Preserve] sealed class WorkshopDto
        {
            public PlayerDto Player { get; set; }
            public long UrAmount { get; set; }
        }
        [Preserve] sealed class GiftClaimDto
        {
            public PlayerDto Player { get; set; }
            public long UrGained { get; set; }
        }
    }
}
