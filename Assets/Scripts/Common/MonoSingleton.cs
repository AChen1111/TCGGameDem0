using UnityEngine;

/// <summary>
/// MonoBehaviour单例:换场景时随场景销毁
/// </summary>
public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
{
    private static T s_instance;
    public static T Instance
    {
        get
        {
            if (s_instance == null)
            {
                GameObject go = new GameObject("[" + typeof(T).Name + "]");
                s_instance = go.AddComponent<T>();
            }
            return s_instance;
        }
    }

    //是否已完成初始化
    public bool IsDone { get; private set; }

    protected virtual bool Persistent => false;

    private void Awake()
    {
        if (s_instance != null && s_instance != this)
        {
            Destroy(gameObject);
            return;
        }
        s_instance = (T)this;
        if (Persistent)
        {
            DontDestroyOnLoad(gameObject);
        }
        OnInit();
        IsDone = true;
    }

    private void OnDestroy()
    {
        if (s_instance == this)
        {
            s_instance = null;
        }
        OnRelease();
    }

    //子类在此写初始化逻辑,不要再定义Awake
    protected virtual void OnInit() { }

    protected virtual void OnRelease() { }
}

/// <summary>
/// MonoBehaviour单例:换场景不销毁
/// </summary>
public abstract class PersistentMonoSingleton<T> : MonoSingleton<T> where T : PersistentMonoSingleton<T>
{
    protected override bool Persistent => true;
}
