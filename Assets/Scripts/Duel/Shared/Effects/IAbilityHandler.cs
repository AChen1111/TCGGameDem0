namespace AChen.Duel.Core
{
    /// <summary>纯规则能力；连锁的可恢复执行现场只保存在 DuelChainLink 与 DuelDecision 中。</summary>
    public interface IAbilityHandler
    {
        string CardId { get; }
        string AbilityId { get; }
        int Speed { get; }
        bool CanActivate(EffectContext context);
        string ValidateActivation(EffectContext context, DuelCommand command);
        void PayCost(EffectContext context, DuelCommand command, DuelChainLink link);
        void Resolve(EffectContext context, DuelChainLink link);
    }
}
