using System;
using System.Collections.Generic;

/// <summary>跨程序集传递错误 key 和参数；不依赖业务翻译服务。</summary>
public sealed class LocalizedMessage
{
    public string Key { get; }
    public IReadOnlyDictionary<string, object> Arguments { get; }

    public LocalizedMessage(string key, IReadOnlyDictionary<string, object> arguments = null)
    {
        Key = key;
        if (arguments != null)
        {
            var copy = new Dictionary<string, object>(StringComparer.Ordinal);
            foreach (var pair in arguments) copy.Add(pair.Key, pair.Value);
            Arguments = copy;
        }
    }

    public override string ToString()
    {
        if (Arguments == null || Arguments.Count == 0) return Key ?? string.Empty;
        var values = new List<string>();
        foreach (var pair in Arguments)
            values.Add(pair.Key + "=" + Convert.ToString(pair.Value, System.Globalization.CultureInfo.InvariantCulture));
        return (Key ?? string.Empty) + " (" + string.Join(", ", values) + ")";
    }
}
