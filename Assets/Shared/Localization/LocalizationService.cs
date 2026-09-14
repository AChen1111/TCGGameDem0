using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

public enum GameLanguage { SimplifiedChinese, English }

public static class LocalizationService
{
    public const string PreferenceKey = "TCG.Localization.Language";
    const string ResourcePath = "Localization/Translations";
    static readonly Dictionary<string, Table.TranslationRow> s_table = new Dictionary<string, Table.TranslationRow>(StringComparer.Ordinal);
    static readonly HashSet<string> s_reported = new HashSet<string>(StringComparer.Ordinal);
    static readonly Regex s_placeholder = new Regex(@"\{([A-Za-z][A-Za-z0-9_]*)\}");
    static bool s_initialized;
    static GameLanguage s_language;
    static LocalizationSettings s_settings;

    public static event Action LanguageChanged;
    public static GameLanguage CurrentLanguage { get { Initialize(); return s_language; } }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetState()
    {
        s_initialized = false;
        s_table.Clear();
        s_reported.Clear();
        s_settings = null;
        LanguageChanged = null;
    }

    public static void Initialize()
    {
        if (s_initialized) return;
        s_initialized = true;
        s_language = PlayerPrefs.GetInt(PreferenceKey, 0) == 1 ? GameLanguage.English : GameLanguage.SimplifiedChinese;
        s_settings = Resources.Load<LocalizationSettings>("Localization/Settings");
        EnsureEnglishFallbacks();
        TextAsset data = Resources.Load<TextAsset>(ResourcePath);
        if (data == null)
        {
            ReportMissing(ResourcePath, "语言表资源不存在");
            return;
        }
        try
        {
            var rows = Table.TranslationRow.LoadBytes(data.bytes);
            foreach (Table.TranslationRow row in rows)
            {
                if (string.IsNullOrWhiteSpace(row.Key) || s_table.ContainsKey(row.Key))
                    throw new System.IO.InvalidDataException("语言表 key 为空或重复: " + row.Key);
                s_table.Add(row.Key, row);
            }
            ALog.Log($"多语言二进制表初始化完成. Language={s_language}; Entries={s_table.Count}", ALogCategories.Localization);
        }
        catch (Exception exception) when (exception is System.IO.IOException || exception is ArgumentException || exception is FormatException)
        {
            s_table.Clear();
            ALog.LogError($"语言表加载失败. Resource={ResourcePath}; Error={exception.Message}", ALogCategories.Localization);
        }
    }

    public static void SetLanguage(GameLanguage language)
    {
        if (language != GameLanguage.SimplifiedChinese && language != GameLanguage.English)
            throw new ArgumentOutOfRangeException(nameof(language));
        Initialize();
        if (s_language == language) return;
        s_language = language;
        PlayerPrefs.SetInt(PreferenceKey, (int)language);
        PlayerPrefs.Save();
        ALog.Log($"切换语言完成. Language={language}", ALogCategories.Localization);
        LanguageChanged?.Invoke();
    }

    public static string GetText(string key, IReadOnlyDictionary<string, object> arguments = null)
    {
        Initialize();
        return Resolve(key, arguments, new HashSet<string>(StringComparer.Ordinal));
    }

    static string Resolve(string key, IReadOnlyDictionary<string, object> arguments, HashSet<string> resolving)
    {
        if (string.IsNullOrEmpty(key) || !s_table.TryGetValue(key, out Table.TranslationRow row))
            return ReportMissing(key, "缺少 key");
        string template = s_language == GameLanguage.English ? row.English : row.Chinese;
        if (string.IsNullOrEmpty(template)) return ReportMissing(key, "缺少当前语言翻译");
        if (!resolving.Add(key)) return ReportMissing(key, "翻译参数循环引用");
        bool failed = false;
        string result = s_placeholder.Replace(template, match =>
        {
            string name = match.Groups[1].Value;
            if (arguments == null || !arguments.TryGetValue(name, out object value) || value == null)
            {
                failed = true;
                return ReportMissing(key, "缺少参数 " + name);
            }
            // 仅显式 LocalizedMessage 参数再次查表, 普通字符串中的花括号保持原样.
            return value is LocalizedMessage nested
                ? Resolve(nested.Key, nested.Arguments, resolving)
                : Convert.ToString(value, CultureInfo.CurrentCulture);
        });
        resolving.Remove(key);
        return failed ? "Null" : result;
    }

    public static TMP_FontAsset CurrentFont
    {
        get
        {
            Initialize();
            TMP_FontAsset font = s_settings == null ? null
                : s_language == GameLanguage.English ? s_settings.englishFont : s_settings.chineseFont;
            if (font == null) ReportMissing("Localization/Settings", "缺少当前语言字体");
            return font;
        }
    }

    // 切语言时同步字体, 并按设计字号为上限做自适应, 避免英文撑破窄框.
    public static void ApplyPresentation(TMP_Text text)
    {
        if (text == null) return;
        TMP_FontAsset font = CurrentFont;
        if (font != null && text.font != font)
        {
            text.font = font;
            text.fontSharedMaterial = font.material;
        }
        EnableAutoSize(text);
    }

    public static void EnableAutoSize(TMP_Text text)
    {
        if (text == null) return;
        float max = text.enableAutoSizing && text.fontSizeMax > 1f ? text.fontSizeMax : text.fontSize;
        if (max < 1f) max = 36f;
        text.fontSizeMax = max;
        // min 已贴近 max 时视为锁定字号, 避免同组按钮因文案长短缩到不同大小.
        if (!(text.enableAutoSizing && text.fontSizeMin >= max - 0.01f))
            text.fontSizeMin = Mathf.Min(8f, max);
        text.enableAutoSizing = true;
    }

    static void EnsureEnglishFallbacks()
    {
        if (s_settings == null) return;
        TMP_FontAsset english = s_settings.englishFont;
        TMP_FontAsset chinese = s_settings.chineseFont;
        if (english == null || chinese == null || ReferenceEquals(english, chinese)) return;
        if (english.fallbackFontAssetTable == null)
            english.fallbackFontAssetTable = new List<TMP_FontAsset>();
        bool added = false;
        if (chinese.fallbackFontAssetTable != null)
        {
            for (int i = 0; i < chinese.fallbackFontAssetTable.Count; i++)
            {
                TMP_FontAsset punct = chinese.fallbackFontAssetTable[i];
                if (punct == null || english.fallbackFontAssetTable.Contains(punct)) continue;
                english.fallbackFontAssetTable.Add(punct);
                added = true;
            }
        }
        if (!english.fallbackFontAssetTable.Contains(chinese))
        {
            english.fallbackFontAssetTable.Add(chinese);
            added = true;
        }
        if (added)
            ALog.Log($"英文字体已挂中文回退. English={english.name}; Chinese={chinese.name}", ALogCategories.Localization);
    }

    public static string ErrorKey(string code)
    {
        if (string.IsNullOrWhiteSpace(code)) return "err.http_error";
        string key = code.ToLowerInvariant();
        return key.StartsWith("err.", StringComparison.Ordinal) ? key : "err." + key;
    }

    internal static string ReportMissing(string key, string reason)
    {
        if (s_reported.Add($"{s_language}|{key}|{reason}"))
            ALog.LogWarning($"多语言显示 Null. 缺少或无效 key={key ?? "<null>"}; Language={s_language}; Reason={reason}", ALogCategories.Localization);
        return "Null";
    }
}
