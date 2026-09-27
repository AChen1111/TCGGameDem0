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
    public Texture Texture => m_Art.texture;
    public void SetTexture(Texture texture, int rarity)
    { ++m_version; m_Art.texture = texture; m_Art.material = m_Versions[rarity]; }
    public async UniTask BindAsync(string pool, string id, int rarity, CancellationToken ct)
    {
        int version = ++m_version;
        m_Art.texture = null;
        m_Art.material = m_Versions[rarity];
        var texture = await CardPoolAddress.LoadCardTextureAsync(pool, id).AttachExternalCancellation(ct);
        if (ct.IsCancellationRequested || version != m_version) return;
        m_Art.texture = texture;
    }
    void OnDisable() => ++m_version;
}
