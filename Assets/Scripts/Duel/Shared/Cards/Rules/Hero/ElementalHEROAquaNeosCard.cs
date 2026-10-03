using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class ElementalHEROAquaNeosCard : CardRules
    {
        public override string CardId => "55171412";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("55171412.1", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("55171412.2", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("55171412.summon", CardRuleKind.SummonProcedure, "summon-materials"),
            CardRuleRequirement.Done("55171412.restrictions", CardRuleKind.Restriction));
        public override bool HasSummonRecipe => true;
        public override DuelZone ContactMaterialDestination => DuelZone.Deck;
        public override IReadOnlyList<string> NamedFusionMaterials => new[] { "89943723", "17955766" };
        public override int MinSummonMaterials => 2;
        public override int MaxSummonMaterials => 2;
        public override bool MatchesSummonMaterials(CardDefinition target, IReadOnlyList<DuelCardState> materials, DuelCardCatalog catalog) =>
            materials.Count == 2 && materials.Any(card => card.CurrentNameId == "89943723") && materials.Any(card => card.CurrentNameId == "17955766");
        public override bool AllowsSummonMethod(DuelCardState card, int player, DuelState state, SummonMethod method) =>
            method == SummonMethod.Contact;
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            yield return new InstanceProgramAbility(CardId, 1, 1, new[] { DuelZone.Monster, DuelZone.ExtraMonster },
                context => context.State.Cards.Any(card => card.Owner == 1 - context.Player && card.Zone == DuelZone.Hand),
                (context, link) => {
                    if (link.Step != 0) return;
                    if (!link.Values.ContainsKey("random-card"))
                    {
                        var hand = context.State.Cards.Where(card => card.Owner == 1 - context.Player && card.Zone == DuelZone.Hand).OrderBy(card => card.InstanceId).ToArray();
                        if (hand.Length == 0) { link.Step = 1; return; }
                        link.Values["random-card"] = hand[context.Engine.NextRandom((uint)hand.Length)].InstanceId;
                    }
                    var operation = context.DestroyMany(link, new[] { context.Card(link.Values["random-card"]) });
                    if (!operation.Completed) return;
                    link.Step = 1;
                }).Cost(1, 1, context => context.State.Cards.Where(card => card.Owner == context.Player && card.Zone == DuelZone.Hand),
                    (context, command, link) => { var card = context.Card(command.Cards[0]); link.Costs.Add(card.Ref); context.MoveAsCost(card, DuelZone.Graveyard); })
                .Once(CardId + ".1", false);
            yield return new PhaseTriggerProgramAbility(CardId, 2, new[] { DuelZone.Monster, DuelZone.ExtraMonster }, true, false,
                (context, fact) => fact.PhaseAtEvent == DuelPhase.End
                    && (fact.Kind == DuelEventKind.PhaseChanged || fact.Kind == DuelEventKind.Summoned && fact.Card.Equals(context.SourceRef)
                        || fact.Kind == DuelEventKind.Moved && fact.Card.Equals(context.SourceRef)
                            && (fact.To == DuelZone.Monster || fact.To == DuelZone.ExtraMonster)),
                context => context.State.Phase == DuelPhase.End,
                context => true, (context, link) => {
                    if (link.Step != 0) return;
                    if (context.Source.Ref.Equals(link.Source) && DuelEngine.OnField(context.Source)) context.Move(context.Source, DuelZone.ExtraDeck);
                    link.Step = 1;
                });
        }
    }
}
