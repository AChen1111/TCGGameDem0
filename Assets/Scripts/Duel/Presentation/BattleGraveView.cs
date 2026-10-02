using System.Linq;
using TMPro;
using UnityEngine;

namespace AChen.Duel.Presentation
{
    /// <summary>墓地和除外共用原模型，分别更新原红绿通道灯光参数。</summary>
    public sealed class BattleGraveView : MonoBehaviour
    {
        [SerializeField] Renderer[] m_renderers;
        [SerializeField] TMP_Text m_count;
        [SerializeField] DuelZone m_kind;
        [SerializeField] int m_player;
        MaterialPropertyBlock m_properties;
        bool m_available, m_hovered, m_pressed, m_cardExists;
        string Prefix => m_kind == DuelZone.Graveyard ? "_Grave" : "_Exclude";
        void Awake() => m_properties = new MaterialPropertyBlock();

        public void Refresh(DuelView view)
        {
            var zone = new ZoneRef(m_kind, m_player);
            int count = view.InZone(zone).Count;
            m_count.text = count.ToString();
            m_cardExists = count > 0;
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
                m_properties.SetFloat(Prefix + "CardExist", m_cardExists ? 1f : 0f);
                m_properties.SetFloat(Prefix + "MouseOver", m_hovered ? 1f : 0f);
                m_properties.SetFloat(Prefix + "PressButton", m_pressed ? 1f : 0f);
                m_properties.SetFloat(Prefix + "EffectAvailable", m_available ? 1f : 0f);
                renderer.SetPropertyBlock(m_properties);
            }
        }
    }
}
