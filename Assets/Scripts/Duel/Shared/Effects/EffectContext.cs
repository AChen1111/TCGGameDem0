using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class EffectContext
    {
        public DuelEngine Engine { get; }
        public DuelState State => Engine.State;
        public DuelCardCatalog Catalog => Engine.Catalog;
        public int Player { get; }
        public DuelCardState Source { get; }
        public CardRef SourceRef { get; }
        public DuelEvent TriggerEvent { get; }
        public EffectContext(DuelEngine engine, int player, int sourceId)
            : this(engine, player, sourceId, engine.Card(sourceId).Ref) { }
        public EffectContext(DuelEngine engine, int player, int sourceId, CardRef sourceRef, DuelEvent triggerEvent = null)
        { Engine = engine; Player = player; Source = engine.Card(sourceId); SourceRef = sourceRef; TriggerEvent = triggerEvent; }

        public void OpenDecision(DuelChainLink link, DecisionKind kind, IEnumerable<DecisionOption> options,
            string prompt, int min = 1, int max = 1, bool canCancel = false, int answeringPlayer = -1)
        {
            int player = answeringPlayer < 0 ? Player : answeringPlayer;
            State.PendingDecision = new DuelDecision { Id = State.NextDecisionId++, Player = player,
                Kind = kind, Min = min, Max = max, Prompt = prompt, CanCancel = canCancel,
                Continuation = link.AbilityId, SourceId = Source.InstanceId, Options = options.ToList() };
            State.Window = TimingWindow.Decision; State.WaitingSeat = player;
            Engine.Emit(DuelEventKind.DecisionOpened, player, mask: 1 << player);
        }

        public IEnumerable<DuelCardState> Deck => State.Players[Player].Deck.Select(Engine.Card);
        public DuelCardState Card(int id) => Engine.Card(id);

        public void SelectCards(DuelChainLink link, IEnumerable<DuelCardState> cards, string prompt, int min = 1, int max = 1)
        {
            State.PendingDecision = new DuelDecision
            {
                Id = State.NextDecisionId++, Player = Player, Kind = DecisionKind.ChooseCards,
                Min = min, Max = max, Prompt = prompt, Continuation = link.AbilityId, SourceId = Source.InstanceId,
                Options = cards.OrderBy(card => card.DefinitionId, System.StringComparer.Ordinal).ThenBy(card => card.InstanceId)
                    .Select(card => new DecisionOption { Id = card.InstanceId.ToString(System.Globalization.CultureInfo.InvariantCulture),
                        HasCard = true, Card = card.Ref, Label = Catalog.Get(card.DefinitionId).Name }).ToList()
            };
            State.Window = TimingWindow.Decision; State.WaitingSeat = Player;
            Engine.Emit(DuelEventKind.DecisionOpened, Player, mask: 1 << Player);
        }

        public void Move(DuelCardState card, DuelZone zone, CardPosition position = CardPosition.FaceUp,
            MoveCause cause = MoveCause.Effect) =>
            Engine.Move(card, zone, card.Owner, position: position, cause: cause, effectSource: SourceRef, effectPlayer: Player);
        public bool TryMove(DuelCardState card, DuelZone zone, CardPosition position = CardPosition.FaceUp,
            MoveCause cause = MoveCause.Effect) =>
            Engine.TryMove(card, zone, card.Owner, position: position, cause: cause, effectSource: SourceRef, effectPlayer: Player);
        public void MoveAsCost(DuelCardState card, DuelZone zone, CardPosition position = CardPosition.FaceUp) =>
            Move(card, zone, position, MoveCause.Cost);
        public bool CanSpecialSummon(DuelCardState card, int player, bool ignore = false, SummonMethod method = SummonMethod.Effect) =>
            Engine.CanSpecialSummonByEffect(card, player, ignore, method, Player);
        public bool SpecialSummon(DuelCardState card, int player, int slot, CardPosition position,
            bool ignore = false, SummonMethod method = SummonMethod.Effect) =>
            Engine.SpecialSummonByEffect(card, player, slot, position, SourceRef, ignore, method, Player);
        public void AddEffect(DuelEffectRecord record)
        { record.Source = SourceRef; record.SourceDefinitionId = Source.DefinitionId; record.ActivationPlayer = Player; Engine.AddEffect(record); }
        public bool IsAffected(DuelCardState card) => Engine.IsAffectedBy(card, SourceRef);
        public bool CanTarget(DuelCardState card) => Engine.CanTargetBy(card, SourceRef);
        public bool Destroy(DuelCardState card) => Engine.DestroyByEffect(card, SourceRef, Player);
        public DuelDestructionOperation DestroyMany(DuelChainLink link, IEnumerable<DuelCardState> cards, int operationKey = 0) =>
            Engine.BeginDestruction(link, cards, SourceRef, Player, MoveCause.Effect, operationKey);
        public void Reveal(DuelCardState card) => Engine.Emit(DuelEventKind.Revealed, Player, card, 3);
        public void Recover(int player, int amount)
        {
            State.Players[player].LifePoints += amount;
            Engine.Emit(DuelEventKind.Recovered, player, amount: amount);
        }
        public void ShuffleDeck()
        {
            Engine.Shuffle(State.Players[Player].Deck);
            Engine.Emit(DuelEventKind.Shuffled, Player);
        }
    }
}
