using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

/// <summary>UI卡图共享五种版本材质，异步绑定用版本号隔离复用后的旧请求。</summary>
public sealed class DeckCardView : MonoBehaviour
{
    [SerializeField] RawImage m_Art;
    [SerializeField] Material[] m_Versions;
    int m_version;
    CardArtwork m_artwork;
    public CardArtwork Texture => m_artwork;
    public RectTransform ArtRect => m_Art.rectTransform;
    public void SetArtVisible(bool visible) => m_Art.enabled = visible;
    public void SetTexture(CardArtwork texture, int rarity)
    { ++m_version; m_artwork = texture; texture.ApplyTo(m_Art); m_Art.material = m_Versions[rarity]; }
    public async UniTask BindAsync(string pool, string id, int rarity, CancellationToken ct)
    {
        int version = ++m_version;
        m_artwork = null;
        m_Art.texture = null;
        m_Art.material = m_Versions[rarity];
        var texture = await CardPoolAddress.LoadCardArtworkAsync(pool, id).AttachExternalCancellation(ct);
        if (ct.IsCancellationRequested || version != m_version) return;
        m_artwork = texture;
        texture.ApplyTo(m_Art);
    }
    void OnDisable() => ++m_version;
}
