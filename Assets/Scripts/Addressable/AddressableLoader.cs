using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

public class AddressableLoader : PersistentMonoSingleton<AddressableLoader>
{
    [SerializeField] AssetReferenceT<SpriteAddressableCatalog> m_spriteCatalogRef;
    [SerializeField] AssetReferenceT<PrefabAddressableCatalog> m_prefabCatalogRef;
    [SerializeField] AssetReferenceT<SceneAddressableCatalog> m_sceneCatalogRef;
    [SerializeField] AssetReferenceT<UISettingsAddressableCatalog> m_uiSettingsCatalogRef;

    SpriteAddressableCatalog m_spriteCatalog;
    PrefabAddressableCatalog m_prefabCatalog;
    SceneAddressableCatalog m_sceneCatalog;
    UISettingsAddressableCatalog m_uiSettingsCatalog;

    AsyncOperationHandle<SpriteAddressableCatalog> m_spriteCatalogHandle;
    AsyncOperationHandle<PrefabAddressableCatalog> m_prefabCatalogHandle;
    AsyncOperationHandle<SceneAddressableCatalog> m_sceneCatalogHandle;
    AsyncOperationHandle<UISettingsAddressableCatalog> m_uiSettingsCatalogHandle;

    readonly Dictionary<string, AsyncOperationHandle<Sprite>> m_spriteHandles = new Dictionary<string, AsyncOperationHandle<Sprite>>();
    readonly Dictionary<string, AsyncOperationHandle<GameObject>> m_prefabHandles = new Dictionary<string, AsyncOperationHandle<GameObject>>();
    readonly Dictionary<string, AsyncOperationHandle<SceneInstance>> m_sceneHandles = new Dictionary<string, AsyncOperationHandle<SceneInstance>>();
    readonly Dictionary<string, AsyncOperationHandle<UISettings>> m_uiSettingsHandles = new Dictionary<string, AsyncOperationHandle<UISettings>>();

    protected override void OnInit()
    {
        LoadCatalogsAsync().Forget();
    }

    async UniTaskVoid LoadCatalogsAsync()
    {
        m_spriteCatalogHandle = Addressables.LoadAssetAsync<SpriteAddressableCatalog>(m_spriteCatalogRef);
        m_prefabCatalogHandle = Addressables.LoadAssetAsync<PrefabAddressableCatalog>(m_prefabCatalogRef);
        m_sceneCatalogHandle = Addressables.LoadAssetAsync<SceneAddressableCatalog>(m_sceneCatalogRef);
        m_uiSettingsCatalogHandle = Addressables.LoadAssetAsync<UISettingsAddressableCatalog>(m_uiSettingsCatalogRef);
        m_spriteCatalog = await m_spriteCatalogHandle.Task;
        m_prefabCatalog = await m_prefabCatalogHandle.Task;
        m_sceneCatalog = await m_sceneCatalogHandle.Task;
        m_uiSettingsCatalog = await m_uiSettingsCatalogHandle.Task;
        m_spriteCatalog.BuildMap();
        m_prefabCatalog.BuildMap();
        m_sceneCatalog.BuildMap();
        m_uiSettingsCatalog.BuildMap();
        IsDone = true;
    }

    public UniTask<Sprite> LoadSprite(string assetName)
    {
        return LoadAsset(m_spriteCatalog, m_spriteHandles, assetName);
    }

    public UniTask<GameObject> LoadPrefab(string assetName)
    {
        return LoadAsset(m_prefabCatalog, m_prefabHandles, assetName);
    }

    public UniTask<UISettings> LoadUISettings(string assetName)
    {
        return LoadAsset(m_uiSettingsCatalog, m_uiSettingsHandles, assetName);
    }

    public async UniTask<SceneInstance> LoadScene(
        string assetName,
        LoadSceneMode loadMode = LoadSceneMode.Single,
        IProgress<float> progress = null)
    {
        if (m_sceneHandles.TryGetValue(assetName, out var existing) && existing.IsValid())
        {
            try
            {
                // 同一场景可能仍在加载中，不能直接读取尚未完成的 Result。
                SceneInstance cachedScene = await AwaitScene(existing, progress);
                if (cachedScene.Scene.isLoaded)
                {
                    return cachedScene;
                }
            }
            catch
            {
                m_sceneHandles.Remove(assetName);
                if (existing.IsValid())
                {
                    Addressables.Release(existing);
                }
                throw;
            }

            Addressables.Release(existing);
            m_sceneHandles.Remove(assetName);
        }

        var handle = Addressables.LoadSceneAsync(m_sceneCatalog.Get(assetName), loadMode);
        m_sceneHandles[assetName] = handle;
        try
        {
            SceneInstance scene = await AwaitScene(handle, progress);
            if (loadMode == LoadSceneMode.Single)
            {
                ReleaseUnloadedSceneHandles(assetName);
            }
            return scene;
        }
        catch
        {
            m_sceneHandles.Remove(assetName);
            if (handle.IsValid())
            {
                Addressables.Release(handle);
            }
            throw;
        }
    }

    public void ReleaseSprite(string assetName)
    {
        Addressables.Release(m_spriteHandles[assetName]);
        m_spriteHandles.Remove(assetName);
    }

    public void ReleasePrefab(string assetName)
    {
        Addressables.Release(m_prefabHandles[assetName]);
        m_prefabHandles.Remove(assetName);
    }

    public void ReleaseUISettings(string assetName)
    {
        Addressables.Release(m_uiSettingsHandles[assetName]);
        m_uiSettingsHandles.Remove(assetName);
    }

    public async UniTask UnloadScene(string assetName)
    {
        if (!m_sceneHandles.TryGetValue(assetName, out var handle) || !handle.IsValid())
        {
            return;
        }

        m_sceneHandles.Remove(assetName);
        await Addressables.UnloadSceneAsync(handle).Task;
    }

    protected override void OnRelease()
    {
        ReleaseAll(m_spriteHandles);
        ReleaseAll(m_prefabHandles);
        ReleaseAll(m_uiSettingsHandles);
        if (m_spriteCatalogHandle.IsValid())
        {
            Addressables.Release(m_spriteCatalogHandle);
        }
        if (m_prefabCatalogHandle.IsValid())
        {
            Addressables.Release(m_prefabCatalogHandle);
        }
        if (m_sceneCatalogHandle.IsValid())
        {
            Addressables.Release(m_sceneCatalogHandle);
        }
        if (m_uiSettingsCatalogHandle.IsValid())
        {
            Addressables.Release(m_uiSettingsCatalogHandle);
        }
    }

    static async UniTask<TAsset> LoadAsset<TAsset, TRef>(
        AddressableCatalog<TRef> catalog,
        Dictionary<string, AsyncOperationHandle<TAsset>> cache,
        string assetName)
        where TAsset : UnityEngine.Object
        where TRef : AssetReference
    {
        if (cache.TryGetValue(assetName, out var existing) && existing.IsValid())
        {
            return existing.Result;
        }

        var handle = Addressables.LoadAssetAsync<TAsset>(catalog.Get(assetName));
        cache[assetName] = handle;
        return await handle.Task;
    }

    static async UniTask<SceneInstance> AwaitScene(
        AsyncOperationHandle<SceneInstance> handle,
        IProgress<float> progress)
    {
        while (!handle.IsDone)
        {
            progress?.Report(handle.PercentComplete);
            await UniTask.Yield();
        }

        SceneInstance scene = await handle.Task;
        progress?.Report(1f);
        return scene;
    }

    void ReleaseUnloadedSceneHandles(string loadedAssetName)
    {
        var staleNames = new List<string>();
        foreach (var pair in m_sceneHandles)
        {
            if (pair.Key == loadedAssetName || !pair.Value.IsValid())
            {
                continue;
            }

            if (!pair.Value.IsDone || pair.Value.Result.Scene.isLoaded)
            {
                continue;
            }

            Addressables.Release(pair.Value);
            staleNames.Add(pair.Key);
        }

        for (int i = 0; i < staleNames.Count; i++)
        {
            m_sceneHandles.Remove(staleNames[i]);
        }
    }

    static void ReleaseAll<T>(Dictionary<string, AsyncOperationHandle<T>> handles)
    {
        foreach (var handle in handles.Values)
        {
            Addressables.Release(handle);
        }
        handles.Clear();
    }
}
