using LitMotion;
using LitMotion.Extensions;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 通用动画工具类
/// </summary>
static class UITween
{
    public enum MoveDirection
    {
        Left,
        Right,
        Up,
        Down
    }

    //移动动画
    public static MotionHandle DoMoveAnim(RectTransform target, MoveDirection direction, float distance, float duration)
    {
        var origin = target.anchoredPosition;
        var opposite = direction switch
        {
            MoveDirection.Left => Vector2.right,
            MoveDirection.Right => Vector2.left,
            MoveDirection.Up => Vector2.down,
            MoveDirection.Down => Vector2.up,
            _ => Vector2.zero
        };
        var from = origin + opposite * distance;
        target.anchoredPosition = from;
        return LMotion.Create(from, origin, duration)
            .WithEase(Ease.OutCubic)
            .BindToAnchoredPosition(target);
    }
    //画布透明度渐入动画
    public static MotionHandle DoFadeAnim(float from, float to, float duration, CanvasGroup target)
    {
        target.alpha = from;
        return LMotion.Create(from, to, duration)
            .WithEase(Ease.OutCubic)
            .BindToAlpha(target);
    }
    //Image的渐入渐出动画
    public static MotionHandle DoFadeAnim(float from, float to, float duration, Image target)
    {
        if (target.gameObject.activeSelf == false)
        {
            target.gameObject.SetActive(true);
        }
        target.color = new Color(target.color.r, target.color.g, target.color.b, from);
        return LMotion.Create(from, to, duration)
            .WithEase(Ease.OutCubic)
            .BindToColorA(target);
    }
}
