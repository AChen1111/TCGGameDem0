using Cysharp.Threading.Tasks;
using System;
using AChen.Networking;
using AChen.Player;
using UnityEngine;

/// <summary>
/// 登录场景入口：优先恢复会话自动进入大厅，否则打开登录窗口。
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
        try
        {
            if (await PlayerSession.Instance.TryRestoreSessionAsync(this.GetCancellationTokenOnDestroy()))
            {
                ALog.Log("自动登录成功并恢复玩家会话.", ALogCategories.Net);
                try
                {
                    await GameFlow.EnterLobbyAsync();
                    return;
                }
                catch (Exception exception)
                {
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
