using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace AChen.Duel.Core
{
    /// <summary>Canonical generated card data identity; little endian, UTF8 strings and ordinal IDs.</summary>
    public static class DuelCardDataDigest
    {
        public static string Compute(IEnumerable<CardDefinition> definitions, IReadOnlyDictionary<string, string> aliases, string sourceHash)
        {
            using (var bytes = new MemoryStream())
            using (var writer = new BinaryWriter(bytes, new UTF8Encoding(false, true), true))
            {
                Text(writer, "achen-duel-card-catalog-v1"); Text(writer, sourceHash);
                var cards = definitions.OrderBy(c => c.CardId, System.StringComparer.Ordinal).ToArray();
                writer.Write(cards.Length);
                foreach (var card in cards)
                {
                    Text(writer, card.CardId); writer.Write((int)card.Kind); writer.Write(card.IsNormal);
                    writer.Write(card.Level); writer.Write(card.Attack); writer.Write(card.Defense.HasValue);
                    if (card.Defense.HasValue) writer.Write(card.Defense.Value);
                    writer.Write((int)card.MonsterType); writer.Write(card.IsTuner); writer.Write(card.Attribute);
                    writer.Write(card.Race); writer.Write(card.Rank); writer.Write(card.LinkRating); writer.Write(card.LinkArrows);
                    Text(writer, card.OriginalNameId); writer.Write(card.MaxCopies);
                    writer.Write(card.SetCodes.Count); foreach (int code in card.SetCodes) writer.Write(code);
                    writer.Write((int)card.SpellTrapType); Text(writer, card.Name); Text(writer, card.RulesText);
                    Text(writer, card.MaterialText); Text(writer, card.OfficialSourceUrl);
                    writer.Write(card.Abilities.Count);
                    foreach (var ability in card.Abilities) { Text(writer, ability.Id); Text(writer, ability.Text); }
                    writer.Write(card.CanNormalSummon);
                }
                writer.Write(aliases.Count);
                foreach (var pair in aliases.OrderBy(x => x.Key, System.StringComparer.Ordinal))
                { Text(writer, pair.Key); Text(writer, pair.Value); }
                writer.Flush();
                using (var sha = SHA256.Create())
                    return System.BitConverter.ToString(sha.ComputeHash(bytes.ToArray())).Replace("-", "").ToLowerInvariant();
            }
        }

        static void Text(BinaryWriter writer, string value)
        { var bytes = Encoding.UTF8.GetBytes(value); writer.Write(bytes.Length); writer.Write(bytes); }
    }
}
