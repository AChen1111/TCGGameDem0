using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace AChen.Configuration
{
    public static class GameConfigTables
    {
        public const string Label = "GameConfig";
        public const string GeneratedLabel = "GameConfigGenerated";
        public static readonly string[] Required = { "avatar-frames", "avatars", "wallpapers", "card-packs", "pool-entries", "rarity-weights", "all-cards", "Cards", "Translations", "wallpaper-offsets" };
        public static bool IsBinary(string name) => true;
        public static string Format(string name) => "bytes";
        public static string FileName(string name) => name + "." + Format(name);
        public static bool ValidName(string name) => name != null && name.Length <= 100 && Regex.IsMatch(name, "^[A-Za-z][A-Za-z0-9_-]*$") && !string.Equals(name, "LocalizationSettings", StringComparison.OrdinalIgnoreCase);
        public static string Address(string category) => "GameConfig/" + category;
        public static string PackagePath(string category) => Address(category) + "." + Format(category);
        public static byte[] Encode(string category, BinaryTable table) => table.Encode();

        public static PublishedGameConfig Assemble(IReadOnlyDictionary<string, byte[]> files)
        {
            var tables = new Dictionary<string, BinaryTable>(StringComparer.Ordinal);
            var extra = new Dictionary<string, IReadOnlyList<IReadOnlyDictionary<string, object>>>(StringComparer.Ordinal);
            var unique = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var file in files)
            {
                if (!ValidName(file.Key) || !unique.Add(file.Key)) throw new FormatException("配置类别无效或重复: " + file.Key);
                try
                {
                    var table = BinaryTable.Decode(file.Value);
                    tables.Add(file.Key, table);
                    if (file.Key != "Cards" && file.Key != "Translations") extra.Add(file.Key, ToRows(table));
                }
                catch (Exception ex) when (ex is FormatException || ex is System.IO.IOException || ex is ArgumentException)
                { throw new FormatException("配置解析失败: " + file.Key + "; " + ex.Message, ex); }
            }
            foreach (string name in Required) if (!files.ContainsKey(name)) throw new FormatException("缺少必需配置: " + name);
            var data = new PublishedGameConfig
            {
                SchemaVersion = 1,
                Catalog = new CatalogData { Avatars = Map<CosmeticData>(tables["avatars"]), AvatarFrames = Map<AvatarFrameData>(tables["avatar-frames"]), Wallpapers = Map<CosmeticData>(tables["wallpapers"]), CardPacks = Map<PackData>(tables["card-packs"]) },
                PoolEntries = Map<PoolEntry>(tables["pool-entries"]), RarityWeights = Map<RarityWeight>(tables["rarity-weights"]),
                AllCards = Map<AllCardEntry>(tables["all-cards"]), WallpaperOffsets = Map<WallpaperOffset>(tables["wallpaper-offsets"]),
                CardTable = files["Cards"], TranslationTable = files["Translations"], Extra = extra
            };
            data.Validate(); return data;
        }

        static IReadOnlyList<IReadOnlyDictionary<string, object>> ToRows(BinaryTable table)
        {
            return table.Rows.Select(row =>
            {
                var values = new Dictionary<string, object>(StringComparer.Ordinal);
                for (int i = 0; i < table.Names.Length; i++) values[table.Names[i]] = row[i];
                return (IReadOnlyDictionary<string, object>)values;
            }).ToArray();
        }

        public static T[] Map<T>(BinaryTable table) where T : new()
        {
            var fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance);
            var columns = fields.Select(field => table.Column(field.Name, TypeName(field.FieldType))).ToArray();
            return table.Rows.Select(row =>
            {
                var result = new T();
                for (int i = 0; i < fields.Length; i++) fields[i].SetValue(result, ConvertValue(row[columns[i]], fields[i].FieldType));
                return result;
            }).ToArray();
        }

        public static BinaryTable FromRows<T>(IEnumerable<T> rows)
        {
            var fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance);
            return new BinaryTable
            {
                Names = fields.Select(f => f.Name).ToArray(), Types = fields.Select(f => TypeName(f.FieldType)).ToArray(),
                Rows = rows.Select(row => fields.Select(f =>
                {
                    object value = f.GetValue(row);
                    return value is DateTimeOffset date ? date.ToString("O", CultureInfo.InvariantCulture) : value;
                }).ToArray()).ToArray()
            };
        }

        static string TypeName(Type type)
        {
            if (type.IsArray) return TypeName(type.GetElementType()) + "[]";
            var underlying = Nullable.GetUnderlyingType(type);
            if (underlying != null) return TypeName(underlying) + "?";
            if (type == typeof(string) || type == typeof(DateTimeOffset)) return "string";
            if (type == typeof(int)) return "int";
            if (type == typeof(long)) return "long";
            if (type == typeof(float)) return "float";
            if (type == typeof(bool)) return "bool";
            throw new FormatException("未支持的配置字段类型: " + type.Name);
        }

        static object ConvertValue(object value, Type type)
        {
            if (value == null) return null;
            type = Nullable.GetUnderlyingType(type) ?? type;
            if (type == typeof(DateTimeOffset))
            {
                string text = value as string ?? throw new FormatException("日期必须是字符串");
                if (!Regex.IsMatch(text, @"^\d{4}-\d{2}-\d{2}T.*(Z|[+-]\d{2}:\d{2})$")
                    || !DateTimeOffset.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                    throw new FormatException("日期必须使用带时区的 ISO 8601: " + text);
                return date;
            }
            if (type.IsArray)
            {
                var source = value as object[] ?? throw new FormatException("字段必须是数组: " + type.Name);
                var result = Array.CreateInstance(type.GetElementType(), source.Length);
                for (int i = 0; i < source.Length; i++) result.SetValue(ConvertValue(source[i], type.GetElementType()), i);
                return result;
            }
            if (type == typeof(int))
            {
                if (value is int integer) return integer;
                if (value is long wide) return checked((int)wide);
                throw new FormatException("字段必须是整数");
            }
            if (type == typeof(long))
            {
                if (value is long wide) return wide;
                if (value is int integer) return (long)integer;
                throw new FormatException("字段必须是长整数");
            }
            if (type == typeof(float))
            {
                float number = value is float single ? single
                    : value is double real ? (float)real
                    : value is int integer ? integer
                    : value is long wideInteger ? wideInteger
                    : throw new FormatException("字段必须是浮点数");
                if (float.IsNaN(number) || float.IsInfinity(number)) throw new FormatException("浮点数必须有限");
                return number;
            }
            if (type == typeof(bool)) return value is bool flag ? flag : throw new FormatException("字段必须是布尔");
            if (type == typeof(string)) return value as string ?? throw new FormatException("字段必须是字符串");
            return value;
        }
    }

}
