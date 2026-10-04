using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class SPLittleKnightCard : CardRules
    {
        public override string CardId => "29301450";
        public override bool HasSummonRecipe => true;
        public override bool MatchesSummonMaterials(CardDefinition target, IReadOnlyList<DuelCardState> materials, DuelCardCatalog catalog) =>
            materials.Count == 2 && materials.All(card => !catalog.Get(card.DefinitionId).IsNormal);
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("29301450.1", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("29301450.2", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("29301450.summon", CardRuleKind.SummonProcedure),
            CardRuleRequirement.Done("29301450.restrictions", CardRuleKind.Restriction));
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            yield return new TriggerProgramAbility(CardId, 1, DragonmaidFlow.Field, false, false,
                (c, fact) => DragonmaidFlow.SummonedSelf(c, fact) && c.Source.SummonMethod == SummonMethod.Link
                    && c.Source.SummonMaterialDefinitions.Any(id => Extra(c.Catalog.Get(id))),
                c => Banishes(c).Any(), (c, link) =>
                {
                    if (link.Step != 0) return;
                    var target = c.State.Cards.FirstOrDefault(card => card.Ref.Equals(link.Targets[0]));
                    if (target != null && c.IsAffected(target)) c.Move(target, DuelZone.Banished);
                    c.AddEffect(new DuelEffectRecord { Kind = EffectRecordKind.CannotDirectAttack, Player = c.Player, ExpiresTurn = c.State.Turn });
                    link.Step = 1;
                }).Target(Banishes).Once(CardId + ".1");
            yield return new ProgramAbility(CardId, 2, 2, DragonmaidFlow.Field,
                c => c.State.Chain.Count > 0 && c.State.Chain.Last().Player != c.Player && Mine(c).Any() && Face(c).Count() >= 2,
                (c, link) =>
                {
                    if (link.Step == 0)
                    {
                        link.Step = 1;
                        c.SelectCards(link, Mine(c), "选择自己场上要除外的怪兽");
                        return;
                    }
                    if (link.Step == 1)
                    {
                        link.Values["own"] = link.Selected[0];
                        link.Step = 2;
                        c.SelectCards(link, Face(c).Where(card => card.InstanceId != link.Values["own"]), "选择另一只要除外的怪兽");
                        return;
                    }
                    if (link.Step != 2) return;
                    foreach (int id in new[] { link.Values["own"], link.Selected[0] })
                    {
                        var card = c.Card(id);
                        if (!DuelEngine.OnField(card) || !c.IsAffected(card) || !c.TryMove(card, DuelZone.Banished)) continue;
                        c.AddEffect(new DuelEffectRecord { Kind = EffectRecordKind.DelayedReturn, Target = card.Ref,
                            Player = card.Controller, ExpiresTurn = c.State.Turn, ExpiresPhase = DuelPhase.End });
                    }
                    link.Step = 3;
                }).Once(CardId + ".2");
        }
        static bool Extra(CardDefinition card) => card.MonsterType == RuleMonsterType.Fusion || card.MonsterType == RuleMonsterType.Synchro
            || card.MonsterType == RuleMonsterType.Xyz || card.MonsterType == RuleMonsterType.Link;
        static IEnumerable<DuelCardState> Banishes(EffectContext context) => context.State.Cards.Where(card =>
            (DuelEngine.OnField(card) || card.Zone == DuelZone.Graveyard) && context.CanTarget(card));
        static IEnumerable<DuelCardState> Mine(EffectContext context) =>
            DragonmaidFlow.FieldMonsters(context, context.Player).Where(context.CanTarget);
        static IEnumerable<DuelCardState> Face(EffectContext context) => context.State.Cards.Where(card =>
            DuelEngine.OnField(card) && DuelEngine.IsPublic(card) && context.Catalog.Get(card.DefinitionId).Kind == RuleCardKind.Monster && context.CanTarget(card));
    }
}
