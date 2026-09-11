using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace AChen.Networking
{
    /// <summary>后端 JSON 约定: camelCase 属性名, 序列化时忽略 null.</summary>
    public static class BackendJson
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore
        };

        public static string Serialize(object value) => JsonConvert.SerializeObject(value, Settings);

        /// <summary>反序列化响应体; 结果为 null 视为服务器响应无效.</summary>
        public static T DeserializeResponse<T>(string json)
        {
            T value = JsonConvert.DeserializeObject<T>(json, Settings);
            if (value == null)
            {
                throw new BackendApiException(0, "INVALID_RESPONSE", "服务器响应无效，请稍后再试");
            }

            return value;
        }
    }
}
