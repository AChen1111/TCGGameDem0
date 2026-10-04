using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class DragonmaidHospitalityCard : CardRules
    {
        public override string CardId => "78231356";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("78231356.1", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("78231356.restrictions", CardRuleKind.Restriction));
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            yield return new ProgramAbility(CardId, 1, 1, new[] { DuelZone.Hand }, c => Maids(c).Any(), (c, link) =>
            {
                if (link.Step == 0)
                {
                    var list = Maids(c).ToArray();
                    link.Step = 1;
                    if (list.Length == 0) { link.Step = 8; return; }
                    c.SelectCards(link, list, "选择特殊召唤的半龙女仆");
                    return;
                }
                if (link.Step == 1) { link.Values["picked"] = link.Selected[0]; link.Step = 2; }
                if (link.Step < 5)
                {
                    DragonmaidFlow.ResumeSummon(c, link, 2, true, SummonMethod.Effect);
                    if (link.Step < 5) return;
                }
                if (link.Step == 5)
                {
                    var summoned = c.Card(link.Values["picked"]);
                    var printed = c.Catalog.Get(summoned.DefinitionId);
                    var dumps = c.Deck.Where(card =>
                    {
                        var other = c.Catalog.Get(card.DefinitionId);
                        return DragonmaidFlow.IsMaid(other) && other.Attribute == printed.Attribute && other.Level != printed.Level;
                    }).ToArray();
                    link.Step = 6;
                    if (dumps.Length == 0) { link.Step = 8; return; }
                    c.OpenDecision(link, DecisionKind.ChooseMode, new[]
                    {
                        new DecisionOption { Id = "yes", Value = "yes", Label = "从卡组送墓" },
                        new DecisionOption { Id = "no", Value = "no", Label = "不送墓" }
                    }, "是否从卡组把半龙女仆送去墓地");
                    return;
                }
                if (link.Step == 6)
                {
                    if (link.Answers[0] != "yes") { link.Step = 8; return; }
                    var summoned = c.Card(link.Values["picked"]);
                    var printed = c.Catalog.Get(summoned.DefinitionId);
                    var dumps = c.Deck.Where(card =>
                    {
                        var other = c.Catalog.Get(card.DefinitionId);
                        return DragonmaidFlow.IsMaid(other) && other.Attribute == printed.Attribute && other.Level != printed.Level;
                    });
                    link.Step = 7;
                    c.SelectCards(link, dumps, "选择送去墓地的半龙女仆");
                    return;
                }
                if (link.Step == 7)
                {
                    c.Move(c.Card(link.Selected[0]), DuelZone.Graveyard);
                    c.ShuffleDeck();
                    link.Step = 8;
                }
            }).Category(EffectCategories.SpecialSummonFromGraveyard | EffectCategories.SendDeckToGraveyard).Once(CardId);
        }
        static IEnumerable<DuelCardState> Maids(EffectContext context) =>
            DragonmaidFlow.Mine(context, DuelZone.Hand, DuelZone.Graveyard).Where(card =>
                DragonmaidFlow.IsMaid(context.Catalog.Get(card.DefinitionId)) && context.CanSpecialSummon(card, context.Player));
    }
}
