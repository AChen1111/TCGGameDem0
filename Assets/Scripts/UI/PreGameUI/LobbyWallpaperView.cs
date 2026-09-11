using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 大厅壁纸与立绘视图: 按背景 Id 加载两张图并整体替换, 用版本号丢弃过期加载, 提供揭示动画.
/// 不订阅事件, 由 PreGameUIPanel 驱动.
/// </summary>
public sealed class LobbyWallpaperView : MonoBehaviour
{
    [SerializeField] GameObject m_wallpaper;
    [SerializeField] GameObject m_heroSprite;
    [SerializeField] WallpaperDisplayConfig m_WallpaperDisplayConfig;

    Image m_heroImage;
    Image m_wallpaperImage;
    int? m_backgroundId;
    int m_backgroundVersion;
    UniTask m_backgroundLoad = UniTask.CompletedTask;

    void Awake()
    {
        m_heroImage = m_heroSprite.GetComponent<Image>();
        m_wallpaperImage = m_wallpaper.GetComponent<Image>();
    }

    /// <summary>切换到指定背景; null 表示清空显示. 相同 Id 不重复加载.</summary>
    public void SetBackground(int? backgroundId, CancellationToken token)
    {
        if (backgroundId == null)
        {
            Clear();
            m_heroImage.sprite = null;
            m_wallpaperImage.sprite = null;
            m_heroSprite.SetActive(false);
            return;
        }

        if (backgroundId == m_backgroundId)
        {
            return;
        }

        m_backgroundId = backgroundId;
        int version = ++m_backgroundVersion;
        m_backgroundLoad = LoadAsync(backgroundId.Value, version, token).Preserve();
    }

    /// <summary>丢弃进行中的加载并重置状态; 不改动当前显示的图片.</summary>
    public void Clear()
    {
        m_backgroundVersion++;
        m_backgroundId = null;
        m_backgroundLoad = UniTask.CompletedTask;
    }

    /// <summary>等待当前背景加载完成; 等待期间若又切换了背景, 继续等最新一次.</summary>
    public async UniTask WaitForReadyAsync(CancellationToken token)
    {
        int version;
        do
        {
            version = m_backgroundVersion;
            await m_backgroundLoad.AttachExternalCancellation(token);
            token.ThrowIfCancellationRequested();
        }
        while (version != m_backgroundVersion);
    }

    /// <summary>入场前隐藏, 等揭示动画再显示.</summary>
    public void HideForIntro()
    {
        m_wallpaper.SetActive(false);
        m_heroSprite.SetActive(false);
    }

    /// <summary>切换壁纸前先隐藏当前画面, 失败时用 RestoreVisible 还原.</summary>
    public void HideForSwitch()
    {
        SetImageAlpha(m_wallpaperImage, 0f);
        m_heroSprite.SetActive(false);
    }

    public void RestoreVisible()
    {
        SetImageAlpha(m_wallpaperImage, 1f);
        m_heroSprite.SetActive(true);
    }

    /// <summary>壁纸淡入后立绘自上而下显现.</summary>
    public void AppendReveal(MotionSequenceBuilder sequence, float duration)
    {
        sequence.Append(UITween.DoFadeAnim(0, 1, duration, m_wallpaperImage));
        m_heroSprite.SetActive(true);
        sequence.Append(UITween.DoVerticalReveal(m_heroImage, duration));
    }

    public async UniTask PlayRevealAsync(float duration)
    {
        var sequence = LSequence.Create();
        AppendReveal(sequence, duration);
        await sequence.Run().AddTo(this);
    }

    async UniTask LoadAsync(int backgroundId, int version, CancellationToken token)
    {
        try
        {
            var images = await UniTask.WhenAll(
                AddressableLoader.Instance.LoadSprite(AddressKeys.GetBackgroundDownAddress(backgroundId)),
                AddressableLoader.Instance.LoadSprite(AddressKeys.GetBackgroundSpriteAddress(backgroundId)))
                .AttachExternalCancellation(token);
            token.ThrowIfCancellationRequested();
            if (this == null || version != m_backgroundVersion) return;

            // 两张图片全部就绪后一起替换, 防止新旧壁纸与立绘混用.
            m_wallpaperImage.sprite = images.Item1;
            m_heroImage.sprite = images.Item2;
            m_heroImage.SetNativeSize();
            m_wallpaperImage.SetNativeSize();
            ApplyOffsets(backgroundId);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception exception)
        {
            if (token.IsCancellationRequested || this == null || version != m_backgroundVersion) return;
            m_backgroundId = null;
            ALog.LogError($"大厅背景加载失败. BackgroundId={backgroundId}; Error={exception.Message}", ALogCategories.UI);
        }
    }

    void ApplyOffsets(int backgroundId)
    {
        Vector3 spriteOffset = Vector3.zero;
        Vector3 downOffset = Vector3.zero;
        if (m_WallpaperDisplayConfig != null)
        {
            m_WallpaperDisplayConfig.GetOffsets(backgroundId, out spriteOffset, out downOffset);
        }

        m_wallpaper.transform.localPosition = downOffset;
        m_heroSprite.transform.localPosition = spriteOffset;
    }

    static void SetImageAlpha(Image image, float alpha)
    {
        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }
}
