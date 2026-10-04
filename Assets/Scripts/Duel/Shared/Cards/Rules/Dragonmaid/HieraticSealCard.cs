using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class HieraticSealCard : CardRules
    {
        public override string CardId => "24361622";
        public override bool HasSummonRecipe => true;
        public override bool MatchesSummonMaterials(CardDefinition target, IReadOnlyList<DuelCardState> materials, DuelCardCatalog catalog) =>
            materials.Count == 2 && materials.All(card => DragonmaidFlow.IsDragon(catalog.Get(card.DefinitionId)));
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("24361622.1", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("24361622.2", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("24361622.summon", CardRuleKind.SummonProcedure),
            CardRuleRequirement.Done("24361622.restrictions", CardRuleKind.Restriction));
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            yield return new ProgramAbility(CardId, 1, 2, new[] { DuelZone.ExtraMonster },
                c => c.State.TurnPlayer != c.Player && Tributes(c).Any() && FaceUp(c).Any(),
                (c, link) =>
                {
                    if (link.Step != 0) return;
                    var target = c.State.Cards.FirstOrDefault(card => card.Ref.Equals(link.Targets[0]) && DuelEngine.OnField(card) && DuelEngine.IsPublic(card));
                    if (target != null && c.IsAffected(target)) c.Move(target, DuelZone.Hand);
                    link.Step = 1;
                }).Cost(1, 1, Tributes, (c, command, link) =>
                {
                    var material = c.Card(command.Cards[0]);
                    link.Costs.Add(material.Ref);
                    c.Move(material, DuelZone.Graveyard, cause: MoveCause.Tribute);
                }).Target(FaceUp).Once(CardId + ".1");
            yield return new TriggerProgramAbility(CardId, 2, new[] { DuelZone.Graveyard }, false, false,
                (c, fact) => fact.Kind == DuelEventKind.Moved && fact.Cause == MoveCause.Tribute && fact.Card.Equals(c.Source.Ref)
                    && (fact.From == DuelZone.Monster || fact.From == DuelZone.ExtraMonster),
                c => Dragons(c).Any(), (c, link) =>
                {
                    if (link.Step == 0)
                    {
                        var list = Dragons(c).ToArray();
                        link.Step = 1;
                        if (list.Length == 0) { link.Step = 8; return; }
                        c.SelectCards(link, list, "选择攻击力守备力变成0的龙族");
                        return;
                    }
                    if (link.Step == 1) { link.Values["picked"] = link.Selected[0]; link.Step = 2; }
                    if (link.Step < 5) DragonmaidFlow.ResumeSummon(c, link, 2, false, SummonMethod.Effect);
                    if (link.Step == 5)
                    {
                        var summoned = c.Card(link.Values["picked"]);
                        if (DuelEngine.OnField(summoned))
                        {
                            c.AddEffect(new DuelEffectRecord { Kind = EffectRecordKind.SetAttack, Target = summoned.Ref, Value = 0 });
                            c.AddEffect(new DuelEffectRecord { Kind = EffectRecordKind.SetDefense, Target = summoned.Ref, Value = 0 });
                        }
                        link.Step = 6;
                    }
                }).Once(CardId + ".2");
        }
        static IEnumerable<DuelCardState> Tributes(EffectContext context) => context.State.Cards.Where(card =>
            card.Controller == context.Player && context.Catalog.Get(card.DefinitionId).Kind == RuleCardKind.Monster
            && (card.Zone == DuelZone.Hand || DuelEngine.OnField(card)));
        static IEnumerable<DuelCardState> FaceUp(EffectContext context) =>
            context.State.Cards.Where(card => DuelEngine.OnField(card) && DuelEngine.IsPublic(card) && context.CanTarget(card));
        static IEnumerable<DuelCardState> Dragons(EffectContext context) =>
            DragonmaidFlow.Mine(context, DuelZone.Hand, DuelZone.Deck).Where(card =>
                DragonmaidFlow.IsDragon(context.Catalog.Get(card.DefinitionId)) && context.CanSpecialSummon(card, context.Player));
    }
}
