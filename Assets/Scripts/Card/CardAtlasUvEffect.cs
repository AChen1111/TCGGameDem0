using UnityEngine;
using UnityEngine.UI;

/// <summary>卡牌特效按卡内 UV 计算，主纹理按图集 UV 采样，不复制材质。</summary>
public sealed class CardAtlasUvEffect : BaseMeshEffect
{
    [SerializeField] RawImage m_image;
    public override void ModifyMesh(VertexHelper vertices)
    {
        if (!IsActive()) return;
        var rect = m_image.uvRect;
        var region = new Vector4(rect.x, rect.y, rect.width, rect.height);
        var vertex = new UIVertex();
        for (int index = 0; index < vertices.currentVertCount; index++)
        {
            vertices.PopulateUIVertex(ref vertex, index);
            vertex.uv1 = new Vector4((vertex.uv0.x - rect.x) / rect.width,
                (vertex.uv0.y - rect.y) / rect.height, 0, 0);
            vertex.uv2 = region;
            vertices.SetUIVertex(vertex, index);
        }
    }
}
