using TMPro;
using UnityEngine;

public sealed class CardOverflowBadge : MonoBehaviour
{
    [SerializeField] GameObject m_Visual;
    [SerializeField] TMP_Text m_Amount;
    public void Show(long ur)
    {
        m_Visual.SetActive(ur > 0);
        m_Amount.text = ur.ToString("N0");
    }
}
