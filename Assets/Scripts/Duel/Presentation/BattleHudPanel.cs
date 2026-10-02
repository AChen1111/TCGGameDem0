using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using AChen.Duel.Presentation;
using UnityEngine.UI;

public sealed class BattleHudProperties : IPanelProperties
{
    public BattleSceneController Scene { get; }
    public BattleHudProperties(BattleSceneController scene) { Scene = scene; }
}

public sealed class BattleHudPanel : APanelController<BattleHudProperties>
{
    // --tag_start: 自动生成--
    [SerializeField] TextMeshProUGUI m_TxtTurn;
    [SerializeField] TextMeshProUGUI m_TxtActionCancel;
    [SerializeField] TextMeshProUGUI m_TxtHint;
    [SerializeField] Button m_BtnPlayer;
    [SerializeField] TextMeshProUGUI m_TxtPlayer;
    [SerializeField] Button m_BtnSource;
    [SerializeField] TextMeshProUGUI m_TxtSource;
    [SerializeField] Button m_BtnCard;
    [SerializeField] TextMeshProUGUI m_TxtSelection;
    [SerializeField] Button m_BtnView;
    [SerializeField] TextMeshProUGUI m_TxtView;
    [SerializeField] Button m_BtnDraw;
    [SerializeField] TextMeshProUGUI m_TxtDraw;
    [SerializeField] Button m_BtnPlace;
    [SerializeField] TextMeshProUGUI m_TxtPlace;
    [SerializeField] Button m_BtnGrave;
    [SerializeField] TextMeshProUGUI m_TxtGrave;
    [SerializeField] Button m_BtnBanished;
    [SerializeField] TextMeshProUGUI m_TxtBanished;
    [SerializeField] Button m_BtnHand;
    [SerializeField] TextMeshProUGUI m_TxtHand;
    [SerializeField] Button m_BtnDeck;
    [SerializeField] TextMeshProUGUI m_TxtDeck;
    [SerializeField] Button m_BtnExtra;
    [SerializeField] TextMeshProUGUI m_TxtExtra;
    [SerializeField] Button m_BtnPosition;
    [SerializeField] TextMeshProUGUI m_TxtPosition;
    [SerializeField] Button m_BtnHighlight;
    [SerializeField] TextMeshProUGUI m_TxtHighlight;
    [SerializeField] Button m_BtnPile;
    [SerializeField] TextMeshProUGUI m_TxtPile;
    [SerializeField] Button m_BtnPhase;
    [SerializeField] TextMeshProUGUI m_TxtPhase;
    [SerializeField] Button m_BtnTurn;
    [SerializeField] Button m_BtnPause;
    [SerializeField] TextMeshProUGUI m_TxtPause;
    [SerializeField] Button m_BtnClock;
    [SerializeField] TextMeshProUGUI m_TxtClock;
    [SerializeField] TextMeshProUGUI m_TxtSkipAnimation;
    [SerializeField] Button m_BtnReset;
    [SerializeField] TextMeshProUGUI m_TxtReset;
    // --tag_end: 自动生成--
    [SerializeField] BattleCardActionsView m_actions;
    [SerializeField] CardDetailView m_detail;
    [SerializeField] TextMeshProUGUI[] m_playerNames;
    [SerializeField] TextMeshProUGUI[] m_playerLPs;
    [SerializeField] AvatarPortraitView[] m_portraits;
    [SerializeField] GameObject m_targetHint;
    [SerializeField] UnityEngine.UI.Button m_cancelAction;
    [SerializeField] UnityEngine.UI.Button m_skipAnimation;
    readonly int[] m_avatarIds = { -1, -1 };
    readonly DuelZone[] m_sources = { DuelZone.Hand, DuelZone.MainDeck, DuelZone.ExtraDeck, DuelZone.Monster,
        DuelZone.SpellTrap, DuelZone.Field, DuelZone.Graveyard, DuelZone.Banished, DuelZone.ExtraMonster };
    int m_player, m_sourceIndex;
    BattleSceneController Scene => Properties.Scene;
    DuelZone SourceZone => m_sources[m_sourceIndex];
    UnityEngine.UI.Button[] Buttons => new[] { m_BtnPlayer, m_BtnSource, m_BtnCard, m_BtnView, m_BtnDraw,
        m_BtnPlace, m_BtnGrave, m_BtnBanished, m_BtnHand, m_BtnDeck, m_BtnExtra, m_BtnPosition,
        m_BtnHighlight, m_BtnPile, m_BtnPhase, m_BtnTurn, m_BtnPause, m_BtnClock, m_BtnReset,
        m_skipAnimation, m_cancelAction };
    List<CardView> Candidates() => Scene.Source.Current.Cards.Where(x => x.Owner == m_player && x.Zone.Kind == SourceZone).ToList();

    protected override void OnOpen()
    {
        m_BtnPlayer.onClick.AddListener(() => { m_player = 1 - m_player; SelectFirst(); });
        m_BtnSource.onClick.AddListener(() => { m_sourceIndex = (m_sourceIndex + 1) % m_sources.Length; SelectFirst(); });
        m_BtnCard.onClick.AddListener(NextCard);
        m_BtnView.onClick.AddListener(() => Scene.OpenZone(new ZoneRef(SourceZone, m_player)));
        m_BtnDraw.onClick.AddListener(Draw);
        m_BtnPlace.onClick.AddListener(() => Act(card => Scene.PrepareSelectedPlacement()));
        m_BtnGrave.onClick.AddListener(() => Move(DuelZone.Graveyard));
        m_BtnBanished.onClick.AddListener(() => Move(DuelZone.Banished));
        m_BtnHand.onClick.AddListener(() => Move(DuelZone.Hand));
        m_BtnDeck.onClick.AddListener(() => Move(DuelZone.MainDeck));
        m_BtnExtra.onClick.AddListener(() => Move(DuelZone.ExtraDeck));
        m_BtnPosition.onClick.AddListener(() => Act(card => Scene.OpenPosition()));
        m_BtnHighlight.onClick.AddListener(() => Act(card => Scene.Source.Submit(new SetCardEffectAvailable(card.InstanceId, !card.EffectAvailable))));
        m_BtnPile.onClick.AddListener(() =>
        {
            var zone = new ZoneRef(SourceZone, m_player);
            Scene.Source.Submit(new SetPileEffectAvailable(zone, !Scene.Source.Current.AvailablePiles.Contains(zone)));
        });
        m_BtnPhase.onClick.AddListener(Scene.OpenPhases);
        m_BtnTurn.onClick.AddListener(() => Scene.Source.Submit(new EndTurn()));
        m_BtnPause.onClick.AddListener(() => Scene.Source.Submit(new PauseTimer(!Scene.Source.Current.TimerPaused)));
        m_BtnClock.onClick.AddListener(() => Scene.Source.Submit(new ResetTimer()));
        m_BtnReset.onClick.AddListener(() => Scene.Source.Submit(new ResetDuel()));
        m_cancelAction.onClick.AddListener(Scene.CancelAction);
        m_skipAnimation.onClick.AddListener(Scene.SkipAnimation);
        Scene.Source.Changed += OnChanged; Scene.SelectionChanged += OnSelection; Scene.Notice += OnNotice;
        m_actions.Initialize(Scene, m_UIFrame.MainCanvas);
        m_detail.SetCallbacks(HideDetail, delegate { }, OpenZoom);
        m_avatarIds[0] = m_avatarIds[1] = -1;
        m_detail.gameObject.SetActive(false);
        Refresh();
    }

    void SelectFirst()
    {
        var cards = Candidates();
        if (cards.Count > 0) Scene.SelectCard(cards[0].InstanceId);
        Refresh();
    }
    void NextCard()
    {
        var cards = Candidates();
        if (cards.Count == 0) { OnNotice("该区域没有卡牌"); return; }
        int index = cards.FindIndex(x => x.InstanceId == Scene.SelectedCardId);
        Scene.ShowCardActions(cards[(index + 1) % cards.Count].InstanceId);
    }
    void Act(Action<CardView> action)
    {
        var cards = Candidates();
        if (cards.Count == 0) { OnNotice("该区域没有卡牌"); return; }
        if (!cards.Any(x => x.InstanceId == Scene.SelectedCardId)) Scene.SelectCard(cards[0].InstanceId);
        action(Scene.Source.Current.Card(Scene.SelectedCardId));
    }
    void Move(DuelZone destination) => Act(card => Scene.Source.Submit(new MoveCard(card.InstanceId, new ZoneRef(destination, card.Owner))));
    void Draw()
    {
        var deck = Scene.Source.Current.InZone(new ZoneRef(DuelZone.MainDeck, m_player));
        if (deck.Count == 0) { OnNotice("主卡组已空"); return; }
        Scene.SelectCard(deck[0].InstanceId);
        Scene.Source.Submit(new MoveCard(deck[0].InstanceId, new ZoneRef(DuelZone.Hand, m_player)));
        m_sourceIndex = 0;
    }
    void OnChanged(DuelViewChange change)
    {
        if (change.Kind == DuelChangeKind.Rejected) OnNotice(change.Message);
        if (change.Kind is DuelChangeKind.Move or DuelChangeKind.Reset)
        {
            var card = Scene.Source.Current.Card(Scene.SelectedCardId);
            m_player = card.Owner; m_sourceIndex = Array.IndexOf(m_sources, card.Zone.Kind);
        }
        if (change.Kind is DuelChangeKind.CancelAction or DuelChangeKind.Move or DuelChangeKind.Reset or DuelChangeKind.Phase or DuelChangeKind.Turn or DuelChangeKind.AnimationCompleted)
            m_TxtHint.text = "";
        if (change.Kind == DuelChangeKind.Reset) HideDetail();
        Refresh();
    }
    void OnSelection(int id)
    {
        var card = Scene.Source.Current.Card(id);
        m_player = card.Owner; m_sourceIndex = Array.IndexOf(m_sources, card.Zone.Kind);
        m_TxtHint.text = "";
        m_detail.Show(new[] { new CardDetailEntry(card.Definition.CardId, card.Definition.SourcePool, Scene.CardObject(id).Art) }, 0);
        Refresh();
    }
    void HideDetail() => m_detail.gameObject.SetActive(false);
    void OpenZoom(Texture texture, int rarity)
    {
        m_actions.Hide();
        RequestOpenWindow(AddressKeys.Prefab.CardZoomWindow, new CardZoomWindowProperty(texture, rarity));
    }
    void OnNotice(string notice) => m_TxtHint.text = notice;
    void Refresh()
    {
        var view = Scene.Source.Current; var cards = Candidates();
        m_TxtPlayer.text = m_player == 0 ? "操作我方" : "操作对方";
        m_TxtSource.text = BattleLabels.Zone(SourceZone);
        m_TxtTurn.text = $"回合 {view.Turn}  ·  {(view.ActivePlayer == 0 ? "我方" : "对方")}  ·  {BattleLabels.Phase(view.Phase)}";
        m_TxtSelection.text = cards.Count == 0 ? "区域为空" : LocalizationService.GetText("card." + view.Card(Scene.SelectedCardId).Definition.CardId + ".name");
        foreach (var button in Buttons) button.interactable = view.CanInteract;
        m_BtnReset.interactable = m_BtnPause.interactable = m_BtnClock.interactable = true;
        m_skipAnimation.interactable = view.Animating;
        m_cancelAction.interactable = view.HasPendingAction;
        m_targetHint.SetActive(view.HasPendingAction && !view.PendingAction.NeedsPosition);
        if (view.HasPendingAction)
            m_TxtHint.text = view.PendingAction.NeedsPosition ? "请选择表示形式" : "请选择黄色边框标示的位置";
        m_BtnPosition.interactable = view.CanInteract && cards.Count > 0 && view.Card(Scene.SelectedCardId).AvailablePositions.Count > 0;
        m_TxtPause.text = view.TimerPaused ? "继续计时" : "暂停计时";
        for (int i = 0; i < view.Players.Count; i++)
        {
            m_playerNames[i].text = view.Players[i].Name;
            m_playerLPs[i].text = view.Players[i].LP.ToString();
            if (m_avatarIds[i] == view.Players[i].AvatarId) continue;
            m_avatarIds[i] = view.Players[i].AvatarId;
            m_portraits[i].SetPortrait(m_avatarIds[i], 1030001);
        }
    }
    protected override void OnClose()
    {
        Scene.Source.Changed -= OnChanged; Scene.SelectionChanged -= OnSelection; Scene.Notice -= OnNotice;
        m_actions.Dispose(); HideDetail();
        m_detail.SetCallbacks(delegate { }, delegate { }, delegate { });
        foreach (var button in Buttons) button.onClick.RemoveAllListeners();
    }
}
