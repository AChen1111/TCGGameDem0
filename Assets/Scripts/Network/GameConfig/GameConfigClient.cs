using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine.Networking;

namespace AChen.Networking
{
    public sealed class GameConfigFetchResult
    {
        public bool NotModified { get; }
        public GameConfigSnapshot Snapshot { get; }
        public string ETag { get; }
        public DateTimeOffset ServerTime { get; }

        internal GameConfigFetchResult(
            bool notModified,
            GameConfigSnapshot snapshot,
            string etag,
            DateTimeOffset serverTime)
        {
            NotModified = notModified;
            Snapshot = snapshot;
            ETag = etag;
            ServerTime = serverTime;
        }
    }

    /// <summary>拉取已发布游戏配置, 支持 ETag 协商缓存与瞬时错误重试.</summary>
    public sealed class GameConfigClient
    {
        const int MaxRetries = 2;
        const string BootstrapPath = "/api/game-config/bootstrap";

        readonly BackendHttpClient m_http;

        public GameConfigClient(BackendConfig config = null)
            : this(new BackendHttpClient(config))
        {
        }

        public GameConfigClient(BackendHttpClient http)
        {
            m_http = http ?? throw new ArgumentNullException(nameof(http));
        }

        public async UniTask<GameConfigFetchResult> FetchAsync(
            string etag,
            CancellationToken cancellationToken = default)
        {
            for (int attempt = 0; ; attempt++)
            {
                try
                {
                    return await FetchOnceAsync(etag, cancellationToken);
                }
                catch (BackendApiException exception) when (
                    attempt < MaxRetries &&
                    (exception.StatusCode >= 500 ||
                     exception.StatusCode == 0 && exception.Code == "NETWORK_ERROR"))
                {
                    await UniTask.Delay(
                        TimeSpan.FromMilliseconds(250 * (1 << attempt)),
                        cancellationToken: cancellationToken);
                }
            }
        }

        async UniTask<GameConfigFetchResult> FetchOnceAsync(
            string etag,
            CancellationToken cancellationToken)
        {
            Dictionary<string, string> headers = string.IsNullOrWhiteSpace(etag)
                ? null
                : new Dictionary<string, string> { ["If-None-Match"] = etag };

            // Unity 把 304 当协议错误抛出, 这里声明为可接受状态由调用方判定.
            BackendHttpResponse response = await m_http.SendRawAsync(
                UnityWebRequest.kHttpVerbGET,
                BootstrapPath,
                null,
                null,
                headers,
                code => code == 304,
                cancellationToken);

            DateTimeOffset serverTime = ParseServerTime(response.GetHeader("X-Server-Time"));
            string responseEtag = response.GetHeader("ETag");
            if (response.StatusCode == 304)
            {
                return new GameConfigFetchResult(true, null, responseEtag ?? etag, serverTime);
            }

            GameConfigSnapshot snapshot;
            try
            {
                snapshot = JsonConvert.DeserializeObject<GameConfigSnapshot>(response.Body, BackendJson.Settings);
                GameConfigSnapshotValidator.Validate(snapshot);
            }
            catch (Exception exception) when (
                exception is JsonException || exception is GameConfigDataException)
            {
                throw new BackendApiException(0, "INVALID_RESPONSE", "服务器返回的游戏配置无效");
            }

            if (string.IsNullOrWhiteSpace(responseEtag))
            {
                throw new BackendApiException(0, "INVALID_RESPONSE", "服务器响应缺少游戏配置版本标识");
            }

            return new GameConfigFetchResult(false, snapshot, responseEtag, serverTime);
        }

        static DateTimeOffset ParseServerTime(string value) =>
            DateTimeOffset.TryParse(value, out DateTimeOffset parsed)
                ? parsed
                : DateTimeOffset.UtcNow;
    }
}
