using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DownLoadSliderTests
{
    [Test]
    public void Set_UpdatesSliderAndPercentText()
    {
        var root = new GameObject("bar");
        var slider = root.AddComponent<Slider>();
        var tmp = new GameObject("text").AddComponent<TextMeshProUGUI>();
        var bar = root.AddComponent<DownLoadSlider>();
        bar.slider = slider;
        bar.text = tmp;
        try
        {
            bar.Set(0.42f);
            Assert.AreEqual(0.42f, slider.value, 0.0001f);
            Assert.AreEqual("42%", tmp.text);

            bar.SetError("hash mismatch");
            Assert.AreEqual("hash mismatch", tmp.text);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(tmp.gameObject);
            UnityEngine.Object.DestroyImmediate(root);
        }
    }
}
