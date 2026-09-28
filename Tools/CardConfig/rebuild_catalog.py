"""Rebuild the local card catalog from its source CSVs.

Run from the repository root. The explicit sets below are reviewed against the
current 94-card catalog; every remaining canonical card is generic.
"""

import csv
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2] / "TableData"
VARIANTS = {"14558128": ("14558127", "Card03"), "94145022": ("94145021", "Card03")}
BLUE = {
    "02129638", "08240199", "38517737", "40908371", "45467446",
    "59822133", "71039903", "79814787", "89631139",
}
HERO = {
    "00213326", "01948619", "08949584", "09411399", "10186633",
    "17955766", "19222426", "19324993", "21143940", "22908820",
    "23204029", "27780618", "32828466", "40044918", "45906428",
    "50720316", "55171412", "56733747", "58004362", "58481572",
    "60461804", "63060238", "75047173", "89943723", "93347961",
}
SKY = {
    "08491308", "09726840", "12421694", "17217034", "20357457",
    "24010609", "25072579", "26077389", "34433770", "37351133",
    "50005218", "56741506", "63013339", "63166096", "63288574",
    "75147529", "76072561", "90673289", "98338152", "98462037",
}
POPULAR = {
    "89631139", "38517737", "26077389", "63288574", "40044918",
    "89943723", "60461804", "14558127",
}


def read(name):
    with (ROOT / (name + ".csv")).open(encoding="utf-8-sig", newline="") as f:
        rows = list(csv.reader(f))
    return rows[:2], rows[2:]


def write(name, header, rows):
    with (ROOT / (name + ".csv")).open("w", encoding="utf-8", newline="") as f:
        writer = csv.writer(f, lineterminator="\n")
        writer.writerows(header)
        writer.writerows(rows)


assert not (BLUE & HERO or BLUE & SKY or HERO & SKY)
header, all_cards = read("all-cards")
original = {row[0]: row[1] for row in all_cards}
canonical = {card_id: pool for card_id, pool in original.items() if card_id not in VARIANTS}
assert (BLUE | HERO | SKY | POPULAR) <= canonical.keys()
write("all-cards", header, [row for row in all_cards if row[0] in canonical])

for name in ("Cards", "card-deck-sections", "card-banlist"):
    header, rows = read(name)
    write(name, header, [row for row in rows if row[0] not in VARIANTS])

header, rows = read("pool-entries")
pool_rows = []
for card_id in sorted(canonical):
    pool = "Card01" if card_id in BLUE else "Card02" if card_id in HERO else "Card03" if card_id in SKY else "CardGeneric"
    pool_rows.append([pool, card_id, "100"])
write("pool-entries", header, pool_rows)

header, packs = read("card-packs")
for row in packs:
    pack_number = int(row[0]) - 1
    selected = {1: ("青眼白龙到来", "Card01"), 5: ("闪刀姬", "Card03"), 10: ("英雄到来", "Card02")}.get(pack_number)
    row[1], row[3] = selected if selected else ("泛用卡池", "CardGeneric")
write("card-packs", header, packs)

write("card-art-variants", [["CardId", "ArtId", "SourcePool"], ["string", "string", "string"]],
      [[canonical_id, art_id, pool] for art_id, (canonical_id, pool) in VARIANTS.items()])
write("card-special-materials", [["CardId", "AllowColorful", "AllowGoldOutline"], ["string", "bool", "bool"]],
      [[card_id, "True", "True"] for card_id in sorted(POPULAR)])
write("rarity-weights", [["Rarity", "Weight"], ["int", "int"]], [["0", "60"], ["1", "10"], ["3", "20"]])

header, translations = read("Translations")
translations = [row for row in translations if not any(row[0].startswith("card." + art_id + ".") for art_id in VARIANTS)]
for row in translations:
    if row[0].startswith("shop.pack."):
        pack_number = int(row[0].rsplit(".", 1)[1]) - 1
        row[1], row[2] = {
            1: ("青眼白龙到来", "Blue-Eyes Arrival"),
            5: ("闪刀姬", "Sky Striker"),
            10: ("英雄到来", "A Hero Lives"),
        }.get(pack_number, ("泛用卡池", "Generic Card Pool"))
write("Translations", header, translations)

print("Canonical cards:", len(canonical), "pool sizes:",
      {key: sum(row[0] == key for row in pool_rows) for key in ("Card01", "Card02", "Card03", "CardGeneric")})
