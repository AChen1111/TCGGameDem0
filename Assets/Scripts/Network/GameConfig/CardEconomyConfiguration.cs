using System;
using System.Collections.Generic;

namespace AChen.Configuration
{
    public sealed class CardEconomyConfiguration
    {
        public const string CraftingTable = "card-crafting";
        public const string RecyclingTable = "card-recycling";
        public const int NormalRarity = 0;
        public const int MaxOwnedCopies = 3;
        public long CraftCostUr { get; }
        readonly long[] m_dismantle;
        readonly long[] m_overflow;

        CardEconomyConfiguration(long cost, long[] dismantle, long[] overflow)
        { CraftCostUr = cost; m_dismantle = dismantle; m_overflow = overflow; }

        public long DismantleUr(int rarity) => m_dismantle[rarity];
        public long OverflowUr(int rarity) => m_overflow[rarity];

        public static CardEconomyConfiguration Load(IReadOnlyDictionary<string, byte[]> files)
        {
            var rows = new Dictionary<string, IReadOnlyList<IReadOnlyDictionary<string, object>>>();
            foreach (string name in new[] { CraftingTable, RecyclingTable })
            {
                if (!files.TryGetValue(name, out var bytes)) throw new FormatException("缺少配置: " + name);
                var table = BinaryTable.Decode(bytes);
                table.Column("Rarity", "int");
                if (name == CraftingTable) table.Column("CostUr", "long");
                else { table.Column("DismantleUr", "long"); table.Column("OverflowUr", "long"); }
                rows.Add(name, GameConfigTables.ToRows(table));
            }
            return FromRows(rows);
        }

        public static CardEconomyConfiguration From(PublishedGameConfig config) => FromRows(config.Extra);

        static CardEconomyConfiguration FromRows(IReadOnlyDictionary<string, IReadOnlyList<IReadOnlyDictionary<string, object>>> tables)
        {
            if (!tables.TryGetValue(CraftingTable, out var craft) || craft.Count != 1)
                throw new FormatException(CraftingTable + ": 必须且只能配置普通版");
            if (Field<int>(craft[0], "Rarity", CraftingTable) != NormalRarity)
                throw new FormatException(CraftingTable + ": 仅支持 Rarity=0");
            long cost = Field<long>(craft[0], "CostUr", CraftingTable);
            if (cost <= 0) throw new FormatException(CraftingTable + ": 合成价格必须为正数");
            if (!tables.TryGetValue(RecyclingTable, out var recycle) || recycle.Count != 5)
                throw new FormatException(RecyclingTable + ": 必须覆盖五种版本");
            var dismantle = new long[5]; var overflow = new long[5];
            foreach (var row in recycle)
            {
                int rarity = Field<int>(row, "Rarity", RecyclingTable);
                long d = Field<long>(row, "DismantleUr", RecyclingTable);
                long o = Field<long>(row, "OverflowUr", RecyclingTable);
                if (rarity < 0 || rarity > 4 || d <= 0 || o <= 0 || dismantle[rarity] != 0)
                    throw new FormatException(RecyclingTable + ": 版本重复、无效或收益不是正数");
                dismantle[rarity] = d; overflow[rarity] = o;
            }
            return new CardEconomyConfiguration(cost, dismantle, overflow);
        }

        static T Field<T>(IReadOnlyDictionary<string, object> row, string field, string table)
        {
            if (!row.TryGetValue(field, out var value) || !(value is T result))
                throw new FormatException(table + ": 字段或类型无效 " + field);
            return result;
        }

        public CardGrantAmounts Grant(int owned, int granted, int rarity)
        {
            if (owned < 0 || granted <= 0 || rarity < 0 || rarity > 4) throw new ArgumentOutOfRangeException(nameof(granted));
            int kept = Math.Min(granted, Math.Max(0, MaxOwnedCopies - owned));
            int overflow = granted - kept;
            return new CardGrantAmounts(kept, overflow, checked(overflow * OverflowUr(rarity)));
        }
    }

    public readonly struct CardGrantAmounts
    {
        public int Kept { get; }
        public int Overflow { get; }
        public long Ur { get; }
        public CardGrantAmounts(int kept, int overflow, long ur) { Kept = kept; Overflow = overflow; Ur = ur; }
    }
}
