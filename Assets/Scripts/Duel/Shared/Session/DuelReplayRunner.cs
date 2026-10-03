namespace AChen.Duel.Core
{
    public sealed class DuelReplayVerification
    {
        public bool Verified { get; internal set; }
        public int CheckedCommands { get; internal set; }
        public long MismatchSequence { get; internal set; }
        public string Error { get; internal set; } = "";
        public string FinalStateHash { get; internal set; } = "";
    }

    /// <summary>Recreates the private authority state and verifies each recorded command boundary.</summary>
    public static class DuelReplayRunner
    {
        public static DuelReplayVerification Run(DuelCardCatalog catalog, DuelReplayRecord replay)
        {
            var outcome = new DuelReplayVerification();
            var start = replay.Start;
            if (start.CatalogHash != catalog.Fingerprint)
            { outcome.Error = "CATALOG_MISMATCH"; return outcome; }
            if (!DuelRulePackage.CreateDefault(catalog).Matches(start))
            { outcome.Error = "RULE_PACKAGE_MISMATCH"; return outcome; }
            var engine = new DuelEngine(catalog, start);
            outcome.FinalStateHash = DuelStateDigest.Compute(engine.State);
            if (replay.InitialStateHash != outcome.FinalStateHash)
            { outcome.Error = "INITIAL_STATE_MISMATCH"; return outcome; }
            foreach (var entry in replay.Entries)
            {
                var actual = engine.Apply(entry.Command);
                outcome.FinalStateHash = DuelStateDigest.Compute(engine.State);
                if (entry.Sequence != outcome.CheckedCommands + 1L || actual.Accepted != entry.Accepted
                    || actual.Error != entry.Error || actual.Revision != entry.Revision || outcome.FinalStateHash != entry.StateHash
                    || DuelStateDigest.ComputeEvents(actual.Events) != entry.EventsHash)
                {
                    outcome.MismatchSequence = entry.Sequence;
                    outcome.Error = "STEP_MISMATCH";
                    return outcome;
                }
                outcome.CheckedCommands++;
            }
            outcome.Verified = true;
            return outcome;
        }
    }
}
