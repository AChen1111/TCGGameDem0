using System;
using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    /// <summary>公共选择处理与逐卡诱发条件分离；每个卡类配置自身条件与使用限制。</summary>
    internal sealed class HeroDeckTriggerAbility : IAbilityHandler, ITriggeredAbility, IActivationSourcePolicy,
        IActivationUsageLimit, IEffectCategoryProvider
    {
        readonly Func<EffectContext, IEnumerable<DuelCardState>> m_candidates;
        readonly Func<EffectContext, DuelEvent, bool> m_trigger;
        readonly Func<EffectContext, bool> m_source;
        readonly DuelZone m_destination;
        public string CardId { get; }
        public string AbilityId { get; }
        public int Speed => 1;
        public bool Mandatory => false;
        public bool OptionalWhen { get; }
        public string UsageKey { get; }
        public int Limit => 1;
        public bool CountNegatedActivation => true;
        public EffectCategories Categories { get; }
        public HeroDeckTriggerAbility(string cardId, int number,
            Func<EffectContext, IEnumerable<DuelCardState>> candidates, DuelZone destination,
            Func<EffectContext, DuelEvent, bool> trigger, Func<EffectContext, bool> source,
            EffectCategories categories, string usageKey = "", bool optionalWhen = false)
        {
            CardId = cardId; AbilityId = cardId + "." + number;
            m_candidates = candidates; m_destination = destination; m_trigger = trigger; m_source = source;
            Categories = categories; UsageKey = usageKey.Length == 0 ? AbilityId : usageKey; OptionalWhen = optionalWhen;
        }
        public bool AllowsSource(EffectContext context) => m_source(context);
        IEnumerable<DuelCardState> Available(EffectContext context) => m_candidates(context).Where(card =>
            m_destination != DuelZone.Hand || card.Zone != DuelZone.Deck
            || !context.Engine.HasEffect(EffectRecordKind.PreventDeckToHand, context.Player));
        public bool CanActivate(EffectContext context) => AllowsSource(context) && Available(context).Any();
        public bool IsTriggered(EffectContext context, DuelEvent fact) => m_trigger(context, fact);
        public string ValidateActivation(EffectContext context, DuelCommand command) =>
            command.Cards.Length == 0 && command.TargetId == 0 ? "" : "NO_ACTIVATION_TARGET";
        public void PayCost(EffectContext context, DuelCommand command, DuelChainLink link) { }
        public void Resolve(EffectContext context, DuelChainLink link)
        {
            if (link.Step == 0)
            {
                var candidates = Available(context).ToArray();
                link.Step = 1;
                if (candidates.Length == 0) { link.Step = 2; return; }
                context.SelectCards(link, candidates, m_destination == DuelZone.Hand ? "选择加入手卡的卡" : "选择送去墓地的卡");
                return;
            }
            if (link.Step == 1)
            {
                var selected = context.Card(link.Selected[0]);
                bool fromDeck = selected.Zone == DuelZone.Deck;
                context.Move(selected, m_destination, m_destination == DuelZone.Hand ? CardPosition.FaceDown : CardPosition.FaceUp);
                if (m_destination == DuelZone.Hand && selected.Zone == DuelZone.Hand) context.Reveal(selected);
                if (fromDeck) context.ShuffleDeck();
                link.Step = 2;
            }
        }
        public static bool FaceUpMonster(EffectContext context) =>
            (context.Source.Zone == DuelZone.Monster || context.Source.Zone == DuelZone.ExtraMonster) && DuelEngine.IsPublic(context.Source);
        public static bool SelfSummoned(EffectContext context, DuelEvent fact) =>
            fact.Kind == DuelEventKind.Summoned && fact.Card.Equals(context.Source.Ref);
    }
}
