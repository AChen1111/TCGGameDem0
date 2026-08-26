using Cysharp.Threading.Tasks;
using UnityEngine;

public class LogInStart : MonoBehaviour
{
    UIFrame m_uiFrame;

    async UniTaskVoid Start()
    {
        UISettings loginSettings = await AddressableLoader.Instance.LoadUISettings(
            AddressKeys.UISettings.LogInSetting);
        if (loginSettings == null)
        {
            ALog.LogError("LogInSetting is null", ALogCategories.UI);
            return;
        }

        m_uiFrame = loginSettings.CreateUIInstance();
        m_uiFrame.OpenWindow(AddressKeys.Prefab.LogInWindow);
    }
}
