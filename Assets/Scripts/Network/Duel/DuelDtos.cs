using System;
using UnityEngine.Scripting;
using Core = AChen.Duel.Core;

namespace AChen.Duel.Client
{
    [Preserve] public sealed class DuelRoomPlayerDto
    {
        public Guid UserId { get; set; }
        public int Seat { get; set; }
        public bool Ready { get; set; }
        public bool HasDeck { get; set; }
        public string Nickname { get; set; } = "";
        public int AvatarId { get; set; }
        public int AvatarFrameId { get; set; }
    }
    [Preserve] public sealed class DuelRoomSummaryDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string HostNickname { get; set; }
        public int HostAvatarId { get; set; }
        public int HostAvatarFrameId { get; set; }
        public int PlayerCount { get; set; }
    }
    [Preserve] public sealed class DuelDeckDto
    {
        public string[] MainDeck { get; set; }
        public string[] ExtraDeck { get; set; }
    }
    [Preserve] public sealed class DuelRoomDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Status { get; set; }
        public int LocalSeat { get; set; }
        public long Sequence { get; set; }
        public DuelRoomPlayerDto[] Players { get; set; } = Array.Empty<DuelRoomPlayerDto>();
        public DuelDeckDto Deck { get; set; }
        public DuelSeatDto Duel { get; set; }
        public double[] RemainingSeconds { get; set; } = { 180, 180 };
        public bool Paused { get; set; }
        public int ProtocolVersion { get; set; }
        public string RuleVersion { get; set; }
        public string RulePackageHash { get; set; }
    }
    [Preserve] public sealed class DuelPlayerSnapshotDto
    {
        public int LifePoints { get; set; }
        public int HandCount { get; set; }
        public int DeckCount { get; set; }
        public int ExtraDeckCount { get; set; }
    }
    [Preserve] public sealed class DuelCardDto
    {
        public string ViewCardId { get; set; } = "";
        public string DefinitionId { get; set; } = "";
        public int Owner { get; set; }
        public int Controller { get; set; }
        public Core.DuelZone Zone { get; set; }
        public int Slot { get; set; }
        public Core.CardPosition Position { get; set; }
        public int? Attack { get; set; }
        public int? Defense { get; set; }
        public bool Negated { get; set; }
        public int Level { get; set; }
        public string HostViewCardId { get; set; } = "";
        public int MaterialCount { get; set; }
    }
    [Preserve] public sealed class DuelActionDto
    {
        public string ActionToken { get; set; }
        public Core.DuelCommandKind Kind { get; set; }
        public string SourceViewCardId { get; set; } = "";
        public string AbilityId { get; set; }
        public Core.DuelPhase Phase { get; set; }
        public string[] TargetViewCardIds { get; set; } = Array.Empty<string>();
        public string[] SelectionViewCardIds { get; set; } = Array.Empty<string>();
        public int MinSelections { get; set; }
        public int MaxSelections { get; set; }
        public int[] Slots { get; set; } = Array.Empty<int>();
        public bool CanAttackDirectly { get; set; }
        public Core.CardPosition[] Positions { get; set; } = Array.Empty<Core.CardPosition>();
        public string Label { get; set; }
        public bool RequiresNameDeclaration { get; set; }
        public Core.RuleCardKind DeclarationKind { get; set; }
        public bool SelectionIsTarget { get; set; }
    }
    [Preserve] public sealed class DuelOptionDto
    {
        public Core.DuelZone DestinationZone { get; set; }
        public int Slot { get; set; } = -1;
        public string ViewCardId { get; set; } = "";
        public string OptionToken { get; set; }
        public string Label { get; set; }
        public string DefinitionId { get; set; } = "";
    }
    [Preserve] public sealed class DuelDecisionDto
    {
        public Core.DecisionKind Kind { get; set; }
        public string Prompt { get; set; }
        public int Min { get; set; }
        public int Max { get; set; }
        public bool CanCancel { get; set; }
        public DuelOptionDto[] Options { get; set; }
    }
    [Preserve] public sealed class DuelChainDto
    {
        public int Number { get; set; }
        public int Player { get; set; }
        public string DefinitionId { get; set; }
        public string AbilityId { get; set; }
        public bool ActivationNegated { get; set; }
        public bool EffectNegated { get; set; }
    }
    [Preserve] public sealed class DuelSeatDto
    {
        public int Seat { get; set; }
        public long Revision { get; set; }
        public int Turn { get; set; }
        public int TurnPlayer { get; set; }
        public Core.DuelPhase Phase { get; set; }
        public Core.TimingWindow Window { get; set; }
        public int WaitingSeat { get; set; }
        public bool Finished { get; set; }
        public int Winner { get; set; }
        public string EndReason { get; set; }
        public DuelPlayerSnapshotDto[] Players { get; set; }
        public DuelCardDto[] Cards { get; set; }
        public DuelChainDto[] Chain { get; set; } = Array.Empty<DuelChainDto>();
        public DuelDecisionDto Decision { get; set; }
        public DuelActionDto[] Actions { get; set; } = Array.Empty<DuelActionDto>();
        public string[] MainDeckDefinitions { get; set; } = Array.Empty<string>();
    }
    [Preserve] public sealed class DuelEventDto
    {
        public string PreviousViewCardId { get; set; } = "";
        public long EventId { get; set; }
        public Core.DuelEventKind Kind { get; set; }
        public int Player { get; set; }
        public string ViewCardId { get; set; } = "";
        public string DefinitionId { get; set; } = "";
        public Core.DuelZone From { get; set; }
        public Core.DuelZone To { get; set; }
        public int Amount { get; set; }
        public int? Attack { get; set; }
        public int? Defense { get; set; }
        public string DeclaredNameId { get; set; }
        public int Turn { get; set; }
        public Core.DuelPhase Phase { get; set; }
        public long ChainId { get; set; }
        public int LinkNumber { get; set; }
        public string AttackerViewCardId { get; set; } = "";
        public string TargetViewCardId { get; set; } = "";
        public DuelCardDto Origin { get; set; }
        public int[] ImpactLifePoints { get; set; } = Array.Empty<int>();
    }
    [Preserve] public sealed class DuelFrameDto
    {
        public DuelEventDto Event { get; set; }
        public DuelSeatDto Snapshot { get; set; }
    }
    [Preserve] public sealed class DuelReceiptDto
    {
        public Guid RequestId { get; set; }
        public bool Accepted { get; set; }
        public string Error { get; set; }
        public long Sequence { get; set; }
        public long Revision { get; set; }
    }
    [Preserve] public sealed class DuelNoticeDto
    {
        public DuelRoomDto Room { get; set; }
        public DuelReceiptDto Result { get; set; }
        public DuelEventDto[] Events { get; set; } = Array.Empty<DuelEventDto>();
        public DuelFrameDto[] Frames { get; set; } = Array.Empty<DuelFrameDto>();
    }
    [Preserve] public sealed class DuelPreviewEntryDto
    {
        public string ViewCardId { get; set; } = "";
        public string DefinitionId { get; set; } = "";
        public bool Known { get; set; }
        public int Count { get; set; }
        public Core.CardPosition Position { get; set; }
    }
    [Preserve] public sealed class DuelPreviewDto
    {
        public long Sequence { get; set; }
        public int Seat { get; set; }
        public Core.DuelZone Zone { get; set; }
        public DuelPreviewEntryDto[] Cards { get; set; }
    }
    [Preserve] public sealed class DuelReplaySummaryDto
    {
        public Guid Id { get; set; }
        public DateTimeOffset StartedAt { get; set; }
        public DateTimeOffset FinishedAt { get; set; }
        public int LocalSeat { get; set; }
        public int Winner { get; set; }
        public int TurnCount { get; set; }
        public DuelRoomPlayerDto[] Players { get; set; }
    }
    [Preserve] public sealed class DuelReplayTrackDto
    {
        public int Seat { get; set; }
        public DuelRoomDto InitialRoom { get; set; }
        public DuelNoticeDto[] Frames { get; set; }
    }
    [Preserve] public sealed class DuelReplayDto
    {
        public DuelReplaySummaryDto Summary { get; set; }
        public DuelReplayTrackDto[] Tracks { get; set; }
    }
    [Preserve] public sealed class DuelNamePageDto
    {
        public int Offset { get; set; }
        public int TotalCount { get; set; }
        public DuelNameDto[] Items { get; set; }
    }
    [Preserve] public sealed class DuelNameDto
    {
        public string NameId { get; set; }
        public string Chinese { get; set; }
    }
}
