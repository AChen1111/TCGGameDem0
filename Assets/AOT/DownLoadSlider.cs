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
        m_retryButton = transform.Find("RetryButton").GetComponent<Button>();
        Set(0f);
    }

    public void BindRetry(UnityAction retry)
    {
        m_retryButton.onClick.RemoveAllListeners();
        m_retryButton.onClick.AddListener(retry);
    }

    public void Set(float progress)
    {
        slider.value = progress;
        m_retryButton.gameObject.SetActive(false);
        text.text = "正在更新游戏内容 " + Mathf.RoundToInt(progress * 100f) + "%";
    }
    public void SetError(LocalizedMessage message)
    {
        text.text = message?.Key == "err.content_not_ready"
            ? "游戏内容尚未准备好，请等待更新完成后重试。"
            : "游戏内容加载失败，请检查网络后重试。";
        m_retryButton.gameObject.SetActive(true);
        m_retryButton.Select();
    }
}
