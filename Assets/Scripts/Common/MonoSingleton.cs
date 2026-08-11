using UnityEngine;

/// <summary>
/// 非泛型基类,便于统一收进列表并查询 IsDone
/// </summary>
public abstract class MonoSingleton : MonoBehaviour
{
    public bool IsDone { get; protected set; }
}

/// <summary>
/// MonoBehaviour单例:换场景时随场景销毁
/// </summary>
public abstract class MonoSingleton<T> : MonoSingleton where T : MonoSingleton<T>
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
