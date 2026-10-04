using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace AChen.Duel.Core
{
    internal static class DragonmaidFlow
    {
        internal const int Maid = 307;
        internal const int Dragon = 8192;
        internal const int Light = 16;
        internal const int Dark = 32;
        internal static readonly DuelZone[] Field = { DuelZone.Monster, DuelZone.ExtraMonster };

        internal static bool IsMaid(CardDefinition card) => card.Kind == RuleCardKind.Monster && card.BelongsTo(Maid);
        internal static bool IsDragon(CardDefinition card) => card.Kind == RuleCardKind.Monster && card.Race == Dragon;
        internal static bool Attr(DuelCardState card, int bit) => (card.CurrentAttribute & bit) != 0;
        internal static bool Phase(DuelEvent fact, DuelPhase phase) =>
            fact.Kind == DuelEventKind.PhaseChanged && fact.Detail == phase.ToString();
        internal static bool SummonedSelf(EffectContext context, DuelEvent fact) =>
            fact.Kind == DuelEventKind.Summoned && fact.Card.Equals(context.Source.Ref);
        internal static bool LeftFieldToGrave(EffectContext context, DuelEvent fact) =>
            fact.Kind == DuelEventKind.Moved && fact.Card.Equals(context.Source.Ref) && fact.To == DuelZone.Graveyard
            && (fact.From == DuelZone.Monster || fact.From == DuelZone.ExtraMonster);
        internal static IEnumerable<DuelCardState> Mine(EffectContext context, params DuelZone[] zones) =>
            context.State.Cards.Where(card => card.Owner == context.Player && zones.Contains(card.Zone));
        internal static IEnumerable<DuelCardState> FieldMonsters(EffectContext context, int player) =>
            context.State.Cards.Where(card => card.Controller == player && DuelEngine.OnField(card) && DuelEngine.IsPublic(card));

        internal static void Pick(EffectContext context, DuelChainLink link, IEnumerable<DuelCardState> cards, DuelZone destination, bool fromDeck)
        {
            if (link.Step != 0 && link.Step != 1) return;
            if (link.Step == 0)
            {
                var list = cards.ToArray();
                link.Step = 1;
                if (list.Length == 0) { link.Step = 2; return; }
                context.SelectCards(link, list, destination == DuelZone.Hand ? "选择加入手卡的卡" : "选择送去墓地的卡");
                return;
            }
            var card = context.Card(link.Selected[0]);
            if (context.TryMove(card, destination, destination == DuelZone.Hand ? CardPosition.FaceDown : CardPosition.FaceUp))
            {
                if (destination == DuelZone.Hand) context.Reveal(card);
                if (fromDeck) context.ShuffleDeck();
            }
            link.Step = 2;
        }

        internal static void ReturnAndSummon(EffectContext context, DuelChainLink link,
            Func<EffectContext, IEnumerable<DuelCardState>> choices, bool defense)
        {
            if (link.Step == 0)
            {
                if (DuelEngine.OnField(context.Source)) context.Move(context.Source, DuelZone.Hand);
                var list = choices(context).Where(card => card.InstanceId != context.Source.InstanceId).ToArray();
                link.Step = 1;
                if (list.Length == 0) { link.Step = 8; return; }
                context.SelectCards(link, list, "选择特殊召唤的怪兽");
                return;
            }
            if (link.Step == 1)
            {
                link.Values["picked"] = link.Selected[0];
                link.Step = 2;
            }
            ResumeSummon(context, link, 2, defense, SummonMethod.Effect);
        }

        internal static void ResumeSummon(EffectContext context, DuelChainLink link, int step, bool defense, SummonMethod method)
        {
            if (link.Step < step || link.Step >= step + 3) return;
            var card = context.Card(link.Values["picked"]);
            if (link.Step == step)
            {
                var slots = context.Engine.GetSpecialSummonDestinations(card, context.Player).ToArray();
                link.Step++;
                if (slots.Length == 0) { link.Step = step + 3; return; }
                context.OpenDecision(link, DecisionKind.ChooseZone, slots.Select(slot => new DecisionOption
                {
                    Id = slot.ToString(CultureInfo.InvariantCulture),
                    Value = slot.ToString(CultureInfo.InvariantCulture),
                    Label = "区域 " + (slot + 1)
                }), "选择特殊召唤区域");
                return;
            }
            if (link.Step == step + 1)
            {
                int slot = int.Parse(link.Answers[0], CultureInfo.InvariantCulture);
                if (defense || context.Catalog.Get(card.DefinitionId).MonsterType == RuleMonsterType.Link)
                {
                    context.SpecialSummon(card, context.Player, slot,
                        context.Catalog.Get(card.DefinitionId).MonsterType == RuleMonsterType.Link
                            ? CardPosition.FaceUpAttack : CardPosition.FaceUpDefense, method: method);
                    link.Step = step + 3;
                    return;
                }
                link.Values["picked-slot"] = slot;
                link.Step++;
                context.OpenDecision(link, DecisionKind.ChoosePosition, new[]
                {
                    new DecisionOption { Id = "attack", Value = ((int)CardPosition.FaceUpAttack).ToString(CultureInfo.InvariantCulture), Label = "攻击表示" },
                    new DecisionOption { Id = "defense", Value = ((int)CardPosition.FaceUpDefense).ToString(CultureInfo.InvariantCulture), Label = "守备表示" }
                }, "选择表示");
                return;
            }
            context.SpecialSummon(card, context.Player, link.Values["picked-slot"],
                (CardPosition)int.Parse(link.Answers[0], CultureInfo.InvariantCulture), method: method);
            link.Step = step + 3;
        }

        internal static IEnumerable<DuelCardState> FusionMaterials(EffectContext context) => context.State.Cards.Where(card =>
            context.Catalog.Get(card.DefinitionId).Kind == RuleCardKind.Monster
            && (card.Controller == context.Player && DuelEngine.OnField(card) || card.Owner == context.Player && card.Zone == DuelZone.Hand));

        internal static IEnumerable<DuelCardState> FusionTargets(EffectContext context, Func<CardDefinition, bool> filter) =>
            context.State.Cards.Where(card => card.Owner == context.Player && card.Zone == DuelZone.ExtraDeck
                && filter(context.Catalog.Get(card.DefinitionId))
                && context.Engine.GetFusionMaterialGroups(card, context.Player, FusionMaterials(context), 2).Count > 0);

        internal static void Fusion(EffectContext context, DuelChainLink link, Func<CardDefinition, bool> filter)
        {
            if (link.Step == 0)
            {
                var candidates = FusionTargets(context, filter).ToArray();
                link.Step = 1;
                if (candidates.Length == 0) { link.Step = 5; return; }
                context.SelectCards(link, candidates, "选择融合怪兽");
                return;
            }
            if (link.Step == 1)
            {
                link.Values["fusion-target"] = link.Selected[0];
                var rules = context.Engine.Rules.Get(context.Card(link.Selected[0]).DefinitionId);
                var candidates = context.Engine.GetFusionMaterialGroups(context.Card(link.Selected[0]), context.Player, FusionMaterials(context), 2)
                    .SelectMany(group => group).Distinct().ToArray();
                link.Step = 2;
                context.SelectCards(link, candidates, "选择融合素材", rules.MinSummonMaterials,
                    Math.Min(rules.MaxSummonMaterials, candidates.Length));
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
                context.OpenDecision(link, DecisionKind.ChooseZone, slots.Select(slot => new DecisionOption
                {
                    Id = slot.ToString(CultureInfo.InvariantCulture),
                    Value = slot.ToString(CultureInfo.InvariantCulture),
                    Label = "区域 " + (slot + 1)
                }), "选择融合召唤区域");
                return;
            }
            if (link.Step == 3)
            {
                link.Values["fusion-slot"] = int.Parse(link.Answers[0], CultureInfo.InvariantCulture);
                link.Step = 4;
                context.OpenDecision(link, DecisionKind.ChoosePosition, new[]
                {
                    new DecisionOption { Id = "attack", Value = "1", Label = "攻击表示" },
                    new DecisionOption { Id = "defense", Value = "2", Label = "守备表示" }
                }, "选择表示");
                return;
            }
            if (link.Step == 4)
            {
                var materials = Enumerable.Range(0, link.Values["material-count"]).Select(index => context.Card(link.Values["material-" + index])).ToArray();
                context.Engine.FusionSummonByEffect(context.Card(link.Values["fusion-target"]), materials, context.Player,
                    link.Values["fusion-slot"], (CardPosition)int.Parse(link.Answers[0], CultureInfo.InvariantCulture), context.SourceRef);
                link.Step = 5;
            }
        }

        internal static IEnumerable<DuelCardState> FieldOrBanished(EffectContext context) => context.State.Cards.Where(card =>
            card.Owner == context.Player && context.Catalog.Get(card.DefinitionId).Kind == RuleCardKind.Monster
            && (DuelEngine.OnField(card) || card.Zone == DuelZone.Banished && card.Position != CardPosition.FaceDown));

        internal static IEnumerable<DuelCardState> ShuffleFusionTargets(EffectContext context, Func<CardDefinition, bool> filter) =>
            context.State.Cards.Where(card => card.Owner == context.Player && card.Zone == DuelZone.ExtraDeck
                && filter(context.Catalog.Get(card.DefinitionId))
                && context.Engine.GetFusionMaterialGroups(card, context.Player, FieldOrBanished(context), 2).Count > 0);

        internal static void FusionShuffle(EffectContext context, DuelChainLink link, Func<CardDefinition, bool> filter)
        {
            if (link.Step == 0)
            {
                var candidates = ShuffleFusionTargets(context, filter).ToArray();
                link.Step = 1;
                if (candidates.Length == 0) { link.Step = 5; return; }
                context.SelectCards(link, candidates, "选择融合怪兽");
                return;
            }
            if (link.Step == 1)
            {
                link.Values["fusion-target"] = link.Selected[0];
                var rules = context.Engine.Rules.Get(context.Card(link.Selected[0]).DefinitionId);
                var candidates = context.Engine.GetFusionMaterialGroups(context.Card(link.Selected[0]), context.Player, FieldOrBanished(context), 2)
                    .SelectMany(group => group).Distinct().ToArray();
                link.Step = 2;
                context.SelectCards(link, candidates, "选择回到卡组的融合素材", rules.MinSummonMaterials,
                    Math.Min(rules.MaxSummonMaterials, candidates.Length));
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
                context.OpenDecision(link, DecisionKind.ChooseZone, slots.Select(slot => new DecisionOption
                {
                    Id = slot.ToString(CultureInfo.InvariantCulture),
                    Value = slot.ToString(CultureInfo.InvariantCulture),
                    Label = "区域 " + (slot + 1)
                }), "选择融合召唤区域");
                return;
            }
            if (link.Step == 3)
            {
                link.Values["fusion-slot"] = int.Parse(link.Answers[0], CultureInfo.InvariantCulture);
                link.Step = 4;
                context.OpenDecision(link, DecisionKind.ChoosePosition, new[]
                {
                    new DecisionOption { Id = "attack", Value = "1", Label = "攻击表示" },
                    new DecisionOption { Id = "defense", Value = "2", Label = "守备表示" }
                }, "选择表示");
                return;
            }
            if (link.Step == 4)
            {
                var materials = Enumerable.Range(0, link.Values["material-count"]).Select(index => context.Card(link.Values["material-" + index])).ToArray();
                var names = materials.Select(card => card.DefinitionId).ToList();
                foreach (var material in materials) context.Move(material, DuelZone.Deck, CardPosition.FaceDown);
                context.ShuffleDeck();
                var target = context.Card(link.Values["fusion-target"]);
                if (context.SpecialSummon(target, context.Player, link.Values["fusion-slot"],
                    (CardPosition)int.Parse(link.Answers[0], CultureInfo.InvariantCulture), method: SummonMethod.Fusion))
                    target.SummonMaterialDefinitions = names;
                link.Step = 5;
            }
        }
    }

    internal sealed class DragonFusionSpell : IAbilityHandler
    {
        readonly Func<CardDefinition, bool> m_filter;
        public string CardId { get; }
        public string AbilityId { get; }
        public int Speed => 1;
        public DragonFusionSpell(string cardId, Func<CardDefinition, bool> filter)
        { CardId = cardId; AbilityId = cardId + ".1"; m_filter = filter; }
        public bool CanActivate(EffectContext context) => DragonmaidFlow.FusionTargets(context, m_filter).Any();
        public string ValidateActivation(EffectContext context, DuelCommand command) =>
            command.Cards.Length == 0 && command.TargetId == 0 ? "" : "NO_ACTIVATION_TARGET";
        public void PayCost(EffectContext context, DuelCommand command, DuelChainLink link) { }
        public void Resolve(EffectContext context, DuelChainLink link) => DragonmaidFlow.Fusion(context, link, m_filter);
    }
}
