using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public sealed class DeckUnsavedWindowProperties : IWindowProperties
{
    public Action Save { get; }
    public Action Discard { get; }
    public DeckUnsavedWindowProperties(Action save, Action discard) { Save = save; Discard = discard; }
}

public class DeckUnsavedWindow : AWindowController<DeckUnsavedWindowProperties>
{
    // --tag_start: 自动生成--
    [SerializeField] TextMeshProUGUI m_TxtMessage;
    [SerializeField] Button m_BtnSave;
    [SerializeField] Button m_BtnDiscard;
    [SerializeField] Button m_BtnContinue;
    // --tag_end: 自动生成--
    protected override void AddListeners()
    { m_BtnSave.onClick.AddListener(Save); m_BtnDiscard.onClick.AddListener(Discard); m_BtnContinue.onClick.AddListener(UI_Close); }
    protected override void RemoveListeners()
    { m_BtnSave.onClick.RemoveListener(Save); m_BtnDiscard.onClick.RemoveListener(Discard); m_BtnContinue.onClick.RemoveListener(UI_Close); }
    void Save() { UI_Close(); Properties.Save(); }
    void Discard() { UI_Close(); Properties.Discard(); }
}
