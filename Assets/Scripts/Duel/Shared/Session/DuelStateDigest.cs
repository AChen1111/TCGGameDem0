using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace AChen.Duel.Core
{
    /// <summary>Server-only state digest. Encoding v1: little endian integers, UTF8 length-prefixed strings,
    /// preserved gameplay list order, ordinal dictionary keys, cards ordered by instance identity.</summary>
    public static class DuelStateDigest
    {
        public static string Compute(DuelState state)
            => Hash(writer => Write(writer, state));

        public static string ComputeEvents(IReadOnlyList<DuelEvent> events)
            => Hash(writer => { Text(writer, "achen-duel-events-v1"); List(writer, events.ToArray(), Event); });

        static string Hash(Action<BinaryWriter> write)
        {
            using (var bytes = new MemoryStream())
            using (var writer = new BinaryWriter(bytes, new UTF8Encoding(false, true), true))
            {
                write(writer);
                writer.Flush();
                using (var sha = SHA256.Create())
                    return BitConverter.ToString(sha.ComputeHash(bytes.ToArray())).Replace("-", "").ToLowerInvariant();
            }
        }

        static void Write(BinaryWriter w, DuelState s)
        {
            Text(w, "achen-duel-state-v1");
            w.Write(s.Revision); w.Write(s.Turn); w.Write(s.TurnPlayer); w.Write((int)s.Phase);
            w.Write((int)s.Window); w.Write(s.WaitingSeat); w.Write(s.ConsecutivePasses); w.Write(s.PendingPhase);
            w.Write(s.PendingSummonId); w.Write((int)s.PendingSummonFrom); LastKnown(w, s.PendingSummonBefore);
            w.Write((int)s.BattleStep); Ref(w, s.Attacker); Ref(w, s.AttackTarget);
            List(w, s.BattleDestroyed, Ref); List(w, s.AttackTargetsAtDeclaration, (b, x) => b.Write(x));
            List(w, s.AttackTargetRefsAtDeclaration, Ref);
            w.Write(s.Finished); w.Write(s.Winner); Text(w, s.EndReason); w.Write(s.RandomState);
            w.Write(s.NextDecisionId); w.Write(s.NextInstanceId); w.Write(s.NextEventId);
            List(w, s.Players, Player);
            List(w, s.Cards.OrderBy(x => x.InstanceId).ToArray(), Card);
            List(w, s.Chain, Chain);
            List(w, s.ChainCleanup, Ref);
            w.Write(s.PendingDecision != null);
            if (s.PendingDecision != null) Decision(w, s.PendingDecision);
            Dictionary(w, s.UsedAbilities);
            List(w, s.Effects, Effect);
            List(w, s.PendingFacts, Event); List(w, s.LastCheckpointEvents, Event);
            List(w, s.PendingTriggers, Trigger);
            w.Write(s.ActiveTrigger != null); if (s.ActiveTrigger != null) Trigger(w, s.ActiveTrigger);
            w.Write(s.TriggerTargetId); List(w, s.TriggerCostIds, (b, x) => b.Write(x));
            w.Write(s.NextEffectId); w.Write(s.NextEventGroupId); w.Write(s.CurrentEventGroupId);
            w.Write(s.NextChainId); w.Write(s.CurrentChainId); w.Write(s.NextDestructionId);
            w.Write(s.PendingDestruction != null); if (s.PendingDestruction != null) Destruction(w, s.PendingDestruction);
            Text(w, s.RuleVersion); Text(w, s.CatalogHash); w.Write(s.ProtocolVersion);
            Text(w, s.RulePackageHash); Text(w, s.NameCatalogHash); Text(w, s.BanlistHash);
            Text(w, s.ContinuationAfterTriggers);
            List(w, s.TurnFacts, Event); List(w, s.PrivateTriggerPlayers, (b, x) => b.Write(x));
            Text(w, s.TriggerModeId);
        }

        static void Player(BinaryWriter w, DuelPlayerState p)
        {
            w.Write(p.LifePoints); w.Write(p.NormalSummonsThisTurn);
            List(w, p.Deck, (b, x) => b.Write(x)); List(w, p.ExtraDeck, (b, x) => b.Write(x));
        }

        static void Card(BinaryWriter w, DuelCardState c)
        {
            w.Write(c.InstanceId); w.Write(c.Generation); Text(w, c.DefinitionId); w.Write(c.Owner);
            w.Write(c.Controller); w.Write((int)c.Zone); w.Write(c.Slot); w.Write((int)c.Position);
            w.Write(c.CurrentAtk); w.Write(c.CurrentDef.HasValue); if (c.CurrentDef.HasValue) w.Write(c.CurrentDef.Value);
            w.Write(c.CurrentLevel); w.Write(c.CurrentAttribute); w.Write(c.CurrentRace); Text(w, c.CurrentNameId);
            w.Write(c.SummonedTurn); w.Write(c.PositionChangedTurn); w.Write(c.AttacksThisTurn); w.Write(c.SetTurn);
            w.Write(c.ProperlySummoned); w.Write(c.Negated); w.Write(c.HostInstanceId);
            w.Write(c.TrackingEpoch); w.Write(c.RevealedToMask); List(w, c.Materials, (b, x) => b.Write(x));
            w.Write((int)c.SummonMethod); List(w, c.SummonMaterialDefinitions, Text);
            Ref(w, c.EquipTarget);
            w.Write(c.CurrentNormal);
            Dictionary(w, c.Counters);
            List(w, c.AttackedTargetsThisTurn, Ref);
            w.Write(c.DirectAttackDeclaredThisTurn);
            w.Write(c.SummonModifiersSuppressed);
        }

        static void Chain(BinaryWriter w, DuelChainLink l)
        {
            w.Write(l.Number); w.Write(l.Player); Ref(w, l.Source); Text(w, l.DefinitionId); Text(w, l.AbilityId);
            w.Write(l.Speed); w.Write(l.ActivationNegated); w.Write(l.EffectNegated);
            List(w, l.Targets, Ref); List(w, l.Costs, Ref); List(w, l.Selected, (b, x) => b.Write(x));
            Text(w, l.Program); w.Write(l.Step); Dictionary(w, l.Values);
            List(w, l.Answers, Text); w.Write((int)l.AnswerKind);
            w.Write(l.StringValues.Count);
            foreach (var pair in l.StringValues.OrderBy(x => x.Key, StringComparer.Ordinal)) { Text(w, pair.Key); Text(w, pair.Value); }
            w.Write(l.TriggerEvent != null); if (l.TriggerEvent != null) Event(w, l.TriggerEvent);
            w.Write(l.ResolutionGroupId);
            Text(w, l.ModeId); w.Write((int)l.Categories);
            w.Write((int)l.ActivationKind); w.Write((int)l.ActivationZone);
            w.Write(l.IsCardActivation);
            w.Write(l.FieldEffectNegated);
            Text(w, l.ReplacementCardId); Text(w, l.ReplacementProgram); LastKnown(w, l.ActivationSource);
            w.Write(l.Destructions.Count);
            foreach (var pair in l.Destructions.OrderBy(x => x.Key)) { w.Write(pair.Key); Destruction(w, pair.Value); }
        }

        static void Decision(BinaryWriter w, DuelDecision d)
        {
            w.Write(d.Id); w.Write(d.Player); w.Write((int)d.Kind); Text(w, d.Prompt);
            w.Write(d.Min); w.Write(d.Max); w.Write(d.CanCancel);
            List(w, d.Options, (b, o) => { Text(b, o.Id); Text(b, o.Label); Ref(b, o.Card); b.Write(o.HasCard); Text(b, o.Value); });
            Text(w, d.Continuation); w.Write(d.SourceId); List(w, d.Selected, (b, x) => b.Write(x));
            List(w, d.Answers, Text); w.Write((int)d.AnswerKind);
            List(w, d.AllowedCardGroups, (b, group) => List(b, group, Ref));
            Text(w, d.MaterialRecipeId);
            w.Write(d.UniqueOriginalNames); w.Write(d.RequireSetCapacity);
        }

        static void Effect(BinaryWriter w, DuelEffectRecord e)
        {
            w.Write(e.Id); w.Write((int)e.Kind); Ref(w, e.Source); Ref(w, e.Target);
            w.Write(e.Player); Text(w, e.NameId); w.Write(e.Value); w.Write(e.ExpiresTurn); w.Write((int)e.ExpiresPhase);
            w.Write(e.RequiresSource); w.Write(e.Remaining); w.Write(e.ActivationPlayer);
            List(w, e.Cards, Ref); List(w, e.Names, Text);
            List(w, e.ProcessedEventGroups, (b, x) => b.Write(x)); Text(w, e.SourceDefinitionId);
            w.Write(e.ChainId); w.Write(e.ResponseToLink);
            w.Write(e.CreatedEventId);
            w.Write(e.SetCode);
            w.Write(e.CalculationOnly);
            w.Write((int)e.ReturnZone); w.Write(e.ReturnSlot); w.Write((int)e.ReturnPosition);
            w.Write(e.RemoveIfActivationNegated);
            w.Write(e.ResetIfSourceNegated);
        }

        static void Event(BinaryWriter w, DuelEvent e)
        {
            w.Write(e.Id); w.Write((int)e.Kind); w.Write(e.Player); Ref(w, e.Card); w.Write(e.HasCard);
            Text(w, e.DefinitionId); w.Write(e.VisibleToMask); w.Write((int)e.From); w.Write((int)e.To);
            w.Write(e.Amount); w.Write(e.Attack); w.Write(e.Defense.HasValue); if (e.Defense.HasValue) w.Write(e.Defense.Value);
            Text(w, e.Detail); w.Write((int)e.Cause); Ref(w, e.EffectSource); w.Write(e.EffectPlayer); w.Write(e.GroupId);
            LastKnown(w, e.Before); LastKnown(w, e.After); w.Write((int)e.SummonMethod);
            w.Write(e.WasSummonMaterial); w.Write((int)e.MaterialMethod); List(w, e.PresentCards, LastKnown);
            Ref(w, e.BattleAttacker); Ref(w, e.BattleTarget);
            Text(w, e.DeclaredNameId);
            w.Write((int)e.PhaseAtEvent); w.Write(e.ChainId); w.Write(e.LinkNumber); w.Write((int)e.ActivationKind); w.Write(e.ActivationNegated);
            w.Write(e.IsCardActivation);
        }

        static void Destruction(BinaryWriter w, DuelDestructionOperation operation)
        {
            w.Write(operation.Id); List(w, operation.Targets, Ref); Ref(w, operation.Source); w.Write(operation.EffectPlayer);
            w.Write((int)operation.Cause); List(w, operation.Destroyed, Ref); List(w, operation.DestroyedBefore, LastKnown);
            List(w, operation.ReplacementSeatsProcessed, (b, x) => b.Write(x)); List(w, operation.Protected, Ref);
            w.Write(operation.Completed); w.Write(operation.ResumeStep); w.Write(operation.ChainNumber); Text(w, operation.Continuation);
        }

        static void Trigger(BinaryWriter w, PendingTrigger t)
        {
            Text(w, t.AbilityId); Ref(w, t.Source); w.Write(t.Player); w.Write(t.Mandatory); w.Write(t.OptionalWhen);
            w.Write(t.EventId); w.Write(t.GroupId); w.Write(t.Public);
        }

        static void LastKnown(BinaryWriter w, CardLastKnown c)
        {
            w.Write(c != null);
            if (c == null) return;
            Ref(w, c.Ref); Text(w, c.DefinitionId); w.Write(c.Owner); w.Write(c.Controller);
            w.Write((int)c.Zone); w.Write((int)c.Position); w.Write(c.Attack);
            w.Write(c.Defense.HasValue); if (c.Defense.HasValue) w.Write(c.Defense.Value);
            w.Write(c.Level); w.Write(c.Attribute); Text(w, c.NameId);
            w.Write(c.Race); w.Write(c.Slot); w.Write(c.ProperlySummoned); w.Write((int)c.SummonMethod); w.Write(c.VisibleToMask);
        }

        static void Ref(BinaryWriter w, CardRef r) { w.Write(r.InstanceId); w.Write(r.Generation); }
        static void Text(BinaryWriter w, string value) { var bytes = Encoding.UTF8.GetBytes(value); w.Write(bytes.Length); w.Write(bytes); }
        static void Dictionary(BinaryWriter w, IDictionary<string, int> values)
        {
            w.Write(values.Count);
            foreach (var pair in values.OrderBy(x => x.Key, StringComparer.Ordinal)) { Text(w, pair.Key); w.Write(pair.Value); }
        }
        static void List<T>(BinaryWriter w, ICollection<T> values, Action<BinaryWriter, T> write)
        { w.Write(values.Count); foreach (var value in values) write(w, value); }
    }
}
