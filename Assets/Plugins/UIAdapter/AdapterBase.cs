// 项目修改: 移除 Feif.UI 命名空间, 保留原有接口和适配行为.
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
[DisallowMultipleComponent]
[ExecuteAlways]
public abstract class AdapterBase : MonoBehaviour
{
    public abstract void Adapt();
}
