using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Sprites;

/// <summary>列表头像在同一张图集、同一个共享材质中合成，不创建独立 Stencil 遮罩。</summary>
public sealed class AvatarCompositeGraphic : MaskableGraphic
{
    Texture m_texture;
    Vector4 m_avatarUV, m_frameUV, m_maskUV;
    float m_aspect;
    bool m_ready;
    public override Texture mainTexture => m_texture;

    public void SetPortrait(Sprite avatar, Sprite frame, Sprite mask)
    {
        m_texture = avatar.texture;
        m_avatarUV = DataUtility.GetOuterUV(avatar);
        m_frameUV = DataUtility.GetOuterUV(frame);
        m_maskUV = DataUtility.GetOuterUV(mask);
        m_aspect = avatar.rect.width / avatar.rect.height;
        m_ready = true;
        SetAllDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        if (!m_ready) return;
        var rect = GetPixelAdjustedRect();
        float aspect = rect.width / rect.height;
        var crop = new Vector2(Mathf.Min(1, aspect / m_aspect), Mathf.Min(1, m_aspect / aspect));
        var corners = new[] { Vector2.zero, Vector2.up, Vector2.one, Vector2.right };
        foreach (var p in corners)
        {
            var avatar = (p - Vector2.one * .5f) * crop + Vector2.one * .5f;
            var v = UIVertex.simpleVert;
            v.position = new Vector3(rect.x + p.x * rect.width, rect.y + p.y * rect.height);
            v.color = color;
            v.uv0 = UV(m_avatarUV, avatar);
            var frame = UV(m_frameUV, p); var mask = UV(m_maskUV, p);
            v.uv1 = new Vector4(frame.x, frame.y, mask.x, mask.y);
            vh.AddVert(v);
        }
        vh.AddTriangle(0, 1, 2); vh.AddTriangle(2, 3, 0);
    }

    static Vector2 UV(Vector4 rect, Vector2 p) => new(Mathf.Lerp(rect.x, rect.z, p.x), Mathf.Lerp(rect.y, rect.w, p.y));
}
