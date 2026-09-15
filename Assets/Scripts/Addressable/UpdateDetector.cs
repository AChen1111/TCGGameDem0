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

    public static async UniTask InitializeLocalAsync(Action<float> onProgress = null)
    {
        IsComplete = false;
        AsyncOperationHandle init = Addressables.InitializeAsync(false);
        try
        {
            await init.Task;
            RequireSuccess(init, "初始化 Addressables");
        }
        finally { if (init.IsValid()) Addressables.Release(init); }
        onProgress?.Invoke(1f);
        IsComplete = true;
        ALog.Log("使用本地 Addressables, 跳过远程目录.", ALogCategories.Net);
    }

    public static async UniTask DownloadAssets(string addressablesBaseUrl, Action<float> onProgress = null)
    {
        IsComplete = false;
        ConfigureContentBaseUrl(addressablesBaseUrl);

        AsyncOperationHandle init = Addressables.InitializeAsync(false);
        try
        {
            await init.Task;
            RequireSuccess(init, "初始化 Addressables");
        }
        finally { if (init.IsValid()) Addressables.Release(init); }
        string catalogUrl = AChen.Configuration.ContentSession.CatalogUrl;
        if (string.IsNullOrWhiteSpace(catalogUrl)) throw new InvalidOperationException("缺少发布目录地址");
        Addressables.ClearResourceLocators();
        var catalog = Addressables.LoadContentCatalogAsync(catalogUrl, false);
        try
        {
            await catalog.Task;
            RequireSuccess(catalog, "加载发布资源目录");
        }
        finally { if (catalog.IsValid()) Addressables.Release(catalog); }

        var keys = new List<object>();
        foreach (IResourceLocator locator in Addressables.ResourceLocators)
        {
            keys.AddRange(locator.Keys);
        }

        AsyncOperationHandle<long> sizeHandle = Addressables.GetDownloadSizeAsync(keys);
        long size;
        try
        {
            size = await sizeHandle.Task;
            RequireSuccess(sizeHandle, "计算资源下载量");
        }
        finally { if (sizeHandle.IsValid()) Addressables.Release(sizeHandle); }
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
            RequireSuccess(handle, "下载发布资源");
            onProgress?.Invoke(1f);
            IsComplete = true;
        }
        finally
        {
            Addressables.Release(handle);
        }
    }

    static void RequireSuccess(AsyncOperationHandle handle, string operation)
    {
        if (handle.Status != AsyncOperationStatus.Succeeded)
            throw new InvalidOperationException(operation + "失败", handle.OperationException);
    }

    public static void ConfigureContentBaseUrl(string addressablesBaseUrl)
    {
        if (!string.IsNullOrWhiteSpace(addressablesBaseUrl))
        {
            AddressablesRuntimeProperties.SetPropertyValue("AChen.ContentBaseUrl", addressablesBaseUrl.TrimEnd('/'));
        }
    }
}
