using System;
using TMPro;
using UnityEngine;

/// <summary>房间与回放共用的紧凑列表行，玩家资料使用项目头像组件。</summary>
public sealed class DuelLobbyRow : MonoBehaviour
{
    [SerializeField] UnityEngine.UI.Button m_Select;
    [SerializeField] AvatarPortraitView m_Portrait;
    [SerializeField] TMP_Text m_Title;
    [SerializeField] TMP_Text m_Subtitle;
    [SerializeField] TMP_Text m_State;
    [SerializeField] GameObject m_Selected;
    Action m_clicked;

    void Awake() => m_Select.onClick.AddListener(Select);
    void OnDestroy() => m_Select.onClick.RemoveListener(Select);
    void Select() => m_clicked();

    public void Bind(string title, string subtitle, string state, int avatar, int frame, Action clicked)
    {
        m_Title.text = title;
        m_Subtitle.text = subtitle;
        m_State.text = state;
        m_Portrait.SetPortrait(avatar, frame);
        m_clicked = clicked;
    }

    public void SetSelected(bool selected) => m_Selected.SetActive(selected);
    public void SetInteractable(bool enabled) => m_Select.interactable = enabled;
}
