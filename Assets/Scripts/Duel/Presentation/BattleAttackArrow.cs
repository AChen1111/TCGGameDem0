using UnityEngine;

namespace AChen.Duel.Presentation
{
    public sealed class BattleAttackArrow : MonoBehaviour
    {
        [SerializeField] MeshFilter m_filter;
        [SerializeField] MeshRenderer m_renderer;
        [SerializeField] Texture m_texture;
        [SerializeField] float m_width = 2.2f;
        Mesh m_mesh;
        void Awake()
        {
            m_mesh = new Mesh { name = "Duel attack arrow" }; m_filter.sharedMesh = m_mesh;
            var properties = new MaterialPropertyBlock(); properties.SetTexture("_BaseMap", m_texture);
            m_renderer.SetPropertyBlock(properties); Hide();
        }
        public void Show(Vector3 from, Vector3 to)
        {
            Vector3 direction = (to - from).normalized;
            Vector3 side = Vector3.Cross(Vector3.up, direction) * m_width * .5f;
            from.y += 1.2f; to.y += 1.2f;
            Vector3 shoulder = to - direction * Mathf.Min(3.5f, Vector3.Distance(from, to) * .25f);
            m_mesh.vertices = new[] { from - side, from + side, shoulder + side, shoulder - side,
                shoulder - side * 2.3f, shoulder + side * 2.3f, to };
            m_mesh.uv = new[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(.75f, 1), new Vector2(.75f, 0),
                new Vector2(.75f, 0), new Vector2(.75f, 1), new Vector2(1, .5f) };
            m_mesh.colors = new[] { Color.red, Color.red, Color.red, Color.red, Color.red, Color.red, Color.red };
            m_mesh.triangles = new[] { 0, 1, 2, 0, 2, 3, 4, 5, 6 }; m_mesh.RecalculateBounds(); m_renderer.enabled = true;
        }
        public void Hide() => m_renderer.enabled = false;
        void OnDestroy() => Destroy(m_mesh);
    }
}
