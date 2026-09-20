using SuperScrollView;
using UnityEngine;
using UnityEngine.UI;

/// <summary>把 SuperScrollView 行宽对齐当前视口, 避免宽屏右侧留空.</summary>
static class LoopListRowFit
{
    public static void FitWidth(LoopListView2 list, Component row)
    {
        if (list == null || row == null)
        {
            return;
        }

        RectTransform rowRt = row.transform as RectTransform;
        RectTransform viewport = list.GetComponent<ScrollRect>()?.viewport;
        if (rowRt == null)
        {
            return;
        }

        if (viewport == null)
        {
            viewport = list.GetComponent<RectTransform>();
        }

        float width = viewport.rect.width;
        if (width <= 1f)
        {
            return;
        }

        rowRt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
    }
}
