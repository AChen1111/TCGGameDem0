using UnityEngine;

namespace AChen.Duel.Presentation
{
    public sealed class BattleSafeArea : MonoBehaviour
    {
        [SerializeField] RectTransform m_root;
        Rect m_previous;
        Vector2 m_size;
        void Update()
        {
            var size = new Vector2(Screen.width, Screen.height);
            Rect area = Screen.safeArea;
            if (area == m_previous && size == m_size) return;
            m_previous = area; m_size = size;
            m_root.anchorMin = new Vector2(area.xMin / size.x, area.yMin / size.y);
            m_root.anchorMax = new Vector2(area.xMax / size.x, area.yMax / size.y);
            m_root.offsetMin = m_root.offsetMax = Vector2.zero;
        }
    }
}
