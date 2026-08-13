using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// 调用堆栈的可视化:把栈帧画成自上而下的调用链,
/// 最上方是最外层调用者,箭头指向被调用方,最下方是日志发生的位置。
/// 节点可点击跳转源码;没有堆栈信息时显示空状态。
/// </summary>
public class ALogStackGraph : VisualElement
{
    private const float NodeWidth = 460f;
    private const float NodeHeight = 58f;
    private const float Gap = 46f;
    private const float Margin = 20f;

    private static readonly Color LineColor = new Color(0.55f, 0.62f, 0.72f);

    private readonly List<Rect> m_nodeRects = new List<Rect>();

    public ALogStackGraph() {
        style.position = Position.Relative;
        generateVisualContent += OnGenerateVisualContent;
    }

    public void SetFrames(List<ALogFrame> frames) {
        Clear();
        m_nodeRects.Clear();

        if (frames == null || frames.Count == 0)
        {
            style.height = 120f;
            var empty = new Label("无堆栈信息");
            empty.AddToClassList("stack-graph-empty");
            Add(empty);
            MarkDirtyRepaint();
            return;
        }

        //堆栈是由内向外的,反过来画才是调用发生的先后顺序
        for (int i = frames.Count - 1; i >= 0; i--)
        {
            int index = frames.Count - 1 - i;
            var rect = new Rect(Margin, Margin + index * (NodeHeight + Gap), NodeWidth, NodeHeight);
            m_nodeRects.Add(rect);
            Add(CreateNode(frames[i], index, index == frames.Count - 1, rect));
        }

        style.height = Margin * 2 + frames.Count * NodeHeight + (frames.Count - 1) * Gap;
        MarkDirtyRepaint();
    }

    private VisualElement CreateNode(ALogFrame frame, int order, bool isLeaf, Rect rect) {
        var node = new VisualElement();
        node.AddToClassList("stack-node");
        if (isLeaf)
        {
            node.AddToClassList("stack-node--leaf");
        }
        node.style.position = Position.Absolute;
        node.style.left = rect.x;
        node.style.top = rect.y;
        node.style.width = rect.width;
        node.style.height = rect.height;

        var title = new Label($"{order + 1}. {frame.Signature}");
        title.AddToClassList("stack-node__title");
        node.Add(title);

        var location = new Label(frame.Location);
        location.AddToClassList("stack-node__location");
        node.Add(location);

        if (frame.CanJump)
        {
            node.AddToClassList("stack-node--clickable");
            node.RegisterCallback<ClickEvent>(_ => ALogSourceJump.Open(frame));
        }
        return node;
    }

    private void OnGenerateVisualContent(MeshGenerationContext context) {
        if (m_nodeRects.Count < 2)
        {
            return;
        }

        Painter2D painter = context.painter2D;
        painter.strokeColor = LineColor;
        painter.fillColor = LineColor;
        painter.lineWidth = 2f;

        float centerX = Margin + NodeWidth * 0.5f;
        for (int i = 0; i < m_nodeRects.Count - 1; i++)
        {
            float top = m_nodeRects[i].yMax;
            float bottom = m_nodeRects[i + 1].yMin;

            painter.BeginPath();
            painter.MoveTo(new Vector2(centerX, top));
            painter.LineTo(new Vector2(centerX, bottom - 10f));
            painter.Stroke();

            painter.BeginPath();
            painter.MoveTo(new Vector2(centerX, bottom));
            painter.LineTo(new Vector2(centerX - 6f, bottom - 11f));
            painter.LineTo(new Vector2(centerX + 6f, bottom - 11f));
            painter.ClosePath();
            painter.Fill();
        }
    }
}
