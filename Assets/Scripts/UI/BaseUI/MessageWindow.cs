using UnityEngine;
using TMPro;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;

public sealed class MessageWindowProperties : IWindowProperties
{
    public string Message { get; }
    public float Duration { get; }

    public MessageWindowProperties(string message, float duration)
    {
        Message = message;
        Duration = duration;
    }
}

public class MessageWindow : AWindowController<MessageWindowProperties>
{
    // --tag_start: 自动生成--
    [SerializeField] TextMeshProUGUI m_TxtMessage;
    // --tag_end: 自动生成--
    CancellationTokenSource m_closeCts;

    protected override void OnOpen()
    {
        PlayOpenAsync().Forget();
    }

    async UniTaskVoid PlayOpenAsync()
    {
        m_TxtMessage.text = Properties.Message;
        await UITween.DoScaleAnim(0, 1, 2, transform).AddTo(gameObject);
        m_closeCts = new CancellationTokenSource();
        CloseAfterAsync(Properties.Duration, m_closeCts.Token).Forget();
    }

    protected override void OnResume()
    {
        m_TxtMessage.text = Properties.Message;
    }

    protected override void OnClose()
    {
        m_closeCts.Cancel();
        m_closeCts.Dispose();
        m_closeCts = null;
    }

    async UniTaskVoid CloseAfterAsync(float duration, CancellationToken token)
    {
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: token);
            UI_Close();
        }
        catch (OperationCanceledException) { }
    }
}
