using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class StrikerDragonCard : CardRules
    {
        public const int Rokket = 0x102;
        public override string CardId => "73539069";
        public override bool HasSummonRecipe => true;
        public override bool MatchesSummonMaterials(CardDefinition target, IReadOnlyList<DuelCardState> materials, DuelCardCatalog catalog) =>
            materials.Count == 1 && DragonmaidFlow.IsDragon(catalog.Get(materials[0].DefinitionId))
            && materials[0].CurrentLevel > 0 && materials[0].CurrentLevel <= 4;
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("73539069.1", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("73539069.2", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("73539069.summon", CardRuleKind.SummonProcedure),
            CardRuleRequirement.Done("73539069.restrictions", CardRuleKind.Restriction));
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            yield return new TriggerProgramAbility(CardId, 1, DragonmaidFlow.Field, false, false,
                (c, fact) => DragonmaidFlow.SummonedSelf(c, fact) && c.Source.SummonMethod == SummonMethod.Link,
                c => c.Deck.Any(card => card.DefinitionId == "36668118"),
                (c, link) => DragonmaidFlow.Pick(c, link, c.Deck.Where(card => card.DefinitionId == "36668118"), DuelZone.Hand, true))
                .Once(CardId + ".1");
            yield return new ProgramAbility(CardId, 2, 1, DragonmaidFlow.Field, c => Mine(c).Any() && Rokkets(c).Any(), (c, link) =>
            {
                if (link.Step == 0)
                {
                    link.Step = 1;
                    c.SelectCards(link, Mine(c), "选择破坏的自己怪兽");
                    return;
                }
                if (link.Step == 1)
                {
                    link.Values["own"] = link.Selected[0];
                    link.Step = 2;
                    c.SelectCards(link, Rokkets(c), "选择加入手卡的弹丸");
                    return;
                }
                if (link.Step != 2) return;
                var monster = c.State.Cards.FirstOrDefault(card => card.InstanceId == link.Values["own"] && DuelEngine.OnField(card));
                var rokket = c.State.Cards.FirstOrDefault(card => card.InstanceId == link.Selected[0] && card.Zone == DuelZone.Graveyard);
                if (monster != null && c.IsAffected(monster) && c.Destroy(monster) && rokket != null && c.IsAffected(rokket))
                {
                    c.Move(rokket, DuelZone.Hand);
                    c.Reveal(rokket);
                }
                link.Step = 3;
            }).Once(CardId + ".2");
        }
        static IEnumerable<DuelCardState> Mine(EffectContext context) =>
            DragonmaidFlow.FieldMonsters(context, context.Player).Where(context.CanTarget);
        static IEnumerable<DuelCardState> Rokkets(EffectContext context) =>
            DragonmaidFlow.Mine(context, DuelZone.Graveyard).Where(card =>
                context.Catalog.Get(card.DefinitionId).BelongsTo(Rokket) && context.CanTarget(card));
    }
}
