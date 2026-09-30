using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using AChen.Configuration;
using AChen.Player;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AChen.Activities
{
    public sealed class ActivityConfiguration
    {
        readonly PlayerSession m_session;
        public ActivityIndexResponse Index { get; private set; } = new ActivityIndexResponse();
        public ActivityConfiguration(PlayerSession session) { m_session = session; }
        public static UniTask WaitUntilReadyAsync(CancellationToken token = default) => PlayerSession.Instance.Activities.WaitUntilReadyAsync(token);
        public bool RequiresLoading(ActivityIndexResponse next) => Index.ReleaseId != next.ReleaseId ||
            !Active(Index, Index.ServerTime).SequenceEqual(Active(next, next.ServerTime));
        static IEnumerable<string> Active(ActivityIndexResponse index, DateTimeOffset now) => index.Activities
            .Where(x => x.Master.IsOpen(now)).Select(x => x.Master.ActivityId).OrderBy(x => x, StringComparer.Ordinal);
        public async UniTask<ActivityListResponse> PrepareAsync(ActivityIndexResponse index, Action<string, float> progress, CancellationToken token)
        {
            if (index.SchemaVersion != 2 || index.Activities.Select(x => x.Master.ActivityId).Distinct().Count() != index.Activities.Count)
                throw new FormatException("活动总表协议无效，须使用版本 2");
            // 缓存的身份同时包括后端地址和服务端刚确认的文件哈希。
            string backend = ActivityCsvConfiguration.Hash(Encoding.UTF8.GetBytes(m_session.ActivityBackendAddress));
            string root = Path.Combine(Application.persistentDataPath, "ActivityConfiguration", backend);
            Directory.CreateDirectory(root);
            var opened = index.Activities.Where(x => x.Master.IsOpen(index.ServerTime)).ToArray();
            var snapshot = new ActivityListResponse { DefinitionsRevision = index.DefinitionsRevision, PlayerStateRevision = index.PlayerStateRevision,
                ServerTime = index.ServerTime, ServerDay = index.ServerDay, NextResetAt = index.NextResetAt };
            for (int i = 0; i < opened.Length; i++)
            {
                var item = opened[i];
                var file = index.Files.Single(x => x.Table == item.Master.DetailTable);
                if (file.Sha256.Length != 64 || !System.Text.RegularExpressions.Regex.IsMatch(file.Sha256, "^[0-9a-f]{64}$") ||
                    file.Url != "/api/activities/files/" + index.ReleaseId + "/" + file.Table + ".bytes") throw new FormatException(file.Table + ": 子表下载信息无效");
                progress("正在准备活动 " + item.Master.ActivityId, i / (float)opened.Length);
                string path = Path.Combine(root, file.Sha256 + ".bytes");
                byte[] bytes = File.Exists(path) ? File.ReadAllBytes(path) : await m_session.DownloadActivityAsync(file.Url, token);
                if (bytes.LongLength != file.Size || ActivityCsvConfiguration.Hash(bytes) != file.Sha256)
                { if (File.Exists(path)) File.Delete(path); throw new FormatException(file.Table + ".bytes: 文件大小或 SHA-256 不匹配，请重试"); }
                var d = ActivityCsvConfiguration.Detail(item.Master, bytes, item.DefinitionVersion);
                token.ThrowIfCancellationRequested();
                if (!File.Exists(path)) File.WriteAllBytes(path, bytes);
                d.Popup.ShouldShow = item.ShouldShow;
                snapshot.Activities.Add(new ActivitySnapshot { Definition = d, Status = item.Status, Eligible = item.Eligible,
                    LockedReasonCode = item.LockedReasonCode, LockedCondition = item.LockedCondition, PlayerState = item.PlayerState });
            }
            progress("活动配置已准备", 1);
            return snapshot;
        }
        public void Commit(ActivityIndexResponse index) => Index = index;
    }
}
