using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AChen.Configuration
{
    // 表自带字段定义, 新增表无需生成代码; 所有长度均受剩余数据约束.
    public sealed class BinaryTable
    {
        public const int Magic = 0x32425441;
        public string[] Names;
        public string[] Types;
        public object[][] Rows;

        public static void ValidateType(string type)
        {
            string scalar = type.EndsWith("[]", StringComparison.Ordinal) ? type.Substring(0, type.Length - 2)
                : type.EndsWith("?", StringComparison.Ordinal) ? type.Substring(0, type.Length - 1) : type;
            if (!new[] { "string", "int", "long", "float", "bool" }.Contains(scalar))
                throw new FormatException("不支持的字段类型: " + type);
        }

        public byte[] Encode()
        {
            ValidateSchema(Names, Types);
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream, new UTF8Encoding(false, true)))
            {
                writer.Write(Magic); writer.Write(1); writer.Write(Names.Length);
                for (int i = 0; i < Names.Length; i++) { writer.Write(Names[i]); writer.Write(Types[i]); }
                writer.Write(Rows.Length);
                foreach (var row in Rows)
                {
                    if (row.Length != Names.Length) throw new FormatException("字段数量错误");
                    for (int i = 0; i < row.Length; i++) WriteValue(writer, Types[i], row[i]);
                }
                writer.Flush(); return stream.ToArray();
            }
        }

        public static BinaryTable Decode(byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes, false))
            using (var reader = new BinaryReader(stream, new UTF8Encoding(false, true)))
            {
                if (reader.ReadInt32() != Magic || reader.ReadInt32() != 1) throw new FormatException("不支持的二进制表版本");
                int columns = Count(reader);
                if (columns == 0 || columns > 1024) throw new FormatException("字段数量无效");
                var table = new BinaryTable { Names = new string[columns], Types = new string[columns] };
                for (int i = 0; i < columns; i++) { table.Names[i] = ReadString(reader); table.Types[i] = ReadString(reader); }
                ValidateSchema(table.Names, table.Types);
                int rows = Count(reader);
                if ((long)rows * columns > stream.Length - stream.Position) throw new FormatException("数据行数量无效");
                table.Rows = new object[rows][];
                for (int r = 0; r < rows; r++)
                {
                    table.Rows[r] = new object[columns];
                    for (int c = 0; c < columns; c++) table.Rows[r][c] = ReadValue(reader, table.Types[c]);
                }
                if (stream.Position != stream.Length) throw new FormatException("二进制表包含多余数据");
                return table;
            }
        }

        public int Column(string name, string type)
        {
            int index = Array.IndexOf(Names, name);
            if (index < 0 || Types[index] != type) throw new FormatException("缺少字段或类型错误: " + name + ":" + type);
            return index;
        }

        static void ValidateSchema(string[] names, string[] types)
        {
            if (names.Length == 0 || names.Length > 1024 || names.Length != types.Length
                || names.Distinct(StringComparer.OrdinalIgnoreCase).Count() != names.Length)
                throw new FormatException("表头为空、重复或数量无效");
            foreach (string name in names)
                if (!System.Text.RegularExpressions.Regex.IsMatch(name, "^[A-Za-z][A-Za-z0-9_]*$")) throw new FormatException("字段名无效: " + name);
            foreach (string type in types) ValidateType(type);
        }

        static int Count(BinaryReader reader)
        {
            int count = reader.ReadInt32();
            if (count < 0 || count > reader.BaseStream.Length - reader.BaseStream.Position) throw new FormatException("二进制长度无效");
            return count;
        }

        static string ReadString(BinaryReader reader)
        {
            // BinaryWriter 使用 7-bit UTF-8 长度; 先验证长度再分配.
            uint length = 0;
            for (int shift = 0; shift < 35; shift += 7)
            {
                byte part = reader.ReadByte();
                if (shift == 28 && part > 7) throw new FormatException("字符串长度溢出");
                length |= (uint)(part & 127) << shift;
                if ((part & 128) == 0)
                {
                    if (length > reader.BaseStream.Length - reader.BaseStream.Position) throw new FormatException("字符串被截断");
                    return new UTF8Encoding(false, true).GetString(reader.ReadBytes((int)length));
                }
            }
            throw new FormatException("字符串长度无效");
        }

        static bool ReadBool(BinaryReader reader)
        {
            byte value = reader.ReadByte();
            if (value > 1) throw new FormatException("布尔编码无效");
            return value == 1;
        }

        static object ReadValue(BinaryReader reader, string type)
        {
            if (type.EndsWith("?", StringComparison.Ordinal)) return ReadBool(reader) ? ReadValue(reader, type.Substring(0, type.Length - 1)) : null;
            if (type.EndsWith("[]", StringComparison.Ordinal))
            {
                int count = Count(reader); var values = new object[count];
                for (int i = 0; i < count; i++) values[i] = ReadValue(reader, type.Substring(0, type.Length - 2));
                return values;
            }
            switch (type)
            {
                case "string": return ReadString(reader);
                case "int": return reader.ReadInt32();
                case "long": return reader.ReadInt64();
                case "bool": return ReadBool(reader);
                case "float":
                    float value = reader.ReadSingle();
                    if (float.IsNaN(value) || float.IsInfinity(value)) throw new FormatException("浮点数必须有限");
                    return value;
                default: throw new FormatException("字段类型无效");
            }
        }

        static void WriteValue(BinaryWriter writer, string type, object value)
        {
            if (type.EndsWith("?", StringComparison.Ordinal))
            {
                writer.Write(value != null);
                if (value != null) WriteValue(writer, type.Substring(0, type.Length - 1), value);
                return;
            }
            if (type.EndsWith("[]", StringComparison.Ordinal))
            {
                var values = (Array)value; writer.Write(values.Length);
                foreach (var item in values) WriteValue(writer, type.Substring(0, type.Length - 2), item);
                return;
            }
            switch (type)
            {
                case "string": writer.Write((string)value); break;
                case "int": writer.Write((int)value); break;
                case "long": writer.Write((long)value); break;
                case "bool": writer.Write((bool)value); break;
                case "float":
                    float number = (float)value;
                    if (float.IsNaN(number) || float.IsInfinity(number)) throw new FormatException("浮点数必须有限");
                    writer.Write(number); break;
                default: throw new FormatException("字段类型无效");
            }
        }
    }
}
