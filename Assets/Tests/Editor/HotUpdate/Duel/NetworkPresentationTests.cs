using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using AChen.Configuration;
using AChen.Duel.Client;
using AChen.Duel.Presentation;
using AChen.Networking;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using Core = AChen.Duel.Core;

/// <summary>仅覆盖联网与回放表现边界，不执行卡效引擎或访问在线服务。</summary>
public sealed class NetworkPresentationTests
{
    PublishedGameConfig m_previousData;
    Table.CardRow[] m_previousCards;
    string m_monster;
    string[] m_previewDefinitions;

    [SetUp]
    public void InstallPackagedConfiguration()
    {
        m_previousData = LocalGameConfiguration.Data;
        var table = (Dictionary<string, Table.CardRow>)typeof(CardCatalog)
            .GetField("s_table", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
        m_previousCards = table.Values.ToArray();
        var data = GameConfigTables.Assemble(Directory.GetFiles("Assets/GameConfiguration", "*.bytes")
            .ToDictionary(Path.GetFileNameWithoutExtension, File.ReadAllBytes));
        typeof(LocalGameConfiguration).GetProperty(nameof(LocalGameConfiguration.Data)).SetValue(null, data);
        CardCatalog.Install(Table.CardRow.LoadBytes(data.CardTable));
        var catalog = Core.DuelCardCatalog.CreateDefault();
        m_monster = catalog.Cards.First(c => c.Kind == Core.RuleCardKind.Monster && !c.IsExtra).CardId;
        m_previewDefinitions = catalog.Cards.Take(2).Select(c => c.CardId).Reverse().ToArray();
    }
    [TearDown]
    public void RestoreConfiguration()
    {
        typeof(LocalGameConfiguration).GetProperty(nameof(LocalGameConfiguration.Data)).SetValue(null, m_previousData);
        if (m_previousCards.Length == 0) CardCatalog.Uninstall();
        else CardCatalog.Install(m_previousCards);
    }
    DuelSeatDto Snapshot(int seat, int turn = 1) => new()
    {
        Seat = seat, Revision = turn, Turn = turn, TurnPlayer = 1, WaitingSeat = -1,
        Phase = Core.DuelPhase.Main1, Window = Core.TimingWindow.Open, Winner = -1,
        Players = new[]
        {
            new DuelPlayerSnapshotDto { LifePoints = 5600, DeckCount = 3, HandCount = 0, ExtraDeckCount = 0 },
            new DuelPlayerSnapshotDto { LifePoints = 8100, DeckCount = 3, HandCount = 0, ExtraDeckCount = 0 }
        },
        Cards = new[]
        {
            new DuelCardDto { ViewCardId = "field-a", DefinitionId = m_monster, Owner = 0, Controller = 0,
                Zone = Core.DuelZone.Monster, Slot = 1, Position = Core.CardPosition.FaceUpAttack },
            new DuelCardDto { ViewCardId = "field-b", DefinitionId = m_monster, Owner = 1, Controller = 1,
                Zone = Core.DuelZone.ExtraMonster, Slot = 0, Position = Core.CardPosition.FaceUpDefense }
        },
        MainDeckDefinitions = new[] { m_previewDefinitions[0], m_previewDefinitions[1], m_previewDefinitions[0] }
    };
    DuelRoomDto Room(int seat, int turn = 1, long sequence = 10) => new()
    {
        Id = Guid.NewGuid(), Code = "654321", Status = "Running", LocalSeat = seat, Sequence = sequence,
        Players = new[]
        {
            new DuelRoomPlayerDto { UserId = Guid.NewGuid(), Seat = 0, Nickname = "席位零", AvatarId = 1010001, AvatarFrameId = 1030001 },
            new DuelRoomPlayerDto { UserId = Guid.NewGuid(), Seat = 1, Nickname = "席位一", AvatarId = 1010002, AvatarFrameId = 1030002 }
        },
        Duel = Snapshot(seat, turn), RemainingSeconds = new double[] { 21, 43 }
    };
    static NetworkDuelSession Source(DuelRoomDto initial, Func<int, Core.DuelZone, System.Threading.CancellationToken, UniTask<DuelPreviewDto>> preview = null)
        => new(initial, _ => Assert.Fail("只读表现用例不应提交动作"),
            preview ?? ((_, _, _) => UniTask.FromResult(new DuelPreviewDto { Cards = Array.Empty<DuelPreviewEntryDto>() })),
            (_, _, _, _) => UniTask.FromResult(new DuelNamePageDto { Items = Array.Empty<DuelNameDto>() }));

    [TestCase(0)]
    [TestCase(1)]
    public void LocalSeatAlwaysMapsToBottomIncludingSharedExtraMonsterSlot(int seat)
    {
        var room = Room(seat);
        using var source = Source(room);
        source.Start();
        Assert.That(source.Current.ViewingSeat, Is.EqualTo(seat));
        Assert.That(source.Current.Players[0].Name, Is.EqualTo(room.Players[seat].Nickname));
        Assert.That(source.Current.Players[0].LP, Is.EqualTo(room.Duel.Players[seat].LifePoints));
        Assert.That(source.Current.Players[0].AvatarFrameId, Is.EqualTo(room.Players[seat].AvatarFrameId));
        Assert.That(source.Current.Seconds[0], Is.EqualTo(room.RemainingSeconds[seat]));
        Assert.That(source.Current.ActivePlayer, Is.EqualTo(seat == 1 ? 0 : 1));
        var extra = source.Current.Cards.Single(c => c.Zone.Kind == DuelZone.ExtraMonster);
        Assert.That(extra.Owner, Is.EqualTo(seat == 1 ? 0 : 1));
        Assert.That(extra.Zone.Slot, Is.EqualTo(seat == 1 ? 1 : 0));
        Assert.That(source.Current.Cards.Where(c => !c.Zone.IsSlot).All(c => c.Zone.Slot == 0), Is.True);
        Assert.That(source.Current.InZone(new ZoneRef(DuelZone.MainDeck, 0)).Count, Is.EqualTo(3));
        Assert.That(source.Current.Cards.Select(c => c.InstanceId).Distinct().Count(), Is.EqualTo(source.Current.Cards.Count));
    }

    [TestCase(Core.DuelZone.Hand)]
    [TestCase(Core.DuelZone.Deck)]
    [TestCase(Core.DuelZone.ExtraDeck)]
    [TestCase(Core.DuelZone.Graveyard)]
    [TestCase(Core.DuelZone.Banished)]
    [TestCase(Core.DuelZone.Material)]
    public void NonSlotPublicCardsIgnorePreviousFieldSlot(Core.DuelZone zone)
    {
        var room = Room(0);
        room.Duel.Cards[0].Zone = zone;
        room.Duel.Cards[0].Slot = 4; // DTO可能保留移动前的场上格序号。
        using var source = Source(room);
        var card = source.Current.Cards.Single(c => c.Known && c.Owner == 0);
        Assert.That(card.Zone.Slot, Is.Zero);
        Assert.That(source.Current.InZone(new ZoneRef(LocalDuelSession.MapZone(zone), 0)), Does.Contain(card));
    }

    [Test]
    public void DuplicateSequenceAndReconnectSnapshotDoNotReplaySummon()
    {
        using var source = Source(Room(0));
        var summons = 0; var resets = 0;
        source.Changed += change => { if (change.Kind == DuelChangeKind.Summoned) summons++; if (change.Kind == DuelChangeKind.Reset) resets++; };
        source.Start();
        var updated = Room(0, 2, 11);
        var notice = new DuelNoticeDto
        {
            Room = updated,
            Frames = new[] { new DuelFrameDto { Event = new DuelEventDto { EventId = 20, Kind = Core.DuelEventKind.Summoned,
                Player = 0, ViewCardId = "field-a", DefinitionId = m_monster }, Snapshot = updated.Duel } }
        };
        source.Receive(notice);
        Assert.That(source.Current.Animating, Is.True);
        source.Receive(notice);
        Assert.That(summons, Is.EqualTo(1));
        source.Receive(notice, resynchronizing: true);
        Assert.That(resets, Is.EqualTo(1));
        Assert.That(source.Current.Animating, Is.False);
        Assert.That(source.Current.Turn, Is.EqualTo(2));
        source.FinishPresentation();
        Assert.That(summons, Is.EqualTo(1));
    }

    [Test]
    public void MainDeckPreviewExposesSortedDefinitionsWithoutEntityHandlesOrOpponentIdentity()
    {
        var calls = 0;
        using var source = Source(Room(1), (seat, zone, _) =>
        {
            calls++;
            Assert.That(seat, Is.EqualTo(0)); Assert.That(zone, Is.EqualTo(Core.DuelZone.Deck));
            return UniTask.FromResult(new DuelPreviewDto { Seat = seat, Zone = zone,
                Cards = new[] { new DuelPreviewEntryDto { Known = false, Count = 3 } } });
        });
        var mine = source.PreviewZoneAsync(new ZoneRef(DuelZone.MainDeck, 0), default).GetAwaiter().GetResult();
        CollectionAssert.AreEqual(new[] { m_previewDefinitions[0], m_previewDefinitions[1], m_previewDefinitions[0] }
            .OrderBy(id => id, StringComparer.Ordinal), mine.Select(c => c.DefinitionId));
        Assert.That(mine.All(c => c.Known && c.CardId == 0), Is.True);
        Assert.That(calls, Is.Zero);
        Assert.That(source.Current.InZone(new ZoneRef(DuelZone.MainDeck, 0)).All(c => !c.Known), Is.True);
        var opponent = source.PreviewZoneAsync(new ZoneRef(DuelZone.MainDeck, 1), default).GetAwaiter().GetResult();
        Assert.That(calls, Is.EqualTo(1));
        Assert.That(opponent.Single().Known, Is.False);
        Assert.That(opponent.Single().DefinitionId, Is.Empty);
        Assert.That(opponent.Single().CardId, Is.Zero);
        Assert.That(opponent.Single().Count, Is.EqualTo(3));
    }

    [Test]
    public void HandTokenAliasKeepsDisplayIdentityAndNewDrawAppendsAfterExistingCards()
    {
        DuelCardDto Hand(string token) => new() { ViewCardId = token, DefinitionId = m_monster,
            Owner = 0, Controller = 0, Zone = Core.DuelZone.Hand, Position = Core.CardPosition.FaceUp };
        var initial = Room(0); initial.Duel.Cards = new[] { Hand("hand-a"), Hand("hand-b") };
        initial.Duel.Players[0].HandCount = 2;
        using var source = Source(initial);
        var order = new BattleHandOrder(); order.Update(source.Current.Cards);
        var original = order.Cards(0).ToArray();
        source.Changed += change => order.Update(change.View.Cards);
        source.Start();
        var room = Room(0, sequence: 11); room.Duel.Cards = new[] { Hand("hand-b"), Hand("renamed-a") };
        room.Duel.Players[0].HandCount = 2;
        source.Receive(new DuelNoticeDto { Room = room, Frames = new[] { new DuelFrameDto { Snapshot = room.Duel,
            Event = new DuelEventDto { Kind = Core.DuelEventKind.Revealed, PreviousViewCardId = "hand-a", ViewCardId = "renamed-a" } } } });
        CollectionAssert.AreEqual(original, order.Cards(0), "仅换投影token的卡保留原显示句柄和位置");
        source.FinishPresentation();
        var drawn = Room(0, sequence: 12); drawn.Duel.Cards = new[] { Hand("hand-b"), Hand("drawn-c"), Hand("renamed-a") };
        drawn.Duel.Players[0].HandCount = 3;
        source.Receive(new DuelNoticeDto { Room = drawn, Frames = new[] { new DuelFrameDto { Snapshot = drawn.Duel,
            Event = new DuelEventDto { Kind = Core.DuelEventKind.Moved, ViewCardId = "drawn-c", From = Core.DuelZone.Deck, Player = 0 } } } });
        CollectionAssert.AreEqual(original, order.Cards(0).Take(2));
        var added = source.Current.InZone(new ZoneRef(DuelZone.Hand, 0)).Single(c => !original.Contains(c.InstanceId)).InstanceId;
        Assert.That(order.Cards(0).Last(), Is.EqualTo(added));
    }

    [Test]
    public void ReplayPauseAndViewSwitchRetainFramePositionAndRemainReadOnly()
    {
        DuelReplayTrackDto Track(int seat)
        {
            DuelNoticeDto Notice(int turn, long eventId) => new()
            {
                Room = Room(seat, turn, 10 + turn),
                Frames = new[] { new DuelFrameDto { Event = new DuelEventDto { EventId = eventId,
                    Kind = Core.DuelEventKind.TurnChanged, Player = 1, Turn = turn }, Snapshot = Snapshot(seat, turn) } }
            };
            return new DuelReplayTrackDto { Seat = seat, InitialRoom = Room(seat), Frames = new[] { Notice(2, 20), Notice(3, 30) } };
        }
        var replay = new ReplayDuelSession(new DuelReplayDto { Summary = new DuelReplaySummaryDto { LocalSeat = 0 },
            Tracks = new[] { Track(0), Track(1) } });
        replay.Start();
        Assert.That(replay.Current.Turn, Is.EqualTo(2));
        replay.TogglePause(); replay.FinishPresentation(); replay.SwitchView(); replay.Tick(100);
        Assert.That(replay.Current.Turn, Is.EqualTo(2));
        Assert.That(replay.Current.ViewingSeat, Is.EqualTo(1));
        Assert.That(replay.Current.Players[0].Name, Is.EqualTo("席位一"));
        Assert.That(replay.Current.ReadOnly, Is.True);
        Assert.That(replay.Current.Choice.Active, Is.False);
        replay.Submit(new SurrenderDuel());
        Assert.That(replay.Current.Turn, Is.EqualTo(2));
        replay.TogglePause(); replay.Tick(.2f); replay.Tick(.7f);
        Assert.That(replay.Current.Turn, Is.EqualTo(3), "切换视角后应从下一步骤续播，而非回到首回合");
        replay.SwitchView();
        Assert.That(replay.Current.Turn, Is.EqualTo(3));
        Assert.That(replay.Current.ViewingSeat, Is.EqualTo(0));
    }
}
