using UnityEngine;
using UnityEngine.UI;

// Resolution-independent frame matching the silver corner tabs in the deck layout.
[RequireComponent(typeof(CanvasRenderer))]
public sealed class DeckPanelFrame : MaskableGraphic
{
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        var r = rectTransform.rect;
        Quad(vh, r.xMin, r.yMin, r.width, 1.5f);
        Quad(vh, r.xMin, r.yMax - 1.5f, r.width, 1.5f);
        Quad(vh, r.xMin, r.yMin, 1.5f, r.height);
        Quad(vh, r.xMax - 1.5f, r.yMin, 1.5f, r.height);
        Corner(vh, new Vector2(r.xMin, r.yMin), 1, 1);
        Corner(vh, new Vector2(r.xMax, r.yMin), -1, 1);
        Corner(vh, new Vector2(r.xMin, r.yMax), 1, -1);
        Corner(vh, new Vector2(r.xMax, r.yMax), -1, -1);
    }
    void Quad(VertexHelper vh, float x, float y, float w, float h)
    {
        int i = vh.currentVertCount;
        vh.AddVert(new Vector3(x,y), color, Vector2.zero);
        vh.AddVert(new Vector3(x+w,y), color, Vector2.zero);
        vh.AddVert(new Vector3(x+w,y+h), color, Vector2.zero);
        vh.AddVert(new Vector3(x,y+h), color, Vector2.zero);
        vh.AddTriangle(i,i+1,i+2); vh.AddTriangle(i,i+2,i+3);
    }
    void Corner(VertexHelper vh, Vector2 point, int x, int y)
    {
        int i = vh.currentVertCount;
        vh.AddVert(point, color, Vector2.zero);
        vh.AddVert(point + new Vector2(x*14,0), color, Vector2.zero);
        vh.AddVert(point + new Vector2(0,y*14), color, Vector2.zero);
        vh.AddTriangle(i,i+1,i+2);
    }
}
