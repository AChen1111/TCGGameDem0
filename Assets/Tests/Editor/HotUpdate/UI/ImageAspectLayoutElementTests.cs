using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class ImageAspectLayoutElementTests
{
    [Test]
    public void PreferredWidth_FollowsParentHeightAndSpriteAspect()
    {
        var parent = new GameObject("Parent", typeof(RectTransform));
        var child = new GameObject("Child", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(ImageAspectLayoutElement));
        Texture2D tex = null;
        Sprite sprite = null;
        try
        {
            ((RectTransform)parent.transform).SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 40f);
            child.transform.SetParent(parent.transform, false);

            tex = new Texture2D(80, 40);
            sprite = Sprite.Create(tex, new Rect(0, 0, 80, 40), new Vector2(0.5f, 0.5f), 100f);
            child.GetComponent<Image>().sprite = sprite;

            Assert.AreEqual(80f, child.GetComponent<ImageAspectLayoutElement>().preferredWidth, 0.01f);
        }
        finally
        {
            Object.DestroyImmediate(child);
            Object.DestroyImmediate(parent);
            if (sprite != null)
                Object.DestroyImmediate(sprite);
            if (tex != null)
                Object.DestroyImmediate(tex);
        }
    }
}
