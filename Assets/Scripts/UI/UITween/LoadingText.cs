using System.Collections;
using UnityEngine;
using TMPro;

public class LoadingText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField, Min(0.05f)] private float interval = 0.5f;

    private Coroutine m_animation;

    private void OnEnable()
    {
        if (text == null)
        {
            text = GetComponent<TextMeshProUGUI>();
        }

        if (text == null)
        {
            ALog.LogWarning($"加载文字动画启动失败: {name} 未绑定 TextMeshProUGUI.", ALogCategories.UI);
            return;
        }

        m_animation = StartCoroutine(AnimateText());
    }

    private void OnDisable()
    {
        if (m_animation != null)
        {
            StopCoroutine(m_animation);
            m_animation = null;
        }
    }

    private IEnumerator AnimateText()
    {
        string[] frames = { "正在加载中.", "正在加载中..", "正在加载中..." };
        // 加载提示不受游戏暂停或时间缩放影响.
        var delay = new WaitForSecondsRealtime(Mathf.Max(0.05f, interval));
        int index = 0;
        while (true)
        {
            text.text = frames[index];
            index = (index + 1) % frames.Length;
            yield return delay;
        }
    }
}
