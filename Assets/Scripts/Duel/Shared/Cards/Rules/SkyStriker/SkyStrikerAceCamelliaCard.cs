using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class SkyStrikerAceCamelliaCard : CardRules
    {
        public override string CardId => "63013339";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("63013339.1", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("63013339.2", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("63013339.summon", CardRuleKind.SummonProcedure, "summon-materials"),
            CardRuleRequirement.Done("63013339.restrictions", CardRuleKind.Restriction));
        public override bool AllowsSummonMethod(DuelCardState card, int player, DuelState state, SummonMethod method) =>
            (card.Zone != DuelZone.ExtraDeck || method == SummonMethod.Link) && SkyFlow.OnceSpecialSummon(CardId, player, state);
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            IEnumerable<DuelCardState> Candidates(EffectContext c) => c.Deck.Where(card => SkyFlow.Striker(c, card));
            yield return new InstanceProgramAbility(CardId, 1, 1, SkyFlow.MonsterZones,
                c => SkyFlow.GraveSpells(c) <= 3 && Candidates(c).Any(), (c, link) =>
                {
                    if (link.Step == 0)
                    {
                        var cards = Candidates(c).ToArray(); link.Step = 1;
                        if (cards.Length == 0) { link.Step = 2; return; }
                        c.SelectCards(link, cards, "选择送去墓地的闪刀卡"); return;
                    }
                    if (link.Step == 1) { c.Move(c.Card(link.Selected[0]), DuelZone.Graveyard); c.ShuffleDeck(); link.Step = 2; }
                }).Category(EffectCategories.SendDeckToGraveyard);
            yield return new TriggerProgramAbility(CardId, 2, new[] { DuelZone.Graveyard }, false, false,
                (c, fact) => fact.Kind == DuelEventKind.Moved && fact.Card.Equals(c.Source.Ref) && fact.To == DuelZone.Graveyard,
                c => c.CanSpecialSummon(c.Source, 1 - c.Player), (c, link) =>
                {
                    if (link.Step < 3 && !EffectSummonFlow.Resume(c, link, c.Source, 0, controller: 1 - c.Player)) return;
                    if (link.Step == 3)
                    {
                        if (c.Source.Zone != DuelZone.Monster || c.Source.Controller != 1 - c.Player) { link.Step = 4; return; }
                        foreach (var card in c.State.Cards.Where(card => card.Ref.Equals(link.Targets[0])
                            && (card.Zone == DuelZone.Monster || card.Zone == DuelZone.ExtraMonster) && c.IsAffected(card)))
                            c.Move(card, DuelZone.Graveyard);
                        c.AddEffect(new DuelEffectRecord { Kind = EffectRecordKind.ReturnControl, Target = c.Source.Ref,
                            Player = c.Source.Owner, ExpiresTurn = c.State.Turn }); link.Step = 4;
                    }
                }).Target(c => c.State.Cards.Where(card => card.Controller == 1 - c.Player
                    && (card.Zone == DuelZone.Monster || card.Zone == DuelZone.ExtraMonster) && c.CanTarget(card)))
                .Category(EffectCategories.SpecialSummonFromGraveyard);
        }
        public override bool HasSummonRecipe => true;
        public override bool MatchesSummonMaterials(CardDefinition target, IReadOnlyList<DuelCardState> materials, DuelCardCatalog catalog)
        {
            var cards = materials.Select(card => catalog.Get(card.DefinitionId)).ToArray();
            return cards.Length == 2 && cards.All(c => !c.IsNormal);
        }
    }
}
