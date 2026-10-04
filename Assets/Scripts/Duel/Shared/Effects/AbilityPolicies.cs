using System;
using System.Collections.Generic;

namespace AChen.Duel.Core
{
    public interface IActivationUsageLimit
    {
        string UsageKey { get; }
        int Limit { get; }
        bool CountNegatedActivation { get; }
    }
    /// <summary>按实例及存在期限制次数；卡名次数继续使用 IActivationUsageLimit。</summary>
    public interface IActivationInstanceUsageLimit : IActivationUsageLimit { }
    public interface IActivationSourcePolicy { bool AllowsSource(EffectContext context); }
    public interface IActivationTargetSelection { IEnumerable<DuelCardState> TargetCandidates(EffectContext context); }

    [Flags]
    public enum EffectCategories
    {
        None = 0, AddFromDeckToHand = 1, SpecialSummonFromDeck = 2, SendDeckToGraveyard = 4,
        AddFromGraveyardToHandDeckExtra = 8, SpecialSummonFromGraveyard = 16, BanishFromGraveyard = 32
    }
    public interface IEffectCategoryProvider { EffectCategories Categories { get; } }

    public sealed class AbilityMode
    {
        public string Id = "";
        public string Label = "";
        public EffectCategories Categories;
        public bool HasTarget;
        public List<CardRef> Targets = new List<CardRef>();
    }
    public interface IActivationModeSelection { IEnumerable<AbilityMode> Modes(EffectContext context); }
    /// <summary>召唤即将成功时可以发动的对策，这个窗口不接受其他速度的效果。</summary>
    public interface ISummonNegation { }
    public interface IActivationNameDeclaration
    {
        RuleCardKind DeclaredKind { get; }
        IEnumerable<string> NameCandidates(EffectContext context);
    }
}
