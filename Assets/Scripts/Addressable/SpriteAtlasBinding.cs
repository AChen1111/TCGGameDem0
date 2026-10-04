// [UNITY-SKILL:SPRITEATLAS]
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.U2D;

/// <summary>与 AddressableLoader 同寿命，按 atlasRequested 标签共享一次图集加载。</summary>
public sealed class SpriteAtlasBinding : IDisposable
{
    readonly AsyncOperationHandle<AtlasAddressableCatalog> m_catalog;
    readonly Dictionary<string, AsyncOperationHandle<SpriteAtlas>> m_atlases = new();
    readonly CancellationTokenSource m_lifetime = new();
    public SpriteAtlasBinding(AssetReferenceT<AtlasAddressableCatalog> reference)
    {
        m_catalog = Addressables.LoadAssetAsync<AtlasAddressableCatalog>(reference);
        SpriteAtlasManager.atlasRequested += OnRequested;
    }
    public async UniTask ReadyAsync() { await m_catalog.Task; }
    public async UniTask<SpriteAtlas> LoadAsync(string tag)
    {
        var catalog = await m_catalog.Task.AsUniTask().AttachExternalCancellation(m_lifetime.Token);
        if (!m_atlases.TryGetValue(tag, out var handle))
        {
            handle = Addressables.LoadAssetAsync<SpriteAtlas>(catalog.Get(tag));
            m_atlases.Add(tag, handle);
        }
        return await handle.Task.AsUniTask().AttachExternalCancellation(m_lifetime.Token);
    }
    void OnRequested(string tag, Action<SpriteAtlas> register) => RegisterAsync(tag, register).Forget();
    async UniTaskVoid RegisterAsync(string tag, Action<SpriteAtlas> register) => register(await LoadAsync(tag));
    public void Dispose()
    {
        SpriteAtlasManager.atlasRequested -= OnRequested;
        m_lifetime.Cancel(); m_lifetime.Dispose();
        foreach (var handle in m_atlases.Values) Addressables.Release(handle);
        m_atlases.Clear(); Addressables.Release(m_catalog);
    }
}
