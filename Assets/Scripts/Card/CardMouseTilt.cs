using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 鼠标在卡面上移动时的倾斜. 只叠加在翻面角度之上, 不改 Yaw.
/// 命中用静止卡面(翻面前的平面), 避免卡一歪射线就打空, 边缘抽搐.
/// 执行顺序 100, 晚于 CardFlip. 最终旋转 = 初始朝向 * 倾斜 * 翻面, 背面上下才和正面一致.
/// </summary>
[DefaultExecutionOrder(100)]
public sealed class CardMouseTilt : MonoBehaviour
{
    [SerializeField, Min(0f)] float m_degreesPerUnit = 20f;
    [SerializeField, Min(0.01f)] float m_lerpSpeed = 12f;

    Quaternion m_rest;
    Quaternion m_tilt = Quaternion.identity;
    // 卡面半宽半高, 用来判断鼠标是否还在牌面上
    Vector2 m_half;
    CardFlip m_flip;

    void Awake()
    {
        m_rest = transform.localRotation;
        m_flip = GetComponent<CardFlip>();
        var collider = GetComponentInChildren<Collider>();
        var extents = collider != null ? collider.bounds.extents : new Vector3(0.5f, 0.73f, 0f);
        m_half = new Vector2(extents.x, extents.y);
    }

    void OnDisable()
    {
        m_tilt = Quaternion.identity;
        Apply(m_tilt);
    }

    void LateUpdate()
    {
        var target = Quaternion.identity;
        if (TryHover(out var offset))
        {
            // 相当于在命中点沿法线推一下: 偏上则绕 X 转, 偏右则绕 Y 反转
            target = Quaternion.Euler(
                offset.y * m_degreesPerUnit,
                -offset.x * m_degreesPerUnit,
                0f);
        }

        // 指数插值, 跟帧率无关地平滑跟上
        m_tilt = Quaternion.Slerp(
            m_tilt,
            target,
            1f - Mathf.Exp(-m_lerpSpeed * Time.deltaTime));
        Apply(m_tilt);
    }

    void Apply(Quaternion tilt)
    {
        float yaw = m_flip != null ? m_flip.Yaw : 0f;
        // 先翻面, 再按静止卡面轴向倾斜, 否则 Y=180 后局部 X 反向, 上下会反
        transform.localRotation = m_rest * tilt * Quaternion.Euler(0f, yaw, 0f);
    }

    bool TryHover(out Vector2 offset)
    {
        offset = default;
        var camera = Camera.main;
        if (camera == null || Mouse.current == null)
        {
            return false;
        }

        // 用未倾斜的卡面朝向建平面, 不跟当前旋转走
        var restWorld = transform.parent != null
            ? transform.parent.rotation * m_rest
            : m_rest;
        var ray = camera.ScreenPointToRay(Mouse.current.position.ReadValue());
        var plane = new Plane(restWorld * Vector3.back, transform.position);
        if (!plane.Raycast(ray, out float enter))
        {
            return false;
        }

        var local = Quaternion.Inverse(restWorld) * (ray.GetPoint(enter) - transform.position);
        if (Mathf.Abs(local.x) > m_half.x || Mathf.Abs(local.y) > m_half.y)
        {
            return false;
        }

        // 相对卡心的偏移. (0,0) 是中心, 此时倾斜为 0
        offset = new Vector2(local.x, local.y);
        return true;
    }
}
