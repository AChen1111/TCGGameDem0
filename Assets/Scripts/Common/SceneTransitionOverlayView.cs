using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class SceneTransitionOverlayView : MonoBehaviour
{
    [SerializeField] Canvas m_canvas;
    [SerializeField] CanvasGroup m_group;
    [SerializeField] RectTransform m_background;
    [SerializeField] TMP_Text m_status;
    [SerializeField] Slider m_progress;
    [SerializeField] Button m_retry;
    [SerializeField] TMP_Text m_retryLabel;
    UniTaskCompletionSource m_pending;
    public CanvasGroup Group => m_group;
    void Awake()
    {
        m_canvas.renderMode = RenderMode.ScreenSpaceOverlay; m_canvas.sortingOrder = 32766;
        m_background.anchorMin = Vector2.zero; m_background.anchorMax = Vector2.one;
        m_background.offsetMin = Vector2.zero; m_background.offsetMax = Vector2.zero;
        m_retry.onClick.AddListener(Retry);
        m_retryLabel.text = "重试 / Retry";
        SceneTransitionOverlay.Bind(this);
        Progress("正在加载 / Loading…", 0);
    }
    public void Progress(string text, float value)
    {
        m_status.text = text; m_progress.value = value; m_retry.gameObject.SetActive(false);
        // 清除旧窗口的键盘焦点，避免提交键触发被遮挡的按钮。
        EventSystem.current.SetSelectedGameObject(null);
    }
    public async UniTask WaitForRetryAsync(string reason, CancellationToken token)
    {
        m_status.text = "活动配置加载失败 / Activity loading failed\n" + reason;
        m_retry.gameObject.SetActive(true); m_retry.interactable = true; m_retry.Select();
        m_pending = new UniTaskCompletionSource();
        try { await m_pending.Task.AttachExternalCancellation(token); }
        finally { m_retry.gameObject.SetActive(false); m_pending = null; }
    }
    void LateUpdate()
    {
        if (!SceneTransitionOverlay.IsActivityBlocking) return;
        // 正常入场动画也可能正在淡出同一个 CanvasGroup，活动切换期间保持全屏遮挡。
        m_group.alpha = 1; m_group.blocksRaycasts = true;
        EventSystem.current.SetSelectedGameObject(m_retry.gameObject.activeSelf ? m_retry.gameObject : null);
    }
    void Retry() { m_retry.interactable = false; m_pending.TrySetResult(); }
}
