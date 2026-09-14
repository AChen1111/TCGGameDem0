using System;

/// <summary>商城商品剩余时间文案.</summary>
public static class ShopRemainingTime
{
    public static string Format(DateTimeOffset? endsAt) => Message(endsAt)?.ToString() ?? string.Empty;

    public static void Apply(LocalizedText text, DateTimeOffset? endsAt)
    {
        LocalizedMessage message = Message(endsAt);
        if (message == null) text.Clear();
        else text.SetMessage(message);
    }

    static LocalizedMessage Message(DateTimeOffset? endsAt)
    {
        if (!endsAt.HasValue)
        {
            return null;
        }

        TimeSpan remaining = endsAt.Value - AChen.Networking.GameConfigManager.Instance.Store.ServerNow;
        if (remaining <= TimeSpan.Zero)
        {
            return new LocalizedMessage("ui.shop.remaining_ended");
        }

        if (remaining.TotalDays >= 1)
        {
            return Count("ui.shop.remaining_days", Math.Ceiling(remaining.TotalDays));
        }

        if (remaining.TotalHours >= 1)
        {
            return Count("ui.shop.remaining_hours", Math.Ceiling(remaining.TotalHours));
        }

        return Count("ui.shop.remaining_minutes", Math.Max(1, Math.Ceiling(remaining.TotalMinutes)));
    }

    static LocalizedMessage Count(string key, double count) => new LocalizedMessage(key,
        new System.Collections.Generic.Dictionary<string, object> { ["count"] = count });
}
