using UnityEngine;

namespace AChen.Duel.Presentation
{
    public sealed class BattleHighlightView : MonoBehaviour
    {
        [SerializeField] Renderer m_surface;
        [SerializeField] Texture m_texture;
        [SerializeField] bool m_hideInactive;
        MaterialPropertyBlock m_properties;
        bool m_effect, m_selected, m_hint;
        void Awake() { m_properties = new MaterialPropertyBlock(); m_properties.SetTexture("_BaseMap", m_texture); Apply(); }
        public void SetTexture(Texture texture) { m_properties.SetTexture("_BaseMap", texture); Apply(); }
        public void SetEffectAvailable(bool available) { m_effect=available; m_properties.SetFloat("_EffectAvailable", available ? 1 : 0); Apply(); }
        public void SetSelected(bool selected) { m_selected=selected; m_properties.SetFloat("_Selected", selected ? 1 : 0); Apply(); }
        public void SetTargetHint(bool visible) { m_hint=visible; m_properties.SetFloat("_Hint", visible ? 1 : 0); Apply(); }
        void Apply() { m_surface.SetPropertyBlock(m_properties); m_surface.enabled=!m_hideInactive || m_effect || m_selected || m_hint; }
    }
}
