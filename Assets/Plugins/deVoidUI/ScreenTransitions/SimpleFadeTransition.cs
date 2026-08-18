using System;
using UnityEngine;

/// <summary>
/// 简单淡入淡出。正式项目建议用 DOTween 等。
/// </summary>
public class SimpleFadeTransition : ATransitionComponent
{
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private bool fadeOut = false;

    private CanvasGroup canvasGroup;
    private float timer;
    private Action currentAction;
    private Transform currentTarget;

    private float startValue;
    private float endValue;

    private bool shouldAnimate;

    public override void Animate(Transform target, Action callWhenFinished) {
        if (currentAction != null) {
            canvasGroup.alpha = endValue;
            currentAction();
        }

        canvasGroup = target.GetComponent<CanvasGroup>();
        if (canvasGroup == null) {
            canvasGroup = target.gameObject.AddComponent<CanvasGroup>();
        }

        if (fadeOut) {
            startValue = 1f;
            endValue = 0f;
        }
        else {
            startValue = 0f;
            endValue = 1f;
        }

        currentAction = callWhenFinished;
        timer = fadeDuration;

        canvasGroup.alpha = startValue;
        shouldAnimate = true;
    }

    private void Update() {
        if (!shouldAnimate) {
            return;
        }

        if (timer > 0f) {
            timer -= Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(endValue, startValue, timer / fadeDuration);
        }
        else {
            canvasGroup.alpha = 1f;
            if (currentAction != null) {
                currentAction();
            }

            currentAction = null;
            shouldAnimate = false;
        }
    }
}
