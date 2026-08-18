using UnityEngine;
using System;

/// <summary>
/// 界面开关动画。可扩展为 Lerp、Animation 等。
/// </summary>
public abstract class ATransitionComponent : MonoBehaviour {
    /// <summary>播放 target 的动画，结束后调用 callWhenFinished。</summary>
    /// <param name="target">目标 Transform</param>
    /// <param name="callWhenFinished">动画结束回调</param>
    public abstract void Animate(Transform target, Action callWhenFinished);
}
