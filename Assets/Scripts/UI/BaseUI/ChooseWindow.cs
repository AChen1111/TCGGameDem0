using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class ChooseWindowProperties : IWindowProperties
{
    public string Message { get; }
    public Action OnOk { get; }
    public Action OnNo { get; }

    public ChooseWindowProperties(string message, Action onOk, Action onNo)
    {
        Message = message;
        OnOk = onOk;
        OnNo = onNo;
    }
}

public class ChooseWindow : AWindowController<ChooseWindowProperties>
{
    // --tag_start: 自动生成--
    [SerializeField] TextMeshProUGUI m_TxtMessage;
    [SerializeField] Button m_BtnNo;
    [SerializeField] Button m_BtnOk;
    // --tag_end: 自动生成--

    protected override void AddListeners()
    {
        m_BtnOk.onClick.AddListener(OnBtnOkClicked);
        m_BtnNo.onClick.AddListener(OnBtnNoClicked);
    }

    protected override void RemoveListeners()
    {
        m_BtnOk.onClick.RemoveListener(OnBtnOkClicked);
        m_BtnNo.onClick.RemoveListener(OnBtnNoClicked);
    }

    protected override void OnOpen()
    {
        Apply();
    }

    protected override void OnResume()
    {
        Apply();
    }

    void Apply()
    {
        m_TxtMessage.text = Properties.Message;
        ALog.Log($"选择弹窗: {Properties.Message}", ALogCategories.UI);
    }

    void OnBtnOkClicked()
    {
        ALog.Log($"选择弹窗确认: {Properties.Message}", ALogCategories.UI);
        UI_Close();
        Properties.OnOk?.Invoke();
    }

    void OnBtnNoClicked()
    {
        ALog.Log($"选择弹窗取消: {Properties.Message}", ALogCategories.UI);
        UI_Close();
        Properties.OnNo?.Invoke();
    }
}
