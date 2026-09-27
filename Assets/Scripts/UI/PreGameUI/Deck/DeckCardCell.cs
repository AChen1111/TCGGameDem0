using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class DeckCardCell : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
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
    bool m_cardDrag;
    public DeckCardView View => m_View;
    void Awake() => m_Button.onClick.AddListener(Select);
    void Select() => m_select(m_index);
    public void Bind(DeckCardData data, int index, bool selected, Action<int> select)
    {
        m_data = data; m_index = index; m_select = select;
        m_Quantity.gameObject.SetActive(!data.InDeck);
        m_Quantity.text = "×" + data.Owned;
        m_Rarity.text = LocalizationService.GetText("card." + data.CardId + ".name");
        m_Dim.enabled = !data.InDeck && data.Owned == 0;
        m_Selected.SetActive(selected);
        data.Window.BindCard(m_View, data);
    }
    public void OnBeginDrag(PointerEventData e)
    {
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
