using System;
using AChen.Events;
using AChen.Player;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChangeNameWindow : AWindowController
{
    // --tag_start: 自动生成--
    [SerializeField] TMP_InputField m_InpName;
    [SerializeField] Button m_BtnOk;
    [SerializeField] Button m_BtnNo;
    // --tag_end: 自动生成--
    Guid? m_playerId;
    string m_savedNickname;
    bool m_isSubmitting;

    protected override void AddListeners()
    {
        m_BtnOk.onClick.AddListener(OnBtnOkClicked);
        m_BtnNo.onClick.AddListener(UI_Close);
    }

    protected override void RemoveListeners()
    {
        EventCenter.RemoveListener(GameEvent.PlayerNicknameChanged, OnNicknameChanged);
        m_BtnOk.onClick.RemoveListener(OnBtnOkClicked);
        m_BtnNo.onClick.RemoveListener(UI_Close);
    }

    protected override void OnOpen()
    {
        m_playerId = null;
        m_savedNickname = null;
        m_isSubmitting = false;
        m_BtnOk.interactable = true;
        m_InpName.SetTextWithoutNotify(string.Empty);
        EventCenter.AddListener(GameEvent.PlayerNicknameChanged, OnNicknameChanged);
        var player = PlayerSession.HasInstance ? PlayerSession.Instance.CurrentPlayer : null;
        OnNicknameChanged(player?.Id, player?.Nickname);
    }

    protected override void OnClose()
    {
        EventCenter.RemoveListener(GameEvent.PlayerNicknameChanged, OnNicknameChanged);
    }

    void OnNicknameChanged(Guid? playerId, string nickname)
    {
        nickname ??= string.Empty;
        // 同一玩家编辑中的输入不被金币刷新或其他资料通知覆盖.
        bool hasDraft = m_savedNickname != null && m_InpName.text != m_savedNickname;
        if (playerId != m_playerId || !hasDraft)
        {
            m_InpName.SetTextWithoutNotify(nickname);
        }
        m_playerId = playerId;
        m_savedNickname = nickname;
    }

    void OnBtnOkClicked()
    {
        SubmitAsync().Forget();
    }

    async UniTaskVoid SubmitAsync()
    {
        if (m_isSubmitting || !IsOpened) return;
        m_isSubmitting = true;
        m_BtnOk.interactable = false;
        string nickname = m_InpName.text;
        bool succeeded = await RunGuardedAsync(
            token => PlayerSession.Instance.RenameAsync(nickname, token),
            "修改昵称",
            "修改昵称失败，请稍后重试");
        if (this == null || !IsOpened) return;

        m_isSubmitting = false;
        m_BtnOk.interactable = true;
        if (succeeded) UI_Close();
    }
}
