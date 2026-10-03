using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class SkyStrikerAceAzaleaTemperanceCard : CardRules
    {
        public override string CardId => "56741506";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("56741506.1", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("56741506.2", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("56741506.summon", CardRuleKind.SummonProcedure, "summon-materials"),
            CardRuleRequirement.Done("56741506.restrictions", CardRuleKind.Restriction));
        public override bool HasSummonRecipe => true;
        public override bool AllowsSummonMethod(DuelCardState card, int player, DuelState state, SummonMethod method) =>
            card.Zone != DuelZone.ExtraDeck || method == SummonMethod.Link;
        public override bool MatchesSummonMaterials(CardDefinition target, IReadOnlyList<DuelCardState> materials, DuelCardCatalog catalog)
        {
            var cards = materials.Select(card => catalog.Get(card.DefinitionId)).ToArray();
            return cards.Length >= 2 && cards.Any(c => c.MonsterType == RuleMonsterType.Link);
        }
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            yield return new TriggerProgramAbility(CardId, 1, new[] { DuelZone.Monster, DuelZone.ExtraMonster }, false, false,
                (context, fact) => HeroDeckTriggerAbility.SelfSummoned(context, fact) && fact.SummonMethod != SummonMethod.Normal,
                context => FreeSlots(context).Any(), (context, link) => {
                    if (link.Step >= 2) return;
                    var target = context.State.Cards.FirstOrDefault(card => card.Ref.Equals(link.Targets[0])
                        && (card.Zone == DuelZone.Monster || card.Zone == DuelZone.ExtraMonster) && DuelEngine.IsPublic(card));
                    if (target == null || !context.Source.Ref.Equals(link.Source) || !DuelEngine.OnField(context.Source)
                        || !context.IsAffected(target)) { link.Step = 2; return; }
                    if (link.Step == 0)
                    {
                        var slots = FreeSlots(context).ToArray(); link.Step = 1;
                        if (slots.Length == 0) { link.Step = 2; return; }
                        context.OpenDecision(link, DecisionKind.ChooseZone, slots.Select(slot => new DecisionOption {
                            Id = slot.ToString(), Value = slot.ToString(), Label = "装备区域 " + slot }), "选择装备区域"); return;
                    }
                    if (context.Engine.TryMove(target, DuelZone.SpellTrap, context.Player,
                        int.Parse(link.Answers[0], System.Globalization.CultureInfo.InvariantCulture), CardPosition.FaceUp,
                        MoveCause.Effect, link.Source, context.Player)) target.EquipTarget = link.Source;
                    link.Step = 2;
                }).Target(context => context.State.Cards.Where(card => card.Controller == 1 - context.Player
                    && (card.Zone == DuelZone.Monster || card.Zone == DuelZone.ExtraMonster) && DuelEngine.IsPublic(card)
                    && card.CurrentAtk <= 2500 && context.CanTarget(card)))
                .Cost(1, 1, context => context.State.Cards.Where(card => card.Owner == context.Player
                    && (card.Zone == DuelZone.Hand || card.Zone == DuelZone.Graveyard)
                    && context.Catalog.Get(card.DefinitionId).Kind == RuleCardKind.Spell), HeroChoiceOperations.BanishCost)
                .Once(CardId + ".1");
            yield return new TriggerProgramAbility(CardId, 2, new[] { DuelZone.Graveyard, DuelZone.Banished }, false, true,
                (context, fact) => fact.Kind == DuelEventKind.Destroyed && fact.Cause == MoveCause.Battle && fact.Card.Equals(context.SourceRef),
                context => SummonChoices(context).Any(), (context, link) => {
                    if (link.Step == 0)
                    {
                        var choices = SummonChoices(context).ToArray(); link.Step = 1;
                        if (choices.Length == 0) { link.Step = 4; return; }
                        context.SelectCards(link, choices, "选择手卡或墓地的闪刀怪兽"); return;
                    }
                    if (link.Step == 1) link.Values["summon-card"] = link.Selected[0];
                    if (link.Step >= 1 && link.Step <= 3) EffectSummonFlow.Resume(context, link, context.Card(link.Values["summon-card"]), 1);
                }).Category(EffectCategories.SpecialSummonFromGraveyard);
        }
        static IEnumerable<DuelCardState> SummonChoices(EffectContext context) => context.State.Cards.Where(card =>
            card.Owner == context.Player && card.DefinitionId != "56741506" && (card.Zone == DuelZone.Hand || card.Zone == DuelZone.Graveyard)
            && context.Catalog.Get(card.DefinitionId).Kind == RuleCardKind.Monster && context.Catalog.Get(card.DefinitionId).BelongsTo(0x115)
            && context.CanSpecialSummon(card, context.Player));
        static IEnumerable<int> FreeSlots(EffectContext context) => Enumerable.Range(0, 5).Where(slot =>
            !context.State.Cards.Any(card => card.Controller == context.Player && card.Zone == DuelZone.SpellTrap && card.Slot == slot));
    }
}
