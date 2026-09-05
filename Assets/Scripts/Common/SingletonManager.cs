using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 按 list 顺序初始化单例,进入登录流程前恢复会话并选择目标场景。
/// </summary>
public class SingletonManager : PersistentMonoSingleton<SingletonManager>
{
    [SerializeField] List<MonoSingleton> m_singletons;
    [SerializeField] string m_sceneName;

    async UniTaskVoid Start()
    {
        for (int i = 0; i < m_singletons.Count; i++)
        {
            MonoSingleton singleton = m_singletons[i];
            singleton.BeginInit();
            await UniTask.WaitUntil(() => singleton.IsDone);
        }
        if (!string.IsNullOrEmpty(m_sceneName))
        {
            string targetScene = m_sceneName;
            try
            {
                if (targetScene == AddressKeys.Scene.LogIn)
                {
                    targetScene = await GameFlow.GetStartupSceneAsync(this.GetCancellationTokenOnDestroy());
                }

                SceneTransitionOverlay.Show();
                await SceneLoader.LoadScene(targetScene);
                ALog.Log($"Init 场景切换完成. Target={targetScene}", ALogCategories.UI);
            }
            catch (OperationCanceledException)
            {
                SceneTransitionOverlay.Hide();
            }
            catch (Exception exception)
            {
                SceneTransitionOverlay.Hide();
                ALog.LogError($"Init 场景切换失败. Target={targetScene}; Error={exception.Message}", ALogCategories.UI);
                throw;
            }
        }
    }
}
