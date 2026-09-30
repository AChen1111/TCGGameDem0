using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace AChen.Configuration
{
    [Serializable] public sealed class ActivityMasterRow
    {
        public string ActivityId = "";
        public int Type;
        public string DetailTable = "";
        public bool IsEnabled;
        public int ScheduleMode;
        public DateTimeOffset? StartsAt, EndsAt;
        public string NameKey = "", Description = "", DescriptionKey = "";
        public int SortOrder, DisplayMode;
        public string BannerResourceKey = "";
        public bool ShowLocked = true, HideWhenCompleted;
        public string PopupTrigger = "lobbyReady", PopupFrequency = "oncePerActivity";
        public int PopupPriority;
        public long PopupPolicyVersion = 1;
        public bool StopWhenCompleted = true;
        public string[] ConditionTypes = Array.Empty<string>();
        public long[] ConditionValues = Array.Empty<long>();
        public string[] ConditionActivityIds = Array.Empty<string>(), ConditionTimes = Array.Empty<string>();

        public bool IsOpen(DateTimeOffset now) => IsEnabled && (ScheduleMode == 0 || now >= StartsAt && now < EndsAt);
        public ActivityDefinition Definition(long version) => new ActivityDefinition
        {
            Id = ActivityId, Type = (ActivityType)Type, ScheduleMode = (ActivityScheduleMode)ScheduleMode,
            StartsAt = StartsAt, EndsAt = EndsAt, NameKey = NameKey, Description = Description, DescriptionKey = DescriptionKey,
            SortOrder = SortOrder, DisplayMode = (ActivityDisplayMode)DisplayMode, BannerResourceKey = BannerResourceKey,
            ShowLocked = ShowLocked, HideWhenCompleted = HideWhenCompleted, DefinitionVersion = version,
            Popup = new ActivityPopupDefinition { Trigger = PopupTrigger, Frequency = PopupFrequency, Priority = PopupPriority,
                PolicyVersion = PopupPolicyVersion, StopWhenCompleted = StopWhenCompleted },
            OpenConditions = new ActivityConditions { AllOf = ConditionTypes.Select((type, i) => new ActivityCondition
            { Type = type, Value = ConditionValues[i], ActivityId = ConditionActivityIds[i],
                Time = ConditionTimes[i].Length == 0 ? (DateTimeOffset?)null : ActivityCsvConfiguration.Date(ConditionTimes[i]) }).ToList() }
        };
    }
    public class ActivityRewardRow
    {
        public string EntryId = "", NameKey = "", Description = "";
        public int PeriodKind, LimitPerPeriod = 1;
        public int? TotalLimit = 1;
        public int SortOrder;
        public int[] RewardTypes = Array.Empty<int>();
        public string[] RewardIds = Array.Empty<string>();
        public long[] Amounts = Array.Empty<long>();
        public int[] CardVariants = Array.Empty<int>();
    }
    public sealed class ActivityGiftRow : ActivityRewardRow { }
    public sealed class ActivityExchangeRow : ActivityRewardRow { public long CostGold; }
    public sealed class ActivitySignInRow : ActivityRewardRow { public int DayIndex; public bool AllowCatchUpClaims; }
    public sealed class ActivityMilestoneRow : ActivityRewardRow { public long Threshold; public int[] PackIds = Array.Empty<int>(); }
    public sealed class ActivityNoticeRow
    {
        public string Content = "", ImageResourceKey = "", ActionKind = "close", ActionTarget = "";
    }
    [Serializable] public sealed class ActivityFileInfo
    {
        public string Table = "", Sha256 = "", Url = "";
        public long Size;
    }
    [Serializable] public sealed class ActivityPackageManifest
    {
        public int SchemaVersion = 2;
        public string ReleaseId = "", SourceHash = "", ReferenceTarget = "Editor", ReferenceConfigHash = "";
        public long ExpectedRevision;
        public List<ActivityFileInfo> Files = new List<ActivityFileInfo>();
    }
    [Serializable] public sealed class ActivityIndexItem
    {
        public ActivityMasterRow Master = new ActivityMasterRow();
        public long DefinitionVersion;
        public string Status = "", LockedReasonCode = "";
        public bool Eligible, ShouldShow;
        public ActivityCondition LockedCondition = new ActivityCondition();
        public ActivityPlayerState PlayerState = new ActivityPlayerState();
    }
    [Serializable] public sealed class ActivityIndexResponse
    {
        public int SchemaVersion = 2;
        public string ReleaseId = "";
        public long DefinitionsRevision, PlayerStateRevision;
        public DateTimeOffset ServerTime, NextResetAt;
        public string ServerDay = "";
        public List<ActivityIndexItem> Activities = new List<ActivityIndexItem>();
        public List<ActivityFileInfo> Files = new List<ActivityFileInfo>();
    }
    public static class ActivityCsvConfiguration
    {
        public const string Label = "ActivityConfig", GeneratedLabel = "ActivityConfig.Generated";
        public static string Hash(byte[] bytes)
        { using (var sha = SHA256.Create()) return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", "").ToLowerInvariant(); }
        public static bool ValidName(string name) => Regex.IsMatch(name, "^[a-zA-Z0-9_-]{1,96}$");
        public static DateTimeOffset Date(string text)
        {
            if (!Regex.IsMatch(text, @"^\d{4}-\d{2}-\d{2}T.*(Z|[+-]\d{2}:\d{2})$") ||
                !DateTimeOffset.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                throw new FormatException("时间必须包含 ISO 8601 时区: " + text);
            return date;
        }
        static void Check(bool valid, string table, string field, string text)
        { if (!valid) throw new FormatException(table + ".csv / " + field + ": " + text); }
        static T[] Read<T>(string name, byte[] bytes) where T : new()
        {
            try
            {
                var table = BinaryTable.Decode(bytes);
                Check(table.Names.Length == typeof(T).GetFields().Length, name, "字段", "须使用玩法的固定结构");
                return GameConfigTables.Map<T>(table);
            }
            catch (Exception error) { throw new FormatException(name + ".csv: " + error.Message, error); }
        }
        public static ActivityMasterRow[] Master(byte[] bytes)
        {
            var rows = Read<ActivityMasterRow>("activities", bytes);
            Check(rows.Select(x => x.ActivityId).Distinct(StringComparer.Ordinal).Count() == rows.Length, "activities", "ActivityId", "ID 重复");
            Check(rows.Select(x => x.DetailTable).Distinct(StringComparer.OrdinalIgnoreCase).Count() == rows.Length, "activities", "DetailTable", "每个活动须引用独立子表");
            foreach (var r in rows)
            {
                string field = "ActivityId=" + r.ActivityId;
                Check(ValidName(r.ActivityId) && ValidName(r.DetailTable) && r.DetailTable != "activities", "activities", field + "/DetailTable", "ID 或无扩展名表名无效");
                Check(r.Type >= 0 && r.Type <= 4 && r.ScheduleMode >= 0 && r.ScheduleMode <= 1 && r.DisplayMode >= 0 && r.DisplayMode <= 2,
                    "activities", field + "/Type,ScheduleMode,DisplayMode", "枚举无效");
                Check(r.ScheduleMode == 1 ? r.StartsAt.HasValue && r.EndsAt.HasValue && r.StartsAt < r.EndsAt : !r.StartsAt.HasValue && !r.EndsAt.HasValue,
                    "activities", field + "/StartsAt,EndsAt", "限时活动须有有效起止时间，长期活动留空");
                Check(r.NameKey.Length > 0 && (r.Type == 0 || r.DisplayMode != 1), "activities", field + "/NameKey,DisplayMode", "名称不能为空，奖励活动须保留页面入口");
                Check(r.PopupPolicyVersion > 0 && (r.PopupTrigger == "lobbyReady" || r.PopupTrigger == "rewardClaimable") &&
                    (r.PopupFrequency == "oncePerActivity" || r.PopupFrequency == "oncePerDay"), "activities", field + "/Popup", "提醒策略无效");
                Check(r.ConditionTypes.Length == r.ConditionValues.Length && r.ConditionTypes.Length == r.ConditionActivityIds.Length && r.ConditionTypes.Length == r.ConditionTimes.Length,
                    "activities", field + "/ConditionTypes,ConditionValues,ConditionActivityIds,ConditionTimes", "条件数组必须等长");
                foreach (var c in r.Definition(0).OpenConditions.AllOf)
                    Check(c.Type == "ownedCardKindsAtLeast" && c.Value >= 0 || c.Type == "activityCompleted" && c.ActivityId != r.ActivityId && rows.Any(x => x.ActivityId == c.ActivityId) ||
                        (c.Type == "playerCreatedBefore" || c.Type == "playerCreatedAfter") && c.Time.HasValue,
                        "activities", field + "/ConditionTypes", "开放条件或关联活动无效");
            }
            return rows;
        }
        public static ActivityDefinition Detail(ActivityMasterRow master, byte[] bytes, long version)
        {
            string name = master.DetailTable;
            var d = master.Definition(version);
            ActivityRewardRow[] rows;
            switch (d.Type)
            {
                case ActivityType.Notice:
                    var notices = Read<ActivityNoticeRow>(name, bytes);
                    Check(notices.Length == 1, name, "Content", "公告必须只有一行");
                    var n = notices[0];
                    Check(n.Content.Length > 0 && (n.ActionKind == "close" || n.ActionKind == "activity"), name, "Content,ActionKind", "公告内容或操作无效");
                    d.Notice = new ActivityNoticeDefinition { Content = n.Content, ImageResourceKey = n.ImageResourceKey, ActionKind = n.ActionKind, ActionTarget = n.ActionTarget };
                    return d;
                case ActivityType.Gift: rows = Read<ActivityGiftRow>(name, bytes); break;
                case ActivityType.Exchange: rows = Read<ActivityExchangeRow>(name, bytes); break;
                case ActivityType.SignIn:
                    var sign = Read<ActivitySignInRow>(name, bytes); rows = sign;
                    Check(sign.Length > 0 && sign.Select(x => x.DayIndex).OrderBy(x => x).SequenceEqual(Enumerable.Range(1, sign.Length)), name, "DayIndex", "签到日须从 1 连续配置");
                    Check(sign.Select(x => x.AllowCatchUpClaims).Distinct().Count() == 1, name, "AllowCatchUpClaims", "各行补领开关须一致");
                    d.RequiredDays = sign.Length; d.AllowCatchUpClaims = sign[0].AllowCatchUpClaims; break;
                case ActivityType.Milestone:
                    var mile = Read<ActivityMilestoneRow>(name, bytes); rows = mile;
                    Check(mile.Length > 0 && mile[0].PackIds.Length > 0 && mile.All(x => x.PackIds.SequenceEqual(mile[0].PackIds)), name, "PackIds", "参与卡包数组须非空且各行一致");
                    Check(mile.All(x => x.Threshold > 0) && mile.Select(x => x.Threshold).Distinct().Count() == mile.Length, name, "Threshold", "阈值须为正数且唯一");
                    d.PackIds = mile[0].PackIds.ToList(); break;
                default: throw new FormatException(name + ": 未知玩法");
            }
            Check(rows.Length > 0 && rows.Select(x => x.EntryId).Distinct().Count() == rows.Length, name, "EntryId", "档位缺失或重复");
            foreach (var r in rows)
            {
                Check(ValidName(r.EntryId) && r.NameKey.Length > 0, name, "EntryId,NameKey", "档位 ID 或名称无效");
                Check(r.PeriodKind >= 0 && r.PeriodKind <= 1 && r.LimitPerPeriod > 0 && (!r.TotalLimit.HasValue || r.TotalLimit > 0), name, r.EntryId + "/PeriodKind,LimitPerPeriod,TotalLimit", "周期或次数上限无效");
                Check(d.Type != ActivityType.SignIn && d.Type != ActivityType.Milestone || r.PeriodKind == 0 && r.LimitPerPeriod == 1 && r.TotalLimit == 1, name, r.EntryId + "/次数", "签到和里程碑档位全活动限领一次");
                Check(r.RewardTypes.Length > 0 && r.RewardTypes.Length == r.RewardIds.Length && r.RewardTypes.Length == r.Amounts.Length && r.RewardTypes.Length == r.CardVariants.Length,
                    name, r.EntryId + "/RewardTypes,RewardIds,Amounts,CardVariants", "奖励数组必须非空且等长");
                var entry = new ActivityEntryDefinition { Id = r.EntryId, NameKey = r.NameKey, Description = r.Description, PeriodKind = (ActivityPeriodKind)r.PeriodKind,
                    LimitPerPeriod = r.LimitPerPeriod, TotalLimit = r.TotalLimit, SortOrder = r.SortOrder };
                if (r is ActivityExchangeRow exchange) { Check(exchange.CostGold > 0, name, r.EntryId + "/CostGold", "兑换金币成本须为正数"); entry.CostGold = exchange.CostGold; }
                if (r is ActivitySignInRow sign) entry.DayIndex = sign.DayIndex;
                if (r is ActivityMilestoneRow mile) entry.Threshold = mile.Threshold;
                for (int i = 0; i < r.RewardTypes.Length; i++)
                {
                    Check((r.RewardTypes[i] == 1 && r.RewardIds[i] == "gold" && r.CardVariants[i] == 0 ||
                        r.RewardTypes[i] == 2 && r.RewardIds[i].Length > 0 && r.Amounts[i] <= int.MaxValue && r.CardVariants[i] >= 0 && r.CardVariants[i] <= 4) && r.Amounts[i] > 0,
                        name, r.EntryId + "/奖励[" + i + "]", "奖励类型、ID、数量或卡牌版本无效");
                    entry.Rewards.Add(new ActivityReward { RewardType = (ActivityRewardType)r.RewardTypes[i], RewardId = r.RewardIds[i], Amount = r.Amounts[i], CardVariant = r.CardVariants[i] });
                }
                try { _ = entry.Rewards.Where(x => x.RewardType == ActivityRewardType.Gold).Sum(x => x.Amount); }
                catch (OverflowException) { throw new FormatException(name + ".csv / Amounts: 金币总和溢出"); }
                d.Entries.Add(entry);
            }
            return d;
        }
        public static Dictionary<string, ActivityDefinition> Package(IReadOnlyDictionary<string, byte[]> files)
        {
            if (!files.ContainsKey("activities")) throw new FormatException("缺少 activities.csv 总表");
            var master = Master(files["activities"]);
            Check(files.Count == master.Length + 1, "activities", "DetailTable", "发布包须仅含总表及全部引用子表");
            var result = new Dictionary<string, ActivityDefinition>();
            foreach (var row in master)
            {
                Check(files.ContainsKey(row.DetailTable), "activities", row.ActivityId + "/DetailTable", "找不到 " + row.DetailTable + ".bytes");
                result.Add(row.ActivityId, Detail(row, files[row.DetailTable], 0));
            }
            foreach (var d in result.Values)
            {
                Check(d.Notice.ActionKind != "activity" || result.ContainsKey(d.Notice.ActionTarget), master.Single(x => x.ActivityId == d.Id).DetailTable, "ActionTarget", "跳转活动不存在");
                Visit(d, new HashSet<string>());
            }
            void Visit(ActivityDefinition node, HashSet<string> path)
            {
                Check(path.Add(node.Id), "activities", node.Id + "/ConditionActivityIds", "前置活动存在循环");
                foreach (var c in node.OpenConditions.AllOf.Where(x => x.Type == "activityCompleted"))
                {
                    var child = result[c.ActivityId];
                    Check(child.Type != ActivityType.Notice && child.Entries.All(x => x.TotalLimit.HasValue), "activities", node.Id + "/ConditionActivityIds", "前置活动必须可永久完成");
                    Visit(child, new HashSet<string>(path));
                }
            }
            return result;
        }
        public static void Resources(ActivityDefinition d, PublishedGameConfig config)
        {
            var texts = Table.TranslationRow.LoadBytes(config.TranslationTable).ToDictionary(x => x.Key);
            var resources = config.Extra.TryGetValue("activity-resources", out var data) ? data.Select(x => (string)x["ResourceKey"]).ToArray() : Array.Empty<string>();
            void Text(string key) => Check(texts.TryGetValue(key, out var t) && t.Chinese.Length > 0 && t.English.Length > 0, d.Id, "NameKey/DescriptionKey", "文案尚未准备: " + key);
            void Image(string key) => Check(key.Length == 0 || resources.Contains(key), d.Id, "图片", "图片尚未准备: " + key);
            Text(d.NameKey); if (d.DescriptionKey.Length > 0) Text(d.DescriptionKey);
            Image(d.BannerResourceKey); Image(d.Notice.ImageResourceKey);
            foreach (var e in d.Entries)
            {
                Text(e.NameKey);
                foreach (var reward in e.Rewards.Where(x => x.RewardType == ActivityRewardType.Card))
                    Check(config.AllCards.Any(x => x.CardId == config.ResolveCardId(reward.RewardId)), d.Id, e.Id + "/RewardIds", "卡牌尚未准备: " + reward.RewardId);
            }
            Check(d.PackIds.All(id => config.Catalog.CardPacks.Any(p => p.Id == id)), d.Id, "PackIds", "参与卡包不存在");
        }
        // 首次发布便固定业务身份；奖励、成本与排期不参与这个签名。
        public static string Identity(ActivityDefinition d) => string.Join("|", new[] { ((int)d.Type).ToString(), d.AllowCatchUpClaims.ToString(),
            string.Join(",", d.PackIds.OrderBy(x => x)), string.Join(";", d.Entries.OrderBy(x => x.Id, StringComparer.Ordinal)
                .Select(e => string.Join(",", e.Id, e.DayIndex, e.Threshold, (int)e.PeriodKind, e.LimitPerPeriod, e.TotalLimit))) });
    }
}
