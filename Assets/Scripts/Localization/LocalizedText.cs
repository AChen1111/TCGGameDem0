using System.Collections.Generic;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(TextMeshProUGUI))]
public sealed class LocalizedText : MonoBehaviour
{
    [SerializeField] string key;
    [Tooltip("由业务设置 key/参数的文本, 首次赋值前保持空白.")]
    [SerializeField] bool dynamicContent;
    TMP_Text m_text;
    LocalizedMessage m_message;
    bool m_cleared;

    void OnEnable()
    {
        LocalizationService.LanguageChanged += Refresh;
        Refresh();
    }

    void OnDisable() => LocalizationService.LanguageChanged -= Refresh;

    public void SetKey(string value, IReadOnlyDictionary<string, object> arguments = null)
        => SetMessage(new LocalizedMessage(value, arguments));

    public void SetMessage(LocalizedMessage message)
    {
        m_message = message;
        m_cleared = false;
        Refresh();
    }

    public void Clear()
    {
        m_message = null;
        m_cleared = true;
        Refresh();
    }

    public void Refresh()
    {
        if (!LocalizationService.IsReady) return;
        if (m_text == null) m_text = GetComponent<TMP_Text>();
        LocalizationService.ApplyPresentation(m_text);
        if (m_cleared || (dynamicContent && m_message == null)) { m_text.text = string.Empty; return; }
        m_text.text = LocalizationService.GetText(m_message?.Key ?? key, m_message?.Arguments);
    }
}

public static class LocalizedTextExtensions
{
    public static LocalizedText Localized(this TMP_Text text) => text.GetComponent<LocalizedText>();
}
