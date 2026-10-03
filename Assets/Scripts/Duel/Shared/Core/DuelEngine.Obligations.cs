using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

namespace AChen.Duel.Core
{
    public sealed partial class DuelEngine
    {
        internal void ProcessPhaseObligations()
        {
            foreach (var record in State.Effects.Where(e => (e.Kind == EffectRecordKind.DelayedDestroy || e.Kind == EffectRecordKind.DelayedSummon
                || e.Kind == EffectRecordKind.DelayedReturn)
                && e.ExpiresPhase == State.Phase && (e.ExpiresTurn == 0 || e.ExpiresTurn <= State.Turn)).OrderBy(e => e.Id).ToArray())
            {
                if (record.Kind == EffectRecordKind.DelayedDestroy)
                {
                    State.Effects.Remove(record);
                    var target = State.Cards.FirstOrDefault(c => c.Ref.Equals(record.Target) && OnField(c));
                    if (target != null && !BeginDestruction(null, new[] { target }, record.Source, record.ActivationPlayer,
                        MoveCause.Effect, continuation: "phase").Completed) return;
                    continue;
                }
                if (record.Kind == EffectRecordKind.DelayedReturn)
                {
                    var target = State.Cards.FirstOrDefault(c => c.Ref.Equals(record.Target) && c.Zone == DuelZone.Banished);
                    if (target == null) { State.Effects.Remove(record); continue; }
                    var slots = Enumerable.Range(0, 5).Where(slot => !State.Cards.Any(c => c.Controller == record.Player
                        && c.Zone == DuelZone.Monster && c.Slot == slot)).ToArray();
                    if (slots.Length == 0 || !Rules.Get(target.DefinitionId).AllowsMonsterZoneEntry(target, record.Player, State, m_catalog))
                    { State.Effects.Remove(record); Move(target, DuelZone.Graveyard, target.Owner, cause: MoveCause.Rule); continue; }
                    OpenObligationDecision(record, "return-zone", DecisionKind.ChooseZone,
                        slots.Select(slot => new DecisionOption { Id = slot.ToString(CultureInfo.InvariantCulture),
                            Value = slot.ToString(CultureInfo.InvariantCulture), Label = "返回区域 " + (slot + 1) }), new[] { target.InstanceId });
                    return;
                }
                var candidates = State.Cards.Where(c => c.Owner == record.Player && c.Zone == DuelZone.Graveyard
                    && (record.Target.InstanceId == 0 || c.Ref.Equals(record.Target))
                    && (record.Cards.Count == 0 || record.Cards.Contains(c.Ref))
                    && (record.Value == 0 || m_catalog.Get(c.DefinitionId).BelongsTo(record.Value))
                    && (record.Names.Count == 0 || record.Names.Contains(m_catalog.Get(c.DefinitionId).OriginalNameId))
                    && CanSpecialSummonByEffect(c, record.Player)).OrderBy(c => c.InstanceId).ToArray();
                if (candidates.Length == 0) { State.Effects.Remove(record); continue; }
                OpenObligationDecision(record, "card", DecisionKind.ChooseCards, candidates.Select(c => new DecisionOption {
                    Id = c.InstanceId.ToString(CultureInfo.InvariantCulture), HasCard = true, Card = c.Ref, Label = m_catalog.Get(c.DefinitionId).Name }));
                return;
            }
        }
        internal void ProcessEndObligations()
        {
            foreach (var record in State.Effects.Where(e => e.Kind == EffectRecordKind.EndShuffleHand
                && e.ExpiresPhase == DuelPhase.End && (e.ExpiresTurn == 0 || e.ExpiresTurn <= State.Turn)).OrderBy(e => e.Id).ToArray())
            {
                State.Effects.Remove(record);
                var hand = State.Cards.Where(c => c.Controller == record.Player && c.Zone == DuelZone.Hand).OrderBy(c => c.InstanceId).ToList();
                int limit = State.Cards.Count(c => c.Controller == 1 - record.Player && OnField(c)) + record.Value;
                bool returned = hand.Count > limit;
                while (hand.Count > limit)
                {
                    int index = (int)NextRandom((uint)hand.Count);
                    var card = hand[index]; hand.RemoveAt(index);
                    Move(card, DuelZone.Deck, card.Owner, position: CardPosition.FaceDown, cause: MoveCause.Effect,
                        effectSource: record.Source, effectPlayer: record.Player);
                }
                if (returned) { Shuffle(State.Players[record.Player].Deck); Emit(DuelEventKind.Shuffled, record.Player); }
            }
            foreach (var record in State.Effects.Where(e => e.Kind == EffectRecordKind.ReturnControl
                && e.ExpiresPhase == DuelPhase.End && (e.ExpiresTurn == 0 || e.ExpiresTurn <= State.Turn)).OrderBy(e => e.Id).ToArray())
            {
                State.Effects.Remove(record);
                var target = State.Cards.FirstOrDefault(c => c.Ref.Equals(record.Target) && OnField(c));
                if (target == null || target.Controller == record.Player) continue;
                var slots = Enumerable.Range(0, 5).Where(slot => !State.Cards.Any(c => c.Controller == record.Player
                    && c.Zone == DuelZone.Monster && c.Slot == slot)).ToArray();
                if (slots.Length == 0) Move(target, DuelZone.Graveyard, target.Owner, cause: MoveCause.Rule);
                else Move(target, DuelZone.Monster, record.Player, slots.Contains(target.Slot) ? target.Slot : slots[0],
                    target.Position, MoveCause.Rule);
            }
            RefreshCharacteristics();
        }
        internal bool ConsumePhaseSkip(DuelPhase phase)
        {
            var skip = ApplicableEffects().FirstOrDefault(e => e.Kind == EffectRecordKind.SkipPhase
                && (e.Player < 0 || e.Player == State.TurnPlayer) && e.Value == (int)phase
                && (e.ExpiresTurn == 0 || e.ExpiresTurn <= State.Turn));
            if (skip == null) return false;
            if (skip.Remaining > 1) skip.Remaining--; else State.Effects.Remove(skip);
            return true;
        }
        internal void ResolveObligationDecision(DuelCommand command)
        {
            var decision = State.PendingDecision;
            var continuation = decision.Continuation.Split('.');
            var record = State.Effects.Single(e => e.Id == long.Parse(continuation[1], CultureInfo.InvariantCulture));
            var choice = decision.Options.Single(o => o.Id == command.Options.Single());
            State.PendingDecision = null;
            if (continuation[2] == "return-zone")
            {
                var target = Card(decision.Selected.Single());
                if (!TryMove(target, DuelZone.Monster, record.Player, int.Parse(choice.Value, CultureInfo.InvariantCulture),
                    record.ReturnPosition, MoveCause.Rule)) Move(target, DuelZone.Graveyard, target.Owner, cause: MoveCause.Rule);
                State.Effects.Remove(record);
                State.Window = State.Phase == DuelPhase.Main1 || State.Phase == DuelPhase.Main2 || State.Phase == DuelPhase.Battle
                    ? TimingWindow.Open : TimingWindow.FastResponse;
                State.WaitingSeat = State.TurnPlayer;
                ProcessPhaseObligations();
                return;
            }
            if (continuation[2] == "card")
            {
                var card = Card(choice.Card.InstanceId);
                OpenObligationDecision(record, "zone", DecisionKind.ChooseZone,
                    GetSpecialSummonDestinations(card, record.Player).Select(slot => new DecisionOption {
                        Id = slot.ToString(CultureInfo.InvariantCulture), Value = slot.ToString(CultureInfo.InvariantCulture), Label = "区域 " + slot }),
                    new[] { card.InstanceId });
                return;
            }
            if (continuation[2] == "zone")
            {
                var card = Card(decision.Selected.Single());
                var positions = m_catalog.Get(card.DefinitionId).MonsterType == RuleMonsterType.Link
                    ? new[] { CardPosition.FaceUpAttack } : new[] { CardPosition.FaceUpAttack, CardPosition.FaceUpDefense };
                OpenObligationDecision(record, "position", DecisionKind.ChoosePosition,
                    positions.Select(position => new DecisionOption { Id = ((int)position).ToString(CultureInfo.InvariantCulture),
                        Value = ((int)position).ToString(CultureInfo.InvariantCulture), Label = position.ToString() }),
                    decision.Selected, new[] { choice.Value });
                return;
            }
            SpecialSummonByEffect(Card(decision.Selected.Single()), record.Player,
                int.Parse(decision.Answers.Single(), CultureInfo.InvariantCulture),
                (CardPosition)int.Parse(choice.Value, CultureInfo.InvariantCulture), record.Source);
            State.Effects.Remove(record);
            State.Window = State.Phase == DuelPhase.Main1 || State.Phase == DuelPhase.Main2 || State.Phase == DuelPhase.Battle
                ? TimingWindow.Open : TimingWindow.FastResponse;
            State.WaitingSeat = State.TurnPlayer;
            ProcessPhaseObligations();
        }

        void OpenObligationDecision(DuelEffectRecord record, string step, DecisionKind kind, IEnumerable<DecisionOption> options,
            IEnumerable<int> selected = null, IEnumerable<string> answers = null)
        {
            State.PendingDecision = new DuelDecision { Id = State.NextDecisionId++, Player = record.Player, Kind = kind,
                Min = 1, Max = 1, Prompt = "处理阶段延迟效果", Continuation = "obligation." + record.Id.ToString(CultureInfo.InvariantCulture) + "." + step,
                SourceId = record.Source.InstanceId, Options = options.ToList(), Selected = selected == null ? new List<int>() : selected.ToList(),
                Answers = answers == null ? new List<string>() : answers.ToList() };
            State.Window = TimingWindow.Decision; State.WaitingSeat = record.Player;
            Emit(DuelEventKind.DecisionOpened, record.Player, mask: 1 << record.Player);
        }

        internal void AdvanceLingeringEffects(IReadOnlyList<DuelEvent> facts)
        {
            foreach (var record in ApplicableEffects().Where(e => e.Kind == EffectRecordKind.DrawOnOpponentSpecialSummon).ToArray())
            foreach (var group in facts.Where(f => f.Id >= record.CreatedEventId && f.Kind == DuelEventKind.Summoned && f.Player != record.Player
                && f.SummonMethod != SummonMethod.Normal && f.SummonMethod != SummonMethod.Flip
                && MatchesSummonOrigin(record, f, facts)).GroupBy(f => f.GroupId))
            {
                if (record.ProcessedEventGroups.Contains(group.Key)) continue;
                record.ProcessedEventGroups.Add(group.Key);
                Draw(record.Player, 1);
                if (State.Finished) return;
            }
        }

        static bool MatchesSummonOrigin(DuelEffectRecord record, DuelEvent fact, IReadOnlyList<DuelEvent> facts)
        {
            if (record.Value == 0) return true;
            var from = fact.Before != null ? (DuelZone?)fact.Before.Zone : facts.Where(e => e.Kind == DuelEventKind.Moved
                && e.Id < fact.Id && e.GroupId == fact.GroupId && e.Card.Equals(fact.Card)).OrderByDescending(e => e.Id)
                .Select(e => (DuelZone?)e.From).FirstOrDefault();
            return from.HasValue && (record.Value & (1 << (int)from.Value)) != 0;
        }
    }
}
