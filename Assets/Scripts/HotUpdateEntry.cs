using System;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

public static class HotUpdateEntry
{
    public const string InitSceneAddress = "Init";

    public static void Boot(Action<float> onProgress)
    {
        ALog.Init();
        BootAsync(onProgress).Forget();
    }

    static async UniTaskVoid BootAsync(Action<float> onProgress)
    {
        await UpdateDetector.DownloadAssets(onProgress);
        var initScene = Addressables.LoadSceneAsync(InitSceneAddress, LoadSceneMode.Single);
        await initScene.Task;
    }
}
