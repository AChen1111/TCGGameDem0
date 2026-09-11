using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>Editor 工具访问后端管理接口的公共 HTTP 封装: 发布密钥头、轮询等待、ProblemDetails 错误转换.</summary>
public static class EditorBackendHttp
{
    public const string PublishKeyHeader = "X-Content-Publish-Key";

    public static void ValidateBaseUrl(string baseUrl)
    {
        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out Uri uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new InvalidOperationException("后端地址必须是绝对 HTTP/HTTPS 地址");
        }
    }

    public static string BuildUrl(string baseUrl, string path)
    {
        return baseUrl.TrimEnd('/') + path;
    }

    /// <summary>发送 JSON 请求; json 为 null 时不带请求体.</summary>
    public static async Task<string> SendJsonAsync(
        string baseUrl,
        string method,
        string path,
        string json,
        string publishKey)
    {
        using (var request = new UnityWebRequest(BuildUrl(baseUrl, path), method))
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            if (json != null)
            {
                request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
                request.SetRequestHeader("Content-Type", "application/json");
            }

            request.SetRequestHeader(PublishKeyHeader, publishKey);
            await AwaitAsync(request);
            ThrowIfFailed(request);
            return request.downloadHandler.text;
        }
    }

    /// <summary>GET 请求; allowNotFound 时 404 返回 null 而不抛出.</summary>
    public static async Task<string> GetAsync(
        string baseUrl,
        string path,
        string publishKey,
        bool allowNotFound = false)
    {
        using (var request = UnityWebRequest.Get(BuildUrl(baseUrl, path)))
        {
            request.SetRequestHeader(PublishKeyHeader, publishKey);
            await AwaitAsync(request);
            if (allowNotFound && request.responseCode == 404)
            {
                return null;
            }

            ThrowIfFailed(request);
            return request.downloadHandler.text;
        }
    }

    /// <summary>等待请求完成; Editor 下没有 PlayerLoop 驱动的 await, 采用短间隔轮询.</summary>
    public static async Task AwaitAsync(UnityWebRequest request, Action<float> onUploadProgress = null)
    {
        UnityWebRequestAsyncOperation operation = request.SendWebRequest();
        while (!operation.isDone)
        {
            onUploadProgress?.Invoke(Mathf.Max(0f, request.uploadProgress));
            await Task.Delay(50);
        }
    }

    public static void ThrowIfFailed(UnityWebRequest request)
    {
        if (request.result == UnityWebRequest.Result.Success)
        {
            return;
        }

        ProblemDetails problem = null;
        try
        {
            problem = JsonUtility.FromJson<ProblemDetails>(request.downloadHandler?.text);
        }
        catch
        {
            // 非 JSON 响应时退回传输层错误文案.
        }

        string message = request.error;
        if (problem != null)
        {
            if (!string.IsNullOrWhiteSpace(problem.detail)) message = problem.detail;
            else if (!string.IsNullOrWhiteSpace(problem.title)) message = problem.title;
        }

        string code = problem != null && !string.IsNullOrWhiteSpace(problem.code)
            ? " [" + problem.code + "]"
            : string.Empty;
        throw new InvalidOperationException(message + code + " (HTTP " + request.responseCode + ")");
    }

    [Serializable]
    sealed class ProblemDetails
    {
        public string title;
        public string detail;
        public string code;
    }
}
