using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class BystialMagnamhutCard : CardRules
    {
        public override string CardId => "33854624";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("33854624.1", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("33854624.2", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("33854624.restrictions", CardRuleKind.Restriction));
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            yield return new ProgramAbility(CardId, 1, 2, new[] { DuelZone.Hand },
                c => Lights(c).Any() && (c.State.TurnPlayer == c.Player || DragonmaidFlow.FieldMonsters(c, 1 - c.Player).Any())
                    && c.Engine.GetSpecialSummonDestinations(c.Source, c.Player).Count > 0,
                (c, link) =>
                {
                    if (link.Step == 0)
                    {
                        var target = c.State.Cards.FirstOrDefault(card => card.Ref.Equals(link.Targets[0]) && card.Zone == DuelZone.Graveyard);
                        if (target != null) c.Move(target, DuelZone.Banished);
                        link.Values["picked"] = c.Source.InstanceId;
                        link.Step = 1;
                    }
                    DragonmaidFlow.ResumeSummon(c, link, 1, false, SummonMethod.Effect);
                }).Target(Lights).Once(CardId + ".1");
            yield return new TriggerProgramAbility(CardId, 2, DragonmaidFlow.Field.Concat(new[] { DuelZone.Graveyard }).ToArray(), false, false,
                (c, fact) => DragonmaidFlow.Phase(fact, DuelPhase.End) && c.Source.SummonedTurn == c.State.Turn,
                c => Dragons(c).Any() && !c.Engine.HasEffect(EffectRecordKind.PreventDeckToHand, c.Player),
                (c, link) =>
                {
                    if (link.Step == 0)
                    {
                        var list = Dragons(c).ToArray();
                        link.Step = 1;
                        if (list.Length == 0) { link.Step = 2; return; }
                        c.SelectCards(link, list, "选择加入手卡的龙族");
                        return;
                    }
                    if (link.Step != 1) return;
                    var card = c.Card(link.Selected[0]);
                    bool deck = card.Zone == DuelZone.Deck;
                    if (c.TryMove(card, DuelZone.Hand, CardPosition.FaceDown))
                    {
                        c.Reveal(card);
                        if (deck) c.ShuffleDeck();
                    }
                    link.Step = 2;
                }).Category(EffectCategories.AddFromDeckToHand).Once(CardId + ".2");
        }
        static IEnumerable<DuelCardState> Lights(EffectContext context) => context.State.Cards.Where(card =>
            card.Zone == DuelZone.Graveyard && context.Catalog.Get(card.DefinitionId).Kind == RuleCardKind.Monster
            && (DragonmaidFlow.Attr(card, DragonmaidFlow.Light) || DragonmaidFlow.Attr(card, DragonmaidFlow.Dark)) && context.CanTarget(card));
        static IEnumerable<DuelCardState> Dragons(EffectContext context) =>
            DragonmaidFlow.Mine(context, DuelZone.Deck, DuelZone.Graveyard).Where(card =>
                card.InstanceId != context.Source.InstanceId && DragonmaidFlow.IsDragon(context.Catalog.Get(card.DefinitionId)));
    }
}
