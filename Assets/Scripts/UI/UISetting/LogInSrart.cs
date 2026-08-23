using Cysharp.Threading.Tasks;
using UnityEngine;

public class LogInSrart : MonoBehaviour
{
    UIFrame m_uiFrame;
    public UISettings m_LogInSetting;
    void Start()
    {
        m_uiFrame = m_LogInSetting.CreateUIInstance();
        m_uiFrame.OpenWindow(AddressKeys.Prefab.LogInWindow);
    }
}
