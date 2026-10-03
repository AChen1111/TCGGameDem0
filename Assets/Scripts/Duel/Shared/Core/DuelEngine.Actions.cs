using System.Linq;
using System.Collections.Generic;

namespace AChen.Duel.Core
{
    public sealed partial class DuelEngine
    {
        internal bool MainOpen(int player) => State.TurnPlayer == player && State.Window == TimingWindow.Open
            && (State.Phase == DuelPhase.Main1 || State.Phase == DuelPhase.Main2);
        internal List<DuelCardState> Monsters(int player) => State.Cards.Where(x => x.Controller == player
            && (x.Zone == DuelZone.Monster || x.Zone == DuelZone.ExtraMonster)).ToList();

        string ValidateNormalSummon(DuelCommand command)
        {
            if (!MainOpen(command.Player) || State.Players[command.Player].NormalSummonsThisTurn != 0) return "NORMAL_SUMMON_NOT_AVAILABLE";
            var card = State.Cards.FirstOrDefault(x => x.InstanceId == command.CardId);
            if (card == null || card.Owner != command.Player || card.Zone != DuelZone.Hand) return "CARD_NOT_IN_HAND";
            var definition = m_catalog.Get(card.DefinitionId);
            if (definition.Kind != RuleCardKind.Monster || !definition.CanNormalSummon) return "CANNOT_NORMAL_SUMMON";
            int required = definition.Level >= 7 ? 2 : definition.Level >= 5 ? 1 : 0;
            if (command.Cards.Distinct().Count() != required || command.Cards.Length != required
                || command.Cards.Any(id => !Monsters(command.Player).Any(x => x.InstanceId == id))) return "INVALID_TRIBUTES";
            if (!Rules.Get(card.DefinitionId).AllowsMonsterZoneEntry(card, command.Player, State, m_catalog,
                command.Cards.Select(Card).ToArray())) return "MONSTER_ZONE_ENTRY_RESTRICTED";
            if (command.Slot < 0 || command.Slot > 4 || State.Cards.Any(x => x.Zone == DuelZone.Monster
                && x.Controller == command.Player && x.Slot == command.Slot && !command.Cards.Contains(x.InstanceId))) return "ZONE_OCCUPIED";
            return "";
        }

        void NormalSummon(DuelCommand command)
        {
            foreach (int id in command.Cards) Move(Card(id), DuelZone.Graveyard, Card(id).Owner, cause: MoveCause.Tribute);
            var card = Card(command.CardId);
            var before = Snapshot(card);
            Move(card, DuelZone.Monster, command.Player, command.Slot,
                command.Kind == DuelCommandKind.SetMonster ? CardPosition.FaceDownDefense : CardPosition.FaceUpAttack);
            card.SummonedTurn = State.Turn; card.SetTurn = command.Kind == DuelCommandKind.SetMonster ? State.Turn : 0;
            card.ProperlySummoned = true; card.SummonMethod = SummonMethod.Normal;
            State.Players[command.Player].NormalSummonsThisTurn++;
            if (command.Kind != DuelCommandKind.SetMonster)
                Emit(DuelEventKind.Summoned, command.Player, card, from: before.Zone, before: before, after: Snapshot(card));
            OpenResponse();
        }

        internal IEnumerable<DuelAction> MainActions(int player)
        {
            if (!MainOpen(player)) yield break;
            foreach (var card in State.Cards.Where(c => c.Owner == player && c.Zone == DuelZone.Hand))
            {
                var definition = m_catalog.Get(card.DefinitionId);
                if (definition.Kind == RuleCardKind.Monster) continue;
                var slots = definition.SpellTrapType == RuleSpellTrapType.Field ? new List<int> { 0 }
                    : Enumerable.Range(0, 5).Where(slot => !State.Cards.Any(c => c.Controller == player
                        && c.Zone == DuelZone.SpellTrap && c.Slot == slot)).ToList();
                if (slots.Count > 0) yield return new DuelAction { Id = "set:" + card.InstanceId,
                    Kind = DuelCommandKind.SetSpellTrap, Card = card.Ref, Slots = slots };
            }
            foreach (var card in Monsters(player))
            {
                var position = card.Position == CardPosition.FaceUpAttack ? CardPosition.FaceUpDefense : CardPosition.FaceUpAttack;
                if (ValidateChangePosition(new DuelCommand { Player = player, CardId = card.InstanceId,
                    Position = position }).Length == 0)
                    yield return new DuelAction { Id = "position:" + card.InstanceId, Kind = DuelCommandKind.ChangePosition,
                        Card = card.Ref, Positions = new List<CardPosition> { position } };
            }
            if (State.Players[player].NormalSummonsThisTurn != 0) yield break;
            var monsters = Monsters(player);
            foreach (var card in State.Cards.Where(x => x.Owner == player && x.Zone == DuelZone.Hand))
            {
                var definition = m_catalog.Get(card.DefinitionId);
                if (definition.Kind != RuleCardKind.Monster || !definition.CanNormalSummon) continue;
                int tributes = definition.Level >= 7 ? 2 : definition.Level >= 5 ? 1 : 0;
                if (monsters.Count < tributes) continue;
                if (!Rules.Get(card.DefinitionId).AllowsMonsterZoneEntry(card, player, State, m_catalog,
                    tributes == 0 ? System.Array.Empty<DuelCardState>() : monsters)) continue;
                var slots = Enumerable.Range(0, 5).Where(slot => !monsters.Any(x => x.Zone == DuelZone.Monster
                    && x.Slot == slot) || tributes > 0).ToList();
                if (slots.Count == 0) continue;
                foreach (var kind in new[] { DuelCommandKind.NormalSummon, DuelCommandKind.SetMonster })
                    yield return new DuelAction { Id = kind + ":" + card.InstanceId, Kind = kind, Card = card.Ref,
                        Slots = slots, MinSelections = tributes, MaxSelections = tributes,
                        Positions = new List<CardPosition> { kind == DuelCommandKind.SetMonster
                            ? CardPosition.FaceDownDefense : CardPosition.FaceUpAttack },
                        SelectionCards = tributes == 0 ? new List<CardRef>() : monsters.Select(x => x.Ref).ToList() };
            }
        }

        string ValidateSetSpellTrap(DuelCommand command)
        {
            if (!MainOpen(command.Player)) return "SET_NOT_AVAILABLE";
            var card = State.Cards.FirstOrDefault(c => c.InstanceId == command.CardId);
            if (card == null || card.Owner != command.Player || card.Zone != DuelZone.Hand) return "CARD_NOT_IN_HAND";
            var definition = m_catalog.Get(card.DefinitionId);
            if (definition.Kind == RuleCardKind.Monster) return "CANNOT_SET_SPELL_TRAP";
            if (definition.SpellTrapType == RuleSpellTrapType.Field) return command.Slot == 0 ? "" : "INVALID_FIELD_SLOT";
            if (command.Slot < 0 || command.Slot > 4 || State.Cards.Any(c => c.Zone == DuelZone.SpellTrap
                && c.Controller == command.Player && c.Slot == command.Slot)) return "ZONE_OCCUPIED";
            return "";
        }

        void SetSpellTrap(DuelCommand command)
        {
            var card = Card(command.CardId);
            bool field = m_catalog.Get(card.DefinitionId).SpellTrapType == RuleSpellTrapType.Field;
            if (field)
            {
                var previous = State.Cards.FirstOrDefault(c => c.Controller == command.Player && c.Zone == DuelZone.Field);
                if (previous != null) Move(previous, DuelZone.Graveyard, previous.Owner);
            }
            Move(card, field ? DuelZone.Field : DuelZone.SpellTrap, command.Player, command.Slot, CardPosition.FaceDown);
            card.SetTurn = State.Turn;
            OpenResponse();
        }

        string ValidateChangePosition(DuelCommand command)
        {
            if (!MainOpen(command.Player)) return "POSITION_CHANGE_NOT_AVAILABLE";
            var card = Monsters(command.Player).FirstOrDefault(c => c.InstanceId == command.CardId);
            if (card == null || m_catalog.Get(card.DefinitionId).MonsterType == RuleMonsterType.Link) return "CANNOT_CHANGE_POSITION";
            if (card.SummonedTurn == State.Turn || card.SetTurn == State.Turn || card.PositionChangedTurn == State.Turn
                || card.AttacksThisTurn > 0) return "POSITION_CHANGE_NOT_AVAILABLE";
            if (card.Position == CardPosition.FaceDownDefense)
                return command.Position == CardPosition.FaceUpAttack ? "" : "INVALID_FLIP_SUMMON_POSITION";
            if (command.Position != CardPosition.FaceUpAttack && command.Position != CardPosition.FaceUpDefense
                || command.Position == card.Position) return "INVALID_POSITION_CHANGE";
            return "";
        }

        void ChangePosition(DuelCommand command)
        {
            var card = Card(command.CardId);
            var before = Snapshot(card);
            bool flip = card.Position == CardPosition.FaceDownDefense;
            card.Position = command.Position; card.PositionChangedTurn = State.Turn;
            if (flip)
            {
                card.SummonMethod = SummonMethod.Flip;
                Emit(DuelEventKind.Revealed, command.Player, card);
                Emit(DuelEventKind.Summoned, command.Player, card, from: before.Zone, detail: "flip", before: before, after: Snapshot(card));
            }
            Emit(DuelEventKind.PositionChanged, command.Player, card);
            OpenResponse();
        }

        internal void OpenResponse()
        {
            State.Window = TimingWindow.FastResponse; State.WaitingSeat = State.TurnPlayer; State.ConsecutivePasses = 0;
        }
    }
}
