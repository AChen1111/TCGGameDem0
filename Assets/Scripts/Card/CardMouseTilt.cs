using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 电脑鼠标悬停、手机陀螺仪姿态控制卡面倾斜. 只叠加在翻面角度之上, 不改 Yaw.
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
    CardPickController m_controller;

    const float MobileMaxTilt = 18f;
    static AttitudeSensor s_sensor;
    static int s_sensorUsers;
    static bool s_enabledSensor;
    bool m_usesSensor;
    AttitudeSensor m_sensor;
    Quaternion m_neutral;
    bool m_hasNeutral;
    double m_readAfter;
    ScreenOrientation m_orientation;

    void Awake()
    {
        m_rest = transform.localRotation;
        m_flip = GetComponent<CardFlip>();
        m_controller = GetComponentInParent<CardPickController>();
        var collider = GetComponentInChildren<Collider>();
        var extents = collider != null ? collider.bounds.extents : new Vector3(0.5f, 0.73f, 0f);
        m_half = new Vector2(extents.x, extents.y);
    }

    void OnEnable()
    {
        m_usesSensor = Application.isMobilePlatform && !Application.isEditor;
        if (m_usesSensor) s_sensorUsers++;
        ResetNeutral();
    }

    void OnDisable()
    {
        if (m_usesSensor)
        {
            m_usesSensor = false;
            if (--s_sensorUsers == 0) ReleaseSensor();
        }
        ResetNeutral();
        m_tilt = Quaternion.identity;
        Apply(m_tilt);
    }

    void OnApplicationFocus(bool focused)
    {
        if (focused) ResetNeutral();
    }

    void ResetNeutral()
    {
        m_hasNeutral = false;
        m_readAfter = m_sensor != null ? m_sensor.lastUpdateTime : 0;
        m_orientation = Screen.orientation;
    }

    static void ReleaseSensor()
    {
        // 多张卡共享传感器，只在最后一张关闭时释放本组件启用的设备。
        if (s_enabledSensor && s_sensor != null && s_sensor.added)
            InputSystem.DisableDevice(s_sensor);
        s_sensor = null;
        s_enabledSensor = false;
    }

    bool TryDeviceTilt(out Quaternion tilt)
    {
        tilt = Quaternion.identity;
        var sensor = AttitudeSensor.current;
        if (sensor != s_sensor)
        {
            ReleaseSensor();
            s_sensor = sensor;
        }
        if (sensor == null) return false;
        if (!sensor.enabled)
        {
            InputSystem.EnableDevice(sensor);
            s_enabledSensor = true;
        }
        if (m_sensor != sensor)
        {
            m_sensor = sensor;
            ResetNeutral();
        }
        if (m_orientation != Screen.orientation) ResetNeutral();
        // 等到启用/恢复后的新采样，避免把默认值或后台旧姿态作为中立位置。
        if (sensor.lastUpdateTime <= m_readAfter) return false;
        var attitude = sensor.attitude.ReadValue();
        if (!InputSystem.settings.compensateForScreenOrientation)
        {
            float roll = Screen.orientation == ScreenOrientation.LandscapeLeft ? -90f
                : Screen.orientation == ScreenOrientation.LandscapeRight ? 90f
                : Screen.orientation == ScreenOrientation.PortraitUpsideDown ? 180f : 0f;
            attitude *= Quaternion.Euler(0f, 0f, roll);
        }
        // 传感器右手坐标转为 Unity 坐标；只取相对屏幕横纵轴的旋转，不叠加滚转。
        attitude = new Quaternion(attitude.x, attitude.y, -attitude.z, -attitude.w);
        float magnitude = Quaternion.Dot(attitude, attitude);
        if (float.IsNaN(magnitude) || float.IsInfinity(magnitude) || magnitude < 0.001f) return false;
        attitude = attitude.normalized;
        if (!m_hasNeutral)
        {
            m_neutral = attitude;
            m_hasNeutral = true;
        }
        var angles = (Quaternion.Inverse(m_neutral) * attitude).eulerAngles;
        tilt = Quaternion.Euler(
            Mathf.Clamp(Mathf.DeltaAngle(0f, angles.x), -MobileMaxTilt, MobileMaxTilt),
            Mathf.Clamp(Mathf.DeltaAngle(0f, angles.y), -MobileMaxTilt, MobileMaxTilt), 0f);
        return true;
    }

    void LateUpdate()
    {
        var target = Quaternion.identity;
        if (m_usesSensor)
        {
            TryDeviceTilt(out target);
        }
        else if (TryHover(out var offset))
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
        if (m_controller == null)
        {
            m_controller = GetComponentInParent<CardPickController>();
        }

        if (m_controller == null || !m_controller.TryGetPointerRay(out Ray ray))
        {
            return false;
        }

        // 用未倾斜的卡面朝向建平面, 不跟当前旋转走
        var restWorld = transform.parent != null
            ? transform.parent.rotation * m_rest
            : m_rest;
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
