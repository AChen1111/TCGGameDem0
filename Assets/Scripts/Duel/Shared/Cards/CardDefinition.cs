using System;
using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public enum RuleCardKind { Monster = 1, Spell = 2, Trap = 3 }
    public enum RuleMonsterType { None, Normal, Effect, Ritual, Fusion, Synchro, Xyz, Link }
    public enum RuleSpellTrapType { None, Normal, QuickPlay, Continuous, Equip, Field, Ritual, Counter }

    public sealed class CardAbilityDefinition
    {
        public string Id { get; }
        public string Text { get; }
        /// <summary>卡文条目不是效果处理器；是否可执行由能力注册表决定。</summary>
        public CardAbilityDefinition(string id, string text) { Id = id; Text = text; }
    }

    /// <summary>不可变的印刷定义；实体状态和效果执行过程由决斗内核持有。</summary>
    public sealed class CardDefinition
    {
        public string CardId { get; }
        public string OriginalNameId { get; }
        public int MaxCopies { get; }
        public bool CanNormalSummon { get; }
        public RuleCardKind Kind { get; }
        public bool IsNormal { get; }
        public RuleMonsterType MonsterType { get; }
        public bool IsExtra => MonsterType == RuleMonsterType.Fusion || MonsterType == RuleMonsterType.Synchro
            || MonsterType == RuleMonsterType.Xyz || MonsterType == RuleMonsterType.Link;
        public bool IsTuner { get; }
        public int Level { get; }
        public int Rank { get; }
        public int LinkRating { get; }
        public int LinkArrows { get; }
        public int Attribute { get; }
        public int Race { get; }
        public IReadOnlyList<int> SetCodes { get; }
        public RuleSpellTrapType SpellTrapType { get; }
        public string Name { get; }
        public string RulesText { get; }
        public string MaterialText { get; }
        public string OfficialSourceUrl { get; }
        public int Attack { get; }
        public int? Defense { get; }
        public IReadOnlyList<CardAbilityDefinition> Abilities { get; }

        public CardDefinition(string cardId, RuleCardKind kind, bool isNormal, int level, int attack, int? defense,
            RuleMonsterType monsterType, bool isTuner, int attribute, int race,
            int rank, int linkRating, int linkArrows, string originalNameId, int maxCopies,
            int[] setCodes, RuleSpellTrapType spellTrapType, string name, string rulesText, string materialText,
            string officialSourceUrl, CardAbilityDefinition[] abilities, bool canNormalSummon)
        {
            CardId = cardId; Kind = kind; IsNormal = isNormal; Level = level;
            Attack = attack; Defense = defense;
            MonsterType = monsterType; IsTuner = isTuner; Attribute = attribute; Race = race;
            Rank = rank; LinkRating = linkRating; LinkArrows = linkArrows;
            OriginalNameId = originalNameId; MaxCopies = maxCopies;
            CanNormalSummon = canNormalSummon;
            SetCodes = Array.AsReadOnly((int[])setCodes.Clone()); SpellTrapType = spellTrapType;
            Name = name; RulesText = rulesText; MaterialText = materialText; OfficialSourceUrl = officialSourceUrl;
            Abilities = Array.AsReadOnly((CardAbilityDefinition[])abilities.Clone());
        }

        public bool BelongsTo(int setCode) => SetCodes.Any(code => (code & 0xfff) == (setCode & 0xfff)
            && (code & setCode & 0xf000) == (setCode & 0xf000));
    }
}
