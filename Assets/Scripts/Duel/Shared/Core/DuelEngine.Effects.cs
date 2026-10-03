using System;
using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed partial class DuelEngine
    {
        internal void AddEffect(DuelEffectRecord record)
        {
            record.Id = State.NextEffectId++;
            record.CreatedEventId = State.NextEventId;
            State.Effects.Add(record);
            RefreshCharacteristics();
        }

        internal IEnumerable<DuelEffectRecord> ApplicableEffects()
        {
            foreach (var record in State.Effects.OrderBy(x => x.Id))
                if (record.ExpiresTurn == 0 || record.ExpiresTurn >= State.Turn)
                    if (!record.RequiresSource || State.Cards.Any(c => c.Ref.Equals(record.Source) && OnField(c) && !c.Negated))
                        yield return record;
            foreach (var card in State.Cards.OrderBy(c => c.InstanceId))
            foreach (var rule in m_rules.Get(card.DefinitionId).CreateContinuousRules())
            {
                var records = new List<DuelEffectRecord>();
                rule.Collect(new EffectContext(this, card.Controller, card.InstanceId), records);
                foreach (var record in records) yield return record;
            }
        }

        internal bool HasEffect(EffectRecordKind kind, int player, DuelCardState card = null) =>
            ApplicableEffects().Any(e => e.Kind == kind && (e.Player < 0 || e.Player == player)
                && (e.Target.InstanceId == 0 || card != null && e.Target.Equals(card.Ref)));

        internal bool IsLinkNegated(DuelChainLink link)
        {
            if (link.EffectNegated || link.FieldEffectNegated) return true;
            var definition = m_catalog.Get(link.DefinitionId);
            int mask = 1 << ((int)definition.Kind - 1);
            if (ApplicableEffects().Any(e => e.Kind == EffectRecordKind.NameNegate && (e.Value == 0 || (e.Value & mask) != 0)
                && e.NameId == definition.OriginalNameId && (e.Player < 0 || e.Player == link.Player))) return true;
            var source = State.Cards.FirstOrDefault(c => c.Ref.Equals(link.Source));
            return source != null && OnField(source) && source.Negated;
        }

        public bool TryNegateEffect(DuelChainLink link, CardRef caster)
        {
            var source = State.Cards.FirstOrDefault(c => c.Ref.Equals(link.Source) && OnField(c));
            if (source != null && !IsAffectedBy(source, caster)) return false;
            link.EffectNegated = true;
            return true;
        }

        internal void RefreshCharacteristics()
        {
            State.Effects.RemoveAll(e => e.ExpiresTurn > 0 && e.ExpiresTurn < State.Turn
                || e.CalculationOnly && State.BattleStep != BattleStep.Calculation
                || e.Target.InstanceId != 0 && !State.Cards.Any(c => c.Ref.Equals(e.Target))
                || e.RequiresSource && !State.Cards.Any(c => c.Ref.Equals(e.Source) && OnField(c)));
            foreach (var card in State.Cards)
            {
                var printed = m_catalog.Get(card.DefinitionId);
                card.CurrentAtk = printed.Attack; card.CurrentDef = printed.Defense; card.CurrentLevel = printed.Level;
                card.CurrentAttribute = printed.Attribute; card.CurrentRace = printed.Race;
                card.CurrentNameId = printed.OriginalNameId; card.Negated = false;
                card.CurrentNormal = printed.IsNormal;
                card.Negated = State.Effects.Any(e => e.Kind == EffectRecordKind.TargetNegate && e.Target.Equals(card.Ref)
                    && OnField(card) && IsPublic(card) || e.Kind == EffectRecordKind.NameNegate && e.NameId == printed.OriginalNameId
                    && (e.Value == 0 || (e.Value & (1 << ((int)printed.Kind - 1))) != 0) && (e.Player < 0 || e.Player == card.Controller));
                if (card.Negated && m_rules.Get(card.DefinitionId).SuppressSummonModifiersWhenNegated)
                    card.SummonModifiersSuppressed = true;
            }
            State.Effects.RemoveAll(e => e.ResetIfSourceNegated && State.Cards.Any(c => c.Ref.Equals(e.Source) && c.Negated));
            foreach (var link in State.Chain.Where(l => l.ActivationZone == DuelZone.Monster || l.ActivationZone == DuelZone.ExtraMonster))
                if (State.Cards.Any(c => c.Ref.Equals(link.Source) && c.Negated
                    && State.Effects.Any(e => e.Kind == EffectRecordKind.TargetNegate && e.Target.Equals(c.Ref) && !e.RequiresSource)))
                    link.FieldEffectNegated = true;
            var identityRecords = ApplicableEffects().OrderBy(e => e.Id).ToArray();
            var identities = identityRecords.Where(e => e.Kind == EffectRecordKind.SetAttribute || e.Kind == EffectRecordKind.SetName
                || e.Kind == EffectRecordKind.SetLevel || e.Kind == EffectRecordKind.TreatAsNormal).ToArray();
            foreach (var record in identities)
            {
                var card = State.Cards.FirstOrDefault(c => c.Ref.Equals(record.Target));
                if (card == null) continue;
                if (!CanApplyCharacteristic(record, card, identityRecords)) continue;
                if (record.Kind == EffectRecordKind.SetAttribute) card.CurrentAttribute = record.Value;
                if (record.Kind == EffectRecordKind.SetName) card.CurrentNameId = record.NameId;
                if (record.Kind == EffectRecordKind.SetLevel) card.CurrentLevel = record.Value;
                if (record.Kind == EffectRecordKind.TreatAsNormal) card.CurrentNormal = record.Value != 0;
            }
            var records = ApplicableEffects().OrderBy(e => e.Id).ToArray();
            foreach (var record in records)
            {
                var target = State.Cards.FirstOrDefault(c => c.Ref.Equals(record.Target));
                if (target == null) continue;
                if (!CanApplyCharacteristic(record, target, records)) continue;
                switch (record.Kind)
                {
                    case EffectRecordKind.TargetNegate: if (OnField(target) && IsPublic(target)) target.Negated = true; break;
                    case EffectRecordKind.SetAttack: target.CurrentAtk = record.Value; break;
                    case EffectRecordKind.SetDefense: target.CurrentDef = record.Value; break;
                    case EffectRecordKind.AddAttack: target.CurrentAtk += record.Value; break;
                    case EffectRecordKind.AddDefense: if (target.CurrentDef.HasValue) target.CurrentDef += record.Value; break;
                    case EffectRecordKind.SetLevel: target.CurrentLevel = record.Value; break;
                    case EffectRecordKind.SetAttribute: target.CurrentAttribute = record.Value; break;
                    case EffectRecordKind.SetName: target.CurrentNameId = record.NameId; break;
                    case EffectRecordKind.TreatAsNormal: target.CurrentNormal = record.Value != 0; break;
                }
            }
            foreach (var card in State.Cards)
            { card.CurrentAtk = Math.Max(0, card.CurrentAtk); if (card.CurrentDef.HasValue) card.CurrentDef = Math.Max(0, card.CurrentDef.Value); }
        }

        bool CanApplyCharacteristic(DuelEffectRecord record, DuelCardState target, IReadOnlyList<DuelEffectRecord> records)
        {
            // 已决的非永续修改保留；持续来源每次适用时重新检查免疫。
            if (record.Id != 0 || !record.RequiresSource || record.Source.Equals(target.Ref)) return true;
            var source = State.Cards.First(c => c.InstanceId == record.Source.InstanceId);
            int mask = 1 << ((int)m_catalog.Get(source.DefinitionId).Kind - 1);
            return !records.Any(e => e.Kind == EffectRecordKind.Unaffected && e.Target.Equals(target.Ref)
                && (e.Value == 0 || (e.Value & mask) != 0));
        }

        internal bool IsAffectedBy(DuelCardState card, CardRef source)
        {
            var sourceCard = Card(source.InstanceId);
            int mask = 1 << ((int)m_catalog.Get(sourceCard.DefinitionId).Kind - 1);
            return !ApplicableEffects().Any(e => e.Kind == EffectRecordKind.Unaffected && e.Target.Equals(card.Ref)
                && (e.Value == 0 || (e.Value & mask) != 0));
        }

        internal bool CanTargetBy(DuelCardState card, CardRef source) =>
            !ApplicableEffects().Any(e => e.Kind == EffectRecordKind.Untargetable && e.Target.Equals(card.Ref));

        internal bool DestroyByEffect(DuelCardState card, CardRef source, int player)
        {
            if (!OnField(card) || !IsAffectedBy(card, source) || HasEffect(EffectRecordKind.EffectIndestructible, card.Controller, card)) return false;
            Move(card, DuelZone.Graveyard, card.Owner, cause: MoveCause.Effect, effectSource: source, effectPlayer: player);
            Emit(DuelEventKind.Destroyed, player, card, cause: MoveCause.Effect, effectSource: source, effectPlayer: player);
            return true;
        }
    }
}
