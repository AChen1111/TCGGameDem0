using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class UrNoticeProperties : IWindowProperties
{
    public LocalizedMessage Message { get; }
    public UrNoticeProperties(LocalizedMessage message) { Message = message; }
}

public sealed class UrNoticeWindow : AWindowController<UrNoticeProperties>
{
    [SerializeField] LocalizedText m_Message;
    [SerializeField] Button m_Ok;
    protected override void AddListeners() => m_Ok.onClick.AddListener(UI_Close);
    protected override void RemoveListeners() => m_Ok.onClick.RemoveListener(UI_Close);
    protected override void OnOpen() => m_Message.SetMessage(Properties.Message);
}
