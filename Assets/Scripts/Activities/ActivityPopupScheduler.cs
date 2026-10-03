using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AChen.Configuration;
using AChen.Player;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AChen.Activities
{
    public sealed class ActivityPopupScheduler
    {
        // 实际自动展示过的活动，在同一次游戏进程内不再弹出；不随账号或大厅重建清除。
        static readonly HashSet<string> s_shownActivityIds = new HashSet<string>(StringComparer.Ordinal);
        static ActivityPopupScheduler() => Application.quitting += ResetShownActivities;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetShownActivities() => s_shownActivityIds.Clear();
        readonly ActivityManager m_manager;
        readonly UIFrame m_frame;
        readonly CancellationToken m_token;
        // 保留当前调度器的频率/策略键，与服务器展示回执使用相同规则。
        readonly HashSet<string> m_shownKeys = new HashSet<string>();
        string m_active;
        string m_navigation;
        public ActivityPopupScheduler(ActivityManager manager, UIFrame frame, CancellationToken token)
        { m_manager = manager; m_frame = frame; m_token = token; }
        public async UniTask RunAsync()
        {
            string visitedDay = "";
            while (!m_token.IsCancellationRequested)
            {
                // 调度器只负责实际大厅访问与弹窗，全局同步由会话管理器负责。
                if (m_manager.IsReady && !m_manager.IsStale && visitedDay != m_manager.Snapshot.ServerDay)
                {
                    try { await m_manager.RefreshAsync(true, m_token); visitedDay = m_manager.Snapshot.ServerDay; }
                    catch (OperationCanceledException) { break; }
                }
                if (!m_frame.IsWindowBusy && m_active == null)
                {
                    if (m_navigation != null)
                    {
                        var id = m_navigation; m_navigation = null;
                        m_frame.OpenWindow(AddressKeys.Prefab.ActivityWindow, new ActivityWindowProperties(id));
                    }
                    else if (m_manager.IsReady && !m_manager.IsStale && !m_manager.IsBusy)
                    {
                        var state = m_manager.Items.Where(IsCandidate).OrderByDescending(x => x.Definition.Popup.Priority)
                            .ThenBy(x => x.Definition.SortOrder).ThenBy(x => x.Definition.Id, StringComparer.Ordinal).FirstOrDefault();
                        if (state != null)
                        {
                            m_active = state.Definition.Id;
                            m_frame.OpenWindow(AddressKeys.Prefab.ActivityPopupWindow, new ActivityPopupProperties(state.Definition.Id,
                                state.Definition.Popup.PolicyVersion, Period(state), PlayerSession.Instance.SessionVersion, this));
                        }
                    }
                }
                await UniTask.Delay(TimeSpan.FromMilliseconds(250), ignoreTimeScale: true, cancellationToken: m_token);
            }
        }
        string Period(ActivitySnapshot state) => state.Definition.Popup.Frequency == "oncePerDay" ? m_manager.Snapshot.ServerDay : "all";
        string Key(ActivitySnapshot state) => state.Definition.Popup.Frequency == "oncePerLogin" ? state.Definition.Id :
            state.Definition.Id + "/" + state.Definition.Popup.PolicyVersion + "/" + Period(state);
        bool IsCandidate(ActivitySnapshot state) => m_manager.IsReady && !m_manager.IsStale && !m_manager.IsBusy &&
            m_manager.Clock.Now < m_manager.Snapshot.NextResetAt && !ContentSession.RestartRequired && state.Definition.Popup.ShouldShow && m_manager.Running(state) &&
            !s_shownActivityIds.Contains(state.Definition.Id) && !m_shownKeys.Contains(Key(state));
        public bool IsValid(ActivityPopupProperties context) => !m_token.IsCancellationRequested && context.SessionVersion == PlayerSession.Instance.SessionVersion &&
            m_manager.TryGet(context.ActivityId, out var state) && state.Definition.Popup.PolicyVersion == context.PolicyVersion && Period(state) == context.PeriodKey && IsCandidate(state);
        public void Visible(ActivityPopupProperties context)
        {
            m_manager.TryGet(context.ActivityId, out var state);
            if (!s_shownActivityIds.Add(context.ActivityId)) return;
            if (!m_shownKeys.Add(Key(state))) return;
            Report(context).Forget();
        }
        async UniTaskVoid Report(ActivityPopupProperties context)
        {
            try { await m_manager.ReportShownAsync(context.ActivityId, new ActivityPopupShownRequest { PolicyVersion = context.PolicyVersion, PeriodKey = context.PeriodKey }, m_token); }
            catch (OperationCanceledException) { }
            catch (Exception error) { ALog.LogWarning("活动展示回执暂未提交: " + error.Message, ALogCategories.Net); }
        }
        public void Finished(string navigation) { m_active = null; m_navigation = navigation; }
    }
}
