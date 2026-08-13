using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 按 list 顺序逐个初始化单例,全部完成后切换场景
/// </summary>
public class SingletonManager : PersistentMonoSingleton<SingletonManager>
{
    [SerializeField] List<MonoSingleton> m_singletons;
    [SerializeField] string m_sceneName;

    private IEnumerator Start()
    {
        for (int i = 0; i < m_singletons.Count; i++)
        {
            MonoSingleton singleton = m_singletons[i];
            singleton.BeginInit();
            yield return new WaitUntil(() => singleton.IsDone);
        }
        SceneManager.LoadScene(m_sceneName);
    }
}
