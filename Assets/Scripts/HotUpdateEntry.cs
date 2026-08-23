using System;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

public static class HotUpdateEntry
{
    public const string InitSceneAddress = "Init";

    public static void Boot(Action<float> onProgress, string addressablesBaseUrl, Action<string> onError)
    {
        ALog.Init();
        BootAsync(onProgress, addressablesBaseUrl, onError).Forget();
    }

    static async UniTaskVoid BootAsync(
        Action<float> onProgress,
        string addressablesBaseUrl,
        Action<string> onError)
    {
        try
        {
            await UpdateDetector.DownloadAssets(addressablesBaseUrl, onProgress);
            var initScene = Addressables.LoadSceneAsync(InitSceneAddress, LoadSceneMode.Single);
            await initScene.Task;
        }
        catch (Exception exception)
        {
            UnityEngine.Debug.LogException(exception);
            onError?.Invoke("Addressables 更新失败：" + exception.Message);
        }
    }
}
