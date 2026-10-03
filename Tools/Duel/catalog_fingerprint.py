"""Canonical card metadata encoding, shared with DuelCardDataDigest.cs."""
import hashlib
import struct


def fingerprint(definitions, aliases, source_hash):
    data = bytearray()

    def text(value):
        encoded = value.encode("utf-8")
        data.extend(struct.pack("<i", len(encoded)))
        data.extend(encoded)

    def integer(value):
        data.extend(struct.pack("<i", value))

    def boolean(value):
        data.extend(struct.pack("<?", value))

    text("achen-duel-card-catalog-v1")
    text(source_hash)
    cards = sorted(definitions, key=lambda card: card["id"])
    integer(len(cards))
    for card in cards:
        text(card["id"])
        integer(card["kind"])
        boolean(card["normal"])
        integer(card["level"])
        integer(card["attack"])
        boolean(card["defense"] is not None)
        if card["defense"] is not None:
            integer(card["defense"])
        integer(card["monsterType"])
        boolean(card["tuner"])
        for key in ("attribute", "race", "rank", "linkRating", "linkArrows"):
            integer(card[key])
        text(card["originalName"])
        integer(card["limit"])
        integer(len(card["sets"]))
        for value in card["sets"]:
            integer(value)
        integer(card["spellType"])
        for key in ("name", "rules", "material", "sourceUrl"):
            text(card[key])
        integer(len(card["abilities"]))
        for identity, description in card["abilities"]:
            text(identity)
            text(description)
        boolean(card["canNormalSummon"])
    integer(len(aliases))
    for identity, original in sorted(aliases.items()):
        text(identity)
        text(original)
    return hashlib.sha256(data).hexdigest()
