using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace AChen.Duel.Core
{
    /// <summary>Immutable identity of the data and rule implementation selected for a duel.</summary>
    public sealed class DuelRulePackage
    {
        public const string CurrentRuleVersion = "ocg-2026-10-03-v1";
        public const int CurrentProtocolVersion = 1;
        public string RuleVersion { get; }
        public int ProtocolVersion { get; }
        public string CatalogHash { get; }
        public string NameCatalogHash { get; }
        public string BanlistHash { get; }
        public string Fingerprint { get; }

        public DuelRulePackage(string ruleVersion, int protocolVersion, string catalogHash, string nameCatalogHash, string banlistHash)
        {
            RuleVersion = ruleVersion; ProtocolVersion = protocolVersion; CatalogHash = catalogHash;
            NameCatalogHash = nameCatalogHash; BanlistHash = banlistHash;
            using (var sha = SHA256.Create())
                Fingerprint = BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(
                    "achen-duel-package-v1\n" + ruleVersion + "\n" + protocolVersion.ToString(CultureInfo.InvariantCulture)
                    + "\n" + catalogHash + "\n" + nameCatalogHash + "\n" + banlistHash))).Replace("-", "").ToLowerInvariant();
        }

        public static DuelRulePackage CreateDefault(DuelCardCatalog cards) => new DuelRulePackage(CurrentRuleVersion,
            CurrentProtocolVersion, cards.Fingerprint, CardNameCatalog.CreateDefault().Fingerprint, cards.BanlistFingerprint);

        public DuelStartRecord Freeze(DuelStartRecord start)
        {
            var frozen = ReplayCopies.Start(start);
            frozen.RuleVersion = RuleVersion; frozen.ProtocolVersion = ProtocolVersion; frozen.CatalogHash = CatalogHash;
            frozen.NameCatalogHash = NameCatalogHash; frozen.BanlistHash = BanlistHash; frozen.RulePackageHash = Fingerprint;
            return frozen;
        }

        public bool Matches(DuelStartRecord start) => start.RuleVersion == RuleVersion && start.ProtocolVersion == ProtocolVersion
            && start.CatalogHash == CatalogHash && start.NameCatalogHash == NameCatalogHash
            && start.BanlistHash == BanlistHash && start.RulePackageHash == Fingerprint;
    }
}
