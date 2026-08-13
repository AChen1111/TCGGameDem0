using UnityEngine;

/// <summary>
/// 非泛型基类,便于统一收进列表并按顺序初始化
/// </summary>
public abstract class MonoSingleton : MonoBehaviour
{
    public bool IsDone { get; protected set; }

    //由 SingletonManager 按 list 顺序调用,内部启动初始化;完成后置 IsDone=true
    public abstract void BeginInit();
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
    }

    public override void BeginInit()
    {
        OnInit();
    }

    private void OnDestroy()
    {
        if (s_instance == this)
        {
            s_instance = null;
        }
        OnRelease();
    }

    //子类在此写初始化逻辑;完成时置 IsDone=true(异步则在完成回调里置)
    protected virtual void OnInit() { IsDone = true; }

    protected virtual void OnRelease() { }
}

/// <summary>
/// MonoBehaviour单例:换场景不销毁
/// </summary>
public abstract class PersistentMonoSingleton<T> : MonoSingleton<T> where T : PersistentMonoSingleton<T>
{
    protected override bool Persistent => true;
}
