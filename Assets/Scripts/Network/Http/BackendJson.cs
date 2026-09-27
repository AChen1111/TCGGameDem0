using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace AChen.Networking
{
    /// <summary>后端 JSON 约定: camelCase 属性名, 序列化时忽略 null.</summary>
    public static class BackendJson
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            ContractResolver = new BackendContractResolver(),
            NullValueHandling = NullValueHandling.Ignore
        };

        sealed class BackendContractResolver : CamelCasePropertyNamesContractResolver
        {
            protected override JsonArrayContract CreateArrayContract(System.Type objectType)
            {
                // CLR 数组不能声明序列化回调。默认 InitializeContract 会无条件扫描其
                // 合成方法参数，在当前 Android IL2CPP/HybridCLR 组合上触发原生崩溃。
                // 数组构造函数已设置元素类型和集合创建逻辑，直接使用，避免无意义的扫描。
                // 普通集合与 DTO 继续走默认流程，保留命名规则、转换器和回调行为。
                return objectType.IsArray
                    ? new JsonArrayContract(objectType)
                    : base.CreateArrayContract(objectType);
            }
        }

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
