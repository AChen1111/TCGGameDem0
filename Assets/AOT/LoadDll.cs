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
        DownLoadSlider bar = FindFirstObjectByType<DownLoadSlider>();
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
                Fail(bar, CodeUpdate.LastError);
                yield break;
            }

            hotUpdate = Assembly.Load(s_bytes[HotUpdateFile]);
            onAssets = value => SetProgress(bar, 0.5f + value * 0.5f);
            addressablesBaseUrl = CodeUpdate.AddressablesBaseUrl;
        }
#else
        yield return LoadAotMetadataFiles();
        if (!string.IsNullOrEmpty(m_LoadError))
        {
            Fail(bar, m_LoadError);
            yield break;
        }

        yield return FetchRemoteContent(bar);
        if (!CodeUpdate.IsComplete)
        {
            Fail(bar, CodeUpdate.LastError);
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
            : entry.GetMethod("Boot", new[] { typeof(Action<float>), typeof(string), typeof(Action<string>) });
        if (boot == null)
        {
            Fail(bar, "HotUpdateEntry.Boot 启动接口不存在。");
            yield break;
        }

        boot.Invoke(null, new object[]
        {
            onAssets,
            addressablesBaseUrl,
            new Action<string>(message => Fail(bar, message))
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
            Fail(bar, exception.Message);
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
    string m_LoadError;

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
                    m_LoadError = "读取 AOT 元数据失败：" + file + "，" + request.error;
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

    static void Fail(DownLoadSlider bar, string message)
    {
        string detail = string.IsNullOrWhiteSpace(message) ? "内容更新失败。" : message;
        Debug.LogError("[ContentDelivery] " + detail);
        if (bar != null)
        {
            bar.SetError(detail);
        }
    }
}
