"""Import the project's rule-card metadata; never copy card-engine source code.

Usage: python Tools/Duel/import_card_catalog.py --cdb-root D:/Game/MDPro3/Data/locales
The CDB files are read-only build inputs. The runtime only uses generated C#.
"""
import argparse
import csv
import hashlib
import json
import re
from pathlib import Path
import sqlite3
from urllib.parse import quote
from generate_name_catalog import generate as generate_names
from catalog_fingerprint import fingerprint as generated_fingerprint


def table(path):
    with path.open(encoding="utf-8-sig", newline="") as stream:
        return list(csv.DictReader(stream))[1:]


def csharp(value):
    return json.dumps(value, ensure_ascii=False)


def export_names(root, cdb_root):
    databases = {}
    for language in ("ja-JP", "zh-CN", "en-US"):
        path = cdb_root / language / "cards.cdb"
        with sqlite3.connect(path.resolve().as_uri() + "?mode=ro", uri=True) as connection:
            connection.row_factory = sqlite3.Row
            databases[language] = {row["id"]: dict(row) for row in connection.execute(
                "SELECT datas.id, ot, alias, type, name FROM datas JOIN texts USING(id)")}
    japanese = databases["ja-JP"]
    rows = []
    for card in sorted(japanese.values(), key=lambda card: card["id"]):
        # ot bit 1 denotes OCG; tokens and non-playable name placeholders are not declarations.
        if not card["ot"] & 1 or card["type"] & 0x4000 or card["alias"]:
            continue
        kind = 1 if card["type"] & 1 else 2 if card["type"] & 2 else 3 if card["type"] & 4 else 0
        if kind == 0:
            continue
        rows.append([str(card["id"]).zfill(8), kind, card["name"],
                     databases["zh-CN"][card["id"]]["name"], databases["en-US"][card["id"]]["name"]])
    with (root / "TableData/card-name-catalog.csv").open("w", encoding="utf-8", newline="") as stream:
        writer = csv.writer(stream, lineterminator="\n")
        writer.writerow(["NameId", "Kind", "Japanese", "Chinese", "English"])
        writer.writerow(["string", "int", "string", "string", "string"])
        writer.writerows(rows)
    return len(rows)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--cdb-root", type=Path, required=True)
    parser.add_argument("--project-root", type=Path, default=Path(__file__).resolve().parents[2])
    args = parser.parse_args()
    root = args.project_root
    source_hashes = {language: hashlib.sha256((args.cdb_root / language / "cards.cdb").read_bytes()).hexdigest()
                     for language in ("ja-JP", "zh-CN", "en-US")}
    project_hashes = {name: hashlib.sha256((root / "TableData" / name).read_bytes()).hexdigest()
                     for name in ("Cards.csv", "Translations.csv", "card-banlist.csv", "card-art-variants.csv")}
    source_fingerprint = hashlib.sha256(json.dumps({"schema": 1, "sources": source_hashes, "project": project_hashes},
                                           sort_keys=True, separators=(",", ":")).encode()).hexdigest()
    limits = {row["CardId"]: int(row["MaxCopies"]) for row in table(root / "TableData/card-banlist.csv")}
    arts = table(root / "TableData/card-art-variants.csv")
    translations = {row["Key"]: row for row in table(root / "TableData/Translations.csv")}
    source = args.cdb_root / "ja-JP" / "cards.cdb"
    with sqlite3.connect(source.resolve().as_uri() + "?mode=ro", uri=True) as database:
        database.row_factory = sqlite3.Row
        cards = []
        definitions = []
        source_index = []
        for row in table(root / "TableData" / "Cards.csv"):
            card = database.execute("SELECT * FROM datas WHERE id=?", (int(row["CardId"]),)).fetchone()
            kind = "Monster" if card["type"] & 1 else "Spell" if card["type"] & 2 else "Trap"
            normal = "true" if card["type"] & 16 else "false"
            frame = next((name for mask, name in ((0x4000000, "Link"), (0x800000, "Xyz"),
                (0x2000, "Synchro"), (0x40, "Fusion"), (0x80, "Ritual"), (0x10, "Normal"),
                (0x20, "Effect")) if card["type"] & mask), "None")
            level = card["level"] & 255
            link = frame == "Link"
            xyz = frame == "Xyz"
            sets = [((card["setcode"] >> shift) & 0xffff) for shift in range(0, 64, 16)
                    if (card["setcode"] >> shift) & 0xffff]
            spell_type = "None" if kind == "Monster" else next((name for mask, name in
                ((0x10000, "QuickPlay"), (0x20000, "Continuous"), (0x40000, "Equip"),
                 (0x80000, "Field"), (0x100000, "Counter"), (0x80, "Ritual")) if card["type"] & mask), "Normal")
            original_id = card["id"]
            alias = card["alias"]
            visited = {original_id}
            while alias:
                if alias in visited:
                    raise ValueError("Cyclic original-name alias: " + str(alias))
                visited.add(alias)
                original_id = alias
                alias = database.execute("SELECT alias FROM datas WHERE id=?", (alias,)).fetchone()[0]
            name = translations["card." + row["CardId"] + ".name"]["Chinese"]
            description = translations["card." + row["CardId"] + ".desc"]["Chinese"].replace("\r", "")
            japanese_name = database.execute("SELECT name FROM texts WHERE id=?", (card["id"],)).fetchone()[0]
            source_url = "https://www.db.yugioh-card.com/yugiohdb/card_search.action?ope=1&stype=1&request_locale=ja&keyword=" + quote(japanese_name)
            if row["CardId"] == "25072579":
                source_url = "https://www.db.yugioh-card.com/yugiohdb/faq_search.action?cid=21600&ope=4&request_locale=ja"
            source_index.append({"cardId": row["CardId"], "japaneseName": japanese_name, "officialSourceUrl": source_url,
                "reviewStatus": "FAQ reviewed 2026-10-02" if row["CardId"] == "25072579" else "Official ruling review required"})
            prefix = re.split("[①②③④⑤⑥⑦⑧⑨⑩]：", description)[0].strip()
            material = ""
            if frame in ("Fusion", "Synchro", "Xyz", "Link") and row["CardId"] != "58481572":
                material = prefix.split("\n", 1)[0]
                prefix = prefix[len(material):].strip()
            effects = []
            if normal == "false":
                segments = re.split("([①②③④⑤⑥⑦⑧⑨⑩])：", description)
                effects = [("①②③④⑤⑥⑦⑧⑨⑩".index(segments[i]) + 1, segments[i + 1].strip())
                           for i in range(1, len(segments), 2)]
                if not effects:
                    effects = [(1, prefix)]  # Two existing legacy Synchro texts are not numbered.
                    prefix = ""
            abilities = ", ".join("new CardAbilityDefinition(%s, %s)" %
                                  (csharp(row["CardId"] + "." + str(number)), csharp(text)) for number, text in effects)
            can_normal_summon = kind == "Monster" and frame in ("Normal", "Effect") and not card["type"] & 0x2000000
            definitions.append({"id": row["CardId"], "kind": {"Monster": 1, "Spell": 2, "Trap": 3}[kind],
                "normal": normal == "true", "level": 0 if link or xyz else level, "attack": card["atk"],
                "defense": None if link else card["def"], "monsterType":
                    {"None": 0, "Normal": 1, "Effect": 2, "Ritual": 3, "Fusion": 4, "Synchro": 5, "Xyz": 6, "Link": 7}[frame],
                "tuner": bool(card["type"] & 0x1000), "attribute": card["attribute"], "race": card["race"],
                "rank": level if xyz else 0, "linkRating": level if link else 0, "linkArrows": card["def"] if link else 0,
                "originalName": str(original_id).zfill(8), "limit": limits.get(row["CardId"], 3), "sets": sets,
                "spellType": {"None": 0, "Normal": 1, "QuickPlay": 2, "Continuous": 3, "Equip": 4, "Field": 5, "Ritual": 6, "Counter": 7}[spell_type],
                "name": name, "rules": prefix, "material": material, "sourceUrl": source_url,
                "abilities": [(row["CardId"] + "." + str(number), text) for number, text in effects],
                "canNormalSummon": bool(can_normal_summon)})
            cards.append("            new CardDefinition(%s, RuleCardKind.%s, %s, %s, %s, %s, "
                         "RuleMonsterType.%s, %s, %s, %s, %s, %s, %s, %s, %s, new int[] { %s }, RuleSpellTrapType.%s, "
                         "%s, %s, %s, %s, new CardAbilityDefinition[] { %s }, %s)," %
                         (csharp(row["CardId"]), kind, normal, 0 if link or xyz else level,
                          card["atk"], "null" if link else card["def"], frame,
                          "true" if card["type"] & 0x1000 else "false", card["attribute"], card["race"],
                          level if xyz else 0, level if link else 0, card["def"] if link else 0,
                          csharp(str(original_id).zfill(8)), limits.get(row["CardId"], 3),
                          ", ".join(str(code) for code in sets), spell_type,
                          csharp(name), csharp(prefix), csharp(material), csharp(source_url), abilities,
                          "true" if can_normal_summon else "false"))
    fingerprint = generated_fingerprint(definitions, {art["ArtId"]: art["CardId"] for art in arts}, source_fingerprint)
    output = root / "Assets/Scripts/Duel/Shared/Cards/CardCatalogData.Generated.cs"
    output.write_text("// Generated by Tools/Duel/import_card_catalog.py; source card IDs are TableData/Cards.csv.\n"
        "namespace AChen.Duel.Core\n{\n    internal static class CardCatalogData\n    {\n"
        "        public const string Fingerprint = " + csharp(fingerprint) + ";\n"
        "        public const string SourceFingerprint = " + csharp(source_fingerprint) + ";\n"
        "        public const string BanlistFingerprint = " + csharp(project_hashes["card-banlist.csv"]) + ";\n"
        "        public static CardDefinition[] CreateCards() => new[]\n        {\n" + "\n".join(cards) +
        "\n        };\n        public static System.Collections.Generic.Dictionary<string, string> CreateArtAliases() =>\n"
        "            new System.Collections.Generic.Dictionary<string, string>(System.StringComparer.Ordinal)\n            {\n" +
        "\n".join("                { %s, %s }," % (csharp(art["ArtId"]), csharp(art["CardId"])) for art in arts) +
        "\n            };\n    }\n}\n", encoding="utf-8", newline="\n")
    name_count = export_names(root, args.cdb_root)
    name_fingerprint = generate_names(root)
    manifest = {"schemaVersion": 2, "fingerprint": fingerprint, "sourceFingerprint": source_fingerprint, "nameFingerprint": name_fingerprint,
        "generatedOutputHashes": {path: hashlib.sha256((root / path).read_bytes()).hexdigest() for path in (
            "Assets/Scripts/Duel/Shared/Cards/CardCatalogData.Generated.cs", "Assets/Scripts/Duel/Shared/Cards/CardNameCatalogData.Generated.cs")},
        "ruleCardCount": len(cards), "declarableNameCount": name_count,
        "sourceHashes": source_hashes, "projectSourceHashes": project_hashes,
        "sources": source_index,
        "declarationRulings": [
            "https://www.db.yugioh-card.com/yugiohdb/faq_search.action?fid=12551&ope=5&request_locale=ja&tag=-1",
            "https://www.db.yugioh-card.com/yugiohdb/faq_search.action?fid=277&ope=5&request_locale=ja"]}
    (root / "Tools/Duel/card-catalog-manifest.json").write_text(json.dumps(manifest, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


if __name__ == "__main__":
    main()
