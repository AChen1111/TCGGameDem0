using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using AChen.Duel.Presentation;
using AChen.Duel.Client;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

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
    [SerializeField] BattleCutinView m_cutin;
    [SerializeField] UnityEngine.UI.Button m_surrender, m_return, m_replayPlay, m_replayView, m_replayExit;
    [SerializeField] TextMeshProUGUI m_replayPlayLabel;
    readonly int[] m_avatarIds = { -1, -1 };
    readonly int[] m_frameIds = { -1, -1 };
    readonly DuelZone[] m_sources = { DuelZone.Hand, DuelZone.MainDeck, DuelZone.ExtraDeck, DuelZone.Monster,
        DuelZone.SpellTrap, DuelZone.Field, DuelZone.Graveyard, DuelZone.Banished, DuelZone.ExtraMonster };
    int m_player, m_sourceIndex;
    CardDetailEntry m_inspectedCard;
    BattleSceneController Scene => Properties.Scene;
    DuelZone SourceZone => m_sources[m_sourceIndex];
    UnityEngine.UI.Button[] Buttons => new[] { m_BtnPlayer, m_BtnSource, m_BtnCard, m_BtnView, m_BtnDraw,
        m_BtnPlace, m_BtnGrave, m_BtnBanished, m_BtnHand, m_BtnDeck, m_BtnExtra, m_BtnPosition,
        m_BtnHighlight, m_BtnPile, m_BtnPhase, m_BtnTurn, m_BtnPause, m_BtnClock, m_BtnReset,
        m_skipAnimation, m_cancelAction, m_surrender, m_return, m_replayPlay, m_replayView, m_replayExit };
    List<CardView> Candidates() => Scene.Source.Current.Cards.Where(x => x.Owner == m_player && x.Zone.Kind == SourceZone).ToList();

    protected override void OnOpen()
    {
        foreach(var button in Buttons)button.gameObject.SetActive(false);
        m_cancelAction.gameObject.SetActive(true);
        m_cutin.Initialize(m_UIFrame.MainCanvas.transform); Scene.BindCutin(m_cutin);
        m_surrender.onClick.AddListener(ConfirmSurrender);
        m_return.onClick.AddListener(()=>DuelClientSession.Instance.ReturnToLobbyAsync().Forget());
        m_replayPlay.onClick.AddListener(()=>{DuelClientSession.Instance.ToggleReplayPause();Refresh();});
        m_replayView.onClick.AddListener(()=>DuelClientSession.Instance.SwitchReplayView());
        m_replayExit.onClick.AddListener(()=>DuelClientSession.Instance.ExitReplayAsync().Forget());
        m_BtnReset.onClick.AddListener(()=>Scene.Source.Submit(new ResetDuel()));
        m_cancelAction.onClick.AddListener(Scene.CancelAction);
        Scene.Source.Changed+=OnChanged;Scene.SelectionChanged+=OnSelection;Scene.Notice+=OnNotice;
        Scene.ChoiceInspectionRequested+=OnChoiceInspection;
        m_actions.Initialize(Scene,m_UIFrame.MainCanvas);
        m_detail.SetCallbacks(HideDetail,delegate{},OpenLargeDetail);
        m_avatarIds[0]=m_avatarIds[1]=m_frameIds[0]=m_frameIds[1]=-1;m_detail.gameObject.SetActive(false);Refresh();
        Scene.CompleteHudInitialization();
    }
    void OnChanged(DuelViewChange change)
    {
        if(change.Kind==DuelChangeKind.Rejected)m_TxtHint.text=change.Message;
        if(change.Kind!=DuelChangeKind.Timer && !change.View.CanInteract)HideDetail();
        if(change.Kind==DuelChangeKind.Reset)HideDetail();
        Refresh();
    }
    void OnSelection(int id)
    {
        var card=Scene.Source.Current.Card(id);
        if(!Scene.CanInspect(card))return;
        ShowDetail(new CardDetailEntry(card.Definition.CardId,card.Definition.SourcePool,Scene.CardObject(id).Art));
        Refresh();
    }
    void HideDetail() => m_detail.gameObject.SetActive(false);
    void OnChoiceInspection(string id)
    { var definition=Scene.ChoiceDefinition(id);ShowDetail(new CardDetailEntry(id,definition.SourcePool,Scene.ArtworkForDefinition(id))); }
    void ShowDetail(CardDetailEntry card)
    { m_inspectedCard=card;m_detail.Show(new[]{card},0); }
    void OpenLargeDetail(CardArtwork artwork, int rarity)
    {
        m_actions.Hide();
        RequestOpenWindow(AddressKeys.Prefab.CardDetailOverlay, new CardDetailWindowProperty(new[]{m_inspectedCard},0));
    }
    public void ConfirmSurrender()
    {
        RequestOpenWindow(AddressKeys.Prefab.ChooseWindow, new ChooseWindowProperties(
            new LocalizedMessage("ui.duel.surrender_confirm"),
            () => DuelClientSession.Instance.SurrenderAsync().Forget(), delegate { }));
    }
    void OnNotice(string notice) => m_TxtHint.text = notice;
    void Refresh()
    {
        var view=Scene.Source.Current;
        m_TxtTurn.text="回合 "+view.Turn+" · "+view.Players[view.ActivePlayer].Name+" · "+BattleLabels.Phase(view.Phase);
        bool replay = Scene.Source.Mode == DuelSessionMode.Replay;
        m_surrender.gameObject.SetActive(Scene.Source.Mode == DuelSessionMode.Online && !view.Finished);
        m_return.gameObject.SetActive(Scene.Source.Mode == DuelSessionMode.Online && view.Finished);
        m_replayPlay.gameObject.SetActive(replay); m_replayView.gameObject.SetActive(replay); m_replayExit.gameObject.SetActive(replay);
        m_replayPlayLabel.text = DuelClientSession.Instance.ReplayPaused ? "播放" : "暂停";
        m_targetHint.SetActive(view.HasPendingAction);
        m_cancelAction.interactable=view.HasPendingAction;
        if(view.HasPendingAction)m_TxtHint.text="请选择黄色边框标示的位置";
        else if(view.Finished)m_TxtHint.text=view.Outcome;
        else if(!view.Animating)m_TxtHint.text="";
        for(int i=0;i<view.Players.Count;i++)
        {
            m_playerNames[i].text=view.Players[i].Name;m_playerLPs[i].text=view.Players[i].LP.ToString();
            if(m_avatarIds[i]==view.Players[i].AvatarId && m_frameIds[i]==view.Players[i].AvatarFrameId)continue;
            m_avatarIds[i]=view.Players[i].AvatarId;m_frameIds[i]=view.Players[i].AvatarFrameId;
            m_portraits[i].SetPortrait(m_avatarIds[i],m_frameIds[i]);
        }
    }
    protected override void OnClose()
    {
        Scene.Source.Changed -= OnChanged; Scene.SelectionChanged -= OnSelection; Scene.Notice -= OnNotice;
        Scene.ChoiceInspectionRequested -= OnChoiceInspection;
        m_actions.Dispose(); HideDetail(); m_cutin.RestoreParent();
        m_detail.SetCallbacks(delegate { }, delegate { }, delegate { });
        foreach (var button in Buttons) button.onClick.RemoveAllListeners();
    }
}
