using System;
using AChen.Configuration;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

public static class HotUpdateEntry
{
    public const string InitSceneAddress = "Init";
    public const string StartupRevision = "migration-b";

    public static void Boot(Action<float> onProgress, StartupContext context, Action<LocalizedMessage> onError)
    {
        BootAsync(onProgress, context, onError).Forget();
    }

    static async UniTaskVoid BootAsync(
        Action<float> onProgress,
        StartupContext context,
        Action<LocalizedMessage> onError)
    {
        try
        {
            ALogSettings.ResetState();
            AChen.Events.EventCenter.ResetState();
            GameFlow.ResetState();
            SceneTransitionOverlay.ResetState();
            Spine.Unity.AttachmentTools.AtlasUtilities.ClearCache();
            AChen.Networking.LocalGameConfiguration.ResetState();
            LocalizationService.ResetState();
            ContentSession.Bind(context);
            string addressablesBaseUrl = context.AddressablesBaseUrl;
            if (string.IsNullOrEmpty(addressablesBaseUrl))
            {
                await UpdateDetector.InitializeLocalAsync(value => onProgress?.Invoke(value * 0.8f));
            }
            else
            {
                await UpdateDetector.DownloadAssets(addressablesBaseUrl, value => onProgress?.Invoke(value * 0.8f));
            }
            await AChen.Networking.LocalGameConfiguration.InitializeAsync(value => onProgress?.Invoke(0.8f + value * 0.19f));
            await SceneTransitionOverlay.PreloadAsync();
            ALog.Log("业务配置已就绪. HotUpdate=" + StartupRevision + "; LogPolicy=" + ALog.PolicyRevision + "; Content=" + context.ReleaseId, ALogCategories.Net);
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
