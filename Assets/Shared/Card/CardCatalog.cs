using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public static class CardCatalog
{
    const string ResourcePath = "Card/Cards";
    static readonly Dictionary<string, Table.CardRow> s_table = new Dictionary<string, Table.CardRow>(StringComparer.Ordinal);
    static bool s_initialized;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetState()
    {
        s_initialized = false;
        s_table.Clear();
    }

    public static void Initialize()
    {
        if (!s_initialized) throw new InvalidOperationException("卡牌配置尚未初始化");
    }

    public static void Install(System.Collections.Generic.IReadOnlyList<Table.CardRow> rows)
    {
        s_table.Clear();
        foreach (var row in rows) s_table.Add(row.CardId, row);
        s_initialized = true;
    }

    public static bool TryGet(string cardId, out Table.CardRow row)
    {
        Initialize();
        if (string.IsNullOrEmpty(cardId))
        {
            row = null;
            return false;
        }

        return s_table.TryGetValue(cardId, out row);
    }

    public static string FormatStat(int value)
    {
        return value < 0 ? "?" : value.ToString();
    }

    public static string FormatTypeLine(Table.CardRow row)
    {
        if (row == null)
        {
            return string.Empty;
        }

        string body = FormatTypeBody(row);
        return "【" + body + "】【" + row.CardId + "】";
    }

    static string FormatTypeBody(Table.CardRow row)
    {
        var kind = (CardKind)row.Kind;
        if (kind == CardKind.Spell)
        {
            return LocalizationService.GetText(SpellTrapKey(true, (CardSpellTrapType)row.SpellTrapType));
        }

        if (kind == CardKind.Trap)
        {
            return LocalizationService.GetText(SpellTrapKey(false, (CardSpellTrapType)row.SpellTrapType));
        }

        var parts = new List<string>(4);
        AddPart(parts, RaceKey((CardRace)row.Race));
        var frame = (CardFrame)row.Frame;
        if (frame == CardFrame.Ritual || frame == CardFrame.Fusion || frame == CardFrame.Synchro
            || frame == CardFrame.Xyz || frame == CardFrame.Link)
        {
            AddPart(parts, FrameKey(frame));
        }

        var flags = (CardFlags)row.Flags;
        if ((flags & CardFlags.Tuner) != 0)
        {
            AddPart(parts, "ui.card.flag.tuner");
        }

        if ((flags & CardFlags.SpecialSummon) != 0)
        {
            AddPart(parts, "ui.card.flag.special_summon");
        }

        if ((flags & CardFlags.Effect) != 0)
        {
            AddPart(parts, "ui.card.flag.effect");
        }

        return parts.Count == 0 ? string.Empty : string.Join("/", parts);
    }

    static void AddPart(List<string> parts, string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            return;
        }

        string text = LocalizationService.GetText(key);
        if (!string.IsNullOrEmpty(text) && text != "Null")
        {
            parts.Add(text);
        }
    }

    static string RaceKey(CardRace race)
    {
        switch (race)
        {
            case CardRace.Warrior: return "ui.card.race.warrior";
            case CardRace.Spellcaster: return "ui.card.race.spellcaster";
            case CardRace.Fairy: return "ui.card.race.fairy";
            case CardRace.Fiend: return "ui.card.race.fiend";
            case CardRace.Zombie: return "ui.card.race.zombie";
            case CardRace.Machine: return "ui.card.race.machine";
            case CardRace.Aqua: return "ui.card.race.aqua";
            case CardRace.Pyro: return "ui.card.race.pyro";
            case CardRace.Rock: return "ui.card.race.rock";
            case CardRace.WingedBeast: return "ui.card.race.winged_beast";
            case CardRace.Plant: return "ui.card.race.plant";
            case CardRace.Insect: return "ui.card.race.insect";
            case CardRace.Thunder: return "ui.card.race.thunder";
            case CardRace.Dragon: return "ui.card.race.dragon";
            case CardRace.Beast: return "ui.card.race.beast";
            case CardRace.BeastWarrior: return "ui.card.race.beast_warrior";
            case CardRace.Dinosaur: return "ui.card.race.dinosaur";
            case CardRace.Fish: return "ui.card.race.fish";
            case CardRace.SeaSerpent: return "ui.card.race.sea_serpent";
            case CardRace.Reptile: return "ui.card.race.reptile";
            case CardRace.Psychic: return "ui.card.race.psychic";
            case CardRace.DivineBeast: return "ui.card.race.divine_beast";
            case CardRace.CreatorGod: return "ui.card.race.creator_god";
            case CardRace.Wyrm: return "ui.card.race.wyrm";
            case CardRace.Cyberse: return "ui.card.race.cyberse";
            case CardRace.Illusion: return "ui.card.race.illusion";
            default: return null;
        }
    }

    static string FrameKey(CardFrame frame)
    {
        switch (frame)
        {
            case CardFrame.Ritual: return "ui.card.frame.ritual";
            case CardFrame.Fusion: return "ui.card.frame.fusion";
            case CardFrame.Synchro: return "ui.card.frame.synchro";
            case CardFrame.Xyz: return "ui.card.frame.xyz";
            case CardFrame.Link: return "ui.card.frame.link";
            default: return null;
        }
    }

    static string SpellTrapKey(bool spell, CardSpellTrapType type)
    {
        if (spell)
        {
            switch (type)
            {
                case CardSpellTrapType.QuickPlay: return "ui.card.spell.quick";
                case CardSpellTrapType.Continuous: return "ui.card.spell.continuous";
                case CardSpellTrapType.Equip: return "ui.card.spell.equip";
                case CardSpellTrapType.Field: return "ui.card.spell.field";
                case CardSpellTrapType.Ritual: return "ui.card.spell.ritual";
                default: return "ui.card.spell.normal";
            }
        }

        switch (type)
        {
            case CardSpellTrapType.Continuous: return "ui.card.trap.continuous";
            case CardSpellTrapType.Counter: return "ui.card.trap.counter";
            default: return "ui.card.trap.normal";
        }
    }
}
