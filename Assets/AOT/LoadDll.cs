using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HybridCLR;
using AChen.Configuration;
using UnityEngine;
using UnityEngine.Networking;

public class LoadDll : MonoBehaviour
{
    public const string DllDir = "HybridCLR";
    public const string HotUpdateFile = "HotUpdate.dll";
    public static string[] AotDllNames => AOTGenericReferences.PatchedAOTAssemblyList.ToArray();

    [SerializeField] string backendUrl = CodeUpdate.DefaultBackendUrl;
    const string channel = CodeUpdate.DefaultChannel;
    Action m_retry;
    bool m_failed;

    static readonly Dictionary<string, byte[]> s_bytes = new Dictionary<string, byte[]>();

    void Awake()
    {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        string overrideUrl = Environment.GetEnvironmentVariable("ACHEN_BACKEND_URL");
        if (!string.IsNullOrWhiteSpace(overrideUrl)) backendUrl = overrideUrl.Trim().TrimEnd('/');
#endif
    }

    IEnumerator Start()
    {
        m_failed = false;
        m_retry = () => StartCoroutine(Start());
        DownLoadSlider bar = FindAnyObjectByType<DownLoadSlider>();
        bar.BindRetry(Retry);
        bar.Set(0f);
        Assembly hotUpdate;
        Action<float> onAssets;

#if UNITY_EDITOR
        hotUpdate = AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == "HotUpdate");
        CodeUpdate.BindEditorLocalSession(backendUrl, channel, "Editor", Application.version);
        onAssets = value => SetProgress(bar, value);
#else
        yield return LoadAotMetadataFiles();
        if (m_LoadError != null)
        {
            Fail(bar, m_LoadError);
            yield break;
        }

        yield return FetchRemoteContent(bar);
        if (!CodeUpdate.IsComplete)
        {
            Fail(bar, CodeUpdate.LastErrorMessage);
            yield break;
        }

        hotUpdate = null;
        Exception loadError = null;
        try
        {
            LoadMetadataForAOTAssemblies();
            hotUpdate = Assembly.Load(s_bytes[HotUpdateFile]);
        }
        catch (Exception exception) { loadError = exception; }
        if (loadError != null)
        {
            Fail(bar, new LocalizedMessage("err.hot_update_load_failed",
                new Dictionary<string, object> { ["message"] = loadError.ToString() }));
            yield break;
        }
        onAssets = value => SetProgress(bar, 0.5f + value * 0.5f);
#endif

        Type entry = hotUpdate.GetType("HotUpdateEntry");
        MethodInfo boot = entry == null
            ? null
            : entry.GetMethod("Boot", new[] { typeof(Action<float>), typeof(StartupContext), typeof(Action<LocalizedMessage>) });
        if (boot == null)
        {
            Fail(bar, new LocalizedMessage("err.hot_update_boot_missing"));
            yield break;
        }

        Action startBusiness = () =>
        {
            m_failed = false;
            try
            {
                boot.Invoke(null, new object[]
                {
                    onAssets, CodeUpdate.Context,
                    new Action<LocalizedMessage>(message => Fail(bar, message))
                });
            }
            catch (Exception exception)
            {
                Fail(bar, new LocalizedMessage("err.hot_update_boot_failed",
                    new Dictionary<string, object> { ["message"] = (exception.InnerException ?? exception).ToString() }));
            }
        };
#if UNITY_EDITOR
        m_retry = startBusiness;
#else
        string loadedDllHash = CodeUpdate.Sha256Of(s_bytes[HotUpdateFile]);
        m_retry = () => StartCoroutine(RetryBusinessContent(bar, loadedDllHash, startBusiness));
#endif
        startBusiness();
    }

    IEnumerator RetryBusinessContent(DownLoadSlider bar, string loadedDllHash, Action startBusiness)
    {
        yield return FetchRemoteContent(bar);
        if (!CodeUpdate.IsComplete)
        {
            Fail(bar, CodeUpdate.LastErrorMessage);
            yield break;
        }
        if (!CodeUpdate.HasExpectedSha256(s_bytes[HotUpdateFile], loadedDllHash))
        {
            Fail(bar, new LocalizedMessage("err.hot_update_restart_required",
                new Dictionary<string, object> { ["message"] = "热更代码已更新，请关闭游戏后重新进入。" }));
            yield break;
        }
        startBusiness();
    }

    IEnumerator FetchRemoteContent(DownLoadSlider bar)
    {
        string platform;
        try
        {
            platform = CodeUpdate.PlatformName();
        }
        catch (Exception exception)
        {
            Fail(bar, new LocalizedMessage("err.unsupported_platform", new Dictionary<string, object> { ["message"] = exception.Message }));
            yield break;
        }

        yield return CodeUpdate.FetchInto(
            s_bytes,
            backendUrl,
            channel,
            platform,
            Application.version,
            value => SetProgress(bar, value * 0.5f));
    }

#if !UNITY_EDITOR
    LocalizedMessage m_LoadError;

    IEnumerator LoadAotMetadataFiles()
    {
        m_LoadError = null;
        foreach (string dll in AotDllNames)
        {
            string file = dll + ".bytes";
            string path = $"{Application.streamingAssetsPath}/{DllDir}/{file}";
            if (!path.Contains("://"))
            {
                path = "file://" + path;
            }

            using (UnityWebRequest request = UnityWebRequest.Get(path))
            {
                yield return request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success)
                {
                    m_LoadError = new LocalizedMessage("err.aot_metadata_failed", new Dictionary<string, object> { ["file"] = file, ["error"] = request.error });
                    yield break;
                }

                s_bytes[file] = request.downloadHandler.data;
            }
        }
    }

    static void LoadMetadataForAOTAssemblies()
    {
        foreach (string dll in AotDllNames)
        {
            byte[] dllBytes = s_bytes[dll + ".bytes"];
            LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(dllBytes, HomologousImageMode.SuperSet);
            Debug.Log($"LoadMetadataForAOTAssembly:{dll} ret:{err}");
        }
    }
#endif

    void Retry()
    {
        if (!m_failed) return;
        m_failed = false;
        FindAnyObjectByType<DownLoadSlider>().Set(0f);
        Debug.Log("[Bootstrap] 重试启动内容更新");
        m_retry?.Invoke();
    }

    static void SetProgress(DownLoadSlider bar, float value)
    {
        if (bar != null)
        {
            bar.Set(value);
        }
    }

    void Fail(DownLoadSlider bar, LocalizedMessage message)
    {
        m_failed = true;
        LocalizedMessage detail = message ?? new LocalizedMessage("err.content_update_failed");
        Debug.LogError("[Bootstrap] 启动内容更新失败. " + detail);
        if (bar != null)
        {
            bar.SetError(detail);
        }
    }
}
