using UnityEngine;
using TMPro;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using UnityEngine.EventSystems;

public sealed class MessageWindowProperties : IWindowProperties
{
    public LocalizedMessage Message { get; }
    public float Duration { get; }

    public MessageWindowProperties(LocalizedMessage message, float duration)
    {
        Message = message;
        Duration = duration;
    }
}

public class MessageWindow : AWindowController<MessageWindowProperties>, IPointerClickHandler
{
    // --tag_start: 自动生成--
    [SerializeField] TextMeshProUGUI m_TxtMessage;
    // --tag_end: 自动生成--
    CancellationTokenSource m_closeCts;
    MotionHandle m_openMotion;
    bool m_clicked;

    public void OnPointerClick(PointerEventData eventData)
    {
        eventData.Use();
        m_clicked = true;
        m_openMotion.TryComplete();
        UI_Close();
    }

    protected override UniTask PlayExitTransition(CancellationToken cancellationToken)
    {
        if (!m_clicked) return base.PlayExitTransition(cancellationToken);
        TransitionCanvasGroup.alpha = 0f;
        return UniTask.CompletedTask;
    }

    protected override void FinishIntro() => m_openMotion.TryComplete();

    protected override void OnOpen()
    {
        m_clicked = false;
        PlayOpenAsync().Forget();
    }

    async UniTaskVoid PlayOpenAsync()
    {
        ApplyMessage();
        CancellationToken token = ScreenToken;
        m_openMotion = UITween.DoScaleAnim(0, 1, 2, transform).AddTo(gameObject);
        await m_openMotion;
        if (token.IsCancellationRequested || m_clicked) return;
        m_closeCts = CancellationTokenSource.CreateLinkedTokenSource(token);
        CloseAfterAsync(Properties.Duration, m_closeCts.Token).Forget();
    }

    protected override void OnResume()
    {
        ApplyMessage();
    }

    void ApplyMessage()
    {
        LocalizedMessage message = Properties.Message;
        m_TxtMessage.Localized().SetMessage(message);
        ALog.Log($"提示弹窗: {message}", ALogCategories.UI);
    }

    protected override void OnClose()
    {
        m_closeCts?.Cancel();
        m_closeCts?.Dispose();
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
