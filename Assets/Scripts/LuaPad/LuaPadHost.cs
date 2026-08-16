using UnityEngine;
using UnityEngine.InputSystem;

public class LuaPadHost : MonoBehaviour
{
    bool m_visible;
    LuaPadSession m_session;

    public static LuaPadHost Instance { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        var go = new GameObject("[LuaPad]");
        DontDestroyOnLoad(go);
        go.AddComponent<LuaPadHost>();
#endif
    }

    void Awake()
    {
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
        m_session?.Dispose();
        m_session = null;
    }

    void Update()
    {
        LuaPadMainThread.Pump();
        Keyboard kb = Keyboard.current;
        if (kb != null && kb.f10Key.wasPressedThisFrame)
        {
            SetVisible(!m_visible);
        }
    }

    public void SetVisible(bool visible)
    {
        m_visible = visible;
        if (visible)
        {
            m_session ??= LuaPadSession.Start();
        }
        m_session?.SetVisible(visible);
    }
}
