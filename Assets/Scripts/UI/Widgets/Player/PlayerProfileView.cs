using System;
using AChen.Events;
using AChen.Player;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 显示玩家昵称与头像，跟随玩家数据自动刷新。
/// </summary>
public class PlayerProfileView : MonoBehaviour
{
    [SerializeField] private TMP_Text m_userName;
    [SerializeField] private Image m_userIcon;
    int? m_avatarId;
    int m_loadVersion;
    bool m_avatarLoaded;

    private void OnEnable()
    {
        EventCenter.AddListener(GameEvent.PlayerNicknameChanged, OnNicknameChanged);
        EventCenter.AddListener(GameEvent.PlayerAvatarChanged, OnAvatarChanged);
        var player = PlayerSession.HasInstance ? PlayerSession.Instance.CurrentPlayer : null;
        OnNicknameChanged(player?.Id, player?.Nickname);
        OnAvatarChanged(player?.AvatarId);
    }

    private void OnDisable()
    {
        EventCenter.RemoveListener(GameEvent.PlayerNicknameChanged, OnNicknameChanged);
        EventCenter.RemoveListener(GameEvent.PlayerAvatarChanged, OnAvatarChanged);
        m_loadVersion++;
        m_avatarLoaded = false;
    }

    void OnNicknameChanged(Guid? playerId, string nickname)
    {
        nickname ??= string.Empty;
        if (m_userName.text != nickname)
        {
            m_userName.text = nickname;
        }
    }

    void OnAvatarChanged(int? avatarId)
    {
        if (avatarId == m_avatarId && m_avatarLoaded)
        {
            return;
        }

        m_avatarId = avatarId;
        m_avatarLoaded = true;
        int version = ++m_loadVersion;
        m_userIcon.sprite = null;
        if (avatarId is int id)
        {
            LoadAvatarAsync(id, version).Forget();
        }
    }

    private async UniTask LoadAvatarAsync(int avatarId, int version)
    {
        try
        {
            Sprite sprite = await AddressableLoader.Instance.LoadSprite(AddressKeys.GetAvatarAddress(avatarId))
                .AttachExternalCancellation(this.GetCancellationTokenOnDestroy());
            // 忽略隐藏前或上一张头像的异步结果.
            if (this != null && isActiveAndEnabled && version == m_loadVersion)
            {
                m_userIcon.sprite = sprite;
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception exception)
        {
            if (this == null || version != m_loadVersion) return;
            m_avatarLoaded = false;
            ALog.LogError($"加载玩家头像失败. AvatarId={avatarId}; Error={exception.Message}", ALogCategories.UI);
        }
    }
}
