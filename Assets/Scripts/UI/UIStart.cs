using UnityEngine;

public class UIStart : MonoBehaviour
{
    [SerializeField] private UISettings m_ui;
    private UIFrame m_uiFrame;
    void Start()
    {
        m_uiFrame = m_ui.CreateUIInstance();
        m_uiFrame.ShowPanel("PreGameUIPanel");
    }

}
