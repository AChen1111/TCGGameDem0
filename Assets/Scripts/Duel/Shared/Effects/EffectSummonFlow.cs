using System.Globalization;
using System.Linq;

namespace AChen.Duel.Core
{
    /// <summary>卡、场位、表示依次决策；全部步骤存于当前连锁项中。</summary>
    internal static class EffectSummonFlow
    {
        public static bool Resume(EffectContext context, DuelChainLink link, DuelCardState card, int initialStep,
            bool ignore = false, SummonMethod method = SummonMethod.Effect, bool extraOnly = false, int controller = -1)
        {
            int targetPlayer = controller < 0 ? context.Player : controller;
            if (link.Step == initialStep)
            {
                link.Values["summon-card"] = card.InstanceId;
                var slots = context.Engine.GetSpecialSummonDestinations(card, targetPlayer).Where(slot => !extraOnly || slot >= 5);
                var options = slots.Select(slot => new DecisionOption { Id = slot.ToString(CultureInfo.InvariantCulture),
                    Value = slot.ToString(CultureInfo.InvariantCulture), Label = "区域 " + (slot + 1) }).ToArray();
                link.Step++;
                if (options.Length == 0) { link.Step = initialStep + 3; return true; }
                context.OpenDecision(link, DecisionKind.ChooseZone, options, "选择特殊召唤区域");
                return false;
            }
            if (link.Step == initialStep + 1)
            {
                link.Values["summon-slot"] = int.Parse(link.Answers[0], CultureInfo.InvariantCulture);
                if (context.Catalog.Get(card.DefinitionId).MonsterType == RuleMonsterType.Link)
                {
                    context.SpecialSummon(card, targetPlayer, link.Values["summon-slot"], CardPosition.FaceUpAttack, ignore, method);
                    link.Step = initialStep + 3;
                    return true;
                }
                link.Step++;
                var options = new[] {
                    new DecisionOption { Id = "attack", Value = ((int)CardPosition.FaceUpAttack).ToString(CultureInfo.InvariantCulture), Label = "攻击表示" },
                    new DecisionOption { Id = "defense", Value = ((int)CardPosition.FaceUpDefense).ToString(CultureInfo.InvariantCulture), Label = "守备表示" } };
                context.OpenDecision(link, DecisionKind.ChoosePosition, options, "选择表示");
                return false;
            }
            if (link.Step == initialStep + 2)
            {
                var summoned = context.Card(link.Values["summon-card"]);
                context.SpecialSummon(summoned, targetPlayer, link.Values["summon-slot"],
                    (CardPosition)int.Parse(link.Answers[0], CultureInfo.InvariantCulture), ignore, method);
                link.Step++;
                return true;
            }
            return true;
        }
    }
}
