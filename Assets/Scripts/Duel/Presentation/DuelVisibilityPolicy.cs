namespace AChen.Duel.Presentation
{
    public interface IDuelVisibilityPolicy
    {
        bool CanInspect(int viewer, CardView card);
        bool IsFaceVisible(int viewer, CardView card);
    }
    /// <summary>演示模式允许查看所有区域；正式对局可替换为席位投影策略。</summary>
    public sealed class DemoVisibilityPolicy : IDuelVisibilityPolicy
    {
        public bool CanInspect(int viewer, CardView card) => true;
        public bool IsFaceVisible(int viewer, CardView card) => card.Zone.Kind == DuelZone.Hand
            ? card.Owner == viewer
            : card.Position is CardPosition.FaceUp or CardPosition.FaceUpAttack or CardPosition.FaceUpDefense;
    }
}
