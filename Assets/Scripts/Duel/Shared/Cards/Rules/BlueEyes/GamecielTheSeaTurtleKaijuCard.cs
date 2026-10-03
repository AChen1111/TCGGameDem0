using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class GamecielTheSeaTurtleKaijuCard : CardRules
    {
        public override string CardId => "55063751";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("55063751.1", CardRuleKind.SummonProcedure),
            CardRuleRequirement.Done("55063751.2", CardRuleKind.SummonProcedure),
            CardRuleRequirement.Done("55063751.3", CardRuleKind.Restriction),
            CardRuleRequirement.Done("55063751.4", CardRuleKind.ActivatedAbility));
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            yield return new BlueDamageProgram(CardId, 4, 2, new[] { DuelZone.Monster, DuelZone.ExtraMonster },
                c => c.State.Chain.Count > 0 && c.State.Chain.Last().Player != c.Player
                    && c.Card(c.State.Chain.Last().Source.InstanceId).CurrentNameId != "55063751"
                    && CounterCards(c).Sum(Count) >= 2, (c, link) =>
                {
                    if (link.Step != 0) return;
                    var previous = c.State.Chain.Single(item => item.Number == link.Values["kaiju-counter-link"]);
                    previous.ActivationNegated = true;
                    var source = c.State.Cards.FirstOrDefault(card => card.Ref.Equals(previous.Source));
                    if (source != null && c.IsAffected(source)) c.Move(source, DuelZone.Banished);
                    link.Step = 1;
                }, step => true).Cost(1, 2, CounterCards, (c, command, link) =>
                {
                    link.Values["kaiju-counter-link"] = c.State.Chain.Last().Number;
                    foreach (int id in command.Cards)
                    { var card = c.Card(id); card.Counters["kaiju"] -= command.Cards.Length == 1 ? 2 : 1; }
                }).Validate((c, command) => command.Cards.Length == 1 && Count(c.Card(command.Cards[0])) >= 2
                    || command.Cards.Length == 2 && command.Cards.All(id => Count(c.Card(id)) >= 1) ? "" : "INSUFFICIENT_KAIJU_COUNTERS");
        }
        static int Count(DuelCardState card) => card.Counters.TryGetValue("kaiju", out int count) ? count : 0;
        static IEnumerable<DuelCardState> CounterCards(EffectContext c) => c.State.Cards.Where(card => DuelEngine.OnField(card) && Count(card) > 0);
        public override bool AllowsMonsterZoneEntry(DuelCardState card, int controller, DuelState state, DuelCardCatalog catalog,
            IReadOnlyList<DuelCardState> leaving = null) =>
            !state.Cards.Any(other => other.InstanceId != card.InstanceId && other.Controller == controller
                && (other.Zone == DuelZone.Monster || other.Zone == DuelZone.ExtraMonster) && DuelEngine.IsPublic(other)
                && (leaving == null || !leaving.Contains(other)) && catalog.Get(other.DefinitionId).BelongsTo(0xd3));
        public override IEnumerable<ICardSummonProcedure> CreateSummonProcedures()
        { yield return new GamecielProcedure(true); yield return new GamecielProcedure(false); }
    }

    sealed class GamecielProcedure : ICardSummonProcedure
    {
        readonly bool m_opponent;
        public GamecielProcedure(bool opponent) { m_opponent = opponent; }
        public string Id => "55063751." + (m_opponent ? "1" : "2");
        static bool HasKaiju(EffectContext c, int player) => c.State.Cards.Any(card => card.Controller == player
            && (card.Zone == DuelZone.Monster || card.Zone == DuelZone.ExtraMonster) && DuelEngine.IsPublic(card)
            && c.Catalog.Get(card.DefinitionId).BelongsTo(0xd3));
        bool Available(EffectContext c) => c.Source.Zone == DuelZone.Hand
            && (m_opponent ? !HasKaiju(c, 1 - c.Player) : HasKaiju(c, 1 - c.Player) && !HasKaiju(c, c.Player));
        IEnumerable<DuelCardState> Tributes(EffectContext c) => c.State.Cards.Where(card => card.Controller != c.Player
            && (card.Zone == DuelZone.Monster || card.Zone == DuelZone.ExtraMonster));
        IEnumerable<int> Slots(EffectContext c, int tribute = 0) => Enumerable.Range(0, 5).Where(slot =>
            !c.State.Cards.Any(card => card.Zone == DuelZone.Monster && card.Controller == (m_opponent ? 1 - c.Player : c.Player)
                && card.Slot == slot && card.InstanceId != tribute));
        public IEnumerable<DuelAction> QueryActions(EffectContext c)
        {
            if (!Available(c)) yield break;
            var tributes = m_opponent ? Tributes(c).ToArray() : System.Array.Empty<DuelCardState>();
            if (m_opponent && tributes.Length == 0) yield break;
            var slots = m_opponent ? tributes.SelectMany(t => Slots(c, t.InstanceId)).Distinct().OrderBy(x => x).ToList() : Slots(c).ToList();
            if (slots.Count == 0) yield break;
            yield return new DuelAction { Id = "procedure:" + c.Source.InstanceId + ":" + Id, Kind = DuelCommandKind.SpecialSummon,
                AbilityId = Id, Card = c.SourceRef, Targets = tributes.Select(t => t.Ref).ToList(), Slots = slots,
                Positions = new List<CardPosition> { CardPosition.FaceUpAttack } };
        }
        public string Validate(EffectContext c, DuelCommand command) => Available(c) && command.Cards.Length == 0
            && command.Position == CardPosition.FaceUpAttack && Slots(c, m_opponent ? command.TargetId : 0).Contains(command.Slot)
            && (m_opponent ? Tributes(c).Any(t => t.InstanceId == command.TargetId) : command.TargetId == 0) ? "" : "INVALID_KAIJU_PROCEDURE";
        public void Execute(EffectContext c, DuelCommand command)
        {
            if (m_opponent) { var tribute = c.Card(command.TargetId); c.Engine.Move(tribute, DuelZone.Graveyard, tribute.Owner, cause: MoveCause.Tribute); }
            c.SpecialSummon(c.Source, m_opponent ? 1 - c.Player : c.Player, command.Slot,
                CardPosition.FaceUpAttack, method: SummonMethod.Procedure);
        }
    }
}
