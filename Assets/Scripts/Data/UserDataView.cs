using AChen.Networking;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UserDataView : MonoBehaviour, IPlayerDataView
{
    [SerializeField] private TMP_Text m_userName;
    [SerializeField] private Image m_userIcon;

    private void OnEnable()
    {
        PlayerDataViews.Bind(this);
    }

    private void OnDisable()
    {
        PlayerDataViews.Unbind(this);
    }

    public void OnPlayerDataChanged(PlayerData data)
    {
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
}
