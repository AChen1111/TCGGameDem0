using AChen.Events;
using AChen.Player;
using TMPro;
using UnityEngine;

public sealed class PlayerUrView : MonoBehaviour
{
    [SerializeField] TMP_Text m_Amount;
    void OnEnable()
    {
        EventCenter.AddListener(GameEvent.PlayerUrChanged, OnChanged);
        OnChanged(PlayerSession.Instance.CurrentPlayer?.Ur);
    }
    void OnDisable() => EventCenter.RemoveListener(GameEvent.PlayerUrChanged, OnChanged);
    void OnChanged(long? amount) => m_Amount.text = amount.HasValue ? amount.Value.ToString("N0") : string.Empty;
    public void Show(long amount) => m_Amount.text = amount.ToString("N0");
}
