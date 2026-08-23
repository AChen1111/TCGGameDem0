using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine.Networking;
using UnityEngine.Scripting;

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

    public sealed class GameConfigClient
    {
        const int MaxRetries = 2;

        static readonly JsonSerializerSettings s_jsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Include
        };

        readonly BackendConfig m_config;

        public GameConfigClient(BackendConfig config = null)
        {
            m_config = config ?? new BackendConfig();
        }

        public async UniTask<GameConfigFetchResult> FetchAsync(
            string etag,
            CancellationToken cancellationToken = default)
        {
            BackendApiException lastException = null;
            for (int attempt = 0; attempt <= MaxRetries; attempt++)
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
                    lastException = exception;
                    await UniTask.Delay(
                        TimeSpan.FromMilliseconds(250 * (1 << attempt)),
                        cancellationToken: cancellationToken);
                }
            }

            throw lastException ?? new BackendApiException(0, "NETWORK_ERROR", "Could not reach backend.");
        }

        async UniTask<GameConfigFetchResult> FetchOnceAsync(
            string etag,
            CancellationToken cancellationToken)
        {
            using (var request = UnityWebRequest.Get(m_config.BaseUrl + "/api/game-config/bootstrap"))
            {
                request.timeout = m_config.TimeoutSeconds;
                request.SetRequestHeader("Accept", "application/json");
                if (!string.IsNullOrWhiteSpace(etag))
                {
                    request.SetRequestHeader("If-None-Match", etag);
                }

                try
                {
                    await request.SendWebRequest().ToUniTask(
                        cancellationToken: cancellationToken,
                        cancelImmediately: true);
                }
                catch (UnityWebRequestException) when (request.responseCode == 304)
                {
                    // Unity reports 304 as a protocol error; it is a successful cache validation.
                }
                catch (UnityWebRequestException)
                {
                    throw CreateException(request);
                }

                DateTimeOffset serverTime = ParseServerTime(request.GetResponseHeader("X-Server-Time"));
                string responseEtag = request.GetResponseHeader("ETag");
                if (request.responseCode == 304)
                {
                    return new GameConfigFetchResult(true, null, responseEtag ?? etag, serverTime);
                }

                if (request.result != UnityWebRequest.Result.Success)
                {
                    throw CreateException(request);
                }

                GameConfigSnapshot snapshot;
                try
                {
                    snapshot = JsonConvert.DeserializeObject<GameConfigSnapshot>(
                        request.downloadHandler.text,
                        s_jsonSettings);
                    GameConfigSnapshotValidator.Validate(snapshot);
                }
                catch (JsonException exception)
                {
                    throw new BackendApiException(0, "INVALID_RESPONSE", exception.Message);
                }
                catch (GameConfigDataException exception)
                {
                    throw new BackendApiException(0, "INVALID_RESPONSE", exception.Message);
                }

                if (string.IsNullOrWhiteSpace(responseEtag))
                {
                    throw new BackendApiException(0, "INVALID_RESPONSE", "Backend omitted the game config ETag.");
                }

                return new GameConfigFetchResult(false, snapshot, responseEtag, serverTime);
            }
        }

        static DateTimeOffset ParseServerTime(string value) =>
            DateTimeOffset.TryParse(value, out DateTimeOffset parsed)
                ? parsed
                : DateTimeOffset.UtcNow;

        static BackendApiException CreateException(UnityWebRequest request)
        {
            if (request.responseCode <= 0)
            {
                return new BackendApiException(0, "NETWORK_ERROR", request.error ?? "Could not reach backend.");
            }

            try
            {
                ProblemDetailsDto problem = JsonConvert.DeserializeObject<ProblemDetailsDto>(
                    request.downloadHandler.text,
                    s_jsonSettings);
                if (problem != null)
                {
                    return new BackendApiException(
                        request.responseCode,
                        string.IsNullOrEmpty(problem.Code) ? "HTTP_ERROR" : problem.Code,
                        string.IsNullOrEmpty(problem.Title) ? "Backend request failed." : problem.Title,
                        problem.Errors);
                }
            }
            catch (JsonException)
            {
                // Fall through to the safe generic response.
            }

            return new BackendApiException(request.responseCode, "HTTP_ERROR", "Backend request failed.");
        }

        [Preserve]
        sealed class ProblemDetailsDto
        {
            public ProblemDetailsDto() { }
            public string Title { get; set; }
            public string Code { get; set; }
            public Dictionary<string, string[]> Errors { get; set; }
        }
    }
}
