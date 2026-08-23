using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public sealed class ImageAspectLayoutElement : MonoBehaviour, ILayoutElement
{
    public float minWidth => preferredWidth;
    public float preferredWidth
    {
        get
        {
            var sprite = GetComponent<Image>().sprite;
            var height = ((RectTransform)transform.parent).rect.height;
            return height * sprite.rect.width / sprite.rect.height;
        }
    }
    public float flexibleWidth => 0;
    public float minHeight => 0;
    public float preferredHeight => 0;
    public float flexibleHeight => 0;
    public int layoutPriority => 1;

    public void CalculateLayoutInputHorizontal() { }
    public void CalculateLayoutInputVertical() { }
}
