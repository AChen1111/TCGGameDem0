using System.Collections.Generic;
using AChen.Configuration;
namespace Table
{
    public sealed class CardRow
    {
        public string CardId { get; private set; }
        public int Kind { get; private set; }
        public int Frame { get; private set; }
        public int Level { get; private set; }
        public int Attribute { get; private set; }
        public int Race { get; private set; }
        public int Atk { get; private set; }
        public int Def { get; private set; }
        public int Flags { get; private set; }
        public int SpellTrapType { get; private set; }
        public static List<CardRow> LoadBytes(byte[] data)
        {
            var table = BinaryTable.Decode(data);
            int cCardId = table.Column("CardId", "string");
            int cKind = table.Column("Kind", "int");
            int cFrame = table.Column("Frame", "int");
            int cLevel = table.Column("Level", "int");
            int cAttribute = table.Column("Attribute", "int");
            int cRace = table.Column("Race", "int");
            int cAtk = table.Column("Atk", "int");
            int cDef = table.Column("Def", "int");
            int cFlags = table.Column("Flags", "int");
            int cSpellTrapType = table.Column("SpellTrapType", "int");
            var result = new List<CardRow>(table.Rows.Length);
            foreach (var row in table.Rows) result.Add(new CardRow
            {
                CardId = (string)row[cCardId],
                Kind = (int)row[cKind],
                Frame = (int)row[cFrame],
                Level = (int)row[cLevel],
                Attribute = (int)row[cAttribute],
                Race = (int)row[cRace],
                Atk = (int)row[cAtk],
                Def = (int)row[cDef],
                Flags = (int)row[cFlags],
                SpellTrapType = (int)row[cSpellTrapType],
            });
            return result;
        }
    }
}
