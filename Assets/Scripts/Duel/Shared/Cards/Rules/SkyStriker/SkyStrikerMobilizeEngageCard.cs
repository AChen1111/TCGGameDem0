using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class SkyStrikerMobilizeEngageCard : CardRules
    {
        public override string CardId => "63166096";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("63166096.1", CardRuleKind.ActivatedAbility));
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            IEnumerable<DuelCardState> Candidates(EffectContext c) => c.Deck.Where(card =>
                SkyFlow.Striker(c, card) && c.Catalog.Get(card.DefinitionId).OriginalNameId != CardId);
            yield return new ProgramAbility(CardId, 1, 1, SkyFlow.SpellZones,
                c => SkyFlow.MainEmpty(c) && Candidates(c).Any() && !c.Engine.HasEffect(EffectRecordKind.PreventDeckToHand, c.Player),
                (c, link) =>
                {
                    if (!SkyFlow.Search(c, link, Candidates(c))) return;
                    if (link.Values["search-succeeded"] == 0) { link.Step = 4; return; }
                    if (link.Step == 2 && SkyFlow.GraveSpells(c) < 3) { link.Step = 4; return; }
                    SkyFlow.OptionalDraw(c, link, 2);
                }).Category(EffectCategories.AddFromDeckToHand);
        }
    }
}
