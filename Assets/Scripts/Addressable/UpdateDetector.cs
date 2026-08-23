using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.Initialization;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class UpdateDetector
{
    public static bool IsComplete { get; private set; }

    public static async UniTask DownloadAssets(string addressablesBaseUrl, Action<float> onProgress = null)
    {
        IsComplete = false;
        ConfigureContentBaseUrl(addressablesBaseUrl);

        AsyncOperationHandle init = Addressables.InitializeAsync(false);
        await init.Task;
        Addressables.Release(init);
        AsyncOperationHandle<List<string>> check = Addressables.CheckForCatalogUpdates(false);
        List<string> catalogs = await check.Task;
        var ids = new List<string>(catalogs);
        Addressables.Release(check);
        if (ids.Count > 0)
        {
            AsyncOperationHandle update = Addressables.UpdateCatalogs(true, ids, false);
            await update.Task;
            Addressables.Release(update);
        }

        var keys = new List<object>();
        foreach (IResourceLocator locator in Addressables.ResourceLocators)
        {
            keys.AddRange(locator.Keys);
        }

        AsyncOperationHandle<long> sizeHandle = Addressables.GetDownloadSizeAsync(keys);
        long size = await sizeHandle.Task;
        Addressables.Release(sizeHandle);
        if (size <= 0)
        {
            onProgress?.Invoke(1f);
            IsComplete = true;
            return;
        }

        AsyncOperationHandle handle = Addressables.DownloadDependenciesAsync(keys, Addressables.MergeMode.Union, false);
        try
        {
            while (!handle.IsDone)
            {
                onProgress?.Invoke(handle.GetDownloadStatus().Percent);
                await UniTask.Yield();
            }

            await handle.Task;
            onProgress?.Invoke(1f);
            IsComplete = true;
        }
        finally
        {
            Addressables.Release(handle);
        }
    }

    public static void ConfigureContentBaseUrl(string addressablesBaseUrl)
    {
        if (!string.IsNullOrWhiteSpace(addressablesBaseUrl))
        {
            AddressablesRuntimeProperties.SetPropertyValue("AChen.ContentBaseUrl", addressablesBaseUrl.TrimEnd('/'));
        }
    }
}
