using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;

namespace AChen.Networking
{
    /// <summary>一次后端响应的快照; UnityWebRequest 已释放, 只保留状态码、正文与响应头.</summary>
    public readonly struct BackendHttpResponse
    {
        public long StatusCode { get; }
        public string Body { get; }
        readonly Dictionary<string, string> m_headers;

        internal BackendHttpResponse(long statusCode, string body, Dictionary<string, string> headers)
        {
            StatusCode = statusCode;
            Body = body;
            m_headers = headers;
        }

        public string GetHeader(string name)
        {
            if (m_headers == null)
            {
                return null;
            }

            foreach (KeyValuePair<string, string> pair in m_headers)
            {
                if (string.Equals(pair.Key, name, StringComparison.OrdinalIgnoreCase))
                {
                    return pair.Value;
                }
            }

            return null;
        }
    }

    /// <summary>
    /// 后端 HTTP 传输层: 拼 URL、JSON 请求体、Bearer 头、超时与错误转换. 无状态, 可被多个 API 客户端共用.
    /// </summary>
    public sealed class BackendHttpClient
    {
        readonly BackendConfig m_config;

        public BackendConfig Config => m_config;

        public BackendHttpClient(BackendConfig config = null)
        {
            m_config = config ?? new BackendConfig();
        }

        /// <summary>发送请求并返回响应正文; 非成功状态转为 BackendApiException.</summary>
        public async UniTask<string> SendAsync(
            string method,
            string path,
            object body = null,
            string accessToken = null,
            CancellationToken cancellationToken = default)
        {
            BackendHttpResponse response = await SendRawAsync(method, path, body, accessToken, null, null, cancellationToken);
            return response.Body;
        }

        public async UniTask<T> SendAsync<T>(
            string method,
            string path,
            object body = null,
            string accessToken = null,
            CancellationToken cancellationToken = default)
        {
            string json = await SendAsync(method, path, body, accessToken, cancellationToken);
            return BackendJson.DeserializeResponse<T>(json);
        }

        /// <summary>
        /// 发送请求并返回完整响应. acceptStatus 返回 true 的非成功状态码(如 304)不抛异常, 交由调用方处理.
        /// </summary>
        public async UniTask<BackendHttpResponse> SendRawAsync(
            string method,
            string path,
            object body,
            string accessToken,
            IReadOnlyDictionary<string, string> headers,
            Func<long, bool> acceptStatus,
            CancellationToken cancellationToken)
        {
            using (var request = new UnityWebRequest(m_config.BaseUrl + path, method))
            {
                request.downloadHandler = new DownloadHandlerBuffer();
                request.timeout = m_config.TimeoutSeconds;
                request.SetRequestHeader("Accept", "application/json");

                if (body != null)
                {
                    byte[] payload = Encoding.UTF8.GetBytes(BackendJson.Serialize(body));
                    request.uploadHandler = new UploadHandlerRaw(payload);
                    request.SetRequestHeader("Content-Type", "application/json");
                }

                if (!string.IsNullOrEmpty(accessToken))
                {
                    request.SetRequestHeader("Authorization", "Bearer " + accessToken);
                }

                if (headers != null)
                {
                    foreach (KeyValuePair<string, string> header in headers)
                    {
                        request.SetRequestHeader(header.Key, header.Value);
                    }
                }

                try
                {
                    await request.SendWebRequest().ToUniTask(
                        cancellationToken: cancellationToken,
                        cancelImmediately: true);
                }
                catch (UnityWebRequestException) when (acceptStatus != null && acceptStatus(request.responseCode))
                {
                    // 调用方声明可接受的协议状态(如 304), 按正常响应返回.
                }
                catch (UnityWebRequestException)
                {
                    throw BackendHttpError.FromRequest(request);
                }

                return new BackendHttpResponse(
                    request.responseCode,
                    request.downloadHandler.text,
                    request.GetResponseHeaders());
            }
        }
    }
}
