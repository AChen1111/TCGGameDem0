using System;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

public static class HotUpdateEntry
{
    public const string InitSceneAddress = "Init";

    public static void Boot(Action<float> onProgress, string addressablesBaseUrl, Action<LocalizedMessage> onError)
    {
        BootAsync(onProgress, addressablesBaseUrl, onError).Forget();
    }

    static async UniTaskVoid BootAsync(
        Action<float> onProgress,
        string addressablesBaseUrl,
        Action<LocalizedMessage> onError)
    {
        try
        {
            if (string.IsNullOrEmpty(addressablesBaseUrl))
            {
                await UpdateDetector.InitializeLocalAsync(value => onProgress?.Invoke(value * 0.8f));
            }
            else
            {
                await UpdateDetector.DownloadAssets(addressablesBaseUrl, value => onProgress?.Invoke(value * 0.8f));
            }
            await AChen.Networking.LocalGameConfiguration.InitializeAsync(value => onProgress?.Invoke(0.8f + value * 0.19f));
            var initScene = Addressables.LoadSceneAsync(InitSceneAddress, LoadSceneMode.Single);
            await initScene.Task;
            onProgress?.Invoke(1f);
            if (initScene.Status != UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
            {
                var error = initScene.OperationException;
                Addressables.Release(initScene);
                throw new InvalidOperationException("初始化场景加载失败", error);
            }
        }
        catch (Exception exception)
        {
            ALog.LogError("启动内容加载失败. Error=" + exception.Message, ALogCategories.Net);
            onError?.Invoke(new LocalizedMessage("err.addressables_update_failed",
                new System.Collections.Generic.Dictionary<string, object> { ["message"] = exception.Message }));
        }
    }
}
