using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class ElementalHEROFlameWingmanInfernalRageCard : CardRules
    {
        public override string CardId => "93347961";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("93347961.1", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("93347961.2", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("93347961.summon", CardRuleKind.SummonProcedure, "summon-materials"),
            CardRuleRequirement.Done("93347961.restrictions", CardRuleKind.Restriction));
        public override bool HasSummonRecipe => true;
        public override int MinSummonMaterials => 2;
        public override int MaxSummonMaterials => 2;
        public override bool MatchesSummonMaterials(CardDefinition target, IReadOnlyList<DuelCardState> materials, DuelCardCatalog catalog) =>
            materials.Count == 2 && materials.All(card => catalog.Get(card.DefinitionId).BelongsTo(0x3008)) && materials[0].CurrentAttribute != materials[1].CurrentAttribute;
        public override bool AllowsSummonMethod(DuelCardState card, int player, DuelState state, SummonMethod method) =>
            method == SummonMethod.Fusion;
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            yield return new HeroDeckTriggerAbility(CardId, 1, context => context.State.Cards.Where(card =>
                card.Owner == context.Player && (card.Zone == DuelZone.Deck || card.Zone == DuelZone.Graveyard)
                && context.Catalog.Get(card.DefinitionId).BelongsTo(0x194)), DuelZone.Hand,
                (context, fact) => HeroDeckTriggerAbility.SelfSummoned(context, fact) && fact.SummonMethod != SummonMethod.Normal,
                HeroDeckTriggerAbility.FaceUpMonster, EffectCategories.AddFromDeckToHand | EffectCategories.AddFromGraveyardToHandDeckExtra);
            yield return new ProgramAbility(CardId, 2, 1, new[] { DuelZone.Monster, DuelZone.ExtraMonster },
                context => context.Source.SummonMethod == SummonMethod.Fusion && context.Source.SummonMaterialDefinitions.Any(id =>
                    context.Catalog.Get(id).IsNormal) && SummonChoices(context).Any(), (context, link) => {
                    if (link.Step >= 5) return;
                    if (link.Step == 0)
                    {
                        var choices = SummonChoices(context).ToArray(); link.Step = 1;
                        if (choices.Length == 0) { link.Step = 5; return; }
                        context.SelectCards(link, choices, "选择无视条件特殊召唤的元素英雄"); return;
                    }
                    if (link.Step == 1)
                    { link.Values["selected-card"] = link.Selected[0]; link.Values["from-deck"] = context.Card(link.Selected[0]).Zone == DuelZone.Deck ? 1 : 0; }
                    if (EffectSummonFlow.Resume(context, link, context.Card(link.Values["selected-card"]), 1, ignore: true) && link.Step == 4)
                    { if (link.Values["from-deck"] == 1) context.ShuffleDeck(); link.Step = 5; }
                }).Pay((context, command, link) => { link.Costs.Add(context.SourceRef); context.MoveAsCost(context.Source, DuelZone.Graveyard); })
                .Once(CardId + ".2").Category(EffectCategories.SpecialSummonFromDeck);
        }
        static IEnumerable<DuelCardState> SummonChoices(EffectContext context) => context.State.Cards.Where(card =>
            card.Owner == context.Player && (card.Zone == DuelZone.Deck || card.Zone == DuelZone.ExtraDeck)
            && context.Catalog.Get(card.DefinitionId).BelongsTo(0x3008) && context.Catalog.Get(card.DefinitionId).Level <= 7
            && !context.Catalog.Get(card.DefinitionId).CanNormalSummon
            && context.Engine.CanSpecialSummonConditions(card, context.Player, ignore: true)
            && (context.Engine.GetSpecialSummonDestinations(card, context.Player).Any() || context.Source.Zone == DuelZone.Monster
                || card.Zone == DuelZone.ExtraDeck && context.Source.Zone == DuelZone.ExtraMonster));
    }
}
