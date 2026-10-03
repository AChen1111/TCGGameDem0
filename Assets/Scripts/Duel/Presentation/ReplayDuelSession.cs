using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AChen.Duel.Client;
using Cysharp.Threading.Tasks;
using Core = AChen.Duel.Core;

namespace AChen.Duel.Presentation
{
    /// <summary>回放按双席位可见轨道播放；切换视角保留同一表现步骤位置。</summary>
    public sealed class ReplayDuelSession : IDuelPresentationSource
    {
        sealed class Step
        {
            public DuelRoomDto Room;
            public DuelFrameDto Frame;
            public bool Attack;
            public int[] Impact;
        }
        readonly DuelReplayDto m_record;
        readonly Dictionary<int, Step[]> m_tracks;
        DuelNetworkProjection m_projection;
        DuelRoomDto m_room;
        DuelSeatDto m_display;
        int m_viewer, m_cursor = -1;
        bool m_animating, m_completed, m_started;
        float m_delay;
        public bool Paused { get; private set; }
        public DuelView Current { get; private set; }
        public DuelSessionMode Mode => DuelSessionMode.Replay;
        public string OperationHint
        {
            get
            {
                if (Current.Finished) return Current.Outcome;
                if (Paused) return "回放已暂停";
                if (m_animating) return m_display.Chain.Length > 0 ? "回放：正在播放连锁与效果处理" : "回放：正在播放对局表现";
                if (m_display.WaitingSeat != m_viewer) return "回放：对手正在操作";
                if (m_display.Decision != null) return "回放：" + m_display.Decision.Prompt;
                return "回放：本方正在操作";
            }
        }
        public IEnumerable<DuelCardSpec> Definitions => m_projection.Definitions;
        public bool HasAttackPreview => false;
        public int AttackPreviewSource => 0;
        public int AttackPreviewTarget => 0;
        public int DeclaredAttacker { get; private set; }
        public int DeclaredTarget { get; private set; }
        public event Action<DuelViewChange> Changed = delegate { };
        public ReplayDuelSession(DuelReplayDto replay)
        {
            m_record = replay; m_viewer = replay.Summary.LocalSeat;
            m_tracks = replay.Tracks.ToDictionary(t => t.Seat, t => Steps(t).ToArray());
            m_projection = new DuelNetworkProjection(m_viewer); m_room = Initial(m_viewer); m_display = m_room.Duel; Rebuild();
        }
        DuelRoomDto Initial(int seat) => m_record.Tracks.Single(t => t.Seat == seat).InitialRoom;
        static IEnumerable<Step> Steps(DuelReplayTrackDto track)
        {
            foreach (var notice in track.Frames)
            {
                var attacked = false; int[] attackLife = null;
                foreach (var frame in notice.Frames)
                {
                    var evt = frame.Event;
                    if (!attacked && evt.AttackerViewCardId.Length != 0 && evt.ImpactLifePoints.Length == 2 &&
                        (evt.Kind == Core.DuelEventKind.Damaged || evt.Kind == Core.DuelEventKind.BattleStepChanged && evt.Amount == (int)Core.BattleStep.AfterCalculation))
                    {
                        var impact = notice.Frames.Last(f => f.Event.AttackerViewCardId == evt.AttackerViewCardId &&
                            (f.Event.Kind == Core.DuelEventKind.Damaged || f.Event.Kind == Core.DuelEventKind.BattleStepChanged && f.Event.Amount == (int)Core.BattleStep.AfterCalculation));
                        attackLife = impact.Event.ImpactLifePoints;
                        yield return new Step { Room = notice.Room, Frame = frame, Attack = true, Impact = attackLife };
                        attacked = true;
                    }
                    yield return new Step { Room = notice.Room, Frame = frame,
                        Impact = attacked && evt.Kind == Core.DuelEventKind.Damaged && evt.AttackerViewCardId.Length != 0 ? attackLife : null };
                }
                yield return new Step { Room = notice.Room };
            }
        }
        void Rebuild()
        {
            Current = m_projection.View(m_display, m_room, m_animating, true, m_room.RemainingSeconds.Select(s => (float)s).ToArray(),
                DuelSelectionView.Empty, PendingActionView.Empty);
            if (!Current.Cards.Any(c => c.InstanceId == DeclaredAttacker)) DeclaredAttacker = DeclaredTarget = 0;
            else if (DeclaredTarget != 0 && !Current.Cards.Any(c => c.InstanceId == DeclaredTarget)) DeclaredTarget = 0;
        }
        void Notify(DuelChangeKind kind) => Changed(new DuelViewChange(kind, Current));
        public DuelCardSpec DefinitionForInstance(int id) => Current.Card(id).Definition;
        public void Start() { m_started = true; Advance(); }
        public void TogglePause()
        {
            Paused = !Paused;
            Notify(DuelChangeKind.Timer);
            if (!Paused && m_completed && m_delay <= 0) Advance();
        }
        public void SwitchView()
        {
            m_viewer = 1 - m_viewer; m_projection = new DuelNetworkProjection(m_viewer);
            // 重建句柄别名，保持跨区域移动在切换后的轨道中仍对应同一显示实例。
            for (var i = 0; i <= m_cursor; i++)
                if (m_tracks[m_viewer][i].Frame != null) m_projection.Alias(m_tracks[m_viewer][i].Frame.Event);
            var step = m_cursor < 0 ? null : m_tracks[m_viewer][m_cursor];
            m_room = step == null ? Initial(m_viewer) : step.Room;
            m_display = step?.Frame?.Snapshot ?? m_room.Duel;
            DeclaredAttacker = DeclaredTarget = 0; m_animating = false; m_completed = true;
            m_delay = .15f; Rebuild(); Notify(DuelChangeKind.Reset);
        }
        public void Tick(float delta)
        {
            if (!m_started || Paused || !m_completed || m_cursor >= m_tracks[m_viewer].Length - 1) return;
            m_delay -= delta;
            if (m_delay <= 0) Advance();
        }
        public void FinishPresentation()
        {
            m_completed = true;
            if (Paused)
            { m_animating = false; Rebuild(); Notify(DuelChangeKind.State); return; }
            Advance();
        }
        void Advance()
        {
            if (Paused) return;
            if (m_cursor + 1 >= m_tracks[m_viewer].Length)
            { m_animating = false; m_completed = true; Rebuild(); Notify(DuelChangeKind.AnimationCompleted); return; }
            var previous = Current; var step = m_tracks[m_viewer][++m_cursor];
            m_room = step.Room; m_completed = false;
            if (step.Frame == null)
            {
                m_display = m_room.Duel; m_animating = false; m_completed = true; m_delay = .65f;
                Rebuild(); Notify(DuelChangeKind.AnimationCompleted); return;
            }
            m_projection.Alias(step.Frame.Event); m_display = step.Frame.Snapshot; m_animating = true; Rebuild();
            var evt = step.Frame.Event;
            if (evt.Kind == Core.DuelEventKind.AttackDeclared)
            { DeclaredAttacker = m_projection.Handle(evt.AttackerViewCardId); DeclaredTarget = m_projection.Handle(evt.TargetViewCardId); }
            if (evt.Kind is Core.DuelEventKind.TurnChanged or Core.DuelEventKind.PhaseChanged) DeclaredAttacker = DeclaredTarget = 0;
            if (step.Attack)
            {
                Current = WithLife(Current, previous.Players.Select(p => p.LP).ToArray());
                Changed(new DuelViewChange(DuelChangeKind.Attack, Current, m_projection.Handle(evt.AttackerViewCardId))
                {
                    AttackerId = m_projection.Handle(evt.AttackerViewCardId), TargetId = m_projection.Handle(evt.TargetViewCardId),
                    ImpactLifePoints = new[] { step.Impact[m_viewer], step.Impact[1 - m_viewer] }
                });
            }
            else
            {
                if (step.Impact != null) Current = WithLife(Current, new[] { step.Impact[m_viewer], step.Impact[1 - m_viewer] });
                Changed(m_projection.Change(step.Frame, Current, previous));
            }
        }
        static DuelView WithLife(DuelView view, IReadOnlyList<int> life) => new(view.Cards, view.AvailablePiles, view.ActivePlayer,
            view.Turn, view.Phase, view.Animating, view.Placement, view.Seconds.ToArray(), view.TimerPaused,
            view.AvailablePhases, view.PendingAction, view.Players.Select((p, i) => new DuelPlayerView(p.Name, p.AvatarId, life[i], p.AvatarFrameId)))
        { ViewingSeat = view.ViewingSeat, ReadOnly = true, Finished = view.Finished, Outcome = view.Outcome };
        public void PresentImpact(IReadOnlyList<int> life) { Current = WithLife(Current, life); Notify(DuelChangeKind.Timer); }
        public UniTask<ZonePreviewCard[]> PreviewZoneAsync(ZoneRef zone, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if (zone.Kind == DuelZone.MainDeck && zone.Player == 0)
                return UniTask.FromResult(m_display.MainDeckDefinitions.OrderBy(id => id, StringComparer.Ordinal).Select(id => new ZonePreviewCard(0, id, true)).ToArray());
            return UniTask.FromResult(Current.InZone(zone).Select(c => new ZonePreviewCard(c.InstanceId, c.Definition.CardId, c.Known)).ToArray());
        }
        public void Submit(DuelInputCommand command)
        { if (command is AnimationCompleted) FinishPresentation(); }
    }
}
