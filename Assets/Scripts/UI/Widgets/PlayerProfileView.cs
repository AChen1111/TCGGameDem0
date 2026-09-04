using System.Collections.Generic;
using AChen.Networking;
using AChen.Player;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 显示玩家昵称与头像，跟随玩家数据自动刷新。点击头像打开选择窗.
/// </summary>
public class PlayerProfileView : MonoBehaviour, IPlayerDataView
{
    [SerializeField] private TMP_Text m_userName;
    [SerializeField] private Image m_userIcon;

    Button m_button;
    PlayerData m_player;

    private void Awake()
    {
        m_button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        PlayerDataViews.Bind(this);
        if (m_button != null)
        {
            m_button.onClick.AddListener(OnAvatarClicked);
        }
    }

    private void OnDisable()
    {
        if (m_button != null)
        {
            m_button.onClick.RemoveListener(OnAvatarClicked);
        }
        PlayerDataViews.Unbind(this);
    }

    public void OnPlayerDataChanged(PlayerData data)
    {
        m_player = data;
        ApplyAsync(data).Forget();
    }

    private async UniTask ApplyAsync(PlayerData data)
    {
        if (data == null)
        {
            return;
        }

        m_userName.text = data.Nickname;
        if (data.AvatarId is int avatarId)
        {
            m_userIcon.sprite = await AddressableLoader.Instance.LoadSprite(AddressKeys.GetAvatarAddress(avatarId));
        }
    }

    void OnAvatarClicked()
    {
        OpenAvatarSelectAsync().Forget();
    }

    async UniTaskVoid OpenAvatarSelectAsync()
    {
        UIFrame frame = GetComponentInParent<UIFrame>();
        if (frame == null)
        {
            ALog.LogError("打开头像选择失败: 找不到 UIFrame", ALogCategories.UI);
            return;
        }

        PlayerData player = m_player ?? PlayerSession.Instance.CurrentPlayer;
        if (player == null)
        {
            ALog.LogWarning("打开头像选择失败: 无玩家数据", ALogCategories.UI);
            return;
        }

        if (!GameConfigManager.HasInstance)
        {
            ALog.LogWarning("打开头像选择失败: 配置未就绪", ALogCategories.UI);
            return;
        }

        GameConfigStore store = GameConfigManager.Instance.Store;
        var avatars = new List<AvatarItemData>();
        int selectedIndex = -1;
        IReadOnlyList<int> owned = player.OwnedAvatarIds;
        for (int i = 0; i < owned.Count; i++)
        {
            int id = owned[i];
            if (!store.TryGetAvatar(id, out AvatarConfig config) || !config.IsEnabled)
            {
                continue;
            }

            string address = string.IsNullOrEmpty(config.ResourceKey)
                ? AddressKeys.GetAvatarAddress(id)
                : config.ResourceKey;
            Sprite sprite = null;
            try
            {
                sprite = await AddressableLoader.Instance.LoadSprite(address);
            }
            catch (System.Exception exception)
            {
                ALog.LogWarning(
                    $"头像封面加载失败. Id={id}, Key={address}, {exception.Message}",
                    ALogCategories.UI);
            }

            if (player.AvatarId == id)
            {
                selectedIndex = avatars.Count;
            }

            avatars.Add(new AvatarItemData
            {
                Id = id,
                Name = config.Name,
                Sprite = sprite,
            });
        }

        ALog.Log($"打开头像选择. Count={avatars.Count}, Selected={selectedIndex}", ALogCategories.UI);
        frame.OpenWindow(
            AddressKeys.Prefab.AvatarSelectWindow,
            new AvatarSelectWindowProperties(avatars, selectedIndex));
    }
}
