using System;
using System.Linq;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace AChen.Duel.Presentation
{
    public sealed class BattleCardRowData
    {
        public CardView Card { get; }
        public Texture Texture { get; }
        public BattleSceneController Scene { get; }
        public BattleCardRowData(CardView card, Texture texture, BattleSceneController scene)
        { Card = card; Texture = texture; Scene = scene; }
    }
    public sealed class BattleCardRow : MonoBehaviour, IRowItem<BattleCardRowData>
    {
        [SerializeField] RectTransform m_root;
        [SerializeField] UnityEngine.UI.Button m_button;
        [SerializeField] UnityEngine.UI.RawImage m_art;
        [SerializeField] UnityEngine.UI.Image m_background;
        [SerializeField] TextMeshProUGUI m_name;
        [SerializeField] TextMeshProUGUI m_info;
        Action<int> m_selected = delegate { };
        BattleCardRowData m_data;
        int m_index;
        public int RowCardCount => 1;
        void Awake() => m_button.onClick.AddListener(Select);
        void Select()
        {
            if (!m_data.Scene.Source.Current.CanBrowseCards) return;
            m_selected(m_index);
            Vector2 anchor = RectTransformUtility.WorldToScreenPoint(m_data.Scene.BattleUICamera, m_root.TransformPoint(m_root.rect.center));
            m_data.Scene.ShowCardActions(m_data.Card.InstanceId, anchor);
        }
        public void SetRowData(int rowIndex, List<BattleCardRowData> allData, int selectedIndex, Action<int> onSelected)
        {
            m_index = rowIndex; m_selected = onSelected; m_data = allData[rowIndex]; m_art.texture = m_data.Texture;
            m_name.text = LocalizationService.GetText("card." + m_data.Card.Definition.CardId + ".name");
            m_info.text = m_data.Card.Zone.Kind==DuelZone.ExtraDeck
                ? m_data.Card.Actions.Any(a=>a.Kind==DuelActionKind.SpecialSummon)?"可特殊召唤":"当前没有可用的召唤动作"
                : m_data.Card.Actions.Any(a=>a.Kind==DuelActionKind.Activate)?"可发动效果":BattleLabels.Position(m_data.Card.Position);
            m_button.interactable = m_data.Scene.Source.Current.CanBrowseCards;
            m_background.color = rowIndex == selectedIndex ? new Color(.19f, .3f, .38f, 1) : new Color(.035f, .065f, .08f, .98f);
        }
    }
}
