using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public static class CodeUpdate
{
    public const string CdnHost = "http://127.0.0.1:8000";
    public const string CdnCodeFolder = "HybridCLR";

    public static string DllUrl => CdnHost + "/" + CdnCodeFolder + "/" + LoadDll.HotUpdateFile;
    public static string HashUrl => DllUrl + ".hash";
    public static string CachePath => Path.Combine(Application.persistentDataPath, LoadDll.DllDir, LoadDll.HotUpdateFile);

    public static string HashOf(byte[] data)
    {
        byte[] hash = SHA1.Create().ComputeHash(data);
        var sb = new StringBuilder(hash.Length * 2);
        for (int i = 0; i < hash.Length; i++)
        {
            sb.Append(hash[i].ToString("x2"));
        }

        return sb.ToString();
    }

    public static bool IsCurrent(string localHash, string remoteHash)
    {
        return localHash == remoteHash;
    }

    public static bool IsComplete { get; private set; }

    public static IEnumerator FetchInto(Dictionary<string, byte[]> bytes, System.Action<float> onProgress = null)
    {
        IsComplete = false;
        string remoteHash = null;
        yield return GetText(HashUrl, value => remoteHash = value);
        string cacheHashPath = CachePath + ".hash";
        if (File.Exists(CachePath) && File.Exists(cacheHashPath) && IsCurrent(File.ReadAllText(cacheHashPath).Trim(), remoteHash))
        {
            onProgress?.Invoke(1f);
            bytes[LoadDll.HotUpdateFile] = File.ReadAllBytes(CachePath);
            IsComplete = true;
            yield break;
        }

        byte[] data = null;
        yield return GetBytes(DllUrl, value => data = value, onProgress);
        Directory.CreateDirectory(Path.GetDirectoryName(CachePath));
        File.WriteAllBytes(CachePath, data);
        File.WriteAllText(cacheHashPath, remoteHash);
        bytes[LoadDll.HotUpdateFile] = data;
        IsComplete = true;
    }

    static IEnumerator GetText(string url, System.Action<string> onDone)
    {
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                throw new System.InvalidOperationException(www.error);
            }

            onDone(www.downloadHandler.text.Trim());
        }
    }

    static IEnumerator GetBytes(string url, System.Action<byte[]> onDone, System.Action<float> onProgress)
    {
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            UnityWebRequestAsyncOperation op = www.SendWebRequest();
            while (!op.isDone)
            {
                onProgress?.Invoke(www.downloadProgress);
                yield return null;
            }

            if (www.result != UnityWebRequest.Result.Success)
            {
                throw new System.InvalidOperationException(www.error);
            }

            onProgress?.Invoke(1f);
            onDone(www.downloadHandler.data);
        }
    }
}
