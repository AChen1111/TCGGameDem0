using System;
using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    internal sealed class HeroContinuousRule : ICardContinuousRule
    {
        readonly Action<EffectContext, IList<DuelEffectRecord>> m_collect;
        public string RuleId { get; }
        public HeroContinuousRule(string ruleId, Action<EffectContext, IList<DuelEffectRecord>> collect)
        { RuleId = ruleId; m_collect = collect; }
        public void Collect(EffectContext context, IList<DuelEffectRecord> output)
        {
            if (!HeroDeckTriggerAbility.FaceUpMonster(context) || context.Source.Negated) return;
            m_collect(context, output);
        }
        public static IEnumerable<DuelCardState> Monsters(EffectContext context, int player) => context.State.Cards.Where(card =>
            card.Controller == player && (card.Zone == DuelZone.Monster || card.Zone == DuelZone.ExtraMonster)
            && DuelEngine.IsPublic(card));
        public static DuelEffectRecord Record(EffectContext context, DuelCardState target, EffectRecordKind kind, int value = 0) =>
            new DuelEffectRecord { Kind = kind, Source = context.SourceRef, Target = target.Ref,
                Player = target.Controller, Value = value, RequiresSource = true };
        public static int AttributeCount(IEnumerable<DuelCardState> monsters) =>
            new[] { 1, 2, 4, 8, 16, 32, 64 }.Count(attribute => monsters.Any(card => (card.CurrentAttribute & attribute) != 0));
    }
}
