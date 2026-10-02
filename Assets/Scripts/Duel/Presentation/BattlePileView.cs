using System.Linq;
using TMPro;
using UnityEngine;

namespace AChen.Duel.Presentation
{
    /// <summary>复用 DuelDeckAppearance 的 CardShuffleTop 厚度表现。</summary>
    public sealed class BattlePileView : MonoBehaviour
    {
        [SerializeField] Transform m_shuffleTop;
        [SerializeField] Renderer[] m_renderers;
        [SerializeField] TMP_Text m_count;
        [SerializeField] DuelZone m_kind;
        [SerializeField] int m_player;
        MaterialPropertyBlock m_properties;
        bool m_available, m_hovered, m_pressed;
        void Awake() => m_properties = new MaterialPropertyBlock();

        public void Refresh(DuelView view)
        {
            var zone = new ZoneRef(m_kind, m_player);
            int count = view.InZone(zone).Count;
            m_shuffleTop.localScale = count == 0 ? Vector3.zero : new Vector3(1f, count, 1f);
            m_count.text = count.ToString();
            SetEffectAvailable(view.AvailablePiles.Contains(zone));
        }
        public void SetEffectAvailable(bool available) { m_available = available; Apply(); }
        public void SetHovered(bool hovered) { m_hovered = hovered; m_count.gameObject.SetActive(hovered); Apply(); }
        public void SetPressed(bool pressed) { m_pressed = pressed; Apply(); }
        void Apply()
        {
            foreach (Renderer renderer in m_renderers)
            {
                renderer.GetPropertyBlock(m_properties);
                m_properties.SetFloat("_EffectAvailable", m_available ? 1f : 0f);
                m_properties.SetFloat("_Selected", m_hovered ? 1f : 0f);
                m_properties.SetFloat("_PressButton", m_pressed ? 1f : 0f);
                renderer.SetPropertyBlock(m_properties);
            }
        }
    }
}
