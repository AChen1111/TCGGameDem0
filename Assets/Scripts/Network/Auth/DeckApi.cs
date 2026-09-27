using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AChen.Decks;
using Cysharp.Threading.Tasks;
using UnityEngine.Scripting;

namespace AChen.Networking
{
    /// <summary>卡组存储协议。业务调用使用 PlayerSession，以执行客户端规则和会话保护。</summary>
    public sealed class DeckApi
    {
        readonly BackendHttpClient m_http;
        public DeckApi(BackendHttpClient http) { m_http = http ?? throw new ArgumentNullException(nameof(http)); }
        const string Root = "/api/player/decks";

        public async UniTask<DeckData> CreateAsync(string accessToken, string name, CancellationToken token = default) =>
            ToData(await m_http.SendAsync<DeckDto>("POST", Root, new CreateBody { Name = name }, accessToken, token));

        public async UniTask<IReadOnlyList<DeckData>> ListAsync(string accessToken, CancellationToken token = default)
        {
            var values = await m_http.SendAsync<DeckDto[]>("GET", Root, null, accessToken, token);
            return Array.AsReadOnly(values.Select(x => ToData(x)).ToArray());
        }

        public async UniTask<DeckData> GetAsync(string accessToken, Guid id, CancellationToken token = default) =>
            ToData(await m_http.SendAsync<DeckDto>("GET", Root + "/" + id.ToString("D"), null, accessToken, token), id);

        public async UniTask<DeckData> SaveAsync(string accessToken, DeckData deck, CancellationToken token = default)
        {
            if (deck == null) throw new ArgumentNullException(nameof(deck));
            var body = new SaveBody { Name = deck.Name, MainDeck = ToDto(deck.MainDeck), ExtraDeck = ToDto(deck.ExtraDeck), ExpectedRevision = deck.Revision };
            return ToData(await m_http.SendAsync<DeckDto>("PUT", Root + "/" + deck.Id.ToString("D"), body, accessToken, token), deck.Id);
        }

        public async UniTask DeleteAsync(string accessToken, Guid id, long revision, CancellationToken token = default) =>
            await m_http.SendAsync("DELETE", Root + "/" + id.ToString("D") + "?expectedRevision="
                + revision.ToString(System.Globalization.CultureInfo.InvariantCulture), null, accessToken, token);

        public static DeckData ParseDeckJson(string json) => ToData(BackendJson.DeserializeResponse<DeckDto>(json));

        static DeckData ToData(DeckDto dto, Guid? expectedId = null)
        {
            if (dto == null || dto.Id == Guid.Empty || expectedId.HasValue && dto.Id != expectedId.Value
                || !DeckValidator.IsValidName(dto.Name) || !dto.Revision.HasValue || dto.Revision < 0
                || dto.MainDeck == null || dto.ExtraDeck == null)
                throw InvalidResponse();
            return new DeckData(dto.Id, dto.Name, ToEntries(dto.MainDeck), ToEntries(dto.ExtraDeck),
                dto.Revision.Value, dto.CreatedAt, dto.UpdatedAt);
        }

        static DeckCardEntry[] ToEntries(EntryDto[] entries) => entries.Select(x =>
        {
            if (x == null || string.IsNullOrWhiteSpace(x.CardId) || x.Rarity < 0 || x.Count <= 0) throw InvalidResponse();
            return new DeckCardEntry(x.CardId, x.Rarity, x.Count);
        }).ToArray();

        static EntryDto[] ToDto(IReadOnlyList<DeckCardEntry> entries) => entries.Select(x =>
            new EntryDto { CardId = x.CardId, Rarity = x.Rarity, Count = x.Count }).ToArray();

        static BackendApiException InvalidResponse() => new BackendApiException(0, "INVALID_RESPONSE", "服务器返回的卡组数据无效");

        [Preserve] sealed class CreateBody { public string Name { get; set; } }
        [Preserve] sealed class SaveBody
        {
            public string Name { get; set; }
            public EntryDto[] MainDeck { get; set; }
            public EntryDto[] ExtraDeck { get; set; }
            public long ExpectedRevision { get; set; }
        }
        [Preserve] sealed class EntryDto
        {
            public string CardId { get; set; }
            public int Rarity { get; set; }
            public int Count { get; set; }
        }
        [Preserve] sealed class DeckDto
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public EntryDto[] MainDeck { get; set; }
            public EntryDto[] ExtraDeck { get; set; }
            public long? Revision { get; set; }
            public DateTimeOffset CreatedAt { get; set; }
            public DateTimeOffset UpdatedAt { get; set; }
        }
    }
}
