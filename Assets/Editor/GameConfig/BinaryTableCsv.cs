using System;
using System.Globalization;
using System.IO;
using System.Linq;
using AChen.Configuration;
using Newtonsoft.Json.Linq;

public static class BinaryTableCsv
{
    public static BinaryTable Load(string path)
    {
        var rows = GameConfigCsvEditorParser.ReadFields(File.ReadAllText(path, new System.Text.UTF8Encoding(false, true)));
        if (rows.Length < 2) throw new FormatException(path + ": 缺少字段名行或类型行");
        var table = new BinaryTable { Names = rows[0], Types = rows[1], Rows = new object[rows.Length - 2][] };
        if (table.Types.Length != table.Names.Length) throw new FormatException(path + ": 第 2 行类型数量与表头不符");
        for (int c = 0; c < table.Types.Length; c++)
        {
            try { BinaryTable.ValidateType(table.Types[c]); }
            catch (Exception ex) { throw new FormatException($"{path}: 第 2 行第 {c + 1} 列: {ex.Message}", ex); }
        }
        for (int r = 2; r < rows.Length; r++)
        {
            if (rows[r].Length != table.Names.Length) throw new FormatException($"{path}: 第 {r + 1} 记录字段数不符");
            table.Rows[r - 2] = new object[table.Names.Length];
            for (int c = 0; c < table.Names.Length; c++)
            {
                try { table.Rows[r - 2][c] = Parse(table.Types[c], rows[r][c]); }
                catch (Exception ex) when (ex is FormatException || ex is OverflowException || ex is Newtonsoft.Json.JsonException || ex is ArgumentException)
                { throw new FormatException($"{path}: 第 {r + 1} 记录第 {c + 1} 列 ({table.Names[c]}): {ex.Message}", ex); }
            }
        }
        return table;
    }

    public static byte[] Compile(string path)
    {
        string name = Path.GetFileNameWithoutExtension(path);
        try { return GameConfigTables.Encode(name, Load(path)); }
        catch (Exception ex) { throw new FormatException(path + ": " + ex.Message, ex); }
    }

    static object Parse(string type, string text)
    {
        if (type.EndsWith("?", StringComparison.Ordinal)) return text.Length == 0 ? null : Parse(type.Substring(0, type.Length - 1), text);
        if (type.EndsWith("[]", StringComparison.Ordinal))
        {
            string scalar = type.Substring(0, type.Length - 2);
            return JArray.Parse(text).Select(token =>
            {
                bool valid = scalar == "string" ? token.Type == JTokenType.String
                    : scalar == "bool" ? token.Type == JTokenType.Boolean
                    : token.Type == JTokenType.Integer || scalar == "float" && token.Type == JTokenType.Float;
                if (!valid) throw new FormatException("数组元素类型不符: " + scalar);
                return Parse(scalar, scalar == "string" ? (string)token : token.ToString(Newtonsoft.Json.Formatting.None));
            }).ToArray();
        }
        switch (type)
        {
            case "string": return text.Replace("\r\n", "\n");
            case "int": return int.Parse(text, NumberStyles.Integer, CultureInfo.InvariantCulture);
            case "long": return long.Parse(text, NumberStyles.Integer, CultureInfo.InvariantCulture);
            case "bool": return bool.Parse(text);
            case "float":
                float value = float.Parse(text, NumberStyles.Float, CultureInfo.InvariantCulture);
                if (float.IsNaN(value) || float.IsInfinity(value)) throw new FormatException("浮点数必须有限");
                return value;
            default: throw new FormatException("不支持的类型: " + type);
        }
    }
}
