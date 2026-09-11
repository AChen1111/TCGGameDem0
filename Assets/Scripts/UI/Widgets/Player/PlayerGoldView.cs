using AChen.Events;
using AChen.Player;
using TMPro;
using UnityEngine;

/// <summary>
/// 显示玩家金币，跟随玩家数据自动刷新。
/// </summary>
public class PlayerGoldView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_GoldText;

    private void OnEnable()
    {
        EventCenter.AddListener(GameEvent.PlayerGoldChanged, OnGoldChanged);
        OnGoldChanged(PlayerSession.HasInstance ? PlayerSession.Instance.CurrentPlayer?.Gold : null);
    }

    private void OnDisable()
    {
        EventCenter.RemoveListener(GameEvent.PlayerGoldChanged, OnGoldChanged);
    }

    void OnGoldChanged(long? value)
    {
        string gold = value?.ToString() ?? string.Empty;
        if (m_GoldText.text != gold)
        {
            m_GoldText.text = gold;
        }
    }
}
