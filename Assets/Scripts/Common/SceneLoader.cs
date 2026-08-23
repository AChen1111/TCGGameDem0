using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.ResourceManagement.ResourceProviders;

/// <summary>
/// 业务场景加载入口。串行化场景操作，并统一提供状态、进度和生命周期事件。
/// 场景资源的实际加载与释放仍由 AddressableLoader 负责。
/// </summary>
public static class SceneLoader
{
    public static bool IsLoading { get; private set; }
    public static float Progress { get; private set; }
    public static string CurrentSceneName { get; private set; }

    public static event Action<string> LoadStarted;
    public static event Action<string, float> LoadProgressChanged;
    public static event Action<string, SceneInstance> LoadCompleted;
    public static event Action<string, Exception> LoadFailed;

    /// <summary>
    /// 使用 AddressableLoader 加载场景。并发请求会等待前一个场景操作完成。
    /// </summary>
    public static async UniTask<SceneInstance> LoadScene(
        string sceneName,
        LoadSceneMode loadMode = LoadSceneMode.Single,
        IProgress<float> progress = null)
    {
        ValidateSceneName(sceneName);
        await UniTask.WaitUntil(() => !IsLoading);

        IsLoading = true;
        Progress = 0f;

        try
        {
            InvokeSafely(LoadStarted, sceneName);
            var sceneProgress = new SceneLoadProgress(sceneName, progress);
            SceneInstance scene = await AddressableLoader.Instance.LoadScene(sceneName, loadMode, sceneProgress);
            if (loadMode == LoadSceneMode.Single)
            {
                CurrentSceneName = sceneName;
            }

            InvokeSafely(LoadCompleted, sceneName, scene);
            return scene;
        }
        catch (Exception exception)
        {
            InvokeSafely(LoadFailed, sceneName, exception);
            throw;
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// 卸载通过 AddressableLoader 加载的场景，主要用于 Additive 场景。
    /// </summary>
    public static async UniTask UnloadScene(string sceneName)
    {
        ValidateSceneName(sceneName);
        await UniTask.WaitUntil(() => !IsLoading);

        IsLoading = true;
        try
        {
            await AddressableLoader.Instance.UnloadScene(sceneName);
            if (CurrentSceneName == sceneName)
            {
                CurrentSceneName = null;
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    static void ValidateSceneName(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            throw new ArgumentException("场景资源名不能为空。", nameof(sceneName));
        }
    }

    static void InvokeSafely<T>(Action<T> action, T argument)
    {
        if (action == null)
        {
            return;
        }

        foreach (Action<T> callback in action.GetInvocationList())
        {
            try
            {
                callback(argument);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }
    }

    static void InvokeSafely<T1, T2>(Action<T1, T2> action, T1 argument1, T2 argument2)
    {
        if (action == null)
        {
            return;
        }

        foreach (Action<T1, T2> callback in action.GetInvocationList())
        {
            try
            {
                callback(argument1, argument2);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }
    }

    sealed class SceneLoadProgress : IProgress<float>
    {
        readonly string m_sceneName;
        readonly IProgress<float> m_externalProgress;

        public SceneLoadProgress(string sceneName, IProgress<float> externalProgress)
        {
            m_sceneName = sceneName;
            m_externalProgress = externalProgress;
        }

        public void Report(float value)
        {
            Progress = value;
            try
            {
                m_externalProgress?.Report(value);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }

            InvokeSafely(LoadProgressChanged, m_sceneName, value);
        }
    }
}
