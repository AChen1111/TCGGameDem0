using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AChen.Decks;
using AChen.Networking;
using Cysharp.Threading.Tasks;

namespace AChen.Player
{
    public sealed partial class PlayerSession
    {
        DeckApi m_deckApi;
        DeckApi DecksApi => m_deckApi ??= new DeckApi(new BackendHttpClient(m_config));

        public UniTask<IReadOnlyList<DeckData>> GetDecksAsync(CancellationToken token = default) =>
            SendAuthenticatedCallAsync((access, ct) => DecksApi.ListAsync(access, ct), token);

        public UniTask<DeckData> GetDeckAsync(Guid id, CancellationToken token = default) =>
            SendAuthenticatedCallAsync((access, ct) => DecksApi.GetAsync(access, id, ct), token);

        public UniTask<DeckData> CreateDeckAsync(string name, CancellationToken token = default) =>
            ExecuteLockedAsync("CreateDeck", name, (_, ct) => SendAuthenticatedCallAsync((access, retryToken) =>
            {
                var empty = new DeckData(Guid.Empty, name, Array.Empty<DeckCardEntry>(), Array.Empty<DeckCardEntry>());
                return DecksApi.CreateAsync(access, PrepareDeckSave(empty).Name, retryToken);
            }, ct), deck => $"Deck={deck.Id}; Revision={deck.Revision}", token);

        /// <summary>冻结本次提交内容。成功返回新版本；调用方用返回值建立下一份草稿，失败不会改变原草稿。</summary>
        public UniTask<DeckData> SaveDeckAsync(DeckDraft draft, CancellationToken token = default)
        {
            if (draft == null) throw new ArgumentNullException(nameof(draft));
            DeckData snapshot = draft.ToData();
            return ExecuteLockedAsync("SaveDeck", snapshot.Id.ToString("D"), (_, ct) =>
                SendAuthenticatedCallAsync((access, retryToken) =>
                    DecksApi.SaveAsync(access, PrepareDeckSave(snapshot), retryToken), ct),
                deck => $"Deck={deck.Id}; Revision={deck.Revision}", token);
        }

        public async UniTask DeleteDeckAsync(Guid id, long expectedRevision, CancellationToken token = default) =>
            await ExecuteLockedAsync("DeleteDeck", id.ToString("D"), (_, ct) =>
                SendAuthenticatedCallAsync(async (access, retryToken) =>
                {
                    await DecksApi.DeleteAsync(access, id, expectedRevision, retryToken);
                    return true;
                }, ct), _ => "Deleted", token);

        /// <summary>按最新配置及当前收藏校验；不需要联网，也不会修改卡组或收藏。</summary>
        public DeckValidationResult ValidateDeck(DeckData deck, DeckValidationMode mode = DeckValidationMode.Playable) =>
            DeckValidator.Validate(deck, LocalGameConfiguration.IsReady ? LocalGameConfiguration.DeckRules : null,
                DeckInventory(), mode);

        DeckData PrepareDeckSave(DeckData deck) => DeckValidator.PrepareSave(deck,
            LocalGameConfiguration.IsReady ? LocalGameConfiguration.DeckRules : null, DeckInventory());

        DeckCardEntry[] DeckInventory() => CurrentPlayer?.OwnedCards
            .Select(x => new DeckCardEntry(x.CardId, x.Rarity, x.Count)).ToArray();
    }
}
