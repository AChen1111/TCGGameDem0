using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace AChen.Duel.Core
{
    internal sealed class HeroFusionSpellAbility : IAbilityHandler, IEffectCategoryProvider
    {
        readonly bool m_miracle;
        public string CardId { get; }
        public string AbilityId => CardId + ".1";
        public int Speed => 1;
        public EffectCategories Categories => m_miracle ? EffectCategories.BanishFromGraveyard : EffectCategories.None;
        public HeroFusionSpellAbility(string cardId, bool miracle = false) { CardId = cardId; m_miracle = miracle; }
        IEnumerable<DuelCardState> Materials(EffectContext context) => context.State.Cards.Where(card =>
            context.Catalog.Get(card.DefinitionId).Kind == RuleCardKind.Monster
            && (card.Controller == context.Player && (card.Zone == DuelZone.Monster || card.Zone == DuelZone.ExtraMonster)
                || card.Owner == context.Player && card.Zone == (m_miracle ? DuelZone.Graveyard : DuelZone.Hand)));
        IReadOnlyList<DuelCardState[]> Groups(EffectContext context, DuelCardState target) =>
            context.Engine.GetFusionMaterialGroups(target, context.Player, Materials(context), maxMaterials: 2);
        IEnumerable<DuelCardState> Targets(EffectContext context) => context.State.Cards.Where(card => card.Owner == context.Player
            && card.Zone == DuelZone.ExtraDeck && context.Catalog.Get(card.DefinitionId).MonsterType == RuleMonsterType.Fusion
            && (!m_miracle || context.Catalog.Get(card.DefinitionId).BelongsTo(0x3008)) && Groups(context, card).Count > 0);
        public bool CanActivate(EffectContext context) => Targets(context).Any();
        public string ValidateActivation(EffectContext context, DuelCommand command) =>
            command.Cards.Length == 0 && command.TargetId == 0 ? "" : "NO_ACTIVATION_TARGET";
        public void PayCost(EffectContext context, DuelCommand command, DuelChainLink link) { }
        public void Resolve(EffectContext context, DuelChainLink link)
        {
            if (link.Step == 0)
            {
                var candidates = Targets(context).ToArray();
                link.Step = 1;
                if (candidates.Length == 0) { link.Step = 5; return; }
                context.SelectCards(link, candidates, "选择融合怪兽");
                return;
            }
            if (link.Step == 1)
            {
                link.Values["fusion-target"] = link.Selected[0];
                var groups = Groups(context, context.Card(link.Selected[0]));
                var rules = context.Engine.Rules.Get(context.Card(link.Selected[0]).DefinitionId);
                var candidates = groups.SelectMany(group => group).Distinct().ToArray();
                link.Step = 2;
                context.SelectCards(link, candidates, "选择融合素材", rules.MinSummonMaterials,
                    System.Math.Min(rules.MaxSummonMaterials, candidates.Length));
                context.State.PendingDecision.MaterialRecipeId = rules.CardId;
                return;
            }
            if (link.Step == 2)
            {
                link.Values["material-count"] = link.Selected.Count;
                for (int index = 0; index < link.Selected.Count; index++) link.Values["material-" + index] = link.Selected[index];
                var consumed = new HashSet<int>(link.Selected);
                var slots = Enumerable.Range(0, 5).Where(slot => !context.State.Cards.Any(card => card.Zone == DuelZone.Monster
                    && card.Controller == context.Player && card.Slot == slot && !consumed.Contains(card.InstanceId)));
                if (!context.State.Cards.Any(card => card.Zone == DuelZone.ExtraMonster && card.Controller == context.Player
                    && !consumed.Contains(card.InstanceId)))
                    slots = slots.Concat(Enumerable.Range(5, 2).Where(slot => !context.State.Cards.Any(card =>
                        card.Zone == DuelZone.ExtraMonster && card.Slot == slot - 5 && !consumed.Contains(card.InstanceId))));
                link.Step = 3;
                context.OpenDecision(link, DecisionKind.ChooseZone, slots.Select(slot => new DecisionOption {
                    Id = slot.ToString(CultureInfo.InvariantCulture), Value = slot.ToString(CultureInfo.InvariantCulture),
                    Label = "区域 " + (slot + 1) }), "选择融合召唤区域");
                return;
            }
            if (link.Step == 3)
            {
                link.Values["fusion-slot"] = int.Parse(link.Answers[0], CultureInfo.InvariantCulture);
                link.Step = 4;
                context.OpenDecision(link, DecisionKind.ChoosePosition, new[] {
                    new DecisionOption { Id = "attack", Value = "1", Label = "攻击表示" },
                    new DecisionOption { Id = "defense", Value = "2", Label = "守备表示" } }, "选择表示");
                return;
            }
            if (link.Step == 4)
            {
                var materials = Enumerable.Range(0, link.Values["material-count"]).Select(index => context.Card(link.Values["material-" + index])).ToArray();
                context.Engine.FusionSummonByEffect(context.Card(link.Values["fusion-target"]), materials, context.Player,
                    link.Values["fusion-slot"], (CardPosition)int.Parse(link.Answers[0], CultureInfo.InvariantCulture), context.SourceRef, m_miracle);
                link.Step = 5;
            }
        }
    }
}
