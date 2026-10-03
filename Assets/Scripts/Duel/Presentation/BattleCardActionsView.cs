using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace AChen.Duel.Presentation
{
    public sealed class BattleCardActionsView : MonoBehaviour
    {
        [SerializeField] RectTransform m_menu;
        [SerializeField] RectTransform m_hudRoot;
        Transform m_originalParent;
        [SerializeField] UnityEngine.UI.Button[] m_buttons;
        [SerializeField] UnityEngine.UI.Image[] m_icons;
        [SerializeField] TextMeshProUGUI[] m_labels;
        [SerializeField] Sprite[] m_kindSprites;
        BattleSceneController m_scene;
        Canvas m_canvas;
        IReadOnlyList<DuelActionView> m_actions;
        bool m_visible;
        int m_card;
        Vector2 m_anchor;

        public void Initialize(BattleSceneController scene, Canvas canvas)
        {
            m_scene = scene; m_canvas = canvas;
            m_originalParent=m_menu.parent;m_menu.SetParent(m_hudRoot.parent,false);
            m_scene.CardActionsRequested += Show;
            m_scene.Source.Changed += Changed;
            for (int i = 0; i < m_buttons.Length; i++)
            {
                int index = i;
                m_buttons[i].onClick.AddListener(() => Choose(index));
            }
            Hide();
        }

        void Show(int id, Vector2 screenAnchor)
        {
            m_card = id; m_anchor = screenAnchor;
            m_actions = m_scene.Source.Current.Card(id).Actions;
            m_visible = m_actions.Count > 0;
            m_menu.gameObject.SetActive(m_visible);
            m_menu.SetAsLastSibling();
            float width = m_actions.Count * 128f;
            m_menu.sizeDelta = new Vector2(width, 145f);
            for (int i = 0; i < m_buttons.Length; i++)
            {
                bool show = i < m_actions.Count;
                m_buttons[i].gameObject.SetActive(show);
                if (!show) continue;
                var action = m_actions[i];
                int sprite = (int)action.Kind * 4;
                m_icons[i].sprite = m_kindSprites[sprite];
                m_buttons[i].spriteState = new UnityEngine.UI.SpriteState
                {
                    highlightedSprite = m_kindSprites[sprite + 1],
                    pressedSprite = m_kindSprites[sprite + 2],
                    disabledSprite = m_kindSprites[sprite + 3]
                };
                m_labels[i].text = action.Label.Length > 0 ? action.Label : Caption(action.Kind);
                m_buttons[i].interactable = m_scene.Source.Current.CanBrowseCards;
                ((RectTransform)m_buttons[i].transform).anchoredPosition = new Vector2(i * 128f, 0);
            }
            Position();
        }

        static string Caption(DuelActionKind kind) => kind switch
        {
            DuelActionKind.NormalSummon => "召唤",
            DuelActionKind.SetMonster => "设置",
            DuelActionKind.SpecialSummon => "特殊召唤",
            DuelActionKind.Activate => "发动效果",
            DuelActionKind.SetSpellTrap => "设置",
            DuelActionKind.Pendulum => "灵摆放置",
            DuelActionKind.ChangePosition => "表示变更",
            DuelActionKind.DebugPlacement => "入场",
            DuelActionKind.Attack => "攻击",
            _ => ""
        };

        void Choose(int index)
        {
            string actionId = m_actions[index].Id;
            Hide();
            m_scene.BeginAction(m_card, actionId);
        }

        void Changed(DuelViewChange change)
        {
            if (change.Kind == DuelChangeKind.Timer) return;
            if (change.Kind == DuelChangeKind.Highlight && m_visible) { Show(m_card, m_anchor); return; }
            Hide();
        }

        void Position()
        {
            var card = m_scene.Source.Current.Card(m_card);
            bool modelVisible = card.Zone.IsSlot || card.Zone.Kind == DuelZone.Hand;
            Vector2 screen = modelVisible ? m_scene.CardScreenAnchor(m_card) : m_anchor;
            RectTransform canvas = (RectTransform)m_hudRoot.parent;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas, screen, m_canvas.worldCamera, out Vector2 point);
            point.y += card.Zone.Kind == DuelZone.Hand ? 172f : 112f;
            float halfWidth = m_menu.sizeDelta.x * .5f;
            point.x = Mathf.Clamp(point.x, canvas.rect.xMin + halfWidth + 12, canvas.rect.xMax - halfWidth - 12);
            point.y = Mathf.Clamp(point.y, canvas.rect.yMin + 125, canvas.rect.yMax - 145);
            m_menu.anchoredPosition = point;
        }

        void LateUpdate() { if (m_visible) Position(); }
        public void Hide() { m_visible = false; m_menu.gameObject.SetActive(false); }
        public void Dispose()
        {
            Hide();m_menu.SetParent(m_originalParent,false);
            m_scene.CardActionsRequested -= Show; m_scene.Source.Changed -= Changed;
            foreach (var button in m_buttons) button.onClick.RemoveAllListeners();
        }
    }
}
