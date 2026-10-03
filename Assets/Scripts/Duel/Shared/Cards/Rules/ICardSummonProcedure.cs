using System.Collections.Generic;

namespace AChen.Duel.Core
{
    /// <summary>不建立连锁的特殊召唤手续；合法性和执行通过共同操作完成。</summary>
    public interface ICardSummonProcedure
    {
        string Id { get; }
        IEnumerable<DuelAction> QueryActions(EffectContext context);
        string Validate(EffectContext context, DuelCommand command);
        void Execute(EffectContext context, DuelCommand command);
    }
}
