using System;
using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class SeatInput
    {
        public long Revision { get; set; }
        public string ActionToken { get; set; } = "";
        public string[] OptionTokens { get; set; } = Array.Empty<string>();
        public string TargetViewCardId { get; set; } = "";
        public string[] SelectionViewCardIds { get; set; } = Array.Empty<string>();
        public int Slot { get; set; }
        public CardPosition Position { get; set; } = CardPosition.FaceUpAttack;
        public DuelPhase Phase { get; set; }
        public string NameId { get; set; } = "";
    }

    public sealed class ProjectedDecisionOption
    {
        public string OptionToken { get; }
        public string Label { get; }
        public string DefinitionId { get; }
        internal ProjectedDecisionOption(string token, string label, string definition)
        { OptionToken = token; Label = label; DefinitionId = definition; }
    }

    public sealed class ProjectedAction
    {
        public string ActionToken { get; }
        public DuelCommandKind Kind { get; }
        public string SourceViewCardId { get; }
        public string AbilityId { get; }
        public DuelPhase Phase { get; }
        public IReadOnlyList<string> TargetViewCardIds { get; }
        public IReadOnlyList<string> SelectionViewCardIds { get; }
        public int MinSelections { get; }
        public int MaxSelections { get; }
        public IReadOnlyList<int> Slots { get; }
        public bool CanAttackDirectly { get; }
        public IReadOnlyList<CardPosition> Positions { get; }
        public string Label { get; }
        public bool RequiresNameDeclaration { get; }
        public RuleCardKind DeclarationKind { get; }
        public bool SelectionIsTarget { get; }
        internal ProjectedAction(string token, DuelAction action, string source, IEnumerable<string> targets, IEnumerable<string> selections)
        {
            ActionToken = token; Kind = action.Kind; SourceViewCardId = source; AbilityId = action.AbilityId;
            Phase = action.Phase;
            TargetViewCardIds = Array.AsReadOnly(targets.ToArray()); Slots = Array.AsReadOnly(action.Slots.ToArray());
            SelectionViewCardIds = Array.AsReadOnly(selections.ToArray());
            MinSelections = action.MinSelections; MaxSelections = action.MaxSelections;
            CanAttackDirectly = action.CanAttackDirectly;
            Positions = Array.AsReadOnly(action.Positions.ToArray());
            Label = action.Label;
            RequiresNameDeclaration = action.RequiresNameDeclaration; DeclarationKind = action.DeclarationKind;
            SelectionIsTarget = action.SelectionIsTarget;
        }
    }

    public sealed class ProjectedDuelEvent
    {
        public long EventId { get; }
        public DuelEventKind Kind { get; }
        public int Player { get; }
        public string ViewCardId { get; }
        public string DefinitionId { get; }
        public DuelZone From { get; }
        public DuelZone To { get; }
        public int Amount { get; }
        public int? Attack { get; }
        public int? Defense { get; }
        public string DeclaredNameId { get; }
        internal ProjectedDuelEvent(DuelEvent fact, bool visible, string viewId)
        {
            EventId = fact.Id; Kind = fact.Kind; Player = fact.Player; From = fact.From; To = fact.To;
            Amount = fact.Amount; ViewCardId = visible ? viewId : "";
            DefinitionId = visible ? fact.DefinitionId : "";
            Attack = visible && fact.HasCard ? fact.Attack : (int?)null;
            Defense = visible && fact.HasCard ? fact.Defense : null;
            DeclaredNameId = fact.DeclaredNameId;
        }
    }

    public sealed class ProjectedDecision
    {
        public DecisionKind Kind { get; }
        public string Prompt { get; }
        public int Min { get; }
        public int Max { get; }
        public bool CanCancel { get; }
        public IReadOnlyList<ProjectedDecisionOption> Options { get; }
        internal ProjectedDecision(DuelDecision decision, IEnumerable<ProjectedDecisionOption> options)
        {
            Kind = decision.Kind; Prompt = decision.Prompt; Min = decision.Min; Max = decision.Max;
            CanCancel = decision.CanCancel; Options = Array.AsReadOnly(options.ToArray());
        }
    }

    public sealed class DuelPlayerSnapshot
    {
        public int LifePoints { get; }
        public int HandCount { get; }
        public int DeckCount { get; }
        public int ExtraDeckCount { get; }
        internal DuelPlayerSnapshot(DuelState state, int seat)
        {
            LifePoints = state.Players[seat].LifePoints;
            HandCount = state.Cards.Count(c => c.Controller == seat && c.Zone == DuelZone.Hand);
            DeckCount = state.Cards.Count(c => c.Owner == seat && c.Zone == DuelZone.Deck);
            ExtraDeckCount = state.Cards.Count(c => c.Owner == seat && c.Zone == DuelZone.ExtraDeck);
        }
    }

    public sealed class ProjectedCard
    {
        public string ViewCardId { get; }
        public string DefinitionId { get; }
        public int Owner { get; }
        public int Controller { get; }
        public DuelZone Zone { get; }
        public int Slot { get; }
        public CardPosition Position { get; }
        public int? Attack { get; }
        public int? Defense { get; }
        internal ProjectedCard(string viewId, DuelCardState card, bool known)
        {
            ViewCardId = viewId; DefinitionId = known ? card.DefinitionId : "";
            Owner = card.Owner; Controller = card.Controller; Zone = card.Zone;
            Slot = card.Slot; Position = card.Position; Attack = known ? card.CurrentAtk : (int?)null;
            Defense = known ? card.CurrentDef : null;
        }
    }

    public sealed class DuelSeatSnapshot
    {
        public int Seat { get; }
        public long Revision { get; }
        public int Turn { get; }
        public int TurnPlayer { get; }
        public DuelPhase Phase { get; }
        public TimingWindow Window { get; }
        public int WaitingSeat { get; }
        public bool Finished { get; }
        public int Winner { get; }
        public string EndReason { get; }
        public IReadOnlyList<DuelPlayerSnapshot> Players { get; }
        public IReadOnlyList<ProjectedCard> Cards { get; }
        public IReadOnlyList<ProjectedChainLink> Chain { get; }
        public ProjectedDecision Decision { get; }
        public IReadOnlyList<ProjectedAction> Actions { get; }
        internal DuelSeatSnapshot(int seat, DuelState state, IEnumerable<ProjectedCard> cards, ProjectedDecision decision,
            IEnumerable<ProjectedAction> actions)
        {
            Seat = seat; Revision = state.Revision;
            Turn = state.Turn; TurnPlayer = state.TurnPlayer; Phase = state.Phase; Window = state.Window;
            WaitingSeat = state.WaitingSeat; Finished = state.Finished; Winner = state.Winner; EndReason = state.EndReason;
            Players = Array.AsReadOnly(new[] { new DuelPlayerSnapshot(state, 0), new DuelPlayerSnapshot(state, 1) });
            Cards = Array.AsReadOnly(cards.ToArray());
            Decision = decision;
            Actions = Array.AsReadOnly(actions.ToArray());
            Chain = Array.AsReadOnly(state.Chain.Select(x => new ProjectedChainLink(x)).ToArray());
        }
    }

    public sealed class ProjectedChainLink
    {
        public int Number { get; }
        public int Player { get; }
        public string DefinitionId { get; }
        public string AbilityId { get; }
        public bool ActivationNegated { get; }
        public bool EffectNegated { get; }
        internal ProjectedChainLink(DuelChainLink link)
        {
            Number = link.Number; Player = link.Player; DefinitionId = link.DefinitionId;
            AbilityId = link.AbilityId; ActivationNegated = link.ActivationNegated; EffectNegated = link.EffectNegated;
        }
    }

    /// <summary>One projector per authenticated seat; its opaque handles never enter the rules state.</summary>
    public sealed class SeatProjection
    {
        readonly int m_seat;
        readonly Dictionary<(CardRef, int), string> m_cardHandles = new Dictionary<(CardRef, int), string>();
        readonly Dictionary<string, DecisionOption> m_options = new Dictionary<string, DecisionOption>(StringComparer.Ordinal);
        readonly Dictionary<string, DuelAction> m_actions = new Dictionary<string, DuelAction>(StringComparer.Ordinal);
        readonly Dictionary<string, string> m_actionTokens = new Dictionary<string, string>(StringComparer.Ordinal);
        readonly Dictionary<string, (CardRef Reference, int Epoch)> m_cardByHandle = new Dictionary<string, (CardRef, int)>(StringComparer.Ordinal);
        long m_revision = -1;
        long m_decisionId = -1;
        long m_actionRevision = -1;
        ProjectedDecision m_decision;
        public SeatProjection(int seat)
        {
            if (seat < 0 || seat > 1) throw new ArgumentOutOfRangeException(nameof(seat));
            m_seat = seat;
        }
        public DuelSeatSnapshot Project(DuelState state) => Project(state, Array.Empty<DuelAction>());
        public IReadOnlyList<ProjectedDuelEvent> ProjectEvents(IEnumerable<DuelEvent> events)
        {
            var result = new List<ProjectedDuelEvent>();
            foreach (var fact in events)
            {
                bool visible = (fact.VisibleToMask & (1 << m_seat)) != 0;
                string handle = "";
                if (visible && fact.HasCard)
                    foreach (var card in m_cardHandles)
                        if (card.Key.Item1.Equals(fact.Card)) { handle = card.Value; break; }
                result.Add(new ProjectedDuelEvent(fact, visible, handle));
            }
            return result.AsReadOnly();
        }
        public DuelSeatSnapshot Project(DuelState state, IReadOnlyList<DuelAction> legalActions)
        {
            var cards = state.Cards.Where(VisibleObject).ToArray();
            var active = new HashSet<(CardRef, int)>(cards.Select(c => (c.Ref, c.TrackingEpoch)));
            foreach (var key in m_cardHandles.Keys.Where(k => !active.Contains(k)).ToArray()) m_cardHandles.Remove(key);
            ProjectDecision(state);
            m_cardByHandle.Clear();
            foreach (var card in cards) m_cardByHandle.Add(Handle(card), (card.Ref, card.TrackingEpoch));
            var actions = ProjectActions(state, legalActions);
            return new DuelSeatSnapshot(m_seat, state, cards
                .Select(c => new ProjectedCard(Handle(c), c, Known(c)))
                .OrderBy(c => c.Controller).ThenBy(c => c.Zone).ThenBy(c => c.Slot).ThenBy(c => c.ViewCardId, StringComparer.Ordinal), m_decision, actions);
        }
        IEnumerable<ProjectedAction> ProjectActions(DuelState state, IReadOnlyList<DuelAction> legalActions)
        {
            m_actions.Clear();
            if (m_actionRevision != state.Revision) { m_actionTokens.Clear(); m_actionRevision = state.Revision; }
            var offered = new HashSet<string>(legalActions.Select(x => x.Id), StringComparer.Ordinal);
            foreach (string id in m_actionTokens.Keys.Where(x => !offered.Contains(x)).ToArray()) m_actionTokens.Remove(id);
            foreach (var action in legalActions)
            {
                if (!m_actionTokens.TryGetValue(action.Id, out string token))
                    m_actionTokens.Add(action.Id, token = Guid.NewGuid().ToString("N"));
                var copy = new DuelAction { Id = action.Id, Kind = action.Kind, Card = action.Card,
                    AbilityId = action.AbilityId, Phase = action.Phase, Slots = new List<int>(action.Slots), Targets = new List<CardRef>(action.Targets),
                    SelectionCards = new List<CardRef>(action.SelectionCards), MinSelections = action.MinSelections, MaxSelections = action.MaxSelections,
                    CanAttackDirectly = action.CanAttackDirectly, Positions = new List<CardPosition>(action.Positions) };
                copy.ActivationOptions = (string[])action.ActivationOptions.Clone(); copy.Label = action.Label;
                copy.RequiresNameDeclaration = action.RequiresNameDeclaration; copy.DeclarationKind = action.DeclarationKind;
                copy.SelectionIsTarget = action.SelectionIsTarget;
                m_actions.Add(token, copy);
                string source = action.Card.InstanceId == 0 ? "" : Handle(state.Cards.Single(c => c.Ref.Equals(action.Card)));
                yield return new ProjectedAction(token, copy, source, action.Targets.Select(target =>
                    m_cardByHandle.Single(x => x.Value.Reference.Equals(target)).Key), action.SelectionCards.Select(card =>
                    m_cardByHandle.Single(x => x.Value.Reference.Equals(card)).Key));
            }
        }
        void ProjectDecision(DuelState state)
        {
            var decision = state.PendingDecision;
            long currentId = decision != null && decision.Player == m_seat ? decision.Id : -1;
            if (state.Revision == m_revision && currentId == m_decisionId) return;
            m_revision = state.Revision; m_decisionId = currentId; m_options.Clear(); m_decision = null;
            if (currentId == -1) return;
            var options = new List<ProjectedDecisionOption>();
            foreach (var option in decision.Options)
            {
                string token = Guid.NewGuid().ToString("N");
                m_options.Add(token, new DecisionOption { Id = option.Id, Label = option.Label, Card = option.Card,
                    HasCard = option.HasCard, Value = option.Value });
                string definition = "", label = option.Label;
                if (option.HasCard)
                {
                    var card = state.Cards.Single(c => c.Ref.Equals(option.Card));
                    bool known = Known(card);
                    definition = known ? card.DefinitionId : "";
                    if (!known) label = "未知卡牌";
                }
                options.Add(new ProjectedDecisionOption(token, label, definition));
            }
            m_decision = new ProjectedDecision(decision, options.OrderBy(x => x.DefinitionId, StringComparer.Ordinal)
                .ThenBy(x => x.Label, StringComparer.Ordinal).ThenBy(x => x.OptionToken, StringComparer.Ordinal));
        }

        public bool TryResolveInput(DuelState state, SeatInput input, out DuelCommand command)
        {
            command = new DuelCommand();
            if (input.Revision != state.Revision || m_revision != state.Revision) return false;
            if (m_actions.TryGetValue(input.ActionToken, out var action) && action.Kind == DuelCommandKind.Surrender)
                return ResolveAction(state, input, out command);
            var decision = state.PendingDecision;
            if (decision == null) return ResolveAction(state, input, out command);
            if (decision.Player != m_seat || decision.Id != m_decisionId) return false;
            if (input.OptionTokens.Length < decision.Min || input.OptionTokens.Length > decision.Max
                || input.OptionTokens.Distinct(StringComparer.Ordinal).Count() != input.OptionTokens.Length) return false;
            var options = new List<string>();
            foreach (string token in input.OptionTokens)
            {
                if (!m_options.TryGetValue(token, out var option)) return false;
                options.Add(option.Id);
            }
            command = new DuelCommand { Kind = DuelCommandKind.Answer, Player = m_seat,
                DecisionId = decision.Id, Options = options.ToArray(), NameId = input.NameId };
            return true;
        }

        public bool TryResolveNameDeclaration(DuelState state, string actionToken, out DuelCommand command)
        {
            command = new DuelCommand();
            if (m_actionRevision != state.Revision || !m_actions.TryGetValue(actionToken, out var action)
                || !action.RequiresNameDeclaration || action.Kind != DuelCommandKind.Activate
                || !state.Cards.Any(card => card.Ref.Equals(action.Card))) return false;
            command = new DuelCommand { Kind = action.Kind, Player = m_seat, CardId = action.Card.InstanceId,
                AbilityId = action.AbilityId, Options = (string[])action.ActivationOptions.Clone() };
            return true;
        }
        bool ResolveAction(DuelState state, SeatInput input, out DuelCommand command)
        {
            command = new DuelCommand();
            if (!m_actions.TryGetValue(input.ActionToken, out var action)) return false;
            if (action.Kind == DuelCommandKind.Timeout || action.Kind == DuelCommandKind.Answer) return false;
            if (action.Card.InstanceId != 0 && !state.Cards.Any(c => c.Ref.Equals(action.Card))) return false;
            if (action.Slots.Count != 0 && !action.Slots.Contains(input.Slot)) return false;
            if (action.Positions.Count != 0 && !action.Positions.Contains(input.Position)) return false;
            int target = 0;
            if (input.TargetViewCardId.Length != 0)
            {
                if (!m_cardByHandle.TryGetValue(input.TargetViewCardId, out var card)
                    || !action.Targets.Contains(card.Reference)
                    || !state.Cards.Any(c => c.Ref.Equals(card.Reference) && c.TrackingEpoch == card.Epoch)) return false;
                target = card.Reference.InstanceId;
            }
            else if (action.Targets.Count != 0 && !(action.Kind == DuelCommandKind.Attack && action.CanAttackDirectly)) return false;
            if (input.SelectionViewCardIds.Length < action.MinSelections || input.SelectionViewCardIds.Length > action.MaxSelections
                || input.SelectionViewCardIds.Distinct(StringComparer.Ordinal).Count() != input.SelectionViewCardIds.Length) return false;
            var selections = new List<int>();
            foreach (string handle in input.SelectionViewCardIds)
            {
                if (!m_cardByHandle.TryGetValue(handle, out var selected) || !action.SelectionCards.Contains(selected.Reference)
                    || !state.Cards.Any(c => c.Ref.Equals(selected.Reference) && c.TrackingEpoch == selected.Epoch)) return false;
                selections.Add(selected.Reference.InstanceId);
            }
            command = new DuelCommand { Kind = action.Kind, Player = m_seat, CardId = action.Card.InstanceId,
                TargetId = target, AbilityId = action.AbilityId, Slot = input.Slot, Position = input.Position, Phase = action.Phase,
                Cards = selections.ToArray(), Options = (string[])action.ActivationOptions.Clone(),
                NameId = action.RequiresNameDeclaration ? input.NameId : "" };
            return true;
        }
        bool VisibleObject(DuelCardState card)
        {
            if ((card.RevealedToMask & (1 << m_seat)) != 0) return true;
            if (card.Zone == DuelZone.Deck) return false;
            if (card.Zone == DuelZone.Hand) return card.Controller == m_seat;
            if (card.Zone == DuelZone.ExtraDeck) return card.Owner == m_seat || FaceUp(card.Position);
            return true;
        }
        bool Known(DuelCardState card)
        {
            if ((card.RevealedToMask & (1 << m_seat)) != 0) return true;
            if (card.Zone == DuelZone.Hand || card.Zone == DuelZone.Deck) return card.Controller == m_seat;
            return card.Controller == m_seat || FaceUp(card.Position);
        }
        static bool FaceUp(CardPosition position) => position != CardPosition.FaceDown && position != CardPosition.FaceDownDefense;
        string Handle(DuelCardState card)
        {
            var key = (card.Ref, card.TrackingEpoch);
            if (!m_cardHandles.TryGetValue(key, out var handle))
                m_cardHandles.Add(key, handle = Guid.NewGuid().ToString("N"));
            return handle;
        }
    }
}
