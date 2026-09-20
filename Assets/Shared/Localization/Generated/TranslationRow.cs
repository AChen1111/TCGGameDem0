using System.Collections.Generic;
using AChen.Configuration;
namespace Table
{
    public sealed class TranslationRow
    {
        public string Key { get; private set; }
        public string Chinese { get; private set; }
        public string English { get; private set; }
        public static List<TranslationRow> LoadBytes(byte[] data)
        {
            var table = BinaryTable.Decode(data);
            int cKey = table.Column("Key", "string");
            int cChinese = table.Column("Chinese", "string");
            int cEnglish = table.Column("English", "string");
            var result = new List<TranslationRow>(table.Rows.Length);
            foreach (var row in table.Rows) result.Add(new TranslationRow
            {
                Key = (string)row[cKey],
                Chinese = (string)row[cChinese],
                English = (string)row[cEnglish],
            });
            return result;
        }
    }
}
