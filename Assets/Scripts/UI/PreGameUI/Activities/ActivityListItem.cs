using System;
using System.Linq;
using AChen.Configuration;
using UnityEngine;
using UnityEngine.UI;

public sealed class ActivityListItem : MonoBehaviour
{
    [SerializeField] Button m_Select;
    [SerializeField] LocalizedText m_Label;
    [SerializeField] LocalizedText m_Status;
    [SerializeField] GameObject m_RedDot;
    [SerializeField] GameObject m_Selected;
    [SerializeField] Image m_Icon;
    [SerializeField] Sprite[] m_TypeIcons;
    Action m_clicked;
    void Awake() => m_Select.onClick.AddListener(() => m_clicked());
    public void Bind(ActivitySnapshot state, bool selected, Action clicked)
    {
        m_clicked = clicked;
        m_Label.SetKey(state.Definition.NameKey);
        bool claimable = state.PlayerState.EntryStates.Any(x => x.CanClaim);
        m_RedDot.SetActive(claimable);
        m_Selected.SetActive(selected);
        m_Icon.sprite = m_TypeIcons[(int)state.Definition.Type];
        m_Status.SetKey(state.Status == "upcoming" ? "ui.activities.upcoming" : !state.Eligible ? "ui.activities.locked" :
            claimable ? "ui.activities.claimable" : state.PlayerState.Completed ? "ui.activities.completed" : "ui.activities.view");
    }
}
