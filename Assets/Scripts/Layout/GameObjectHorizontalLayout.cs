using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 类似 UI Horizontal Layout Group, 沿局部 X 排列子 GameObject.
/// 默认只改 localPosition.x, Y/Z 保持原值. 开启缩放控制后会同时写入 localScale.
/// 子物体原点按槽位中心放置.
/// </summary>
[ExecuteAlways]
[DisallowMultipleComponent]
[AddComponentMenu("Layout/GameObject Horizontal Layout")]
public class GameObjectHorizontalLayout : MonoBehaviour
{
    public enum ChildAlignment
    {
        Left = 0,
        Center = 1,
        Right = 2,
    }

    [Header("Padding")]
    [SerializeField] float _paddingLeft;
    [SerializeField] float _paddingRight;

    [Header("Child Layout")]
    [SerializeField] float _spacing = 0.1f;
    [SerializeField, Min(0.0001f), Tooltip("未缩放时每个子物体沿 X 占用的宽度")]
    float _childSize = 1f;
    [SerializeField] ChildAlignment _childAlignment = ChildAlignment.Center;
    [SerializeField] bool _reverseArrangement;
    [SerializeField] bool _ignoreInactive = true;
    [SerializeField, Tooltip("开启后把所有参与排列的子物体缩放到 Child Scale")]
    bool _controlChildScale;
    [SerializeField, ShowIf(nameof(_controlChildScale)), Tooltip("写入子物体的 localScale. 占位宽度按 Child Size * |X| 计算")]
    Vector3 _childScale = Vector3.one;

    readonly List<Transform> _children = new List<Transform>(16);
    bool _dirty = true;

    void OnEnable()
    {
        _dirty = true;
    }

    void OnValidate()
    {
        if (_childSize < 0.0001f)
        {
            _childSize = 0.0001f;
        }

        _dirty = true;
    }

    void OnTransformChildrenChanged()
    {
        _dirty = true;
    }

    void LateUpdate()
    {
#if UNITY_EDITOR
        // 编辑模式持续对齐, 避免在 Scene 里拖子物体后排版被拉开
        if (!Application.isPlaying)
        {
            ApplyLayout(false);
            return;
        }
#endif
        if (_dirty)
        {
            ApplyLayout(false);
        }
    }

    [Button("立即重排")]
    public void Rebuild()
    {
        ApplyLayout(true);
    }

    void ApplyLayout(bool log)
    {
        _dirty = false;
        CollectChildren();

        int n = _children.Count;
        if (n == 0)
        {
            if (log)
            {
                ALog.Log($"GameObject 水平布局完成. Parent={name}; Count=0", ALogCategories.Default);
            }

            return;
        }

        float occupiedWidth = GetOccupiedWidth();
        float contentWidth = n * occupiedWidth + (n - 1) * _spacing;
        float startX = GetStartX(contentWidth, occupiedWidth);
        float step = occupiedWidth + _spacing;
        bool changed = false;

        for (int slot = 0; slot < n; slot++)
        {
            Transform child = _children[_reverseArrangement ? n - 1 - slot : slot];
            float x = startX + slot * step;
            Vector3 pos = child.localPosition;
            if (!Mathf.Approximately(pos.x, x))
            {
                pos.x = x;
                child.localPosition = pos;
                changed = true;
            }

            if (_controlChildScale && !ApproximatelyScale(child.localScale, _childScale))
            {
                child.localScale = _childScale;
                changed = true;
            }
        }

        if (log)
        {
            ALog.Log(
                $"GameObject 水平布局完成. Parent={name}; Count={n}; ChildSize={_childSize}; Spacing={_spacing}; ControlScale={_controlChildScale}; Scale={_childScale}; Changed={changed}",
                ALogCategories.Default);
        }
    }

    float GetOccupiedWidth()
    {
        if (_controlChildScale)
        {
            return _childSize * Mathf.Abs(_childScale.x);
        }

        return _childSize;
    }

    static bool ApproximatelyScale(Vector3 a, Vector3 b)
    {
        return Mathf.Approximately(a.x, b.x)
            && Mathf.Approximately(a.y, b.y)
            && Mathf.Approximately(a.z, b.z);
    }

    float GetStartX(float contentWidth, float occupiedWidth)
    {
        float half = occupiedWidth * 0.5f;
        switch (_childAlignment)
        {
            case ChildAlignment.Left:
                return _paddingLeft + half;
            case ChildAlignment.Right:
                return -_paddingRight - contentWidth + half;
            default:
                return (_paddingLeft - _paddingRight) * 0.5f - contentWidth * 0.5f + half;
        }
    }

    void CollectChildren()
    {
        _children.Clear();
        Transform root = transform;
        int count = root.childCount;
        for (int i = 0; i < count; i++)
        {
            Transform child = root.GetChild(i);
            if (_ignoreInactive && !child.gameObject.activeSelf)
            {
                continue;
            }

            _children.Add(child);
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        CollectChildren();
        int n = _children.Count;
        if (n == 0)
        {
            return;
        }

        float occupiedWidth = GetOccupiedWidth();
        float contentWidth = n * occupiedWidth + (n - 1) * _spacing;
        float startX = GetStartX(contentWidth, occupiedWidth);
        float step = occupiedWidth + _spacing;

        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = new Color(0.3f, 0.75f, 1f, 0.9f);
        for (int slot = 0; slot < n; slot++)
        {
            float x = startX + slot * step;
            Gizmos.DrawWireCube(new Vector3(x, 0f, 0f), new Vector3(occupiedWidth, 0.05f, 0.05f));
        }
    }
#endif
}
