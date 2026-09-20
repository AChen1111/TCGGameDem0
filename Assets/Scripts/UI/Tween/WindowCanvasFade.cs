using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>窗口 CanvasGroup 渐入渐出, 关闭等淡出结束后再出栈.</summary>
sealed class WindowCanvasFade
{
    const float Duration = 0.25f;

    readonly AWindowController m_host;
    CanvasGroup m_group;
    bool m_closing;

    public WindowCanvasFade(AWindowController host)
    {
        m_host = host;
    }

    public void PlayOpen()
    {
        m_closing = false;
        CanvasGroup group = Group;
        if (group == null)
        {
            return;
        }

        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = true;
        FadeInAsync().Forget();
    }

    public void RequestClose(Action close)
    {
        if (m_closing)
        {
            return;
        }

        m_closing = true;
        FadeOutThenCloseAsync(close).Forget();
    }

    public void Reset()
    {
        m_closing = false;
        CanvasGroup group = Group;
        if (group == null)
        {
            return;
        }

        group.alpha = 1f;
        group.interactable = true;
        group.blocksRaycasts = true;
    }

    CanvasGroup Group => m_group != null ? m_group : m_group = m_host.GetComponent<CanvasGroup>();

    async UniTaskVoid FadeInAsync()
    {
        CanvasGroup group = Group;
        if (group == null)
        {
            return;
        }

        await UITween.FadeInAsync(group, Duration, m_host);
        if (m_host == null || m_closing)
        {
            return;
        }

        group.interactable = true;
        group.blocksRaycasts = true;
    }

    async UniTaskVoid FadeOutThenCloseAsync(Action close)
    {
        CanvasGroup group = Group;
        if (group != null)
        {
            await UITween.FadeOutAsync(group, Duration, m_host);
            if (m_host == null)
            {
                return;
            }
        }

        ALog.Log($"窗口淡出完成. Screen={m_host.ScreenId}", ALogCategories.UI);
        close();
    }
}
