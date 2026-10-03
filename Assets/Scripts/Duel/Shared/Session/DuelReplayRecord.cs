using System;
using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    /// <summary>Server-only complete replay. Never attach this record to a player-visible response.</summary>
    public sealed class DuelReplayRecord
    {
        readonly DuelStartRecord m_start;
        public DuelStartRecord Start => ReplayCopies.Start(m_start);
        public string InitialStateHash { get; }
        public IReadOnlyList<DuelReplayEntry> Entries { get; }
        internal DuelReplayRecord(DuelStartRecord start, IEnumerable<DuelReplayEntry> entries, string initialStateHash)
        { m_start = ReplayCopies.Start(start); Entries = Array.AsReadOnly(entries.ToArray()); InitialStateHash = initialStateHash; }
    }

    public sealed class DuelReplayEntry
    {
        readonly DuelCommand m_command;
        public long Sequence { get; }
        public DuelCommand Command => ReplayCopies.Command(m_command);
        public bool Accepted { get; }
        public string Error { get; }
        public long Revision { get; }
        public string StateHash { get; }
        public string EventsHash { get; }
        internal DuelReplayEntry(long sequence, DuelCommand command, DuelStepResult result, string stateHash)
        {
            Sequence = sequence; m_command = ReplayCopies.Command(command); Accepted = result.Accepted;
            Error = result.Error; Revision = result.Revision; StateHash = stateHash;
            EventsHash = DuelStateDigest.ComputeEvents(result.Events);
        }
    }

    public sealed class DuelReplayRecorder
    {
        readonly DuelStartRecord m_start;
        readonly List<DuelReplayEntry> m_entries = new List<DuelReplayEntry>();
        readonly string m_initialStateHash;
        public DuelReplayRecorder(DuelStartRecord start) { m_start = ReplayCopies.Start(start); }
        public DuelReplayRecorder(DuelStartRecord start, DuelState initialState)
        { m_start = ReplayCopies.Start(start); m_initialStateHash = DuelStateDigest.Compute(initialState); }
        public void Append(DuelCommand command, DuelStepResult result, string stateHash) =>
            m_entries.Add(new DuelReplayEntry(m_entries.Count + 1L, command, result, stateHash));
        public DuelReplayRecord Capture() => new DuelReplayRecord(m_start, m_entries, m_initialStateHash);
    }

    static class ReplayCopies
    {
        internal static DuelStartRecord Start(DuelStartRecord value) => new DuelStartRecord
        {
            MainDecks = value.MainDecks.Select(x => (string[])x.Clone()).ToArray(),
            ExtraDecks = value.ExtraDecks.Select(x => (string[])x.Clone()).ToArray(),
            Seed = value.Seed, FirstPlayer = value.FirstPlayer, Shuffle = value.Shuffle,
            OpeningHand = value.OpeningHand, RuleVersion = value.RuleVersion, CatalogHash = value.CatalogHash,
            ProtocolVersion = value.ProtocolVersion, RulePackageHash = value.RulePackageHash,
            NameCatalogHash = value.NameCatalogHash, BanlistHash = value.BanlistHash
        };
        internal static DuelCommand Command(DuelCommand value) => new DuelCommand
        {
            Kind = value.Kind, Player = value.Player, CardId = value.CardId, TargetId = value.TargetId,
            Slot = value.Slot, Position = value.Position, AbilityId = value.AbilityId,
            DecisionId = value.DecisionId, Options = (string[])value.Options.Clone(), Cards = (int[])value.Cards.Clone(),
            Phase = value.Phase, NameId = value.NameId
        };
    }
}
