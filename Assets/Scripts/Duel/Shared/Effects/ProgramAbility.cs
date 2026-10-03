using System;
using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public interface IActivationTargetRequirement { bool RequiresTarget { get; } }

    /// <summary>不可变卡规则组合的处理程序。局内执行位置只存储于 link，不保存在委托或处理器字段。</summary>
    public class ProgramAbility : IAbilityHandler, IActivationSourcePolicy, IActivationUsageLimit,
        IActivationTargetSelection, IActivationTargetRequirement, IActivationCostSelection, IEffectCategoryProvider
    {
        readonly DuelZone[] m_zones;
        readonly Func<EffectContext, bool> m_can;
        readonly Action<EffectContext, DuelChainLink> m_resolve;
        Func<EffectContext, IEnumerable<DuelCardState>> m_targets = c => Array.Empty<DuelCardState>();
        Func<EffectContext, IEnumerable<DuelCardState>> m_costCards = c => Array.Empty<DuelCardState>();
        Action<EffectContext, DuelCommand, DuelChainLink> m_pay = (c, command, link) => { };
        Func<EffectContext, DuelCommand, string> m_validate = (c, command) => "";
        public string CardId { get; }
        public string AbilityId { get; }
        public int Speed { get; }
        public bool RequiresTarget { get; private set; }
        public int MinCosts { get; private set; }
        public int MaxCosts { get; private set; }
        public int Limit { get; private set; } = int.MaxValue;
        public string UsageKey { get; private set; }
        public bool CountNegatedActivation { get; private set; } = true;
        public EffectCategories Categories { get; private set; }
        public ProgramAbility(string cardId, int number, int speed, DuelZone[] zones,
            Func<EffectContext, bool> can, Action<EffectContext, DuelChainLink> resolve)
        { CardId = cardId; AbilityId = cardId + "." + number; UsageKey = AbilityId; Speed = speed; m_zones = zones; m_can = can; m_resolve = resolve; }
        public ProgramAbility Target(Func<EffectContext, IEnumerable<DuelCardState>> targets)
        { m_targets = targets; RequiresTarget = true; return this; }
        public ProgramAbility Cost(int min, int max, Func<EffectContext, IEnumerable<DuelCardState>> candidates,
            Action<EffectContext, DuelCommand, DuelChainLink> pay)
        { MinCosts = min; MaxCosts = max; m_costCards = candidates; m_pay = pay; return this; }
        public ProgramAbility Pay(Action<EffectContext, DuelCommand, DuelChainLink> pay) { m_pay = pay; return this; }
        public ProgramAbility Validate(Func<EffectContext, DuelCommand, string> validate) { m_validate = validate; return this; }
        public ProgramAbility Once(string key, bool countsNegatedActivation = true)
        { UsageKey = key; Limit = 1; CountNegatedActivation = countsNegatedActivation; return this; }
        public ProgramAbility Category(EffectCategories categories) { Categories = categories; return this; }
        public bool AllowsSource(EffectContext context) => m_zones.Contains(context.Source.Zone)
            && (!(context.Source.Zone == DuelZone.Monster || context.Source.Zone == DuelZone.ExtraMonster)
                || context.Source.Position == CardPosition.FaceUpAttack || context.Source.Position == CardPosition.FaceUpDefense);
        public IEnumerable<DuelCardState> TargetCandidates(EffectContext context) => m_targets(context);
        public IEnumerable<DuelCardState> CostCandidates(EffectContext context) => m_costCards(context);
        public bool CanActivate(EffectContext context) => AllowsSource(context) && m_can(context)
            && (!RequiresTarget || m_targets(context).Any()) && m_costCards(context).Count() >= MinCosts;
        public string ValidateActivation(EffectContext context, DuelCommand command)
        {
            var candidates = m_costCards(context).Select(c => c.InstanceId).ToArray();
            if (command.Cards.Length < MinCosts || command.Cards.Length > MaxCosts
                || command.Cards.Distinct().Count() != command.Cards.Length || command.Cards.Any(id => !candidates.Contains(id))) return "INVALID_COST";
            if (!RequiresTarget && command.TargetId != 0) return "NO_ACTIVATION_TARGET";
            return m_validate(context, command);
        }
        public void PayCost(EffectContext context, DuelCommand command, DuelChainLink link) => m_pay(context, command, link);
        public void Resolve(EffectContext context, DuelChainLink link) => m_resolve(context, link);
    }

    public sealed class InstanceProgramAbility : ProgramAbility, IActivationInstanceUsageLimit
    {
        public InstanceProgramAbility(string id, int number, int speed, DuelZone[] zones,
            Func<EffectContext, bool> can, Action<EffectContext, DuelChainLink> resolve)
            : base(id, number, speed, zones, can, resolve) { Once(AbilityId, false); }
    }

    public class TriggerProgramAbility : ProgramAbility, ITriggeredAbility
    {
        readonly Func<EffectContext, DuelEvent, bool> m_trigger;
        public bool Mandatory { get; }
        public bool OptionalWhen { get; }
        public TriggerProgramAbility(string id, int number, DuelZone[] zones, bool mandatory, bool optionalWhen,
            Func<EffectContext, DuelEvent, bool> trigger, Func<EffectContext, bool> can, Action<EffectContext, DuelChainLink> resolve)
            : base(id, number, 1, zones, can, resolve)
        { Mandatory = mandatory; OptionalWhen = optionalWhen; m_trigger = trigger; }
        public bool IsTriggered(EffectContext context, DuelEvent fact) => m_trigger(context, fact);
    }

    public sealed class InstanceTriggerProgramAbility : TriggerProgramAbility, IActivationInstanceUsageLimit
    {
        public InstanceTriggerProgramAbility(string id, int number, DuelZone[] zones, bool mandatory, bool optionalWhen,
            Func<EffectContext, DuelEvent, bool> trigger, Func<EffectContext, bool> can, Action<EffectContext, DuelChainLink> resolve)
            : base(id, number, zones, mandatory, optionalWhen, trigger, can, resolve) { Once(AbilityId, false); }
    }

    public sealed class ContinuousProgram : ICardContinuousRule
    {
        readonly Action<EffectContext, IList<DuelEffectRecord>> m_collect;
        public string RuleId { get; }
        public ContinuousProgram(string id, Action<EffectContext, IList<DuelEffectRecord>> collect) { RuleId = id; m_collect = collect; }
        public void Collect(EffectContext context, IList<DuelEffectRecord> output) => m_collect(context, output);
    }

    public class PhaseTriggerProgramAbility : TriggerProgramAbility, IPhaseAbility
    {
        readonly Func<EffectContext, bool> m_phase;
        public PhaseTriggerProgramAbility(string id, int number, DuelZone[] zones, bool mandatory, bool optionalWhen,
            Func<EffectContext, DuelEvent, bool> trigger, Func<EffectContext, bool> phase,
            Func<EffectContext, bool> can, Action<EffectContext, DuelChainLink> resolve)
            : base(id, number, zones, mandatory, optionalWhen, trigger, can, resolve) { m_phase = phase; }
        public bool AllowsPhase(EffectContext context) => m_phase(context);
    }

    public sealed class InstancePhaseTriggerProgramAbility : PhaseTriggerProgramAbility, IActivationInstanceUsageLimit
    {
        public InstancePhaseTriggerProgramAbility(string id, int number, DuelZone[] zones, bool mandatory, bool optionalWhen,
            Func<EffectContext, DuelEvent, bool> trigger, Func<EffectContext, bool> phase,
            Func<EffectContext, bool> can, Action<EffectContext, DuelChainLink> resolve)
            : base(id, number, zones, mandatory, optionalWhen, trigger, phase, can, resolve) { Once(AbilityId, false); }
    }
}
