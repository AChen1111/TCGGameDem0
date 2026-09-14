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
            await UpdateDetector.DownloadAssets(addressablesBaseUrl, onProgress);
            var initScene = Addressables.LoadSceneAsync(InitSceneAddress, LoadSceneMode.Single);
            await initScene.Task;
        }
        catch (Exception exception)
        {
            UnityEngine.Debug.LogException(exception);
            onError?.Invoke(new LocalizedMessage("err.addressables_update_failed",
                new System.Collections.Generic.Dictionary<string, object> { ["message"] = exception.Message }));
        }
    }
}
