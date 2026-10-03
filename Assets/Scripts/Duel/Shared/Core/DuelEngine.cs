using System;
using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed partial class DuelEngine
    {
        public DuelState State { get; private set; }
        readonly DuelCardCatalog m_catalog;
        readonly DuelAbilityRegistry m_abilities;
        readonly CardRuleCatalog m_rules;
        public CardRuleCatalog Rules => m_rules;
        public DuelAbilityRegistry Abilities => m_abilities;
        public DuelCardCatalog Catalog => m_catalog;
        readonly List<DuelEvent> m_events = new List<DuelEvent>();

        public DuelEngine(DuelCardCatalog catalog, DuelStartRecord start)
            : this(catalog, start, DuelAbilityRegistry.CreateDefault()) { }

        public DuelEngine(DuelCardCatalog catalog, DuelStartRecord start, DuelAbilityRegistry abilities)
        {
            m_catalog = catalog;
            m_abilities = abilities;
            m_rules = CardRuleCatalog.CreateDefault(catalog);
            State = new DuelState { TurnPlayer = start.FirstPlayer, WaitingSeat = start.FirstPlayer,
                RandomState = start.Seed, Window = TimingWindow.FastResponse,
                RuleVersion = start.RuleVersion, CatalogHash = start.CatalogHash,
                ProtocolVersion = start.ProtocolVersion, RulePackageHash = start.RulePackageHash,
                NameCatalogHash = start.NameCatalogHash, BanlistHash = start.BanlistHash };
            for (int player = 0; player < 2; player++)
            {
                foreach (string id in start.MainDecks[player]) AddCard(player, id, DuelZone.Deck);
                foreach (string id in start.ExtraDecks[player]) AddCard(player, id, DuelZone.ExtraDeck);
                if (start.Shuffle) Shuffle(State.Players[player].Deck);
                Draw(player, start.OpeningHand);
            }
            RefreshCharacteristics();
            m_events.Clear();
            State.PendingFacts.Clear();
        }

        void AddCard(int player, string id, DuelZone zone)
        {
            var definition = m_catalog.Get(id);
            var card = new DuelCardState { InstanceId = State.NextInstanceId++, Generation = 1,
                DefinitionId = definition.CardId, Owner = player, Controller = player, Zone = zone,
                Position = CardPosition.FaceDown, CurrentAtk = definition.Attack, CurrentDef = definition.Defense,
                CurrentLevel = definition.Level, CurrentAttribute = definition.Attribute,
                CurrentRace = definition.Race, CurrentNameId = definition.OriginalNameId, CurrentNormal = definition.IsNormal };
            State.Cards.Add(card);
            (zone == DuelZone.Deck ? State.Players[player].Deck : State.Players[player].ExtraDeck).Add(card.InstanceId);
        }

        internal DuelCardState Card(int id) => State.Cards.First(x => x.InstanceId == id);

        internal void Draw(int player, int count)
        {
            if (HasEffect(EffectRecordKind.PreventDeckToHand, player)) return;
            for (int i = 0; i < count && !State.Finished; i++)
            {
                if (State.Players[player].Deck.Count == 0) { Finish(1 - player, "DRAW_EMPTY_DECK"); return; }
                var card = Card(State.Players[player].Deck[0]);
                Move(card, DuelZone.Hand, player, 0, CardPosition.FaceDown, MoveCause.Draw);
                Emit(DuelEventKind.Drawn, player, card, 1 << player, cause: MoveCause.Draw);
            }
        }

        internal static bool OnField(DuelCardState card) => card.Zone == DuelZone.Monster || card.Zone == DuelZone.ExtraMonster
            || card.Zone == DuelZone.SpellTrap || card.Zone == DuelZone.Field;
        internal static bool IsPublic(DuelCardState card) => card.Zone == DuelZone.Graveyard || card.Zone == DuelZone.Material
            || card.Zone == DuelZone.Banished && card.Position != CardPosition.FaceDown
            || OnField(card) && card.Position != CardPosition.FaceDown && card.Position != CardPosition.FaceDownDefense;

        internal void Emit(DuelEventKind kind, int player, DuelCardState card = null, int mask = 3,
            DuelZone from = DuelZone.Deck, int amount = 0, string detail = "", MoveCause cause = MoveCause.Rule,
            CardRef effectSource = default, int effectPlayer = -1, CardLastKnown before = null, CardLastKnown after = null)
        {
            var fact = new DuelEvent { Id = State.NextEventId++, Kind = kind, Player = player,
                PhaseAtEvent = State.Phase, ChainId = State.Chain.Count == 0 ? 0 : State.CurrentChainId,
                HasCard = card != null, Card = card == null ? default : card.Ref,
                DefinitionId = card == null ? "" : card.DefinitionId, VisibleToMask = mask,
                From = from, To = card == null ? default : card.Zone, Amount = amount,
                Attack = card == null ? 0 : card.CurrentAtk, Defense = card == null ? null : card.CurrentDef, Detail = detail,
                GroupId = State.CurrentEventGroupId, Cause = cause, EffectSource = effectSource, EffectPlayer = effectPlayer,
                Before = before, After = after ?? (card == null ? null : Snapshot(card)), SummonMethod = card == null ? default : card.SummonMethod,
                PresentCards = State.Cards.Select(Snapshot).ToList(), BattleAttacker = State.Attacker, BattleTarget = State.AttackTarget };
            m_events.Add(fact);
            State.PendingFacts.Add(fact);
            State.TurnFacts.Add(fact);
        }

        internal void Finish(int winner, string reason)
        {
            State.Finished = true; State.Winner = winner; State.EndReason = reason; State.Window = TimingWindow.Finished;
            State.PendingDecision = null; State.WaitingSeat = -1;
            State.PendingDestruction = null;
            Emit(DuelEventKind.Finished, winner, detail: reason);
        }

        internal uint NextRandom(uint bound)
        {
            ulong value = unchecked(State.RandomState += 0x9e3779b97f4a7c15UL);
            value = unchecked((value ^ value >> 30) * 0xbf58476d1ce4e5b9UL);
            value = unchecked((value ^ value >> 27) * 0x94d049bb133111ebUL);
            return (uint)((value ^ value >> 31) % bound);
        }

        internal void Shuffle(List<int> cards)
        {
            for (int i = cards.Count - 1; i > 0; i--)
            {
                int index = (int)NextRandom((uint)(i + 1));
                int value = cards[i]; cards[i] = cards[index]; cards[index] = value;
            }
            foreach (int id in cards) Card(id).TrackingEpoch++;
        }

        public DuelStepResult Apply(DuelCommand command)
        {
            m_events.Clear();
            string error = Validate(command);
            if (error.Length != 0) return new DuelStepResult { Error = error, Revision = State.Revision };
            State.CurrentEventGroupId = State.NextEventGroupId++;
            switch (command.Kind)
            {
                case DuelCommandKind.Pass: Pass(); break;
                case DuelCommandKind.Surrender: Finish(1 - command.Player, "SURRENDER"); break;
                case DuelCommandKind.Timeout: Finish(1 - command.Player, "TIMEOUT"); break;
                case DuelCommandKind.DisconnectTimeout: Finish(1 - command.Player, "DISCONNECT_TIMEOUT"); break;
                case DuelCommandKind.Abort: Finish(-1, "BOTH_DISCONNECTED"); break;
                case DuelCommandKind.NormalSummon:
                case DuelCommandKind.SetMonster: NormalSummon(command); break;
                case DuelCommandKind.SetSpellTrap: SetSpellTrap(command); break;
                case DuelCommandKind.ChangePosition: ChangePosition(command); break;
                case DuelCommandKind.Attack: DeclareAttack(command); break;
                case DuelCommandKind.SpecialSummon: ExecuteSpecialSummon(command); break;
                case DuelCommandKind.Activate: Activate(command); break;
                case DuelCommandKind.Answer: Answer(command); break;
                case DuelCommandKind.AdvancePhase:
                    State.PendingPhase = (int)command.Phase; State.Window = TimingWindow.FastResponse;
                    State.ConsecutivePasses = 1; State.WaitingSeat = 1 - command.Player; break;
            }
            RefreshCharacteristics();
            ProcessCheckpoints();
            RefreshCharacteristics();
            State.Revision++;
            return new DuelStepResult { Accepted = true, Revision = State.Revision, Events = new List<DuelEvent>(m_events) };
        }

        string Validate(DuelCommand command)
        {
            if (command.Player < 0 || command.Player > 1) return "INVALID_SEAT";
            if (State.Finished) return "DUEL_FINISHED";
            if (command.Kind == DuelCommandKind.Surrender || command.Kind == DuelCommandKind.Timeout
                || command.Kind == DuelCommandKind.DisconnectTimeout || command.Kind == DuelCommandKind.Abort) return "";
            if (State.WaitingSeat != command.Player) return "NOT_YOUR_WINDOW";
            if (State.PendingDecision != null) return command.Kind == DuelCommandKind.Answer ? ValidateAnswer(command) : "DECISION_REQUIRED";
            if (command.Kind == DuelCommandKind.Pass && State.Window != TimingWindow.Resolving) return "";
            if (command.Kind == DuelCommandKind.NormalSummon || command.Kind == DuelCommandKind.SetMonster)
                return ValidateNormalSummon(command);
            if (command.Kind == DuelCommandKind.SetSpellTrap) return ValidateSetSpellTrap(command);
            if (command.Kind == DuelCommandKind.ChangePosition) return ValidateChangePosition(command);
            if (command.Kind == DuelCommandKind.Attack) return ValidateAttack(command);
            if (command.Kind == DuelCommandKind.SpecialSummon) return ValidateSpecialSummon(command);
            if (command.Kind == DuelCommandKind.Activate) return ValidateActivation(command);
            if (command.Kind == DuelCommandKind.AdvancePhase)
                return MainOpen(command.Player) && (command.Phase == DuelPhase.End
                    || State.Phase == DuelPhase.Main1 && State.Turn > 1 && command.Phase == DuelPhase.Battle)
                    || State.TurnPlayer == command.Player && State.Window == TimingWindow.Open
                    && State.Phase == DuelPhase.Battle && command.Phase == DuelPhase.Main2 ? "" : "INVALID_PHASE_TRANSITION";
            return "ACTION_NOT_LEGAL";
        }

        void Pass()
        {
            if (State.Window == TimingWindow.Open)
            {
                State.PendingPhase = (int)NextPhase();
                State.Window = TimingWindow.FastResponse;
            }
            if (++State.ConsecutivePasses < 2) { State.WaitingSeat = 1 - State.WaitingSeat; return; }
            State.ConsecutivePasses = 0;
            if (State.Chain.Count > 0) { ResolveChain(); return; }
            State.PendingTriggers.RemoveAll(t => !t.Public);
            State.PrivateTriggerPlayers.Clear();
            if (State.BattleStep != BattleStep.None) { AdvanceBattle(); return; }
            if (State.PendingPhase >= 0)
            {
                var phase = (DuelPhase)State.PendingPhase; State.PendingPhase = -1; EnterPhase(phase); return;
            }
            if (State.Phase == DuelPhase.Draw || State.Phase == DuelPhase.Standby) EnterPhase(NextPhase());
            else if (State.Phase == DuelPhase.End) FinishEndPhase();
            else { State.LastCheckpointEvents.Clear(); State.Window = TimingWindow.Open; State.WaitingSeat = State.TurnPlayer; }
        }

        DuelPhase NextPhase()
        {
            switch (State.Phase)
            {
                case DuelPhase.Draw: return DuelPhase.Standby;
                case DuelPhase.Standby: return DuelPhase.Main1;
                case DuelPhase.Main1: return State.Turn == 1 ? DuelPhase.End : DuelPhase.Battle;
                case DuelPhase.Battle: return DuelPhase.Main2;
                default: return DuelPhase.End;
            }
        }

        void EnterPhase(DuelPhase phase)
        {
            State.Phase = phase; State.WaitingSeat = State.TurnPlayer; State.ConsecutivePasses = 0;
            if (ConsumePhaseSkip(phase))
            {
                if (phase == DuelPhase.End) StartNextTurn();
                else EnterPhase(NextPhase());
                return;
            }
            State.Window = phase == DuelPhase.Main1 || phase == DuelPhase.Main2 || phase == DuelPhase.Battle
                ? TimingWindow.Open : TimingWindow.FastResponse;
            Emit(DuelEventKind.PhaseChanged, State.TurnPlayer, detail: phase.ToString());
            ProcessPhaseObligations();
        }

        void StartNextTurn()
        {
            State.Turn++; State.TurnPlayer = 1 - State.TurnPlayer;
            RefreshCharacteristics();
            foreach (var player in State.Players) player.NormalSummonsThisTurn = 0;
            foreach (var card in State.Cards)
            { card.AttacksThisTurn = 0; card.AttackedTargetsThisTurn.Clear(); card.DirectAttackDeclaredThisTurn = false; }
            State.UsedAbilities.Clear();
            State.TurnFacts.Clear();
            EnterPhase(DuelPhase.Draw);
            Emit(DuelEventKind.TurnChanged, State.TurnPlayer);
            if (State.Phase == DuelPhase.Draw) Draw(State.TurnPlayer, 1);
        }

        void FinishEndPhase()
        {
            ProcessEndObligations();
            if (State.PendingDecision != null) return;
            if (State.PendingFacts.Count > 0)
            {
                State.ContinuationAfterTriggers = "end.turn";
                OpenResponse();
                return;
            }
            var hand = State.Cards.Where(c => c.Owner == State.TurnPlayer && c.Zone == DuelZone.Hand).ToArray();
            if (hand.Length <= 6) { StartNextTurn(); return; }
            State.PendingDecision = new DuelDecision { Id = State.NextDecisionId++, Player = State.TurnPlayer,
                Kind = DecisionKind.ChooseCards, Min = hand.Length - 6, Max = hand.Length - 6,
                Prompt = "弃置手牌至六张", Continuation = "end.discard",
                Options = hand.Select(card => new DecisionOption { Id = card.InstanceId.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    HasCard = true, Card = card.Ref, Label = m_catalog.Get(card.DefinitionId).Name }).ToList() };
            State.Window = TimingWindow.Decision; State.WaitingSeat = State.TurnPlayer;
            Emit(DuelEventKind.DecisionOpened, State.TurnPlayer, mask: 1 << State.TurnPlayer);
        }

        public IReadOnlyList<DuelAction> QueryLegalActions(int player)
        {
            var actions = new List<DuelAction>();
            if (State.Finished || player < 0 || player > 1) return actions;
            actions.Add(new DuelAction { Id = "surrender", Kind = DuelCommandKind.Surrender });
            if (player != State.WaitingSeat || State.PendingDecision != null) return actions;
            actions.Add(new DuelAction { Id = "pass", Kind = DuelCommandKind.Pass });
            actions.AddRange(AbilityActions(player));
            actions.AddRange(SpecialSummonActions(player));
            if (MainOpen(player))
            {
                actions.Add(new DuelAction { Id = "phase:end", Kind = DuelCommandKind.AdvancePhase, Phase = DuelPhase.End });
                if (State.Phase == DuelPhase.Main1 && State.Turn > 1)
                    actions.Add(new DuelAction { Id = "phase:battle", Kind = DuelCommandKind.AdvancePhase, Phase = DuelPhase.Battle });
            }
            else if (State.Window == TimingWindow.Open && State.Phase == DuelPhase.Battle)
            {
                actions.Add(new DuelAction { Id = "phase:main2", Kind = DuelCommandKind.AdvancePhase, Phase = DuelPhase.Main2 });
                actions.AddRange(AttackActions(player));
            }
            actions.AddRange(MainActions(player));
            return actions;
        }
    }
}
