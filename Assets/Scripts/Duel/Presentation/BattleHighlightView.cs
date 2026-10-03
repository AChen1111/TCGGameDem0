using UnityEngine;

namespace AChen.Duel.Presentation
{
    public sealed class BattleHighlightView : MonoBehaviour
    {
        [SerializeField] Renderer m_surface;
        [SerializeField] Texture m_texture;
        [SerializeField] bool m_hideInactive;
        MaterialPropertyBlock m_properties;
        Material m_template;
        Texture m_page;
        bool m_textured;
        bool m_effect, m_selected, m_hint;
        public Bounds WorldBounds => m_surface.bounds;
        void Awake()
        {
            m_properties = new MaterialPropertyBlock(); m_template = m_surface.sharedMaterial;
            m_textured = m_template.HasProperty("_BaseMap");
            if (m_textured)
            {
                var artwork = CardArtwork.FromTexture(m_texture);
                if(m_surface is SpriteRenderer spriteRenderer)
                {
                    var sprite=spriteRenderer.sprite; artwork=new CardArtwork(sprite);
                    m_properties.SetFloat("_SpriteAtlasUV",1);
                }
                m_page = artwork.Texture; m_surface.sharedMaterial = BattleArtworkMaterials.Acquire(m_template, m_page);
                var uv=artwork.UvRect;
                m_properties.SetVector("_CardUvRect", new Vector4(uv.x, uv.y, uv.width, uv.height));
            }
            Apply();
        }
        public void SetTexture(Texture texture) => SetArtwork(CardArtwork.FromTexture(texture));
        public void SetArtwork(CardArtwork artwork)
        {
            if (m_textured && m_page != artwork.Texture)
            {
                BattleArtworkMaterials.Release(m_template, m_page);
                m_page = artwork.Texture; m_surface.sharedMaterial = BattleArtworkMaterials.Acquire(m_template, m_page);
            }
            var uv = artwork.UvRect; m_properties.SetVector("_CardUvRect", new Vector4(uv.x, uv.y, uv.width, uv.height)); Apply();
        }
        public void SetEffectAvailable(bool available) { m_effect=available; m_properties.SetFloat("_EffectAvailable", available ? 1 : 0); Apply(); }
        public void SetSelected(bool selected) { m_selected=selected; m_properties.SetFloat("_Selected", selected ? 1 : 0); Apply(); }
        public void SetNegated(bool negated) { m_properties.SetFloat("_Negated", negated ? 1 : 0); Apply(); }
        public void SetTargetHint(bool visible) { m_hint=visible; m_properties.SetFloat("_Hint", visible ? 1 : 0); Apply(); }
        void Apply() { m_surface.SetPropertyBlock(m_properties); m_surface.enabled=!m_hideInactive || m_effect || m_selected || m_hint; }
        void OnDestroy() { if (m_textured) BattleArtworkMaterials.Release(m_template, m_page); }
    }
}
