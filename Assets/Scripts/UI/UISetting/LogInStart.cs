using Cysharp.Threading.Tasks;
using System;
using AChen.Networking;
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
            SceneTransitionOverlay.Hide();
            ALog.LogError("LogInSetting is null", ALogCategories.UI);
            return;
        }

        m_uiFrame = loginSettings.CreateUIInstance();
        try
        {
            if (await PlayerSession.Instance.TryRestoreSessionAsync(this.GetCancellationTokenOnDestroy()))
            {
                ALog.Log("自动登录成功并恢复玩家会话.", ALogCategories.Net);
                SceneTransitionOverlay.Show();
                try
                {
                    await SceneLoader.LoadScene(AddressKeys.Scene.GameScene);
                    return;
                }
                catch (Exception exception)
                {
                    SceneTransitionOverlay.Hide();
                    ALog.LogError(
                        "自动登录后加载大厅场景失败: " + exception.Message,
                        ALogCategories.UI);
                }
            }
        }
        catch (BackendApiException exception)
        {
            ALog.LogWarning(
                $"自动登录失败，显示登录界面. Code={exception.Code}; Status={exception.StatusCode}",
                ALogCategories.Net);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        m_uiFrame.OpenWindow(AddressKeys.Prefab.LogInWindow);
    }
}
