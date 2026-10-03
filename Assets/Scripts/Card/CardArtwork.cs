using UnityEngine;
using UnityEngine.UI;

/// <summary>卡图使用共享图集页与独立 UV 区域，尺寸仍表示原卡图尺寸。</summary>
public sealed class CardArtwork
{
    public Texture Texture { get; }
    public Rect UvRect { get; }
    public float Width { get; }
    public float Height { get; }

    public CardArtwork(Sprite sprite)
    {
        Texture = sprite.texture;
        var rect = sprite.textureRect;
        UvRect = new Rect(rect.x / Texture.width, rect.y / Texture.height,
            rect.width / Texture.width, rect.height / Texture.height);
        Width = sprite.rect.width; Height = sprite.rect.height;
    }
    public CardArtwork(Texture texture, Rect uvRect, float width, float height)
    { Texture = texture; UvRect = uvRect; Width = width; Height = height; }
    public static CardArtwork FromTexture(Texture texture) =>
        new CardArtwork(texture, new Rect(0, 0, 1, 1), texture.width, texture.height);
    public void ApplyTo(RawImage image) { image.texture = Texture; image.uvRect = UvRect; }
}
