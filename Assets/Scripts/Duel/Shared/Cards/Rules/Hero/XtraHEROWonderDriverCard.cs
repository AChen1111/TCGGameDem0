using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class XtraHEROWonderDriverCard : CardRules
    {
        public override string CardId => "01948619";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("01948619.1", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("01948619.2", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("01948619.summon", CardRuleKind.SummonProcedure, "summon-materials"),
            CardRuleRequirement.Done("01948619.restrictions", CardRuleKind.Restriction));
        public override bool HasSummonRecipe => true;
        public override bool MatchesSummonMaterials(CardDefinition target, IReadOnlyList<DuelCardState> materials, DuelCardCatalog catalog)
        {
            var cards = materials.Select(card => catalog.Get(card.DefinitionId)).ToArray();
            return cards.Length == 2 && cards.All(c => c.BelongsTo(0x8));
        }
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            yield return new TriggerProgramAbility(CardId, 1, new[] { DuelZone.Monster, DuelZone.ExtraMonster }, true, false,
                (context, fact) => fact.Kind == DuelEventKind.Summoned && fact.Player == context.Player && fact.SummonMethod != SummonMethod.Flip
                    && context.Catalog.Get(fact.DefinitionId).BelongsTo(0x8) && context.Engine.LinkedMonsters(context.Source).Any(card => card.Ref.Equals(fact.Card)),
                context => FreeSlots(context).Any(), (context, link) => {
                    var target = context.State.Cards.FirstOrDefault(card => card.Ref.Equals(link.Targets[0]) && card.Zone == DuelZone.Graveyard);
                    if (target == null) { link.Step = 2; return; }
                    if (link.Step == 0)
                    {
                        var slots = FreeSlots(context).ToArray(); link.Step = 1;
                        if (slots.Length == 0) { link.Step = 2; return; }
                        context.OpenDecision(link, DecisionKind.ChooseZone, slots.Select(slot => new DecisionOption {
                            Id = slot.ToString(), Value = slot.ToString(), Label = "区域 " + slot }), "选择盖放区域"); return;
                    }
                    if (link.Step == 1)
                    {
                        context.Engine.Move(target, DuelZone.SpellTrap, context.Player, int.Parse(link.Answers[0], System.Globalization.CultureInfo.InvariantCulture),
                            CardPosition.FaceDown, MoveCause.Effect, context.SourceRef, context.Player);
                        target.SetTurn = context.State.Turn; link.Step = 2;
                    }
                }).Target(context => context.State.Cards.Where(card => card.Owner == context.Player && card.Zone == DuelZone.Graveyard
                    && context.Catalog.Get(card.DefinitionId).Kind == RuleCardKind.Spell
                    && (context.Catalog.Get(card.DefinitionId).BelongsTo(0x46) || context.Catalog.Get(card.DefinitionId).BelongsTo(0xa5)
                        && context.Catalog.Get(card.DefinitionId).SpellTrapType == RuleSpellTrapType.QuickPlay))).Once(CardId + ".1");
            yield return new TriggerProgramAbility(CardId, 2, new[] { DuelZone.Graveyard }, false, false,
                (context, fact) => fact.Kind == DuelEventKind.Destroyed && fact.To == DuelZone.Graveyard && fact.Card.Equals(context.Source.Ref)
                    && (fact.Cause == MoveCause.Battle || fact.Cause == MoveCause.Effect && fact.EffectPlayer == 1 - fact.Before.Controller),
                context => HandHeroes(context).Any(), (context, link) => {
                    if (link.Step == 0)
                    {
                        var cards = HandHeroes(context).ToArray(); link.Step = 1;
                        if (cards.Length == 0) { link.Step = 4; return; }
                        context.SelectCards(link, cards, "选择手卡中的英雄"); return;
                    }
                    if (link.Step == 1) link.Values["chosen-hero"] = link.Selected[0];
                    if (link.Step >= 1 && link.Step <= 3) EffectSummonFlow.Resume(context, link, context.Card(link.Values["chosen-hero"]), 1);
                });
        }
        static IEnumerable<DuelCardState> HandHeroes(EffectContext context) => context.State.Cards.Where(card =>
            card.Owner == context.Player && card.Zone == DuelZone.Hand && context.Catalog.Get(card.DefinitionId).BelongsTo(0x8)
            && context.CanSpecialSummon(card, context.Player));
        static IEnumerable<int> FreeSlots(EffectContext context) => Enumerable.Range(0, 5).Where(slot =>
            !context.State.Cards.Any(card => card.Controller == context.Player && card.Zone == DuelZone.SpellTrap && card.Slot == slot));
    }
}
