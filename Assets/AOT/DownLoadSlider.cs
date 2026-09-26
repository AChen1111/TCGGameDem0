using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class DownLoadSlider : MonoBehaviour
{
    public Slider slider;
    public TMP_Text text;
    Button m_retryButton;

    void Awake()
    {
        Set(0f);
    }

    Button Retry()
    {
        if (m_retryButton == null)
        {
            var child = transform.Find("SafeArea/RetryButton") ?? transform.Find("RetryButton");
            if (child != null) m_retryButton = child.GetComponent<Button>();
        }
        return m_retryButton;
    }

    public void BindRetry(UnityAction retry)
    {
        Retry().onClick.RemoveAllListeners();
        Retry().onClick.AddListener(retry);
    }

    public void Set(float progress)
    {
        slider.value = progress;
        if (Retry() != null) Retry().gameObject.SetActive(false);
        text.text = "正在更新游戏内容 " + Mathf.RoundToInt(progress * 100f) + "%";
    }
    public void SetError(LocalizedMessage message)
    {
        text.text = message?.Key == "err.content_not_ready"
            ? "游戏内容尚未准备好，请等待更新完成后重试。"
            : "游戏内容加载失败，请重试。";
        if (message?.Arguments != null && message.Arguments.TryGetValue("message", out var detail))
            text.text += "\n" + detail;
        else if (message?.Arguments != null && message.Arguments.TryGetValue("error", out var error))
            text.text += "\n" + error;
        if (Retry() != null)
        {
            Retry().gameObject.SetActive(true);
            Retry().Select();
        }
    }
}
