using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class SageWithEyesOfBlueCard : CardRules
    {
        public override string CardId => "08240199";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("08240199.1", CardRuleKind.ActivatedAbility, "optional-trigger"),
            CardRuleRequirement.Done("08240199.2", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("08240199.restrictions", CardRuleKind.Restriction));
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            yield return new ProgramAbility(CardId, 2, 1, new[] { DuelZone.Hand }, c => BlueCandidates(c).Any(), (c, link) =>
            {
                if (link.Step >= 5) return;
                if (link.Step == 0)
                {
                    var target = c.State.Cards.FirstOrDefault(card => card.Ref.Equals(link.Targets[0])
                        && (card.Zone == DuelZone.Monster || card.Zone == DuelZone.ExtraMonster));
                    if (target == null || !c.IsAffected(target)) { link.Step = 5; return; }
                    c.Move(target, DuelZone.Graveyard);
                    var candidates = BlueCandidates(c).ToArray();
                    if (target.Zone != DuelZone.Graveyard || candidates.Length == 0) { link.Step = 5; return; }
                    link.Step = 1; c.SelectCards(link, candidates, "选择青眼怪兽特殊召唤"); return;
                }
                if (link.Step == 1) link.Values["selected-blue"] = link.Selected[0];
                if (EffectSummonFlow.Resume(c, link, c.Card(link.Values["selected-blue"]), 1) && link.Step == 4)
                { c.ShuffleDeck(); link.Step = 5; }
            }).Target(c => c.State.Cards.Where(card => card.Controller == c.Player
                && (card.Zone == DuelZone.Monster || card.Zone == DuelZone.ExtraMonster) && DuelEngine.IsPublic(card)
                && !c.Catalog.Get(card.DefinitionId).IsNormal && !c.Engine.HasEffect(EffectRecordKind.TreatAsNormal, c.Player, card)
                && c.CanTarget(card)))
                .Pay((c, command, link) => { link.Costs.Add(c.SourceRef); c.MoveAsCost(c.Source, DuelZone.Graveyard); })
                .Once(CardId + ".2").Category(EffectCategories.SpecialSummonFromDeck);
            yield return new TriggerProgramAbility(CardId, 1, new[] { DuelZone.Monster, DuelZone.ExtraMonster }, false, true,
                (c, fact) => fact.Kind == DuelEventKind.Summoned && fact.Card.Equals(c.SourceRef) && fact.SummonMethod == SummonMethod.Normal,
                c => Candidates(c).Any() && !c.Engine.HasEffect(EffectRecordKind.PreventDeckToHand, c.Player),
                (c, link) =>
                {
                    if (link.Step == 0)
                    {
                        link.Step = 1; var cards = Candidates(c).ToArray();
                        if (cards.Length > 0 && !c.Engine.HasEffect(EffectRecordKind.PreventDeckToHand, c.Player))
                            c.SelectCards(link, cards, "加入光属性等级一调整");
                        return;
                    }
                    if (link.Step != 1 || link.Selected.Count == 0) return;
                    var card = c.Card(link.Selected[0]); c.Move(card, DuelZone.Hand); c.Reveal(card); c.ShuffleDeck(); link.Step = 2;
                }).Category(EffectCategories.AddFromDeckToHand);
        }
        static IEnumerable<DuelCardState> Candidates(EffectContext c) => c.Deck.Where(card => card.DefinitionId != "08240199"
            && c.Catalog.Get(card.DefinitionId).IsTuner && c.Catalog.Get(card.DefinitionId).Level == 1
            && c.Catalog.Get(card.DefinitionId).Attribute == 16);
        static IEnumerable<DuelCardState> BlueCandidates(EffectContext c) => c.Deck.Where(card =>
            c.Catalog.Get(card.DefinitionId).BelongsTo(0xdd) && c.CanSpecialSummon(card, c.Player));
    }
}
