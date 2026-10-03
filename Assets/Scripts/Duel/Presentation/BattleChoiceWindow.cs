using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using AChen.Duel.Presentation;
using UnityEngine.UI;

public enum BattleChoiceKind { ActionPosition, Phase, Position, Placement = ActionPosition }
public sealed class BattleChoiceProperties : IWindowProperties
{
    public BattleSceneController Scene { get; }
    public BattleChoiceKind Kind { get; }
    public BattleChoiceProperties(BattleSceneController scene, BattleChoiceKind kind) { Scene = scene; Kind = kind; }
}
public sealed class BattleChoiceWindow : AWindowController<BattleChoiceProperties>
{
    // --tag_start: 自动生成--
    [SerializeField] TextMeshProUGUI m_TxtTitle;
    [SerializeField] Button m_BtnOption0;
    [SerializeField] TextMeshProUGUI m_TxtOption0;
    [SerializeField] Button m_BtnOption1;
    [SerializeField] TextMeshProUGUI m_TxtOption1;
    [SerializeField] Button m_BtnOption2;
    [SerializeField] TextMeshProUGUI m_TxtOption2;
    [SerializeField] Button m_BtnEndTurn;
    [SerializeField] TextMeshProUGUI m_TxtEndTurn;
    [SerializeField] Button m_BtnCancel;
    [SerializeField] TextMeshProUGUI m_TxtCancel;
    [SerializeField] Button m_BtnReset;
    [SerializeField] TextMeshProUGUI m_TxtReset;
    [SerializeField] Button m_BtnSurrender;
    [SerializeField] TextMeshProUGUI m_TxtSurrender;
    // --tag_end: 自动生成--
    [SerializeField] UnityEngine.UI.Button[] m_options;
    [SerializeField] TextMeshProUGUI[] m_labels;
    [SerializeField] UnityEngine.UI.Button[] m_phaseOptions;
    [SerializeField] UnityEngine.UI.Image[] m_phaseCursors;
    [SerializeField] GameObject m_positionGroup;
    [SerializeField] GameObject m_phaseGroup;
    [SerializeField] UnityEngine.UI.Button m_surrender;
    readonly List<DuelInputCommand> m_commands = new List<DuelInputCommand>();
    bool m_choiceConfirmed;
    protected override void OnOpen()
    {
        m_choiceConfirmed = false;
        var scene = Properties.Scene; var view = scene.Source.Current;
        bool choosingPhase = Properties.Kind == BattleChoiceKind.Phase;
        m_phaseGroup.SetActive(choosingPhase); m_positionGroup.SetActive(!choosingPhase);
        m_surrender.gameObject.SetActive(choosingPhase);
        m_surrender.onClick.AddListener(() => Submit(new SurrenderDuel()));
        m_commands.Clear();
        if (choosingPhase)
        {
            m_TxtTitle.text = "请选择前往的阶段";
            for (int i = 0; i < 6; i++)
            {
                var phase = (DuelPhase)i;
                m_phaseOptions[i].interactable = view.AvailablePhases.Contains(phase);
                m_phaseCursors[i].gameObject.SetActive(view.Phase == phase);
                m_phaseOptions[i].onClick.AddListener(() => Submit(new ChangePhase(phase)));
            }
            m_phaseOptions[6].interactable = view.CanInteract && view.AvailablePhases.Contains(DuelPhase.End);
            m_phaseOptions[6].onClick.AddListener(() => Submit(new EndTurn()));
        }
        else
        {
            bool action = Properties.Kind == BattleChoiceKind.ActionPosition;
            m_TxtTitle.text = action ? "请选择表示形式" : "切换表示形式";
            var positions = action ? view.PendingAction.Positions : view.Card(scene.SelectedCardId).AvailablePositions;
            for (int i = 0; i < positions.Count; i++)
                m_commands.Add(action ? new ChooseActionPosition(positions[i]) : new ChangePosition(scene.SelectedCardId, positions[i]));
            for (int i = 0; i < m_options.Length; i++)
            {
                int index = i;
                m_options[i].gameObject.SetActive(i < positions.Count);
                if (i >= positions.Count) continue;
                m_labels[i].text = BattleLabels.Position(positions[i]);
                m_options[i].onClick.AddListener(() => Submit(m_commands[index]));
            }
        }
        m_BtnCancel.onClick.AddListener(UI_Close);
        m_BtnReset.onClick.AddListener(() => scene.Source.Submit(new ResetDuel()));
        scene.Source.Changed += OnChanged;
    }
    void Submit(DuelInputCommand command)
    {
        m_choiceConfirmed = true;
        Properties.Scene.Source.Submit(command);
        UI_Close();
    }
    void OnChanged(DuelViewChange change)
    {
        if (change.Kind is DuelChangeKind.Reset or DuelChangeKind.CancelAction) UI_Close();
    }
    protected override void OnClose()
    {
        Properties.Scene.Source.Changed -= OnChanged;
        foreach (var option in m_options) option.onClick.RemoveAllListeners();
        foreach (var option in m_phaseOptions) option.onClick.RemoveAllListeners();
        m_BtnCancel.onClick.RemoveAllListeners(); m_BtnReset.onClick.RemoveAllListeners();
        m_surrender.onClick.RemoveAllListeners();
        if (!m_choiceConfirmed && Properties.Kind == BattleChoiceKind.ActionPosition && Properties.Scene.Source.Current.HasPendingAction)
            Properties.Scene.CancelAction();
    }
}
