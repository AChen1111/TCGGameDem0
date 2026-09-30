using System;
using System.Collections.Generic;

namespace AChen.Configuration
{
    public enum ActivityType { Notice, Gift, SignIn, Exchange, Milestone }
    public enum ActivityDisplayMode { Page, Popup, PageAndPopup }
    public enum ActivityScheduleMode { Permanent, Timed }
    public enum ActivityPeriodKind { WholeActivity, Daily }
    public enum ActivityRewardType { Gold = 1, Card = 2 }

    [Serializable] public sealed class ActivityCondition
    {
        public string Type = "";
        public long Value;
        public string ActivityId = "";
        public DateTimeOffset? Time;
    }
    [Serializable] public sealed class ActivityConditions { public List<ActivityCondition> AllOf = new List<ActivityCondition>(); }
    [Serializable] public sealed class ActivityReward
    {
        public ActivityRewardType RewardType;
        public string RewardId = "gold";
        public long Amount;
        public int CardVariant;
    }
    [Serializable] public sealed class ActivityEntryDefinition
    {
        public string Id = "";
        public string NameKey = "";
        public string Description = "";
        public ActivityPeriodKind PeriodKind;
        public int LimitPerPeriod = 1;
        public int? TotalLimit = 1;
        public int SortOrder;
        public int DayIndex;
        public long Threshold;
        public long CostGold;
        public List<ActivityReward> Rewards = new List<ActivityReward>();
    }
    [Serializable] public sealed class ActivityNoticeDefinition
    {
        public string Content = "";
        public string ImageResourceKey = "";
        public string ActionKind = "close";
        public string ActionTarget = "";
    }
    [Serializable] public sealed class ActivityPopupDefinition
    {
        public string Trigger = "lobbyReady";
        public string Frequency = "oncePerActivity";
        public int Priority;
        public long PolicyVersion = 1;
        public bool StopWhenCompleted = true;
        public bool ShouldShow;
    }
    [Serializable] public sealed class ActivityDefinition
    {
        public string Id = "";
        public string NameKey = "";
        public string Description = "";
        public string DescriptionKey = "";
        public ActivityType Type;
        public ActivityScheduleMode ScheduleMode;
        public DateTimeOffset? StartsAt;
        public DateTimeOffset? EndsAt;
        public ActivityDisplayMode DisplayMode;
        public int SortOrder;
        public string BannerResourceKey = "";
        public bool ShowBeforeStart;
        public bool HideWhenCompleted;
        public bool ShowLocked = true;
        public long DefinitionVersion;
        public ActivityConditions OpenConditions = new ActivityConditions();
        public ActivityPopupDefinition Popup = new ActivityPopupDefinition();
        public ActivityNoticeDefinition Notice = new ActivityNoticeDefinition();
        public int RequiredDays;
        public bool AllowCatchUpClaims = true;
        public string MetricType = "gachaDrawCount";
        public List<int> PackIds = new List<int>();
        public List<ActivityEntryDefinition> Entries = new List<ActivityEntryDefinition>();
    }
    [Serializable] public sealed class ActivityEntryState
    {
        public string EntryId = "";
        public string PeriodKey = "all";
        public int ClaimedCount;
        public int TotalClaimedCount;
        public bool CanClaim;
        public string Status = "locked";
    }
    [Serializable] public sealed class ActivityPlayerState
    {
        public bool Completed;
        public long Progress;
        public List<ActivityEntryState> EntryStates = new List<ActivityEntryState>();
    }
    [Serializable] public sealed class ActivitySnapshot
    {
        public ActivityDefinition Definition = new ActivityDefinition();
        public string Status = "running";
        public bool Eligible;
        public string LockedReasonCode = "";
        public ActivityCondition LockedCondition = new ActivityCondition();
        public ActivityPlayerState PlayerState = new ActivityPlayerState();
    }
    [Serializable] public sealed class ActivityListResponse
    {
        public int SchemaVersion = 2;
        public long DefinitionsRevision;
        public long PlayerStateRevision;
        public DateTimeOffset ServerTime;
        public string ServerDay = "";
        public DateTimeOffset NextResetAt;
        public List<ActivitySnapshot> Activities = new List<ActivitySnapshot>();
    }
    [Serializable] public sealed class ActivityClaimRequest
    {
        public string EntryId = "";
        public long DefinitionVersion;
        public long ExpectedRevision;
        public string PeriodKey = "all";
        public string RequestId = "";
    }
    [Serializable] public sealed class ActivityPopupShownRequest
    {
        public long PolicyVersion;
        public string PeriodKey = "all";
    }
}
