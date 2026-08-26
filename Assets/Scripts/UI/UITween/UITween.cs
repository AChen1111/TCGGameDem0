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

    public static MotionHandle DoVerticalReveal(Image target, float duration)
    {
        target.type = Image.Type.Filled;
        target.fillMethod = Image.FillMethod.Vertical;
        target.fillOrigin = (int)Image.OriginVertical.Top;
        target.fillAmount = 0;
        return LMotion.Create(0f, 1f, duration)
            .WithEase(Ease.OutCubic)
            .Bind(value => target.fillAmount = value);
    }

    public static MotionHandle DoScaleAnim(float from, float to, float duration, Transform target)
    {
        var fromScale = Vector3.one * from;
        target.localScale = fromScale;
        return LMotion.Create(fromScale, Vector3.one * to, duration)
            .WithEase(Ease.OutBack)
            .BindToLocalScale(target);
    }

    //短促缩放反馈：放大后回到原始尺寸
    public static MotionHandle DoPunchScale(Transform target, float scale, float duration)
    {
        var origin = target.localScale;
        var seq = LSequence.Create();
        seq.Append(LMotion.Create(origin, origin * scale, duration * 0.5f)
            .WithEase(Ease.OutCubic)
            .BindToLocalScale(target));
        seq.Append(LMotion.Create(origin * scale, origin, duration * 0.5f)
            .WithEase(Ease.OutCubic)
            .BindToLocalScale(target));
        return seq.Run();
    }
}
