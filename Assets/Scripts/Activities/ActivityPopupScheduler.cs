using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AChen.Configuration;
using AChen.Player;
using Cysharp.Threading.Tasks;

namespace AChen.Activities
{
    public sealed class ActivityPopupScheduler
    {
        readonly ActivityManager m_manager;
        readonly UIFrame m_frame;
        readonly CancellationToken m_token;
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
                    else if (m_manager.IsReady && !m_manager.IsStale && !m_manager.IsBusy && m_manager.PopupSessionCount < 3)
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
        string Key(ActivitySnapshot state) => state.Definition.Id + "/" + state.Definition.Popup.PolicyVersion + "/" + Period(state);
        bool IsCandidate(ActivitySnapshot state) => m_manager.IsReady && !m_manager.IsStale && !m_manager.IsBusy &&
            m_manager.Clock.Now < m_manager.Snapshot.NextResetAt && !ContentSession.RestartRequired && state.Definition.Popup.ShouldShow && m_manager.Running(state) &&
            !m_manager.PopupShownKeys.Contains(Key(state));
        public bool IsValid(ActivityPopupProperties context) => !m_token.IsCancellationRequested && context.SessionVersion == PlayerSession.Instance.SessionVersion &&
            m_manager.TryGet(context.ActivityId, out var state) && state.Definition.Popup.PolicyVersion == context.PolicyVersion && Period(state) == context.PeriodKey && IsCandidate(state);
        public void Visible(ActivityPopupProperties context)
        {
            string key = context.ActivityId + "/" + context.PolicyVersion + "/" + context.PeriodKey;
            if (!m_manager.PopupShownKeys.Add(key)) return;
            m_manager.PopupSessionCount++;
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
