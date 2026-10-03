using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed partial class DuelEngine
    {
        public DuelDestructionOperation BeginDestruction(DuelChainLink link, IEnumerable<DuelCardState> targets,
            CardRef source, int player, MoveCause cause = MoveCause.Effect, int operationKey = 0, string continuation = "chain")
        {
            if (link != null && link.Destructions.TryGetValue(operationKey, out var existing)) return existing;
            var operation = new DuelDestructionOperation { Id = State.NextDestructionId++,
                Targets = targets.Select(c => c.Ref).Distinct().ToList(), Source = source, EffectPlayer = player,
                Cause = cause, ChainNumber = link == null ? 0 : link.Number, ResumeStep = operationKey, Continuation = continuation };
            if (link != null) link.Destructions.Add(operationKey, operation);
            State.PendingDestruction = operation;
            AdvanceDestruction(operation);
            return operation;
        }

        IEnumerable<DuelCardState> Destructible(DuelDestructionOperation operation) => State.Cards.Where(card =>
            operation.Targets.Contains(card.Ref) && !operation.Protected.Contains(card.Ref)
                && (OnField(card) || operation.Cause == MoveCause.Effect && card.Zone == DuelZone.Hand)
                && (operation.Cause != MoveCause.Effect || IsAffectedBy(card, operation.Source)
                    && !HasEffect(EffectRecordKind.EffectIndestructible, card.Controller, card)));

        IEnumerable<DuelEffectRecord> ReplacementCandidates(DuelDestructionOperation operation, int player)
        {
            var cards = Destructible(operation).Where(c => OnField(c) && c.Controller == player).ToArray();
            return ApplicableEffects().Where(record => record.Kind == EffectRecordKind.DestroyReplacement
                && record.Player == player && State.Cards.Any(c => c.Ref.Equals(record.Source) && c.Zone == DuelZone.Graveyard)
                && cards.Any(c => (record.Value == 0 || c.CurrentRace == record.Value)
                    && (record.Target.InstanceId == 0 || record.Target.Equals(c.Ref))))
                .GroupBy(record => record.Source).Select(group => group.First()).OrderBy(record => record.Source.InstanceId);
        }

        void AdvanceDestruction(DuelDestructionOperation operation)
        {
            var eligible = Destructible(operation).Select(c => c.Ref).ToArray();
            foreach (var protection in ApplicableEffects().Where(record => record.Kind == EffectRecordKind.DestroyReplacement
                && record.Target.InstanceId != 0 && record.Remaining > 0 && eligible.Contains(record.Target)).ToArray())
            {
                if (operation.Protected.Contains(protection.Target)) continue;
                operation.Protected.Add(protection.Target);
                protection.Remaining--;
                if (protection.Remaining == 0) State.Effects.Remove(protection);
            }
            for (int player = 0; player < 2; player++)
            {
                if (operation.ReplacementSeatsProcessed.Contains(player)) continue;
                var replacements = ReplacementCandidates(operation, player).ToArray();
                if (replacements.Length == 0) { operation.ReplacementSeatsProcessed.Add(player); continue; }
                var options = replacements.Select(record => new DecisionOption {
                    Id = record.Source.InstanceId.ToString(CultureInfo.InvariantCulture), Value = record.Source.InstanceId.ToString(CultureInfo.InvariantCulture),
                    HasCard = true, Card = record.Source, Label = "除外 " + m_catalog.Get(Card(record.Source.InstanceId).DefinitionId).Name + " 代替破坏" }).ToList();
                options.Add(new DecisionOption { Id = "none", Value = "none", Label = "不使用破坏替代" });
                State.PendingDecision = new DuelDecision { Id = State.NextDecisionId++, Player = player,
                    Kind = DecisionKind.ChooseMode, Prompt = "是否使用破坏替代？", Min = 1, Max = 1,
                    Continuation = "destruction.replace", SourceId = operation.Source.InstanceId, Options = options };
                State.Window = TimingWindow.Decision; State.WaitingSeat = player;
                Emit(DuelEventKind.DecisionOpened, player, mask: 1 << player); return;
            }
            var destroyed = Destructible(operation).Select(card => (card, before: Snapshot(card))).ToArray();
            foreach (var target in destroyed)
            {
                Move(target.card, DuelZone.Graveyard, target.card.Owner, cause: operation.Cause,
                    effectSource: operation.Source, effectPlayer: operation.EffectPlayer);
                operation.Destroyed.Add(target.before.Ref); operation.DestroyedBefore.Add(target.before);
                Emit(DuelEventKind.Destroyed, target.before.Controller, target.card, cause: operation.Cause,
                    effectSource: operation.Source, effectPlayer: operation.EffectPlayer,
                    before: target.before, after: Snapshot(target.card));
            }
            operation.Completed = true; State.PendingDestruction = null;
        }

        void AnswerDestruction(DuelCommand command)
        {
            var operation = State.PendingDestruction;
            var choice = State.PendingDecision.Options.Single(option => option.Id == command.Options[0]);
            if (choice.HasCard)
            {
                var record = ReplacementCandidates(operation, command.Player).Single(candidate => candidate.Source.Equals(choice.Card));
                operation.Protected.AddRange(Destructible(operation).Where(card => OnField(card) && card.Controller == command.Player
                    && (record.Value == 0 || card.CurrentRace == record.Value)
                    && (record.Target.InstanceId == 0 || record.Target.Equals(card.Ref))).Select(card => card.Ref));
                var substitute = Card(choice.Card.InstanceId);
                Move(substitute, DuelZone.Banished, substitute.Owner, cause: MoveCause.Rule);
            }
            operation.ReplacementSeatsProcessed.Add(command.Player); State.PendingDecision = null;
            AdvanceDestruction(operation);
            if (!operation.Completed) return;
            if (operation.Continuation == "chain") ResolveChain();
            else if (operation.Continuation == "battle") CompleteBattleDestruction(operation);
            else if (operation.Continuation == "phase")
            { ProcessPhaseObligations(); if (State.PendingDecision == null) OpenResponse(); }
        }

        void CompleteBattleDestruction(DuelDestructionOperation operation)
        {
            State.BattleDestroyed.Clear(); State.BattleStep = BattleStep.DamageEnd;
            OpenResponse();
            Emit(DuelEventKind.BattleStepChanged, State.TurnPlayer, Card(State.Attacker.InstanceId),
                amount: (int)State.BattleStep, detail: State.BattleStep.ToString(), cause: MoveCause.Battle);
        }
    }
}
