using LitMotion;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 卡牌翻面. 只负责绕 Y 轴转 180 度, 不处理鼠标倾斜.
/// 调用 Flip() 会打断当前动画, 立刻翻向另一面.
/// 执行顺序 50, 先于 CardMouseTilt(100), 倾斜脚本会再叠一层旋转.
/// </summary>
[DefaultExecutionOrder(50)]
public sealed class CardFlip : MonoBehaviour
{
    [SerializeField, Min(0.01f)] float m_duration = 0.45f;
    [SerializeField] Ease m_ease = Ease.InOutCubic;

    // 进播放时的朝向, 翻面只在这之上加 Y 旋转
    Quaternion m_rest;
    MotionHandle m_handle;
    // true 表示目标是卡背朝向相机
    bool m_showBack;

    /// <summary>当前绕 Y 转了多少度. 0 正面, 180 背面. 倾斜脚本会读这个值.</summary>
    public float Yaw { get; private set; }

    void Awake()
    {
        m_rest = transform.localRotation;
    }

    void OnDisable()
    {
        m_handle.TryCancel();
        Yaw = m_showBack ? 180f : 0f;
        Apply();
    }

    void Update()
    {
        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame)
        {
            return;
        }

        var camera = Camera.main;
        if (camera == null)
        {
            return;
        }

        // 从鼠标位置打一条射线, 打到这张卡的子物体(正面/背面/侧边)就翻面
        var ray = camera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out var hit) && hit.transform.IsChildOf(transform))
        {
            Flip();
        }
    }

    void LateUpdate()
    {
        Apply();
    }

    [Button("翻面")]
    public void Flip()
    {
        m_handle.TryCancel();
        m_showBack = !m_showBack;
        float from = Yaw;
        // DeltaAngle 走劣弧, 中途再点一次会从当前角度折回去, 而不是继续转满 180
        float to = from + Mathf.DeltaAngle(from, m_showBack ? 180f : 0f);
        ALog.Log($"卡牌翻面. Card={name}; ShowBack={m_showBack}", ALogCategories.Default);
        m_handle = LMotion.Create(from, to, m_duration)
            .WithEase(m_ease)
            .Bind(v => Yaw = v)
            .AddTo(this);
    }

    void Apply()
    {
        transform.localRotation = m_rest * Quaternion.Euler(0f, Yaw, 0f);
    }
}
