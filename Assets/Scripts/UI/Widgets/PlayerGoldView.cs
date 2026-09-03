using AChen.Networking;
using TMPro;
using UnityEngine;

/// <summary>
/// 显示玩家金币，跟随玩家数据自动刷新。
/// </summary>
public class PlayerGoldView : MonoBehaviour, IPlayerDataView
{
    [SerializeField] private TextMeshProUGUI m_GoldText;

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
        if (data == null)
        {
            return;
        }

        m_GoldText.text = data.Gold.ToString();
    }
}
