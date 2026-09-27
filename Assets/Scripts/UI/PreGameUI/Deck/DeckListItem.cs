using AChen.Decks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class DeckListItem : MonoBehaviour
{
    [SerializeField] Button m_Open;
    [SerializeField] Button m_Delete;
    [SerializeField] TMP_Text m_DeckName;
    [SerializeField] TMP_Text m_Count;
    DeckData m_data;
    DeckListWindow m_window;
    void Awake()
    { m_Open.onClick.AddListener(() => m_window.Edit(m_data.Id)); m_Delete.onClick.AddListener(() => m_window.Delete(m_data)); }
    public void Bind(DeckData data, DeckListWindow window)
    {
        m_data = data; m_window = window; m_DeckName.text = data.Name;
        m_Count.text = window.DeckSummary(data);
    }
}
