#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

[Serializable]
public sealed class EditorGameConfigDocument
{
    public EditorAvatarConfig[] avatars = Array.Empty<EditorAvatarConfig>();
    public EditorWallpaperConfig[] wallpapers = Array.Empty<EditorWallpaperConfig>();
    public EditorCardPackConfig[] cardPacks = Array.Empty<EditorCardPackConfig>();
}

[Serializable]
public sealed class EditorAvatarConfig
{
    public int id;
    public string name;
    public string resourceKey;
    public long priceGold;
    public string startsAt;
    public string endsAt;
    public int sortOrder;
    public bool isEnabled;
}

[Serializable]
public sealed class EditorWallpaperConfig
{
    public int id;
    public string name;
    public string resourceKey;
    public long priceGold;
    public string startsAt;
    public string endsAt;
    public int sortOrder;
    public bool isEnabled;
}

[Serializable]
public sealed class EditorCardPackConfig
{
    public int id;
    public string title;
    public string coverResourceKey;
    public long priceGold;
    public string startsAt;
    public string endsAt;
    public int sortOrder;
    public bool isEnabled;
}

public static class GameConfigCsvEditorParser
{
    static readonly string[] Header =
    {
        "Table", "Id", "Name", "ResourceKey", "PriceGold",
        "StartsAt", "EndsAt", "SortOrder", "IsEnabled"
    };

    public static EditorGameConfigDocument ParseFile(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("找不到游戏配置 CSV。", path);
        }

        return Parse(File.ReadAllText(path));
    }

    public static EditorGameConfigDocument Parse(string csv)
    {
        List<CsvRow> rows = ReadRows(csv);
        if (rows.Count == 0 || !rows[0].Fields.SequenceEqual(Header, StringComparer.OrdinalIgnoreCase))
        {
            throw new FormatException("CSV 表头无效，请使用游戏配置模板。");
        }

        var avatars = new List<EditorAvatarConfig>();
        var wallpapers = new List<EditorWallpaperConfig>();
        var cardPacks = new List<EditorCardPackConfig>();
        foreach (CsvRow row in rows.Skip(1))
        {
            if (row.Fields.All(string.IsNullOrWhiteSpace))
            {
                continue;
            }

            if (row.Fields.Length != Header.Length)
            {
                throw Error(row.Line, "字段数量不正确");
            }

            string table = row.Fields[0].Trim();
            bool catalogItem = table.Equals("Avatar", StringComparison.OrdinalIgnoreCase) ||
                table.Equals("Wallpaper", StringComparison.OrdinalIgnoreCase);
            int id = ParseInt(row.Fields[1], row.Line, "Id");
            if (id < 0 || !catalogItem && id == 0)
            {
                throw Error(row.Line, catalogItem ? "Id 不能为负数" : "卡包 Id 必须大于 0");
            }

            string name = RestoreSpreadsheetValue(row.Fields[2]).Trim();
            string resourceKey = RestoreSpreadsheetValue(row.Fields[3]).Trim();
            ValidateText(name, 64, row.Line, "Name");
            ValidateText(resourceKey, 128, row.Line, "ResourceKey");
            long priceGold = ParseLong(row.Fields[4], row.Line, "PriceGold");
            if (priceGold < 0)
            {
                throw Error(row.Line, "PriceGold 不能为负数");
            }

            int sortOrder = ParseInt(row.Fields[7], row.Line, "SortOrder");
            bool isEnabled = ParseBool(row.Fields[8], row.Line);
            if (table.Equals("Avatar", StringComparison.OrdinalIgnoreCase))
            {
                avatars.Add(new EditorAvatarConfig { id = id, name = name, resourceKey = resourceKey, priceGold = priceGold, startsAt = ParseDate(row.Fields[5], row.Line, "StartsAt"), endsAt = ParseDate(row.Fields[6], row.Line, "EndsAt"), sortOrder = sortOrder, isEnabled = isEnabled });
                ValidateDates(avatars[^1].startsAt, avatars[^1].endsAt, row.Line);
            }
            else if (table.Equals("Wallpaper", StringComparison.OrdinalIgnoreCase))
            {
                wallpapers.Add(new EditorWallpaperConfig { id = id, name = name, resourceKey = resourceKey, priceGold = priceGold, startsAt = ParseDate(row.Fields[5], row.Line, "StartsAt"), endsAt = ParseDate(row.Fields[6], row.Line, "EndsAt"), sortOrder = sortOrder, isEnabled = isEnabled });
                ValidateDates(wallpapers[^1].startsAt, wallpapers[^1].endsAt, row.Line);
            }
            else if (table.Equals("CardPack", StringComparison.OrdinalIgnoreCase))
            {
                cardPacks.Add(new EditorCardPackConfig
                {
                    id = id,
                    title = name,
                    coverResourceKey = resourceKey,
                    priceGold = priceGold,
                    startsAt = ParseDate(row.Fields[5], row.Line, "StartsAt"),
                    endsAt = ParseDate(row.Fields[6], row.Line, "EndsAt"),
                    sortOrder = sortOrder,
                    isEnabled = isEnabled
                });
                ValidateDates(cardPacks[^1].startsAt, cardPacks[^1].endsAt, row.Line);
            }
            else
            {
                throw Error(row.Line, "Table 只能是 Avatar、Wallpaper 或 CardPack");
            }
        }

        EnsureUnique(avatars.Select(value => value.id), "头像 ID");
        EnsureUnique(wallpapers.Select(value => value.id), "壁纸 ID");
        EnsureUnique(cardPacks.Select(value => value.id), "卡包 ID");
        EnsureUnique(avatars.Select(value => value.resourceKey), "头像 ResourceKey", StringComparer.OrdinalIgnoreCase);
        EnsureUnique(wallpapers.Select(value => value.resourceKey), "壁纸 ResourceKey", StringComparer.OrdinalIgnoreCase);
        return new EditorGameConfigDocument
        {
            avatars = avatars.OrderBy(value => value.sortOrder).ThenBy(value => value.id).ToArray(),
            wallpapers = wallpapers.OrderBy(value => value.sortOrder).ThenBy(value => value.id).ToArray(),
            cardPacks = cardPacks.OrderBy(value => value.sortOrder).ThenBy(value => value.id).ToArray()
        };
    }

    static List<CsvRow> ReadRows(string csv)
    {
        var rows = new List<CsvRow>();
        var fields = new List<string>();
        var field = new System.Text.StringBuilder();
        bool quoted = false;
        int line = 1;
        int rowLine = 1;
        for (int i = 0; i <= (csv ?? string.Empty).Length; i++)
        {
            char value = i < (csv ?? string.Empty).Length ? csv[i] : '\0';
            if (quoted)
            {
                if (value == '"' && i + 1 < csv.Length && csv[i + 1] == '"')
                {
                    field.Append('"');
                    i++;
                }
                else if (value == '"')
                {
                    quoted = false;
                }
                else if (value == '\0')
                {
                    throw Error(rowLine, "引号未闭合");
                }
                else
                {
                    field.Append(value);
                    if (value == '\n') line++;
                }
                continue;
            }

            if (value == '"' && field.Length == 0) quoted = true;
            else if (value == ',') { fields.Add(field.ToString()); field.Clear(); }
            else if (value == '\r' || value == '\n' || value == '\0')
            {
                fields.Add(field.ToString());
                field.Clear();
                if (fields.Count > 1 || fields[0].Length > 0)
                {
                    rows.Add(new CsvRow(rowLine, fields.ToArray()));
                }
                fields.Clear();
                if (value == '\r' && i + 1 < csv.Length && csv[i + 1] == '\n') i++;
                line++;
                rowLine = line;
            }
            else field.Append(value);
        }

        if (rows.Count > 0 && rows[0].Fields.Length > 0)
        {
            rows[0].Fields[0] = rows[0].Fields[0].TrimStart('\uFEFF');
        }
        return rows;
    }

    static void ValidateDates(string startsAt, string endsAt, int line)
    {
        if (string.IsNullOrEmpty(startsAt) || string.IsNullOrEmpty(endsAt)) return;
        if (DateTimeOffset.TryParse(startsAt, out DateTimeOffset start) &&
            DateTimeOffset.TryParse(endsAt, out DateTimeOffset end) && end <= start)
            throw Error(line, "EndsAt 必须晚于 StartsAt");
    }

    static int ParseInt(string value, int line, string field) =>
        int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed)
            ? parsed : throw Error(line, field + " 不是有效整数");

    static long ParseLong(string value, int line, string field) =>
        long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out long parsed)
            ? parsed : throw Error(line, field + " 不是有效整数");

    static bool ParseBool(string value, int line) =>
        bool.TryParse(value, out bool parsed) ? parsed : throw Error(line, "IsEnabled 必须是 True 或 False");

    static string ParseDate(string value, int line, string field)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        if (!DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTimeOffset parsed))
            throw Error(line, field + " 必须是 ISO-8601 时间");
        return parsed.ToString("O", CultureInfo.InvariantCulture);
    }

    static void ValidateText(string value, int maxLength, int line, string field)
    {
        if (value.Length == 0 || value.Length > maxLength || value.Any(char.IsControl))
            throw Error(line, field + $" 必须为 1-{maxLength} 个非控制字符");
    }

    static void EnsureUnique<T>(IEnumerable<T> values, string label, IEqualityComparer<T> comparer = null)
    {
        var set = new HashSet<T>(comparer ?? EqualityComparer<T>.Default);
        if (values.Any(value => !set.Add(value))) throw new FormatException(label + " 不能重复。");
    }

    static string RestoreSpreadsheetValue(string value) =>
        value.Length > 1 && value[0] == '\'' && "=+-@".Contains(value[1]) ? value.Substring(1) : value;

    static FormatException Error(int line, string message) => new FormatException($"CSV 第 {line} 行：{message}。");

    sealed class CsvRow
    {
        public int Line { get; }
        public string[] Fields { get; }
        public CsvRow(int line, string[] fields) { Line = line; Fields = fields; }
    }
}
#endif
