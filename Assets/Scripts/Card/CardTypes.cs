public enum CardKind
{
    None = 0,
    Monster = 1,
    Spell = 2,
    Trap = 3
}

public enum CardFrame
{
    None = 0,
    Normal = 1,
    Effect = 2,
    Ritual = 3,
    Fusion = 4,
    Synchro = 5,
    Xyz = 6,
    Link = 7,
    Spell = 8,
    Trap = 9
}

public enum CardAttribute
{
    None = 0,
    Light = 1,
    Dark = 2,
    Fire = 3,
    Water = 4,
    Wind = 5,
    Earth = 6,
    Divine = 7
}

public enum CardRace
{
    None = 0,
    Warrior = 1,
    Spellcaster = 2,
    Fairy = 3,
    Fiend = 4,
    Zombie = 5,
    Machine = 6,
    Aqua = 7,
    Pyro = 8,
    Rock = 9,
    WingedBeast = 10,
    Plant = 11,
    Insect = 12,
    Thunder = 13,
    Dragon = 14,
    Beast = 15,
    BeastWarrior = 16,
    Dinosaur = 17,
    Fish = 18,
    SeaSerpent = 19,
    Reptile = 20,
    Psychic = 21,
    DivineBeast = 22,
    CreatorGod = 23,
    Wyrm = 24,
    Cyberse = 25,
    Illusion = 26
}

[System.Flags]
public enum CardFlags
{
    None = 0,
    Effect = 1,
    Tuner = 2,
    Flip = 4,
    Spirit = 8,
    Gemini = 16,
    Union = 32,
    Toon = 64,
    Pendulum = 128,
    SpecialSummon = 256
}

public enum CardSpellTrapType
{
    None = 0,
    Normal = 1,
    QuickPlay = 2,
    Continuous = 3,
    Equip = 4,
    Field = 5,
    Ritual = 6,
    Counter = 7
}
