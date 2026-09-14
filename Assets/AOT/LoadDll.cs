using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HybridCLR;
using UnityEngine;
using UnityEngine.Networking;

public class LoadDll : MonoBehaviour
{
    public const string DllDir = "HybridCLR";
    public const string HotUpdateFile = "HotUpdate.dll.bytes";
    public static string[] AotDllNames => AOTGenericReferences.PatchedAOTAssemblyList.ToArray();

    [SerializeField] string backendUrl = CodeUpdate.DefaultBackendUrl;
    [SerializeField] string channel = CodeUpdate.DefaultChannel;
    [SerializeField] bool useRemoteContentInEditor;

    static readonly Dictionary<string, byte[]> s_bytes = new Dictionary<string, byte[]>();

    IEnumerator Start()
    {
        DownLoadSlider bar = FindAnyObjectByType<DownLoadSlider>();
        Assembly hotUpdate;
        Action<float> onAssets;
        string addressablesBaseUrl;

#if UNITY_EDITOR
        if (!useRemoteContentInEditor)
        {
            hotUpdate = AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == "HotUpdate");
            onAssets = value => SetProgress(bar, value);
            addressablesBaseUrl = string.Empty;
        }
        else
        {
            yield return FetchRemoteContent(bar);
            if (!CodeUpdate.IsComplete)
            {
                Fail(bar, CodeUpdate.LastErrorMessage);
                yield break;
            }

            hotUpdate = Assembly.Load(s_bytes[HotUpdateFile]);
            onAssets = value => SetProgress(bar, 0.5f + value * 0.5f);
            addressablesBaseUrl = CodeUpdate.AddressablesBaseUrl;
        }
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

        LoadMetadataForAOTAssemblies();
        hotUpdate = Assembly.Load(s_bytes[HotUpdateFile]);
        onAssets = value => SetProgress(bar, 0.5f + value * 0.5f);
        addressablesBaseUrl = CodeUpdate.AddressablesBaseUrl;
#endif

        Type entry = hotUpdate.GetType("HotUpdateEntry");
        MethodInfo boot = entry == null
            ? null
            : entry.GetMethod("Boot", new[] { typeof(Action<float>), typeof(string), typeof(Action<LocalizedMessage>) });
        if (boot == null)
        {
            Fail(bar, new LocalizedMessage("err.hot_update_boot_missing"));
            yield break;
        }

        boot.Invoke(null, new object[]
        {
            onAssets,
            addressablesBaseUrl,
            new Action<LocalizedMessage>(message => Fail(bar, message))
        });
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

    static void SetProgress(DownLoadSlider bar, float value)
    {
        if (bar != null)
        {
            bar.Set(value);
        }
    }

    static void Fail(DownLoadSlider bar, LocalizedMessage message)
    {
        LocalizedMessage detail = message ?? new LocalizedMessage("err.content_update_failed");
        ALog.LogError("启动内容更新失败. Key=" + detail.Key + "; Detail=" + detail, ALogCategories.Localization);
        if (bar != null)
        {
            bar.SetError(detail);
        }
    }
}
