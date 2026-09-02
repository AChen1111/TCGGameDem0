using AChen.Networking;
using TMPro;
using UnityEngine;

public class GoldDataView : MonoBehaviour, IPlayerDataView
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
