using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AChen.Duel.Presentation
{
    public sealed class BattleSelectionItem : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] UnityEngine.UI.Button m_button;
        [SerializeField] UnityEngine.UI.RawImage m_art;
        [SerializeField] Texture m_back;
        [SerializeField] TextMeshProUGUI m_label, m_order;
        [SerializeField] GameObject m_selected;
        Vector2 m_pointerStart;
        bool m_click;
        public Vector2 ScreenAnchor(Camera camera) => RectTransformUtility.WorldToScreenPoint(camera, transform.position);
        public void OnPointerDown(PointerEventData eventData) { m_pointerStart = eventData.position; m_click = true; }
        public void OnPointerUp(PointerEventData eventData) { m_click = Vector2.Distance(m_pointerStart, eventData.position) <= 8; }
        public void Bind(BattleSceneController scene, DuelSelectionOption option, Action select)
        {
            bool card = option.DefinitionId.Length > 0 || option.CardId != 0 || option.Label == "隐藏卡牌";
            m_art.gameObject.SetActive(card);
            m_art.texture = option.DefinitionId.Length > 0 ? scene.TextureForDefinition(option.DefinitionId) : m_back;
            if (!card) { m_label.rectTransform.anchoredPosition = new Vector2(6, -70); m_label.rectTransform.sizeDelta = new Vector2(148, 150); }
            m_label.text = option.Label; m_label.gameObject.SetActive(!card);
            m_button.onClick.RemoveAllListeners(); m_button.onClick.AddListener(() => { if (m_click) select(); });
            SetSelected(0);
        }
        public void SetSelected(int order)
        { m_selected.SetActive(order > 0); m_order.text = order > 0 ? order.ToString() : ""; }
    }
}
