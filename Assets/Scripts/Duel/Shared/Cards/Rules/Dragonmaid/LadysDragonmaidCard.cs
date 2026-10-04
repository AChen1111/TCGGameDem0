using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class LadysDragonmaidCard : CardRules
    {
        public override string CardId => "48658295";
        public override bool AllowsSummonMethod(DuelCardState card, int player, DuelState state, SummonMethod method) =>
            method == SummonMethod.Procedure || method == SummonMethod.Effect && card.Zone != DuelZone.ExtraDeck && card.ProperlySummoned;
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("48658295.1", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("48658295.2", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("48658295.summon", CardRuleKind.SummonProcedure),
            CardRuleRequirement.Done("48658295.restrictions", CardRuleKind.Restriction));
        public override IEnumerable<ICardSummonProcedure> CreateSummonProcedures() { yield return new LadysDragonmaidProcedure(); }
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            yield return new TriggerProgramAbility(CardId, 1, DragonmaidFlow.Field, false, false, DragonmaidFlow.SummonedSelf,
                c => Small(c).Any(), (c, link) =>
                {
                    if (link.Step == 0)
                    {
                        var list = Small(c).ToArray();
                        link.Step = 1;
                        if (list.Length == 0) { link.Step = 8; return; }
                        c.SelectCards(link, list, "选择特殊召唤的半龙女仆");
                        return;
                    }
                    if (link.Step == 1) { link.Values["picked"] = link.Selected[0]; link.Step = 2; }
                    DragonmaidFlow.ResumeSummon(c, link, 2, false, SummonMethod.Effect);
                }).Category(EffectCategories.SpecialSummonFromDeck).Once(CardId + ".1");
            yield return new TriggerProgramAbility(CardId, 2, DragonmaidFlow.Field, false, false,
                (c, fact) => DragonmaidFlow.Phase(fact, DuelPhase.Standby),
                c => DragonmaidFlow.ShuffleFusionTargets(c, card => card.MonsterType == RuleMonsterType.Fusion && card.Race == DragonmaidFlow.Dragon).Any(),
                (c, link) => DragonmaidFlow.FusionShuffle(c, link, card => card.MonsterType == RuleMonsterType.Fusion && card.Race == DragonmaidFlow.Dragon))
                .Once(CardId + ".2");
        }
        static IEnumerable<DuelCardState> Small(EffectContext context) => context.Deck.Where(card =>
        {
            var printed = context.Catalog.Get(card.DefinitionId);
            return DragonmaidFlow.IsMaid(printed) && printed.Level > 0 && printed.Level <= 4 && context.CanSpecialSummon(card, context.Player);
        });
    }

    sealed class LadysDragonmaidProcedure : ICardSummonProcedure
    {
        public string Id => "48658295.summon";
        static bool Pair(EffectContext context, DuelCardState field, DuelCardState grave)
        {
            var left = context.Catalog.Get(field.DefinitionId);
            var right = context.Catalog.Get(grave.DefinitionId);
            return DragonmaidFlow.IsMaid(left) && DragonmaidFlow.IsMaid(right) && field.CurrentAttribute == grave.CurrentAttribute
                && left.Level != right.Level;
        }
        static IEnumerable<DuelCardState> Field(EffectContext context) => DragonmaidFlow.FieldMonsters(context, context.Player);
        static IEnumerable<DuelCardState> Grave(EffectContext context) => DragonmaidFlow.Mine(context, DuelZone.Graveyard);
        bool Available(EffectContext context) => context.Source.Zone == DuelZone.ExtraDeck
            && Field(context).Any(field => Grave(context).Any(grave => Pair(context, field, grave)))
            && context.Engine.GetSpecialSummonDestinations(context.Source, context.Player).Count > 0;
        public IEnumerable<DuelAction> QueryActions(EffectContext context)
        {
            if (!Available(context)) yield break;
            yield return new DuelAction { Id = "procedure:" + context.Source.InstanceId + ":" + Id, Kind = DuelCommandKind.SpecialSummon,
                AbilityId = Id, Card = context.Source.Ref, Slots = context.Engine.GetSpecialSummonDestinations(context.Source, context.Player).ToList(),
                Positions = new List<CardPosition> { CardPosition.FaceUpAttack, CardPosition.FaceUpDefense },
                SelectionCards = Field(context).Concat(Grave(context)).Select(card => card.Ref).Distinct().ToList(), MinSelections = 2, MaxSelections = 2 };
        }
        public string Validate(EffectContext context, DuelCommand command)
        {
            if (!Available(context) || command.Cards.Length != 2) return "INVALID_LADY_MATERIALS";
            var cards = command.Cards.Select(context.Card).ToArray();
            var field = cards.FirstOrDefault(card => DuelEngine.OnField(card) && card.Controller == context.Player);
            var grave = cards.FirstOrDefault(card => card.Zone == DuelZone.Graveyard && card.Owner == context.Player);
            return field != null && grave != null && field.InstanceId != grave.InstanceId && Pair(context, field, grave)
                && context.Engine.GetSpecialSummonDestinations(context.Source, context.Player).Contains(command.Slot) ? "" : "INVALID_LADY_MATERIALS";
        }
        public void Execute(EffectContext context, DuelCommand command)
        {
            foreach (int id in command.Cards) context.Move(context.Card(id), DuelZone.Banished);
            context.SpecialSummon(context.Source, context.Player, command.Slot, command.Position, method: SummonMethod.Procedure);
        }
    }
}
