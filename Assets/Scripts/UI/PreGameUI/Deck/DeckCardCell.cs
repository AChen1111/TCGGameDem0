using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class DeckCardCell : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] DeckCardView m_View;
    [SerializeField] TMP_Text m_Quantity;
    // Keep the authored field name stable; this label displays the card name.
    [SerializeField] TMP_Text m_Rarity;
    [SerializeField] Image m_Dim;
    [SerializeField] GameObject m_Selected;
    [SerializeField] Button m_Button;
    DeckCardData m_data;
    Action<int> m_select;
    int m_index;
    bool m_cardDrag, m_suppressClick;
    public DeckCardView View => m_View;
    public DeckCardData Data => m_data;
    void Awake() => m_Button.onClick.AddListener(Click);
    void Select() => m_select(m_index);
    void Click()
    {
        if (m_suppressClick) return;
        var data = m_data;
        var texture = m_View.Texture;
        var center = m_View.ArtRect.TransformPoint(m_View.ArtRect.rect.center);
        Select();
        if (!data.InDeck && data.Owned > 0) data.Window.AddCardFromClick(data, texture, center);
    }
    public void OnPointerDown(PointerEventData e) => m_suppressClick = false;
    public void Bind(DeckCardData data, int index, bool selected, Action<int> select)
    {
        m_data = data; m_index = index; m_select = select;
        m_Quantity.gameObject.SetActive(!data.InDeck);
        m_Quantity.text = data.Owned.ToString();
        m_Rarity.gameObject.SetActive(false);
        m_Dim.enabled = !data.InDeck && data.Owned == 0;
        m_Selected.SetActive(selected);
        data.Window.BindCard(m_View, data);
    }
    public void OnBeginDrag(PointerEventData e)
    {
        m_suppressClick = true;
        m_cardDrag = Mathf.Abs(e.delta.x) >= Mathf.Abs(e.delta.y);
        if (m_cardDrag)
        {
            // Selection refreshes the virtual row, so preserve its loaded art first.
            var texture = m_View.Texture;
            Select();
            m_data.Window.BeginCardDrag(m_data, texture, e.position);
        }
        else m_data.Window.ScrollFor(m_data).OnBeginDrag(e);
    }
    public void OnDrag(PointerEventData e)
    {
        if (m_cardDrag) m_data.Window.MoveCardDrag(e.position);
        else m_data.Window.ScrollFor(m_data).OnDrag(e);
    }
    public void OnEndDrag(PointerEventData e)
    {
        if (m_cardDrag) m_data.Window.EndCardDrag();
        else m_data.Window.ScrollFor(m_data).OnEndDrag(e);
    }
}
