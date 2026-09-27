using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Threading;
using AChen.Decks;
using Cysharp.Threading.Tasks;

public sealed class DeckNameWindowProperties : IWindowProperties
{
    public string Name { get; }
    public bool Renaming { get; }
    public Func<string, CancellationToken, UniTask> Submit { get; }
    public Action Completed { get; }
    public DeckNameWindowProperties(string name, bool renaming, Func<string, CancellationToken, UniTask> submit, Action completed)
    { Name = name; Renaming = renaming; Submit = submit; Completed = completed; }
}

public class DeckNameWindow : AWindowController<DeckNameWindowProperties>
{
    // --tag_start: 自动生成--
    [SerializeField] TextMeshProUGUI m_TxtTitle;
    [SerializeField] TMP_InputField m_InpName;
    [SerializeField] Button m_BtnCancel;
    [SerializeField] Button m_BtnConfirm;
    // --tag_end: 自动生成--
    bool m_busy;
    protected override void AddListeners()
    {
        m_BtnCancel.onClick.AddListener(UI_Close);
        m_BtnConfirm.onClick.AddListener(Submit);
        m_InpName.onValueChanged.AddListener(ValidateName);
    }
    protected override void RemoveListeners()
    {
        m_BtnCancel.onClick.RemoveListener(UI_Close);
        m_BtnConfirm.onClick.RemoveListener(Submit);
        m_InpName.onValueChanged.RemoveListener(ValidateName);
    }
    protected override void OnOpen()
    {
        m_busy = false;
        m_TxtTitle.text = LocalizationService.GetText(Properties.Renaming ? "ui.deck.rename" : "ui.deck.name_prompt");
        m_InpName.SetTextWithoutNotify(Properties.Name);
        m_InpName.interactable = m_BtnCancel.interactable = true;
        ValidateName(m_InpName.text);
        m_InpName.ActivateInputField();
    }
    void ValidateName(string name) => m_BtnConfirm.interactable = !m_busy && DeckValidator.IsValidName(name);
    void Submit() => SubmitAsync().Forget();
    async UniTask SubmitAsync()
    {
        m_busy = true; ValidateName(m_InpName.text);
        m_InpName.interactable = m_BtnCancel.interactable = false;
        bool success = await RunGuardedAsync(ct => Properties.Submit(m_InpName.text.Trim(), ct), "卡组命名", "err.deck.save");
        if (!IsOpened) return;
        if (success) { UI_Close(); Properties.Completed(); }
        else { m_busy = false; m_InpName.interactable = m_BtnCancel.interactable = true; ValidateName(m_InpName.text); }
    }
}
