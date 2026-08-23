using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DownLoadSlider : MonoBehaviour
{
    public Slider slider;
    public TMP_Text text;

    void Awake()
    {
        Set(0f);
    }

    public void Set(float progress)
    {
        slider.value = progress;
        text.text = Mathf.RoundToInt(progress * 100f) + "%";
    }

    public void SetError(string message)
    {
        text.text = string.IsNullOrWhiteSpace(message) ? "内容更新失败" : message;
    }
}
