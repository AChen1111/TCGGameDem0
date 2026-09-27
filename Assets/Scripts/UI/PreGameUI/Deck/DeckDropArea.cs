using UnityEngine;
using UnityEngine.EventSystems;

public sealed class DeckDropArea : MonoBehaviour, IDropHandler
{
    [SerializeField] DeckEditWindow m_Window;
    [SerializeField] bool m_IntoDeck;
    public void OnDrop(PointerEventData e) => m_Window.DropCard(m_IntoDeck);
}
