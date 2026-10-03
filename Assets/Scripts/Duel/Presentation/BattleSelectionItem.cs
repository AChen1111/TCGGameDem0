using System;
using TMPro;
using UnityEngine;

namespace AChen.Duel.Presentation
{
    public sealed class BattleSelectionItem : MonoBehaviour
    {
        [SerializeField] UnityEngine.UI.Button m_button;
        [SerializeField] UnityEngine.UI.RawImage m_art;
        [SerializeField] Texture m_back;
        [SerializeField] TextMeshProUGUI m_label, m_order;
        [SerializeField] GameObject m_selected;
        public void Bind(BattleSceneController scene, DuelSelectionOption option, Action select)
        {
            bool card = option.DefinitionId.Length > 0 || option.CardId > 0;
            m_art.gameObject.SetActive(card);
            m_art.texture = option.DefinitionId.Length > 0 ? scene.TextureForDefinition(option.DefinitionId) : m_back;
            if (!card) { m_label.rectTransform.anchoredPosition = new Vector2(6,-70); m_label.rectTransform.sizeDelta = new Vector2(148,150); }
            m_label.text = option.Label; m_label.gameObject.SetActive(!card || option.DefinitionId.Length==0);
            m_button.onClick.RemoveAllListeners(); m_button.onClick.AddListener(() => select());
            SetSelected(0);
        }
        public void SetSelected(int order)
        { m_selected.SetActive(order > 0); m_order.text = order > 0 ? order.ToString() : ""; }
    }
}
