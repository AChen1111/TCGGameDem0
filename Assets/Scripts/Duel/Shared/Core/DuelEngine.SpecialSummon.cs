using System;
using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed partial class DuelEngine
    {
        internal string ValidateSpecialSummon(DuelCommand command)
        {
            if (!MainOpen(command.Player)) return "SPECIAL_SUMMON_NOT_AVAILABLE";
            var source = State.Cards.FirstOrDefault(c => c.InstanceId == command.CardId);
            if (command.AbilityId.Length != 0)
            {
                if (source == null || source.Owner != command.Player) return "INVALID_SUMMON_SOURCE";
                var procedure = Rules.Get(source.DefinitionId).CreateSummonProcedures().FirstOrDefault(p => p.Id == command.AbilityId);
                if (procedure == null) return "UNKNOWN_SUMMON_PROCEDURE";
                if (!CanSpecialSummonCard(source, command.Player, false, SummonMethod.Procedure)) return "SPECIAL_SUMMON_RESTRICTED";
                return procedure.Validate(new EffectContext(this, command.Player, source.InstanceId), command);
            }
            if (source == null || source.Owner != command.Player || source.Zone != DuelZone.ExtraDeck) return "CARD_NOT_IN_EXTRA_DECK";
            var definition = m_catalog.Get(source.DefinitionId);
            if (!HasExtraProcedure(source)) return "UNSUPPORTED_SUMMON_PROCEDURE";
            if (!CanSpecialSummonCard(source, command.Player, false, ProcedureMethod(definition))) return "SPECIAL_SUMMON_RESTRICTED";
            if (command.Cards.Distinct().Count() != command.Cards.Length) return "DUPLICATE_MATERIAL";
            var materials = Monsters(command.Player).Where(c => command.Cards.Contains(c.InstanceId)).ToArray();
            if (materials.Length != command.Cards.Length || definition.MonsterType != RuleMonsterType.Fusion
                && materials.Any(c => !IsPublic(c))) return "INVALID_MATERIAL_ZONE";
            if (!ValidExtraMaterials(definition, materials)) return "INVALID_SUMMON_MATERIALS";
            if (!Rules.Get(source.DefinitionId).AllowsMonsterZoneEntry(source, command.Player, State, m_catalog, materials))
                return "MONSTER_ZONE_ENTRY_RESTRICTED";
            if (command.Position != CardPosition.FaceUpAttack && command.Position != CardPosition.FaceUpDefense) return "INVALID_SUMMON_POSITION";
            if (definition.MonsterType == RuleMonsterType.Link && command.Position != CardPosition.FaceUpAttack) return "LINK_HAS_NO_DEFENSE_POSITION";
            if (!ExtraDestinations(command.Player, definition, materials).Contains(command.Slot)) return "INVALID_EXTRA_DESTINATION";
            return "";
        }

        bool ValidExtraMaterials(CardDefinition target, IReadOnlyList<DuelCardState> materials)
        {
            var cards = materials.Select(c => m_catalog.Get(c.DefinitionId)).ToArray();
            if (materials.Any(c => !Rules.Get(c.DefinitionId).CanUseAsMaterial(target, c, State)
                || HasEffect(EffectRecordKind.CannotBeMaterial, c.Controller, c)
                || target.MonsterType == RuleMonsterType.Link && HasEffect(EffectRecordKind.CannotUseAsLinkMaterial, c.Controller, c)
                || target.MonsterType == RuleMonsterType.Xyz && HasEffect(EffectRecordKind.CannotUseAsXyzMaterial, c.Controller, c))) return false;
            if (!Rules.Get(target.CardId).MatchesSummonMaterials(target, materials, m_catalog)) return false;
            if (target.MonsterType == RuleMonsterType.Fusion)
                return true;
            if (target.MonsterType == RuleMonsterType.Link)
            {
                var sums = new HashSet<int> { 0 };
                foreach (var card in cards)
                {
                    var next = new HashSet<int>();
                    foreach (int sum in sums)
                    { next.Add(sum + 1); if (card.MonsterType == RuleMonsterType.Link) next.Add(sum + card.LinkRating); }
                    sums = next;
                }
                return cards.Length > 0 && sums.Contains(target.LinkRating);
            }
            if (target.MonsterType == RuleMonsterType.Xyz)
                return true;
            return cards.Length >= 2 && cards.Count(c => c.IsTuner) == 1
                && materials.All(c => c.CurrentLevel > 0) && materials.Sum(c => c.CurrentLevel) == target.Level;
        }
        bool HasExtraProcedure(DuelCardState source)
        {
            var definition = m_catalog.Get(source.DefinitionId);
            return definition.MonsterType == RuleMonsterType.Synchro || definition.MonsterType == RuleMonsterType.Xyz
                || definition.MonsterType == RuleMonsterType.Link || definition.MonsterType == RuleMonsterType.Fusion
                && Rules.Get(source.DefinitionId).HasSummonRecipe
                && Rules.Get(source.DefinitionId).AllowsSummonMethod(source, source.Controller, State, SummonMethod.Contact);
        }

        static SummonMethod ProcedureMethod(CardDefinition definition) => definition.MonsterType == RuleMonsterType.Link
            ? SummonMethod.Link : definition.MonsterType == RuleMonsterType.Xyz ? SummonMethod.Xyz
                : definition.MonsterType == RuleMonsterType.Fusion ? SummonMethod.Contact : SummonMethod.Synchro;

        IEnumerable<int> ExtraDestinations(int player, CardDefinition target, IReadOnlyList<DuelCardState> materials)
        {
            var consumed = new HashSet<int>(materials.Select(c => c.InstanceId));
            for (int slot = 0; slot < 5; slot++)
                if (!State.Cards.Any(c => c.Zone == DuelZone.Monster && c.Controller == player && c.Slot == slot
                    && !consumed.Contains(c.InstanceId)) && (target.MonsterType != RuleMonsterType.Link
                        || State.Cards.Any(c => !consumed.Contains(c.InstanceId) && LinkPointsAt(c, player, slot)))) yield return slot;
            if (!State.Cards.Any(c => c.Zone == DuelZone.ExtraMonster && c.Controller == player && !consumed.Contains(c.InstanceId)))
                for (int slot = 0; slot < 2; slot++)
                    if (!State.Cards.Any(c => c.Zone == DuelZone.ExtraMonster && c.Slot == slot && !consumed.Contains(c.InstanceId)))
                        yield return slot + 5;
        }

        bool LinkPointsAt(DuelCardState link, int player, int slot)
        {
            if (!OnField(link) || !IsPublic(link) || m_catalog.Get(link.DefinitionId).MonsterType != RuleMonsterType.Link)
                return false;
            var origin = BoardPoint(link.Controller, link.Zone, link.Slot);
            var destination = BoardPoint(player, DuelZone.Monster, slot);
            int orientation = link.Controller == 0 ? 1 : -1;
            int dx = (destination.x - origin.x) * orientation;
            int dy = (destination.y - origin.y) * orientation;
            int bit = ArrowBit(dx, dy);
            return bit != 0 && (m_catalog.Get(link.DefinitionId).LinkArrows & bit) != 0;
        }

        public IEnumerable<DuelCardState> LinkedMonsters(DuelCardState link)
        {
            if ((link.Zone != DuelZone.Monster && link.Zone != DuelZone.ExtraMonster) || !IsPublic(link)
                || m_catalog.Get(link.DefinitionId).MonsterType != RuleMonsterType.Link) yield break;
            var origin = BoardPoint(link.Controller, link.Zone, link.Slot);
            int orientation = link.Controller == 0 ? 1 : -1;
            foreach (var target in State.Cards.Where(c => c.Zone == DuelZone.Monster || c.Zone == DuelZone.ExtraMonster))
            {
                var destination = BoardPoint(target.Controller, target.Zone, target.Slot);
                int bit = ArrowBit((destination.x - origin.x) * orientation, (destination.y - origin.y) * orientation);
                if (bit != 0 && (m_catalog.Get(link.DefinitionId).LinkArrows & bit) != 0) yield return target;
            }
        }

        static (int x, int y) BoardPoint(int player, DuelZone zone, int slot) => zone == DuelZone.ExtraMonster
            ? (slot == 0 ? 2 : 6, 2) : (player == 0 ? slot * 2 : 8 - slot * 2, player == 0 ? 0 : 4);

        static int ArrowBit(int dx, int dy)
        {
            if (dx == -2 && dy == -2) return 1;
            if (dx == 0 && dy == -2) return 2;
            if (dx == 2 && dy == -2) return 4;
            if (dx == -2 && dy == 0) return 8;
            if (dx == 2 && dy == 0) return 32;
            if (dx == -2 && dy == 2) return 64;
            if (dx == 0 && dy == 2) return 128;
            if (dx == 2 && dy == 2) return 256;
            return 0;
        }

        internal void ExecuteSpecialSummon(DuelCommand command, bool openResponse = true, CardRef effectSource = default)
        {
            var source = Card(command.CardId);
            if (command.AbilityId.Length != 0)
            {
                Rules.Get(source.DefinitionId).CreateSummonProcedures().Single(p => p.Id == command.AbilityId)
                    .Execute(new EffectContext(this, command.Player, source.InstanceId), command);
                OpenResponse(); return;
            }
            var definition = m_catalog.Get(source.DefinitionId);
            var method = ProcedureMethod(definition);
            var before = Snapshot(source);
            bool xyz = definition.MonsterType == RuleMonsterType.Xyz;
            var history = command.Cards.Select(id => Card(id).DefinitionId).ToList();
            var destination = method == SummonMethod.Contact ? Rules.Get(source.DefinitionId).ContactMaterialDestination
                : xyz ? DuelZone.Material : DuelZone.Graveyard;
            foreach (int id in command.Cards)
            {
                var material = Card(id);
                if (xyz)
                {
                    foreach (int attached in material.Materials)
                    { Card(attached).HostInstanceId = source.InstanceId; source.Materials.Add(attached); }
                    material.Materials.Clear();
                }
                Move(material, destination, material.Owner, cause: MoveCause.SummonMaterial,
                    wasSummonMaterial: true, materialMethod: method);
                if (xyz) { material.HostInstanceId = source.InstanceId; source.Materials.Add(id); }
            }
            if (method == SummonMethod.Contact && destination == DuelZone.Deck)
            { Shuffle(State.Players[command.Player].Deck); Emit(DuelEventKind.Shuffled, command.Player); }
            Move(source, command.Slot >= 5 ? DuelZone.ExtraMonster : DuelZone.Monster,
                command.Player, command.Slot >= 5 ? command.Slot - 5 : command.Slot, command.Position);
            source.SummonedTurn = State.Turn; source.ProperlySummoned = true;
            source.SummonMethod = method; source.SummonMaterialDefinitions = history;
            Emit(DuelEventKind.Summoned, command.Player, source, from: before.Zone,
                effectSource: effectSource, effectPlayer: effectSource.InstanceId == 0 ? -1 : command.Player,
                before: before, after: Snapshot(source));
            if (openResponse) OpenResponse();
        }

        public IReadOnlyList<DuelCardState[]> GetExtraSummonMaterialGroups(DuelCardState source, int player)
        {
            var groups = new List<DuelCardState[]>();
            var definition = m_catalog.Get(source.DefinitionId);
            if (source.Zone != DuelZone.ExtraDeck || !HasExtraProcedure(source)
                || !CanSpecialSummonCard(source, player, false, ProcedureMethod(definition))) return groups;
            var field = Monsters(player).Where(card => definition.MonsterType == RuleMonsterType.Fusion || IsPublic(card)).ToArray();
            for (int mask = 1; mask < (1 << field.Length); mask++)
            {
                var selected = field.Where((card, index) => (mask & (1 << index)) != 0).ToArray();
                if (ValidExtraMaterials(definition, selected) && Rules.Get(source.DefinitionId).AllowsMonsterZoneEntry(source, player, State, m_catalog, selected)
                    && ExtraDestinations(player, definition, selected).Any()) groups.Add(selected);
            }
            return groups;
        }

        public IReadOnlyList<int> GetExtraSummonDestinations(DuelCardState source, int player, IReadOnlyList<DuelCardState> materials) =>
            ExtraDestinations(player, m_catalog.Get(source.DefinitionId), materials).ToArray();

        public bool ExtraSummonByEffect(DuelCardState source, IReadOnlyList<DuelCardState> materials, int player, int slot,
            CardPosition position = CardPosition.FaceUpAttack, CardRef effectSource = default)
        {
            if (!GetExtraSummonMaterialGroups(source, player).Any(group => group.Length == materials.Count
                && group.All(materials.Contains)) || !GetExtraSummonDestinations(source, player, materials).Contains(slot)
                || position != CardPosition.FaceUpAttack && position != CardPosition.FaceUpDefense
                || m_catalog.Get(source.DefinitionId).MonsterType == RuleMonsterType.Link && position != CardPosition.FaceUpAttack) return false;
            ExecuteSpecialSummon(new DuelCommand { Player = player, CardId = source.InstanceId,
                Cards = materials.Select(card => card.InstanceId).ToArray(), Slot = slot, Position = position }, false, effectSource);
            return true;
        }

        internal IEnumerable<DuelAction> SpecialSummonActions(int player)
        {
            if (!MainOpen(player)) yield break;
            foreach (var card in State.Cards.Where(c => c.Owner == player))
            foreach (var procedure in Rules.Get(card.DefinitionId).CreateSummonProcedures())
                if (CanSpecialSummonCard(card, player, false, SummonMethod.Procedure))
                    foreach (var action in procedure.QueryActions(new EffectContext(this, player, card.InstanceId))) yield return action;
            var allField = Monsters(player).ToArray();
            foreach (var source in State.Cards.Where(c => c.Owner == player && c.Zone == DuelZone.ExtraDeck))
            {
                var definition = m_catalog.Get(source.DefinitionId);
                if (!HasExtraProcedure(source) || !CanSpecialSummonCard(source, player, false, ProcedureMethod(definition))) continue;
                var field = allField.Where(c => definition.MonsterType == RuleMonsterType.Fusion || IsPublic(c)).ToArray();
                var groups = new List<DuelCardState[]>();
                var slots = new HashSet<int>();
                for (int mask = 1; mask < (1 << field.Length); mask++)
                {
                    var selected = field.Where((c, i) => (mask & (1 << i)) != 0).ToArray();
                    if (!ValidExtraMaterials(definition, selected)) continue;
                    if (!Rules.Get(source.DefinitionId).AllowsMonsterZoneEntry(source, player, State, m_catalog, selected)) continue;
                    var destinations = ExtraDestinations(player, definition, selected).ToArray();
                    if (destinations.Length == 0) continue;
                    groups.Add(selected); foreach (int slot in destinations) slots.Add(slot);
                }
                if (groups.Count == 0) continue;
                yield return new DuelAction { Id = "special:" + source.InstanceId, Kind = DuelCommandKind.SpecialSummon,
                    Card = source.Ref, Slots = slots.OrderBy(x => x).ToList(),
                    Positions = definition.MonsterType == RuleMonsterType.Link ? new List<CardPosition> { CardPosition.FaceUpAttack }
                        : new List<CardPosition> { CardPosition.FaceUpAttack, CardPosition.FaceUpDefense },
                    SelectionCards = groups.SelectMany(x => x).Select(x => x.Ref).Distinct().ToList(),
                    MinSelections = groups.Min(x => x.Length), MaxSelections = groups.Max(x => x.Length) };
            }
        }
    }
}
