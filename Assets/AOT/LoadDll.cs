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
    public static readonly string[] AotDllNames =
    {
        "mscorlib.dll",
        "System.dll",
        "System.Core.dll",
    };

    static readonly Dictionary<string, byte[]> s_bytes = new Dictionary<string, byte[]>();

    IEnumerator Start()
    {
        Assembly hotUpdate;
#if UNITY_EDITOR
        hotUpdate = System.AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == "HotUpdate");
#else
        yield return LoadFiles();
        LoadMetadataForAOTAssemblies();
        hotUpdate = Assembly.Load(s_bytes[HotUpdateFile]);
#endif
        hotUpdate.GetType("HotUpdateEntry").GetMethod("Boot").Invoke(null, null);
        yield break;
    }

#if !UNITY_EDITOR
    IEnumerator LoadFiles()
    {
        var files = new List<string> { HotUpdateFile };
        foreach (string dll in AotDllNames)
        {
            files.Add(dll + ".bytes");
        }

        foreach (string file in files)
        {
            string path = $"{Application.streamingAssetsPath}/{DllDir}/{file}";
            if (!path.Contains("://"))
            {
                path = "file://" + path;
            }

            UnityWebRequest www = UnityWebRequest.Get(path);
            yield return www.SendWebRequest();
            s_bytes[file] = www.downloadHandler.data;
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
}
