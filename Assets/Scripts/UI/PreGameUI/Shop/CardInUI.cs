using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class CardInUIData
{
    public string CardId { get; }
    public string SourcePool { get; }
    public Texture Texture { get; }
    public int Index { get; }

    public CardInUIData(string cardId, string sourcePool, Texture texture, int index)
    {
        CardId = cardId ?? string.Empty;
        SourcePool = sourcePool ?? string.Empty;
        Texture = texture;
        Index = index;
    }
}

public class CardInUI : MonoBehaviour
{
    [SerializeField] RawImage m_RawCard;
    [SerializeField] Button m_BtnAll;

    CardInUIData m_Data;
    Action<int> m_OnSelected;

    public void SetData(CardInUIData data, Action<int> onSelected)
    {
        m_Data = data;
        m_OnSelected = onSelected;
        bool hasTexture = data != null && data.Texture != null;
        m_RawCard.texture = hasTexture ? data.Texture : null;
        m_RawCard.enabled = hasTexture;
    }

    void Awake()
    {
        m_BtnAll.onClick.AddListener(OnClicked);
    }

    void OnDestroy()
    {
        m_BtnAll.onClick.RemoveListener(OnClicked);
    }

    void OnClicked()
    {
        if (m_Data == null)
        {
            return;
        }

        m_OnSelected?.Invoke(m_Data.Index);
    }
}
