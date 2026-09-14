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
        text.Localized().SetKey("ui.update.percent", new System.Collections.Generic.Dictionary<string, object>
        {
            ["percent"] = Mathf.RoundToInt(progress * 100f)
        });
    }

    public void SetError(LocalizedMessage message)
    {
        text.Localized().SetMessage(message ?? new LocalizedMessage("err.content_update_failed"));
    }
}
