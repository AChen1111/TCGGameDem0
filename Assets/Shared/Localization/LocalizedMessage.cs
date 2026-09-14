using System;
using System.Collections.Generic;

/// <summary>保留 key 和参数, 在显示时才解析语言.</summary>
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

    public override string ToString() => LocalizationService.GetText(Key, Arguments);
}
