using System;
using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed partial class DuelEngine
    {
        internal void Move(DuelCardState card, DuelZone zone, int controller, int slot = 0,
            CardPosition position = CardPosition.FaceUp, MoveCause cause = MoveCause.Rule,
            CardRef effectSource = default, int effectPlayer = -1, bool wasSummonMaterial = false,
            SummonMethod materialMethod = SummonMethod.Normal)
            => TryMove(card, zone, controller, slot, position, cause, effectSource, effectPlayer, wasSummonMaterial, materialMethod);

        internal bool TryMove(DuelCardState card, DuelZone zone, int controller, int slot = 0,
            CardPosition position = CardPosition.FaceUp, MoveCause cause = MoveCause.Rule,
            CardRef effectSource = default, int effectPlayer = -1, bool wasSummonMaterial = false,
            SummonMethod materialMethod = SummonMethod.Normal)
        {
            var before = Snapshot(card);
            var from = card.Zone;
            if ((zone == DuelZone.Hand || zone == DuelZone.Deck) && m_catalog.Get(card.DefinitionId).IsExtra)
                zone = DuelZone.ExtraDeck;
            if (from == DuelZone.Deck && zone == DuelZone.Hand && HasEffect(EffectRecordKind.PreventDeckToHand, controller)) return false;
            if ((zone == DuelZone.Monster || zone == DuelZone.ExtraMonster)
                && !Rules.Get(card.DefinitionId).AllowsMonsterZoneEntry(card, controller, State, m_catalog)) return false;
            if (zone == DuelZone.ExtraDeck && (cause == MoveCause.Effect || cause == MoveCause.Cost)
                && HasEffect(EffectRecordKind.CannotReturnToExtra, card.Controller, card)) return false;
            if (zone == DuelZone.Graveyard && ApplicableEffects().Any(record => record.Kind == EffectRecordKind.BanishOpponentGraveyard
                && (record.Player < 0 || record.Player == card.Owner)
                && (record.Target.InstanceId == 0 || record.Target.Equals(card.Ref)) && IsAffectedBy(card, record.Source)))
            { zone = DuelZone.Banished; position = CardPosition.FaceUp; }
            bool requestedField = zone == DuelZone.Monster || zone == DuelZone.ExtraMonster || zone == DuelZone.SpellTrap || zone == DuelZone.Field;
            if ((from == DuelZone.SpellTrap || from == DuelZone.Field) && !requestedField && cause != MoveCause.ActivationNegated
                && HasEffect(EffectRecordKind.BanishWhenLeavesField, card.Controller, card))
            { zone = DuelZone.Banished; position = CardPosition.FaceUp; }
            if (zone == DuelZone.Hand || zone == DuelZone.Deck || zone == DuelZone.ExtraDeck)
                position = CardPosition.FaceDown;
            State.Players[card.Owner].Deck.Remove(card.InstanceId);
            State.Players[card.Owner].ExtraDeck.Remove(card.InstanceId);
            bool previousField = OnField(card);
            bool nextField = zone == DuelZone.Monster || zone == DuelZone.ExtraMonster || zone == DuelZone.SpellTrap || zone == DuelZone.Field;
            if (card.HostInstanceId != 0)
            {
                Card(card.HostInstanceId).Materials.Remove(card.InstanceId);
                card.HostInstanceId = 0;
            }
            if (previousField && !nextField)
            {
                foreach (int attached in card.Materials.ToArray())
                    Move(Card(attached), DuelZone.Graveyard, Card(attached).Owner, cause: MoveCause.Rule);
                card.Materials.Clear();
                foreach (var equip in State.Cards.Where(c => c.EquipTarget.Equals(before.Ref)).ToArray())
                    if (State.PendingDestruction == null || !Destructible(State.PendingDestruction).Contains(equip))
                        Move(equip, DuelZone.Graveyard, equip.Owner, cause: MoveCause.Rule);
                ResetFieldState(card);
            }
            if (from != zone && !(previousField && nextField)) card.Generation++;
            if (zone == DuelZone.Deck) { State.Players[card.Owner].Deck.Add(card.InstanceId); card.TrackingEpoch++; }
            if (zone == DuelZone.ExtraDeck) State.Players[card.Owner].ExtraDeck.Add(card.InstanceId);
            card.Zone = zone; card.Controller = controller; card.Slot = slot; card.Position = position;
            card.RevealedToMask = 0;
            if (from != zone) card.EquipTarget = default;
            if (zone == DuelZone.Deck || zone == DuelZone.Hand || zone == DuelZone.ExtraDeck)
            { card.ProperlySummoned = false; card.Counters.Clear(); }
            var after = Snapshot(card);
            Emit(DuelEventKind.Moved, controller, card,
                before.VisibleToMask | after.VisibleToMask, from, cause: cause,
                effectSource: effectSource, effectPlayer: effectPlayer, before: before, after: after);
            var move = m_events[m_events.Count - 1];
            move.WasSummonMaterial = wasSummonMaterial; move.MaterialMethod = materialMethod;
            return true;
        }

        internal CardLastKnown Snapshot(DuelCardState card) => new CardLastKnown
        {
            Ref = card.Ref, DefinitionId = card.DefinitionId, Owner = card.Owner, Controller = card.Controller,
            Zone = card.Zone, Slot = card.Slot, Position = card.Position, Attack = card.CurrentAtk,
            Defense = card.CurrentDef, Level = card.CurrentLevel, Race = card.CurrentRace,
            Attribute = card.CurrentAttribute, NameId = card.CurrentNameId,
            ProperlySummoned = card.ProperlySummoned, SummonMethod = card.SummonMethod,
            VisibleToMask = IsPublic(card) ? 3 : (1 << card.Owner) | card.RevealedToMask
        };

        void ResetFieldState(DuelCardState card)
        {
            var definition = m_catalog.Get(card.DefinitionId);
            card.CurrentAtk = definition.Attack; card.CurrentDef = definition.Defense;
            card.CurrentLevel = definition.Level; card.CurrentAttribute = definition.Attribute;
            card.CurrentRace = definition.Race; card.CurrentNameId = definition.OriginalNameId;
            card.Negated = false; card.SummonedTurn = 0; card.PositionChangedTurn = 0;
            card.SetTurn = 0; card.AttacksThisTurn = 0; card.SummonMaterialDefinitions.Clear();
            card.Counters.Clear();
            card.AttackedTargetsThisTurn.Clear();
            card.DirectAttackDeclaredThisTurn = false;
            card.SummonModifiersSuppressed = false;
        }

        bool CanSpecialSummonCard(DuelCardState card, int player, bool ignore, SummonMethod method)
            => CanSpecialSummonConditions(card, player, ignore, method);

        public bool CanSpecialSummonConditions(DuelCardState card, int player, bool ignore = false,
            SummonMethod method = SummonMethod.Effect, bool fromMaterial = false)
        {
            var definition = m_catalog.Get(card.DefinitionId);
            if (definition.Kind != RuleCardKind.Monster || OnField(card) || card.Zone == DuelZone.Material && !fromMaterial) return false;
            if (card.Zone == DuelZone.Banished && card.Position == CardPosition.FaceDown) return false;
            bool fromPublic = fromMaterial || card.Zone == DuelZone.Graveyard || card.Zone == DuelZone.Banished;
            if (fromPublic && (definition.IsExtra || !definition.CanNormalSummon) && !card.ProperlySummoned) return false;
            if (!ignore && !Rules.Get(card.DefinitionId).AllowsSummonMethod(card, player, State, method)) return false;
            if (!ignore && method == SummonMethod.Effect && !fromPublic && !definition.IsExtra && !definition.CanNormalSummon) return false;
            if (HasEffect(EffectRecordKind.CannotSpecialSummon, player, card)
                || !fromMaterial && card.Zone != DuelZone.Graveyard && ApplicableEffects().Any(e => e.Kind == EffectRecordKind.CannotSummonName
                    && (e.Player < 0 || e.Player == player) && e.NameId == definition.OriginalNameId)
                || ApplicableEffects().Any(e => e.Kind == EffectRecordKind.OnlySpecialSummonSet && (e.Player < 0 || e.Player == player)
                    && !definition.BelongsTo(e.Value))
                || ApplicableEffects().Any(e => e.Kind == EffectRecordKind.OnlySummonMethod && (e.Player < 0 || e.Player == player)
                    && e.Value != (int)method)
                || card.Zone == DuelZone.ExtraDeck && ApplicableEffects().Any(e => e.Kind == EffectRecordKind.OnlyExtraDeckRace
                    && (e.Player < 0 || e.Player == player) && e.Value != definition.Race)
                || card.Zone == DuelZone.ExtraDeck && ApplicableEffects().Any(e => e.Kind == EffectRecordKind.OnlyExtraDeckSet
                    && (e.Player < 0 || e.Player == player) && !definition.BelongsTo(e.Value))) return false;
            return true;
        }

        internal bool CanSpecialSummonByEffect(DuelCardState card, int player, bool ignore = false,
            SummonMethod method = SummonMethod.Effect, int summoningPlayer = -1)
        {
            int actor = summoningPlayer < 0 ? player : summoningPlayer;
            if (!CanSpecialSummonCard(card, actor, ignore, method)) return false;
            if (!Rules.Get(card.DefinitionId).AllowsMonsterZoneEntry(card, player, State, m_catalog)) return false;
            return GetSpecialSummonDestinations(card, player).Count != 0;
        }

        public IReadOnlyList<int> GetSpecialSummonDestinations(DuelCardState card, int player) =>
            (card.Zone == DuelZone.ExtraDeck ? ExtraDestinations(player, m_catalog.Get(card.DefinitionId), Array.Empty<DuelCardState>())
                : Enumerable.Range(0, 5).Where(slot => !Monsters(player).Any(c => c.Zone == DuelZone.Monster && c.Slot == slot))).ToArray();

        public bool CanSpecialSummonGroup(IReadOnlyList<DuelCardState> cards, int player, bool ignore = false,
            SummonMethod method = SummonMethod.Effect, int summoningPlayer = -1)
        {
            int actor = summoningPlayer < 0 ? player : summoningPlayer;
            if (cards.Count == 0 || cards.Select(card => card.Ref).Distinct().Count() != cards.Count
                || ApplicableEffects().Any(record => record.Kind == EffectRecordKind.SummonGroupLimit
                    && (record.Player < 0 || record.Player == actor) && record.Value < cards.Count)
                || cards.Any(card => !CanSpecialSummonByEffect(card, player, ignore, method, actor))) return false;
            var destinations = cards.Select(card => GetSpecialSummonDestinations(card, player)).ToArray();
            bool Assign(int index, int used)
            {
                if (index == cards.Count) return true;
                foreach (int slot in destinations[index])
                {
                    if ((used & (1 << slot)) != 0 || slot >= 5 && (used & (3 << 5)) != 0) continue;
                    if (Assign(index + 1, used | (1 << slot))) return true;
                }
                return false;
            }
            return Assign(0, 0);
        }

        internal bool SpecialSummonByEffect(DuelCardState card, int player, int slot, CardPosition position,
            CardRef source = default, bool ignore = false, SummonMethod method = SummonMethod.Effect, int summoningPlayer = -1)
        {
            int actor = summoningPlayer < 0 ? player : summoningPlayer;
            if (!CanSpecialSummonByEffect(card, player, ignore, method, actor)) return false;
            var definition = m_catalog.Get(card.DefinitionId);
            if (position != CardPosition.FaceUpAttack && position != CardPosition.FaceUpDefense) return false;
            if (definition.MonsterType == RuleMonsterType.Link && position != CardPosition.FaceUpAttack) return false;
            var destinations = GetSpecialSummonDestinations(card, player);
            if (!destinations.Contains(slot)) return false;
            var before = Snapshot(card);
            Move(card, slot >= 5 ? DuelZone.ExtraMonster : DuelZone.Monster, player,
                slot >= 5 ? slot - 5 : slot, position, MoveCause.Effect, source, actor);
            card.SummonedTurn = State.Turn; card.SummonMethod = method;
            if (method == SummonMethod.Fusion || method == SummonMethod.Procedure || method == SummonMethod.Synchro
                || method == SummonMethod.Xyz || method == SummonMethod.Link || method == SummonMethod.MaskChange
                || method == SummonMethod.Contact) card.ProperlySummoned = true;
            Emit(DuelEventKind.Summoned, actor, card, from: before.Zone, cause: MoveCause.Effect,
                effectSource: source, effectPlayer: actor, before: before, after: Snapshot(card));
            return true;
        }

        public IReadOnlyList<DuelCardState[]> GetFusionMaterialGroups(DuelCardState target, int player,
            IEnumerable<DuelCardState> candidates, int maxMaterials = 0)
        {
            var definition = m_catalog.Get(target.DefinitionId);
            var rules = Rules.Get(target.DefinitionId);
            var groups = new List<DuelCardState[]>();
            if (definition.MonsterType != RuleMonsterType.Fusion || !rules.HasSummonRecipe) return groups;
            var available = candidates.Where(c => c.InstanceId != target.InstanceId
                && m_catalog.Get(c.DefinitionId).Kind == RuleCardKind.Monster
                && c.Zone != DuelZone.Material && Rules.Get(c.DefinitionId).CanUseAsMaterial(definition, c, State)
                && !HasEffect(EffectRecordKind.CannotBeMaterial, player, c)).Distinct().OrderBy(c => c.InstanceId).ToArray();
            int maximum = Math.Min(available.Length, rules.MaxSummonMaterials);
            if (maxMaterials > 0) maximum = Math.Min(maximum, maxMaterials);
            var selected = new List<DuelCardState>();
            void Visit(int from)
            {
                if (selected.Count >= rules.MinSummonMaterials && rules.MatchesSummonMaterials(definition, selected, m_catalog))
                    groups.Add(selected.ToArray());
                if (selected.Count == maximum) return;
                for (int i = from; i < available.Length; i++)
                { selected.Add(available[i]); Visit(i + 1); selected.RemoveAt(selected.Count - 1); }
            }
            if (maximum >= rules.MinSummonMaterials) Visit(0);
            return groups;
        }

        public bool FusionSummonByEffect(DuelCardState target, IReadOnlyList<DuelCardState> materials, int player,
            int slot, CardPosition position, CardRef source = default, bool banishMaterials = false)
        {
            var definition = m_catalog.Get(target.DefinitionId);
            if (definition.MonsterType != RuleMonsterType.Fusion || target.Zone != DuelZone.ExtraDeck
                || !CanSpecialSummonCard(target, player, false, SummonMethod.Fusion)
                || materials.Count == 0 || materials.Select(c => c.InstanceId).Distinct().Count() != materials.Count
                || !Rules.Get(target.DefinitionId).MatchesSummonMaterials(definition, materials, m_catalog)
                || materials.Any(c => !State.Cards.Contains(c) || c.Zone == DuelZone.Material
                    || !Rules.Get(c.DefinitionId).CanUseAsMaterial(definition, c, State)
                    || HasEffect(EffectRecordKind.CannotBeMaterial, player, c))
                || (position != CardPosition.FaceUpAttack && position != CardPosition.FaceUpDefense)
                || !ExtraDestinations(player, definition, materials).Contains(slot)) return false;
            var history = materials.Select(c => c.DefinitionId).ToList();
            foreach (var card in materials)
                Move(card, banishMaterials ? DuelZone.Banished : DuelZone.Graveyard, card.Owner,
                    cause: MoveCause.Effect, effectSource: source, effectPlayer: player,
                    wasSummonMaterial: true, materialMethod: SummonMethod.Fusion);
            bool summoned = SpecialSummonByEffect(target, player, slot, position, source, method: SummonMethod.Fusion);
            if (summoned) target.SummonMaterialDefinitions = history;
            return summoned;
        }
    }
}
