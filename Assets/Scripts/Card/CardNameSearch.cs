using System;

/// <summary>工坊与卡组编辑共用现有的本地化卡名包含匹配。</summary>
public static class CardNameSearch
{
    public static bool Matches(string cardId, string search) =>
        LocalizationService.GetText("card." + cardId + ".name")
            .IndexOf(search.Trim(), StringComparison.OrdinalIgnoreCase) >= 0;
}
