using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class BlueEyesAlternativeWhiteDragonCard : CardRules
    {
        public override string CardId => "38517737";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("38517737.1", CardRuleKind.ContinuousRule),
            CardRuleRequirement.Done("38517737.2", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("38517737.summon", CardRuleKind.SummonProcedure),
            CardRuleRequirement.Done("38517737.restrictions", CardRuleKind.Restriction));
        public override IEnumerable<ICardSummonProcedure> CreateSummonProcedures() { yield return new AlternativeWhiteDragonProcedure(); }
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            yield return new InstanceProgramAbility(CardId, 2, 1, new[] { DuelZone.Monster, DuelZone.ExtraMonster },
                c => c.Source.AttacksThisTurn == 0, (c, link) =>
                {
                    if (link.Step != 0) return;
                    var target = c.State.Cards.FirstOrDefault(card => card.Ref.Equals(link.Targets[0])
                        && (card.Zone == DuelZone.Monster || card.Zone == DuelZone.ExtraMonster));
                    var operation = c.DestroyMany(link, target == null ? System.Array.Empty<DuelCardState>() : new[] { target });
                    if (operation.Completed) link.Step = 1;
                }).Target(c => c.State.Cards.Where(card => card.Controller != c.Player
                    && (card.Zone == DuelZone.Monster || card.Zone == DuelZone.ExtraMonster) && c.CanTarget(card)))
                .Pay((c, command, link) => c.AddEffect(new DuelEffectRecord { Kind = EffectRecordKind.CannotAttack,
                    Target = c.SourceRef, Player = c.Player, ExpiresTurn = c.State.Turn }));
        }
        public override IEnumerable<ICardContinuousRule> CreateContinuousRules()
        {
            yield return new ContinuousProgram(CardId + ".1", (c, records) =>
            {
                if ((DuelEngine.OnField(c.Source) || c.Source.Zone == DuelZone.Graveyard) && !c.Source.Negated)
                    records.Add(new DuelEffectRecord { Kind = EffectRecordKind.SetName, Target = c.SourceRef, NameId = "89631139" });
            });
        }
    }

    sealed class AlternativeWhiteDragonProcedure : ICardSummonProcedure
    {
        public string Id => "38517737.summon";
        string Key(EffectContext c) => c.Player + ":" + Id;
        IEnumerable<DuelCardState> RevealCandidates(EffectContext c) => c.State.Cards.Where(card => card.Controller == c.Player
            && card.Zone == DuelZone.Hand && card.CurrentNameId == "89631139");
        bool Available(EffectContext c) => c.Source.Zone == DuelZone.Hand && !c.State.UsedAbilities.ContainsKey(Key(c))
            && RevealCandidates(c).Any();
        public IEnumerable<DuelAction> QueryActions(EffectContext c)
        {
            if (!Available(c)) yield break;
            var slots = c.Engine.GetSpecialSummonDestinations(c.Source, c.Player).ToList();
            if (slots.Count == 0) yield break;
            yield return new DuelAction { Id = "procedure:" + c.Source.InstanceId + ":" + Id, Kind = DuelCommandKind.SpecialSummon,
                AbilityId = Id, Card = c.SourceRef, Slots = slots, Positions = new List<CardPosition> { CardPosition.FaceUpAttack, CardPosition.FaceUpDefense },
                SelectionCards = RevealCandidates(c).Select(card => card.Ref).ToList(), MinSelections = 1, MaxSelections = 1 };
        }
        public string Validate(EffectContext c, DuelCommand command) => Available(c) && command.Cards.Length == 1
            && RevealCandidates(c).Any(card => card.InstanceId == command.Cards[0]) && command.TargetId == 0
            && c.Engine.GetSpecialSummonDestinations(c.Source, c.Player).Contains(command.Slot)
            && (command.Position == CardPosition.FaceUpAttack || command.Position == CardPosition.FaceUpDefense) ? "" : "INVALID_ALTERNATIVE_PROCEDURE";
        public void Execute(EffectContext c, DuelCommand command)
        {
            c.Reveal(c.Card(command.Cards[0]));
            c.SpecialSummon(c.Source, c.Player, command.Slot, command.Position, method: SummonMethod.Procedure);
            c.State.UsedAbilities.Add(Key(c), 1);
        }
    }
}
