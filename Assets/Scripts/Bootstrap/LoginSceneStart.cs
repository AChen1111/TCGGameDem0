using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 登录场景入口：创建登录界面,会话恢复由 Init 场景完成。
/// </summary>
public class LoginSceneStart : MonoBehaviour
{
    UIFrame m_uiFrame;

    async UniTaskVoid Start()
    {
        UISettings loginSettings = await AddressableLoader.Instance.LoadUISettings(
            AddressKeys.UISettings.LogInSetting);
        if (loginSettings == null)
        {
            SceneTransitionOverlay.Hide();
            ALog.LogError("LogInSetting is null", ALogCategories.UI);
            return;
        }

        m_uiFrame = loginSettings.CreateUIInstance();
        m_uiFrame.OpenWindow(AddressKeys.Prefab.LogInWindow);
    }
}
