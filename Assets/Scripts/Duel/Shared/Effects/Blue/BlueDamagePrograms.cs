using System;

namespace AChen.Duel.Core
{
    internal class BlueDamageProgram : ProgramAbility, IDamageStepAbility
    {
        readonly Func<BattleStep, bool> m_allowed;
        public BlueDamageProgram(string id, int number, int speed, DuelZone[] zones,
            Func<EffectContext, bool> can, Action<EffectContext, DuelChainLink> resolve, Func<BattleStep, bool> allowed)
            : base(id, number, speed, zones, can, resolve) { m_allowed = allowed; }
        public bool AllowsDamageStep(BattleStep step) => m_allowed(step);
    }

    internal sealed class BlueInstanceDamageProgram : BlueDamageProgram, IActivationInstanceUsageLimit
    {
        public BlueInstanceDamageProgram(string id, int number, int speed, DuelZone[] zones,
            Func<EffectContext, bool> can, Action<EffectContext, DuelChainLink> resolve, Func<BattleStep, bool> allowed)
            : base(id, number, speed, zones, can, resolve, allowed) { Once(AbilityId, false); }
    }

    internal sealed class BlueDamageTriggerProgram : TriggerProgramAbility, IDamageStepAbility
    {
        readonly Func<BattleStep, bool> m_allowed;
        public BlueDamageTriggerProgram(string id, int number, DuelZone[] zones, bool mandatory,
            Func<EffectContext, DuelEvent, bool> trigger, Func<EffectContext, bool> can,
            Action<EffectContext, DuelChainLink> resolve, Func<BattleStep, bool> allowed)
            : base(id, number, zones, mandatory, false, trigger, can, resolve) { m_allowed = allowed; }
        public bool AllowsDamageStep(BattleStep step) => m_allowed(step);
    }
}
