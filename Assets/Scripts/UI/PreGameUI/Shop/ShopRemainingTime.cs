using System;

/// <summary>商城商品剩余时间文案.</summary>
public static class ShopRemainingTime
{
    public static string Format(DateTimeOffset? endsAt)
    {
        if (!endsAt.HasValue)
        {
            return string.Empty;
        }

        TimeSpan remaining = endsAt.Value - AChen.Networking.GameConfigManager.Instance.Store.ServerNow;
        if (remaining <= TimeSpan.Zero)
        {
            return "已结束";
        }

        if (remaining.TotalDays >= 1)
        {
            return $"{Math.Ceiling(remaining.TotalDays)}天";
        }

        if (remaining.TotalHours >= 1)
        {
            return $"{Math.Ceiling(remaining.TotalHours)}小时";
        }

        return $"{Math.Max(1, Math.Ceiling(remaining.TotalMinutes))}分钟";
    }
}
