namespace AChen.Duel.Presentation
{
    public static class BattleLabels
    {
        public static string Zone(DuelZone zone) => zone switch
        { DuelZone.MainDeck => "主卡组", DuelZone.ExtraDeck => "额外卡组", DuelZone.Hand => "手牌",
          DuelZone.Monster => "怪兽区", DuelZone.SpellTrap => "魔陷区", DuelZone.Field => "场地区",
          DuelZone.Graveyard => "墓地", DuelZone.Banished => "除外", DuelZone.ExtraMonster => "额外怪兽区", _ => "" };
        public static string Phase(DuelPhase phase) => phase switch
        { DuelPhase.Draw => "抽卡", DuelPhase.Standby => "准备", DuelPhase.Main1 => "主要 1",
          DuelPhase.Battle => "战斗", DuelPhase.Main2 => "主要 2", DuelPhase.End => "结束", _ => "" };
        public static string Position(CardPosition position) => position switch
        { CardPosition.FaceUpAttack => "攻击表示", CardPosition.FaceUpDefense => "守备表示",
          CardPosition.FaceDownDefense => "盖放怪兽", CardPosition.FaceUp => "表侧表示", CardPosition.FaceDown => "盖放", _ => "" };
    }
}
