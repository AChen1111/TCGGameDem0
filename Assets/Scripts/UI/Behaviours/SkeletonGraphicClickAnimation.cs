using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 点击 SkeletonGraphic 时播放一次临时动画,结束后恢复原来的动画。
/// </summary>
[RequireComponent(typeof(SkeletonGraphic))]
public sealed class SkeletonGraphicClickAnimation : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] SkeletonGraphic m_skeletonGraphic;
    [SerializeField, SpineAnimation] string m_clickAnimation;
    [SerializeField, Min(0)] int m_trackIndex;
    [SerializeField] bool m_ignoreClickWhilePlaying = true;

    bool m_isPlayingClickAnimation;

    void Reset()
    {
        m_skeletonGraphic = GetComponent<SkeletonGraphic>();
    }

    void Awake()
    {
        if (m_skeletonGraphic == null)
        {
            m_skeletonGraphic = GetComponent<SkeletonGraphic>();
        }
    }

    void OnDisable()
    {
        m_isPlayingClickAnimation = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        PlayClickAnimation();
    }

    public void PlayClickAnimation()
    {
        if (m_skeletonGraphic == null || !m_skeletonGraphic.IsValid)
        {
            ALog.LogWarning($"点击 Spine 动画失败: {name} 的 SkeletonGraphic 无效.", ALogCategories.UI);
            return;
        }

        if (m_ignoreClickWhilePlaying && m_isPlayingClickAnimation)
        {
            return;
        }

        SkeletonData skeletonData = m_skeletonGraphic.SkeletonDataAsset.GetSkeletonData(false);
        if (string.IsNullOrEmpty(m_clickAnimation)
            || skeletonData.FindAnimation(m_clickAnimation) == null)
        {
            ALog.LogWarning(
                $"点击 Spine 动画失败: Object={name}; Animation={m_clickAnimation}.",
                ALogCategories.UI);
            return;
        }

        Spine.AnimationState state = m_skeletonGraphic.AnimationState;
        TrackEntry previous = state.GetCurrent(m_trackIndex);
        string previousAnimation = previous?.Animation?.Name;
        bool previousLoop = previous?.Loop ?? true;

        if (string.IsNullOrEmpty(previousAnimation))
        {
            previousAnimation = m_skeletonGraphic.startingAnimation;
            previousLoop = m_skeletonGraphic.startingLoop;
        }

        m_isPlayingClickAnimation = true;
        TrackEntry clickEntry = state.SetAnimation(m_trackIndex, m_clickAnimation, false);
        clickEntry.Complete += OnClickAnimationComplete;

        if (!string.IsNullOrEmpty(previousAnimation))
        {
            state.AddAnimation(m_trackIndex, previousAnimation, previousLoop, 0f);
        }

        ALog.Log(
            $"播放点击 Spine 动画: Object={name}; Animation={m_clickAnimation}; Return={previousAnimation}.",
            ALogCategories.UI);
    }

    void OnClickAnimationComplete(TrackEntry trackEntry)
    {
        trackEntry.Complete -= OnClickAnimationComplete;
        m_isPlayingClickAnimation = false;
    }
}
