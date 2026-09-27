using System;
using AChen.Events;
using AChen.Player;
using TMPro;
using UnityEngine;

public class PlayerProfileView : MonoBehaviour
{
    [SerializeField] TMP_Text m_userName;
    [SerializeField] AvatarPortraitView m_Portrait;
    void OnEnable()
    {
        EventCenter.AddListener(GameEvent.PlayerNicknameChanged, OnNicknameChanged);
        EventCenter.AddListener(GameEvent.PlayerAvatarChanged, OnPortraitChanged);
        EventCenter.AddListener(GameEvent.PlayerAvatarFrameChanged, OnPortraitChanged);
        var player = PlayerSession.Instance.CurrentPlayer;
        OnNicknameChanged(player.Id, player.Nickname);
        OnPortraitChanged(player.AvatarId);
    }
    void OnDisable()
    {
        EventCenter.RemoveListener(GameEvent.PlayerNicknameChanged, OnNicknameChanged);
        EventCenter.RemoveListener(GameEvent.PlayerAvatarChanged, OnPortraitChanged);
        EventCenter.RemoveListener(GameEvent.PlayerAvatarFrameChanged, OnPortraitChanged);
    }
    void OnNicknameChanged(Guid? id, string nickname) => m_userName.text = nickname;
    void OnPortraitChanged(int? id)
    {
        if (!id.HasValue) return; // 清理会话的事件没有可显示的玩家。
        var player = PlayerSession.Instance.CurrentPlayer;
        m_Portrait.SetPortrait(player.AvatarId.Value, player.AvatarFrameId);
    }
}
