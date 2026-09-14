using System;
using AChen.Events;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using UnityEngine.ResourceManagement.ResourceProviders;

/// <summary>串行加载业务场景, 通过事件中心通知开始与失败.</summary>
public static class SceneLoader
{
    static bool s_isLoading;

    public static async UniTask<SceneInstance> LoadScene(
        string sceneName,
        LoadSceneMode loadMode = LoadSceneMode.Single)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            throw new ArgumentException("场景资源名不能为空。", nameof(sceneName));
        }

        await UniTask.WaitUntil(() => !s_isLoading);
        s_isLoading = true;
        try
        {
            EventCenter.Dispatch(GameEvent.SceneLoadStarted, sceneName, loadMode);
            return await AddressableLoader.Instance.LoadScene(sceneName, loadMode);
        }
        catch (Exception exception)
        {
            EventCenter.Dispatch(GameEvent.SceneLoadFailed, sceneName, exception);
            throw;
        }
        finally
        {
            s_isLoading = false;
        }
    }

    public static async UniTask<SceneInstance> ReloadScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            throw new ArgumentException("场景资源名不能为空。", nameof(sceneName));
        }

        await UniTask.WaitUntil(() => !s_isLoading);
        s_isLoading = true;
        try
        {
            EventCenter.Dispatch(GameEvent.SceneLoadStarted, sceneName, LoadSceneMode.Single);
            SceneInstance scene = await AddressableLoader.Instance.ReloadScene(sceneName);
            ALog.Log($"业务场景重装完成. Scene={sceneName}", ALogCategories.UI);
            return scene;
        }
        catch (Exception exception)
        {
            EventCenter.Dispatch(GameEvent.SceneLoadFailed, sceneName, exception);
            throw;
        }
        finally
        {
            s_isLoading = false;
        }
    }
}
