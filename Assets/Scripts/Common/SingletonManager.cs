using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 按 list 顺序逐个初始化单例,全部完成后切换场景
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
            await AddressableLoader.Instance.LoadScene(m_sceneName);
        }
    }
}
