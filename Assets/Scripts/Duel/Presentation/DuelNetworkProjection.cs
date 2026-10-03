using System;
using System.Collections.Generic;
using System.Linq;
using AChen.Duel.Client;
using AChen.Networking;
using Core = AChen.Duel.Core;

namespace AChen.Duel.Presentation
{
    /// <summary>仅将席位投影转换为显示模型，句柄不含服务端实体编号。</summary>
    internal sealed class DuelNetworkProjection
    {
        readonly Core.DuelCardCatalog m_catalog = Core.DuelCardCatalog.CreateDefault();
        readonly Dictionary<string, DuelCardSpec> m_specs;
        readonly Dictionary<string, int> m_handles = new(StringComparer.Ordinal);
        int m_next;
        public int Seat { get; }
        public IEnumerable<DuelCardSpec> Definitions => m_specs.Values;
        static readonly DuelCardSpec Hidden = new("", "", CardKind.Monster, CardFrame.Normal,
            CardSpellTrapType.None, CardFlags.None, Array.Empty<DuelActionProfile>());
        public DuelNetworkProjection(int seat)
        {
            Seat = seat;
            m_specs = m_catalog.Cards.Select(rule =>
            {
                CardCatalog.TryGet(rule.CardId, out var row);
                return new DuelCardSpec(rule.CardId, LocalGameConfiguration.Data.SourcePoolForArt(rule.CardId),
                    (CardKind)row.Kind, (CardFrame)row.Frame, (CardSpellTrapType)row.SpellTrapType, (CardFlags)row.Flags,
                    Array.Empty<DuelActionProfile>());
            }).ToDictionary(s => s.CardId, StringComparer.Ordinal);
        }
        public int Display(int seat) => seat == Seat ? 0 : 1;
        public int Actual(int display) => display == 0 ? Seat : 1 - Seat;
        public int Handle(string token)
        {
            if (token.Length == 0) return 0;
            if (!m_handles.TryGetValue(token, out var handle)) m_handles.Add(token, handle = ++m_next);
            return handle;
        }
        public void Alias(DuelEventDto evt)
        {
            if (evt.PreviousViewCardId.Length != 0 && evt.ViewCardId.Length != 0)
                m_handles[evt.ViewCardId] = Handle(evt.PreviousViewCardId);
        }
        public ZoneRef Zone(DuelCardDto card)
        {
            var kind = LocalDuelSession.MapZone(card.Zone);
            var slot = card.Zone == Core.DuelZone.ExtraMonster && Seat == 1 ? 1 - card.Slot : card.Slot;
            return kind is DuelZone.Monster or DuelZone.SpellTrap or DuelZone.Field or DuelZone.ExtraMonster
                ? new ZoneRef(kind, Display(card.Controller), slot)
                : new ZoneRef(kind, Display(card.Controller));
        }
        public DuelCardSpec Spec(string id) => m_specs[id];
        public static DuelPhase Phase(Core.DuelPhase phase) => (DuelPhase)(int)phase;
        public static Core.DuelZone RuleZone(DuelZone zone) => zone switch
        {
            DuelZone.MainDeck => Core.DuelZone.Deck, DuelZone.ExtraDeck => Core.DuelZone.ExtraDeck,
            DuelZone.Hand => Core.DuelZone.Hand, DuelZone.Monster => Core.DuelZone.Monster,
            DuelZone.SpellTrap => Core.DuelZone.SpellTrap, DuelZone.Field => Core.DuelZone.Field,
            DuelZone.Graveyard => Core.DuelZone.Graveyard, DuelZone.Banished => Core.DuelZone.Banished,
            DuelZone.ExtraMonster => Core.DuelZone.ExtraMonster, DuelZone.Material => Core.DuelZone.Material,
            _ => throw new ArgumentOutOfRangeException(nameof(zone))
        };
        public static DuelActionKind ActionKind(Core.DuelCommandKind kind) => kind switch
        {
            Core.DuelCommandKind.NormalSummon => DuelActionKind.NormalSummon, Core.DuelCommandKind.SetMonster => DuelActionKind.SetMonster,
            Core.DuelCommandKind.SpecialSummon => DuelActionKind.SpecialSummon, Core.DuelCommandKind.Activate => DuelActionKind.Activate,
            Core.DuelCommandKind.SetSpellTrap => DuelActionKind.SetSpellTrap, Core.DuelCommandKind.ChangePosition => DuelActionKind.ChangePosition,
            Core.DuelCommandKind.Attack => DuelActionKind.Attack, _ => throw new ArgumentOutOfRangeException(nameof(kind))
        };
        public DuelView View(DuelSeatDto snapshot, DuelRoomDto room, bool animating, bool readOnly, float[] seconds,
            DuelSelectionView choice, PendingActionView pending)
        {
            var cards = snapshot.Cards.Select(card =>
            {
                var known = card.DefinitionId.Length != 0;
                var id = Handle(card.ViewCardId);
                var actions = animating || readOnly ? Array.Empty<DuelActionView>() : snapshot.Actions
                    .Where(a => a.SourceViewCardId == card.ViewCardId && a.SourceViewCardId.Length != 0)
                    .Select(a => new DuelActionView(a.ActionToken, ActionKind(a.Kind), a.Slots.Length != 0,
                        a.Positions.Select(p => new DuelActionPositionView(LocalDuelSession.MapPosition(p), Array.Empty<ZoneRef>()))) { Label = a.Label }).ToArray();
                var definition = known ? Spec(card.DefinitionId) : Hidden;
                var printed = known ? m_catalog.Get(card.DefinitionId) : null;
                return new CardView(id, Display(card.Controller), definition, Zone(card), LocalDuelSession.MapPosition(card.Position),
                    actions.Any(a => a.Kind is DuelActionKind.Activate or DuelActionKind.SpecialSummon), Array.Empty<ZoneRef>(),
                    actions.Where(a => a.Kind == DuelActionKind.ChangePosition).SelectMany(a => a.Positions), actions)
                {
                    Known = known, Negated = known && card.Negated, Attack = card.Attack.GetValueOrDefault(), Defense = card.Defense,
                    Level = !known ? 0 : printed.MonsterType == Core.RuleMonsterType.Link ? printed.LinkRating
                        : printed.MonsterType == Core.RuleMonsterType.Xyz ? printed.Rank : card.Level,
                    HostInstanceId = Handle(card.HostViewCardId), MaterialCount = card.MaterialCount
                };
            }).ToList();
            // 未公开的手牌与牌库只按数量造显示背面，不绑定真实实体或牌序。
            for (var seat = 0; seat < 2; seat++)
            {
                AddHidden(cards, seat, DuelZone.MainDeck, snapshot.Players[seat].DeckCount);
                AddHidden(cards, seat, DuelZone.Hand, snapshot.Players[seat].HandCount);
                AddHidden(cards, seat, DuelZone.ExtraDeck, snapshot.Players[seat].ExtraDeckCount);
            }
            var players = Enumerable.Range(0, 2).Select(display =>
            {
                var seat = Actual(display); var player = room.Players.Single(p => p.Seat == seat);
                return new DuelPlayerView(player.Nickname, player.AvatarId, snapshot.Players[seat].LifePoints, player.AvatarFrameId);
            });
            return new DuelView(cards, cards.Where(c => c.EffectAvailable && !c.Zone.IsSlot).Select(c => c.Zone).Distinct(),
                Display(snapshot.TurnPlayer), snapshot.Turn, Phase(snapshot.Phase), animating, PlacementView.Empty,
                new[] { seconds[Seat], seconds[1 - Seat] }, room.Paused, readOnly ? Array.Empty<DuelPhase>() : snapshot.Actions
                    .Where(a => a.Kind == Core.DuelCommandKind.AdvancePhase).Select(a => Phase(a.Phase)), pending, players)
            {
                Choice = animating || readOnly ? DuelSelectionView.Empty : choice, ReadOnly = readOnly, ViewingSeat = Seat,
                Finished = !animating && snapshot.Finished,
                Outcome = snapshot.Finished ? (snapshot.Winner < 0 ? "平局" : room.Players.Single(p => p.Seat == snapshot.Winner).Nickname + " 获胜")
                    + " · " + EndReason(snapshot.EndReason) : ""
            };
        }
        void AddHidden(List<CardView> cards, int seat, DuelZone zone, int count)
        {
            var display = Display(seat); var present = cards.Count(c => c.Zone.Kind == zone && c.Zone.Player == display);
            for (var i = present; i < count; i++) cards.Add(new CardView(Handle("hidden:" + seat + ":" + zone + ":" + i), display,
                Hidden, new ZoneRef(zone, display), CardPosition.FaceDown, false, Array.Empty<ZoneRef>(), Array.Empty<CardPosition>()) { Known = false });
        }
        static string EndReason(string reason) => reason switch
        { "TIMEOUT" => "行动超时", "DISCONNECT_TIMEOUT" => "连接中断", "SURRENDER" => "投降", "DRAW_EMPTY_DECK" => "牌库耗尽", "LIFE_POINTS_ZERO" => "生命值归零", _ => "对局结束" };
        public DuelViewChange Change(DuelFrameDto frame, DuelView view, DuelView previous)
        {
            var evt = frame.Event;
            var kind = evt.Kind switch
            {
                Core.DuelEventKind.Moved => DuelChangeKind.Move, Core.DuelEventKind.Revealed => DuelChangeKind.Position,
                Core.DuelEventKind.PositionChanged => DuelChangeKind.Position, Core.DuelEventKind.Activated => DuelChangeKind.Effect,
                Core.DuelEventKind.Summoned => DuelChangeKind.Summoned, Core.DuelEventKind.Resolved => DuelChangeKind.ChainResolved,
                Core.DuelEventKind.Negated => DuelChangeKind.ChainResolved, Core.DuelEventKind.PhaseChanged => DuelChangeKind.Phase,
                Core.DuelEventKind.TurnChanged => DuelChangeKind.Turn, _ => DuelChangeKind.State
            };
            var id = Handle(evt.ViewCardId);
            var origin = evt.Origin == null ? default : Zone(evt.Origin);
            if (kind == DuelChangeKind.Move)
            {
                var old = previous.Cards.FirstOrDefault(c => c.InstanceId == id);
                origin = old != null ? old.Zone : new ZoneRef(LocalDuelSession.MapZone(evt.From), Display(evt.Player));
            }
            return new DuelViewChange(kind, view, id)
            {
                DefinitionId = evt.DefinitionId, ChainId = evt.ChainId, LinkNumber = evt.LinkNumber,
                AttackerId = Handle(evt.AttackerViewCardId), TargetId = Handle(evt.TargetViewCardId),
                Origin = origin, IsNegated = evt.Kind == Core.DuelEventKind.Negated
            };
        }
    }
}
