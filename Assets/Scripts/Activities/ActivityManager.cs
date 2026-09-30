using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using AChen.Configuration;
using AChen.Networking;
using AChen.Player;
using Cysharp.Threading.Tasks;

namespace AChen.Activities
{
    public abstract class Activity : IDisposable
    {
        public ActivitySnapshot Snapshot { get; private set; }
        public void Apply(ActivitySnapshot snapshot) => Snapshot = snapshot;
        public virtual void Dispose() { }
    }
    public sealed class NoticeActivity : Activity { }
    public sealed class GiftActivity : Activity { }
    public sealed class SignInActivity : Activity { }
    public sealed class ExchangeActivity : Activity { }
    public sealed class MilestoneActivity : Activity { }

    public sealed class ActivityClock
    {
        DateTimeOffset m_server;
        long m_received;
        public DateTimeOffset Now => m_server.AddSeconds((Stopwatch.GetTimestamp() - m_received) / (double)Stopwatch.Frequency);
        public void Synchronize(DateTimeOffset server) { m_server = server; m_received = Stopwatch.GetTimestamp(); }
    }
    public sealed class ActivityManager
    {
        readonly PlayerSession m_session;
        readonly SemaphoreSlim m_sync = new SemaphoreSlim(1, 1);
        readonly Dictionary<string, ActivityClaimRequest> m_requests = new Dictionary<string, ActivityClaimRequest>();
        readonly Dictionary<string, (string id, ActivityPopupShownRequest request)> m_receipts = new Dictionary<string, (string, ActivityPopupShownRequest)>();
        Dictionary<string, Activity> m_items = new Dictionary<string, Activity>();
        ActivityConfiguration m_configuration;
        bool m_watching;
        long m_version = -1;
        CancellationTokenSource m_lifetime = new CancellationTokenSource();
        public readonly ActivityClock Clock = new ActivityClock();
        public ActivityListResponse Snapshot { get; private set; } = new ActivityListResponse();
        public IEnumerable<ActivitySnapshot> Items => m_items.Values.Select(x => x.Snapshot);
        public bool IsReady { get; private set; }
        public bool IsStale { get; private set; } = true;
        public bool IsBusy { get; private set; }
        public bool IsLoading { get; private set; }
        public event Action Changed;
        public ActivityManager(PlayerSession session) { m_session = session; m_configuration = new ActivityConfiguration(session); }

        public void Reset()
        {
            m_lifetime.Cancel(); m_lifetime.Dispose(); m_lifetime = new CancellationTokenSource();
            m_configuration = new ActivityConfiguration(m_session); m_watching = false; SceneTransitionOverlay.EndActivities();
            foreach (var item in m_items.Values) item.Dispose();
            m_items.Clear(); m_requests.Clear(); m_receipts.Clear(); Snapshot = new ActivityListResponse();
            IsReady = false; IsStale = true; IsBusy = false; IsLoading = false; m_version = m_session.SessionVersion;
            Changed?.Invoke();
        }
        public async UniTask RefreshAsync(bool visit, CancellationToken token)
        {
            if (m_version != m_session.SessionVersion) Reset();
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(token, m_lifetime.Token);
            var ct = linked.Token;
            await m_sync.WaitAsync(ct);
            long version = m_session.SessionVersion;
            IsLoading = true; Changed?.Invoke();
            try
            {
                while (true)
                {
                    try
                    {
                        await FlushReceiptsAsync(ct);
                        var index = await m_session.LoadActivitiesAsync(visit, ct);
                        long received = Stopwatch.GetTimestamp();
                        ct.ThrowIfCancellationRequested();
                        if (version != m_session.SessionVersion) throw new OperationCanceledException();
                        bool gate = !IsReady || IsStale || m_configuration.RequiresLoading(index);
                        if (gate) { IsStale = true; SceneTransitionOverlay.ActivityProgress("正在准备活动配置", 0); Changed?.Invoke(); }
                        var response = await m_configuration.PrepareAsync(index,
                            (text, value) => { if (gate) SceneTransitionOverlay.ActivityProgress(text, value); }, ct);
                        ct.ThrowIfCancellationRequested();
                        var preparedTime = index.ServerTime.AddSeconds((Stopwatch.GetTimestamp() - received) / (double)Stopwatch.Frequency);
                        if (index.Activities.Any(x => x.Master.IsOpen(preparedTime) != x.Master.IsOpen(index.ServerTime)) || preparedTime >= index.NextResetAt)
                            continue; // 下载跨过排期或日切边界，重新读取状态后再安装。
                        Install(response); Clock.Synchronize(preparedTime); m_configuration.Commit(index); SceneTransitionOverlay.EndActivities();
                        return;
                    }
                    catch (OperationCanceledException) { throw; }
                    catch (Exception error)
                    {
                        IsStale = true; Changed?.Invoke();
                        if (ContentSession.RestartRequired)
                        {
                            SceneTransitionOverlay.EndActivities(); ContentUpdatePrompt.ShowRestart();
                            await UniTask.WaitUntil(() => false, cancellationToken: ct);
                        }
                        await SceneTransitionOverlay.ActivityRetryAsync(error.Message, ct);
                    }
                }
            }
            catch (OperationCanceledException) { throw; }
            catch { IsStale = true; Changed?.Invoke(); throw; }
            finally { IsLoading = false; m_sync.Release(); Changed?.Invoke(); }
        }
        public async UniTask WaitUntilReadyAsync(CancellationToken token)
        {
            await RefreshAsync(false, token);
            if (!m_watching) { m_watching = true; WatchAsync(m_lifetime.Token).Forget(); }
        }
        async UniTaskVoid WatchAsync(CancellationToken token)
        {
            try
            {
                float refresh = UnityEngine.Time.realtimeSinceStartup + 60;
                while (true)
                {
                    await UniTask.Delay(250, ignoreTimeScale: true, cancellationToken: token);
                    var now = Clock.Now;
                    bool boundary = m_configuration.Index.Activities.Any(x => x.Master.IsOpen(now) != x.Master.IsOpen(m_configuration.Index.ServerTime)) || now >= Snapshot.NextResetAt;
                    if (UnityEngine.Time.realtimeSinceStartup >= refresh || boundary)
                    {
                        if (boundary) { IsStale = true; SceneTransitionOverlay.ActivityProgress("正在更新活动配置", 0); Changed?.Invoke(); }
                        await RefreshAsync(false, token); refresh = UnityEngine.Time.realtimeSinceStartup + 60;
                    }
                }
            }
            catch (OperationCanceledException) { }
        }
        void Install(ActivityListResponse snapshot)
        {
            // A delayed response must not replace a newer complete snapshot.
            if (IsReady && snapshot.ServerTime < Snapshot.ServerTime) return;
            if (snapshot.SchemaVersion != 2 || snapshot.Activities.Select(x => x.Definition.Id).Distinct().Count() != snapshot.Activities.Count)
                throw new FormatException("活动快照协议无效");
            var next = new Dictionary<string, Activity>();
            foreach (var state in snapshot.Activities)
            {
                var d = state.Definition;
                if (d.NameKey.Length == 0 || d.Entries.Select(x => x.Id).Distinct().Count() != d.Entries.Count)
                    throw new FormatException("活动名称或领取项无效");
                Activity item;
                if (m_items.TryGetValue(d.Id, out var old))
                {
                    if (old.Snapshot.Definition.Type != d.Type) throw new FormatException("同一活动的类型发生变化");
                    item = old;
                }
                else item = d.Type switch
                { ActivityType.Notice => new NoticeActivity(), ActivityType.Gift => new GiftActivity(), ActivityType.SignIn => new SignInActivity(),
                    ActivityType.Exchange => new ExchangeActivity(), ActivityType.Milestone => new MilestoneActivity(), _ => throw new FormatException("未知活动类型") };
                next.Add(d.Id, item);
            }
            foreach (var item in m_items.Where(x => !next.ContainsKey(x.Key))) item.Value.Dispose();
            foreach (var state in snapshot.Activities) next[state.Definition.Id].Apply(state);
            m_items = next; Snapshot = snapshot; Clock.Synchronize(snapshot.ServerTime); IsReady = true; IsStale = false;
            Changed?.Invoke();
        }
        public bool TryGet(string id, out ActivitySnapshot state)
        {
            bool exists = m_items.TryGetValue(id, out var item); state = exists ? item.Snapshot : null; return exists;
        }
        public bool Running(ActivitySnapshot state) => state.Status == "running" && (state.Definition.ScheduleMode == ActivityScheduleMode.Permanent ||
            Clock.Now >= state.Definition.StartsAt && Clock.Now < state.Definition.EndsAt);
        public bool CanClaim(ActivitySnapshot state, string entry) => IsReady && !IsStale && !IsBusy && Clock.Now < Snapshot.NextResetAt && !ContentSession.RestartRequired && Running(state) &&
            state.Eligible && (state.PlayerState.EntryStates.Single(x => x.EntryId == entry).CanClaim || HasPendingRequest(state.Definition.Id, entry));
        public bool HasPendingRequest(string id, string entry) => m_requests.ContainsKey(id + "/" + entry);
        public IEnumerable<ActivitySnapshot> PageItems => Items.Where(x => x.Definition.DisplayMode != ActivityDisplayMode.Popup && (x.Eligible || x.Definition.ShowLocked) && (!x.PlayerState.Completed || !x.Definition.HideWhenCompleted))
            .OrderBy(x => x.Definition.SortOrder).ThenBy(x => x.Definition.Id, StringComparer.Ordinal);
        public int ClaimableCount => PageItems.Count(x => x.PlayerState.EntryStates.Any(e => CanClaim(x, e.EntryId)));
        public async UniTask<ActivityClaimResult> ClaimAsync(string id, string entryId, CancellationToken token)
        {
            var state = m_items[id].Snapshot;
            if (!CanClaim(state, entryId)) throw new InvalidOperationException("当前活动不可领取，请刷新");
            string operation = id + "/" + entryId;
            if (!m_requests.TryGetValue(operation, out var request))
                m_requests[operation] = request = new ActivityClaimRequest { EntryId = entryId, DefinitionVersion = state.Definition.DefinitionVersion,
                    ExpectedRevision = m_session.CurrentPlayer.Revision, PeriodKey = state.PlayerState.EntryStates.Single(x => x.EntryId == entryId).PeriodKey, RequestId = Guid.NewGuid().ToString("D") };
            IsBusy = true; Changed?.Invoke();
            try
            {
                var response = await m_session.ClaimActivityAsync(id, request, state.Definition.Type == ActivityType.Exchange, token);
                m_requests.Remove(operation);
                try { await RefreshAsync(false, token); }
                catch (OperationCanceledException) { throw; }
                catch (Exception error) { ALog.LogWarning("奖励已领取，活动刷新暂未完成: " + error.Message, ALogCategories.Net); }
                return response;
            }
            catch (BackendApiException exception)
            {
                // A transport timeout is unknown: retain its request for idempotent manual retry.
                if (exception.StatusCode > 0) m_requests.Remove(operation);
                IsStale = true;
                if (exception.Code == "ACTIVITY_VERSION_CHANGED") await RefreshAsync(false, token);
                throw;
            }
            catch { IsStale = true; throw; }
            finally { IsBusy = false; Changed?.Invoke(); }
        }
        public async UniTask ReportShownAsync(string id, ActivityPopupShownRequest request, CancellationToken token)
        {
            m_receipts[id + "/" + request.PolicyVersion + "/" + request.PeriodKey] = (id, request);
            await m_sync.WaitAsync(token);
            try { await FlushReceiptsAsync(token); }
            finally { m_sync.Release(); }
            await RefreshAsync(false, token);
        }
        async UniTask FlushReceiptsAsync(CancellationToken token)
        {
            foreach (var receipt in m_receipts.ToArray())
            {
                try { await m_session.ReportActivityPopupAsync(receipt.Value.id, receipt.Value.request, token); m_receipts.Remove(receipt.Key); }
                catch (OperationCanceledException) { throw; }
                catch (BackendApiException error) when (error.Code is "ACTIVITY_DISABLED" or "ACTIVITY_ENDED" or "ACTIVITY_VERSION_CHANGED" or "PERIOD_CHANGED") { m_receipts.Remove(receipt.Key); }
                catch (Exception error) { ALog.LogWarning("保留活动展示回执，下一次同步重试: " + error.Message, ALogCategories.Net); break; }
            }
        }
    }
}
