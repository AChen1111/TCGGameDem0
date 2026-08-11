using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 等待列表中所有单例初始化完成后,按场景名切换场景
/// </summary>
public class SingletonManager : PersistentMonoSingleton<SingletonManager>
{
    [SerializeField] List<MonoSingleton> m_singletons;
    [SerializeField] string m_sceneName;

    private bool m_bLoaded;

    private void Update()
    {
        if (m_bLoaded)
        {
            return;
        }
        for (int i = 0; i < m_singletons.Count; i++)
        {
            if (!m_singletons[i].IsDone)
            {
                return;
            }
        }
        m_bLoaded = true;
        SceneManager.LoadScene(m_sceneName);
    }
}
