using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.Networking;
using UnityEngine.Scripting;

namespace AChen.Networking
{
    /// <summary>把失败的 UnityWebRequest 转成携带后端 ProblemDetails 的 BackendApiException.</summary>
    static class BackendHttpError
    {
        public static BackendApiException FromRequest(UnityWebRequest request)
        {
            if (request.responseCode <= 0)
            {
                return new BackendApiException(0, "NETWORK_ERROR", "无法连接服务器，请检查网络");
            }

            try
            {
                ProblemDetailsDto problem = JsonConvert.DeserializeObject<ProblemDetailsDto>(
                    request.downloadHandler.text,
                    BackendJson.Settings);
                if (problem != null)
                {
                    return new BackendApiException(
                        request.responseCode,
                        string.IsNullOrEmpty(problem.Code) ? "HTTP_ERROR" : problem.Code,
                        string.IsNullOrEmpty(problem.Title) ? "服务器请求失败，请稍后再试" : problem.Title,
                        problem.Errors);
                }
            }
            catch (JsonException)
            {
            }

            return new BackendApiException(request.responseCode, "HTTP_ERROR", "服务器请求失败，请稍后再试");
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
