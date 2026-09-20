using System;
using System.Threading;
using AChen.Events;
using AChen.Networking;
using AChen.Player;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 跨场景的游戏流程入口。
/// </summary>
public static class GameFlow
{
    static bool s_enteringLobby;
    static bool s_returningToLogin;

    public static void Initialize()
    {
        EventCenter.RemoveListener(GameEvent.PlayerLoggedIn, OnAuthenticated);
        EventCenter.RemoveListener(GameEvent.PlayerRegistered, OnAuthenticated);
        EventCenter.RemoveListener(GameEvent.GameExitRequested, OnExitRequested);
        EventCenter.RemoveListener(GameEvent.LogoutRequested, OnLogoutRequested);
        EventCenter.AddListener(GameEvent.PlayerLoggedIn, OnAuthenticated);
        EventCenter.AddListener(GameEvent.PlayerRegistered, OnAuthenticated);
        EventCenter.AddListener(GameEvent.GameExitRequested, OnExitRequested);
        EventCenter.AddListener(GameEvent.LogoutRequested, OnLogoutRequested);
        SceneTransitionOverlay.Initialize();
    }

    [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetState()
    {
        s_enteringLobby = false;
        s_returningToLogin = false;
    }

    static async UniTask CheckContentAsync(CancellationToken token)
    {
        while (true)
        {
            try { await LocalGameConfiguration.CheckVersionAsync(token); return; }
            catch (OperationCanceledException) { throw; }
            catch (Exception exception)
            {
                ALog.LogWarning("登录内容检查失败: " + exception.Message, ALogCategories.Net);
                if (AChen.Configuration.ContentSession.RestartRequired)
                {
                    ContentUpdatePrompt.ShowRestart();
                    await UniTask.WaitUntil(() => false, cancellationToken: token);
                }
                await ContentUpdatePrompt.WaitForRetryAsync(token);
            }
        }
    }

    static void OnExitRequested()
    {
        ALog.Log("收到退出请求, 结束游戏.", ALogCategories.UI);
        Application.Quit();
    }

    static void OnLogoutRequested() => ReturnToLoginAsync().Forget();

    static async UniTaskVoid ReturnToLoginAsync()
    {
        if (s_returningToLogin) return;

        s_returningToLogin = true;
        ALog.Log("收到登出请求, 返回登录.", ALogCategories.UI);
        SceneTransitionOverlay.Show();
        try
        {
            await PlayerSession.Instance.LogoutAsync();
        }
        catch (Exception exception)
        {
            ALog.LogWarning($"登出接口失败, 仍返回登录. Error={exception.Message}", ALogCategories.Net);
        }

        try
        {
            await SceneLoader.LoadScene(AddressKeys.Scene.LogIn);
            s_returningToLogin = false;
        }
        catch (Exception exception)
        {
            ALog.LogError($"登出后加载登录场景失败. Error={exception.Message}", ALogCategories.UI);
            SceneTransitionOverlay.Hide();
            s_returningToLogin = false;
            TryShowLoginFailedMessage();
        }
    }

    static void TryShowLoginFailedMessage()
    {
        UIFrame frame = UnityEngine.Object.FindFirstObjectByType<UIFrame>();
        if (frame == null)
        {
            ALog.LogWarning("返回登录失败且找不到 UIFrame, 无法提示.", ALogCategories.UI);
            return;
        }

        frame.OpenWindow(
            AddressKeys.Prefab.MessageWindow,
            new MessageWindowProperties(new LocalizedMessage("err.enter_login_failed"), 2f));
    }

    static void OnAuthenticated(AuthUser user, PlayerData player)
    {
        EnterLobbyAsync().Forget();
    }

    public static async UniTask<string> GetStartupSceneAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (await PlayerSession.Instance.TryRestoreSessionAsync(cancellationToken))
            {
                await GameConfigManager.Instance.InitializeAsync(cancellationToken: cancellationToken);
                ALog.Log("Init 恢复玩家会话成功, 进入 GameScene.", ALogCategories.Net);
                return AddressKeys.Scene.GameScene;
            }

            ALog.Log("Init 未找到登录凭证, 进入 LogIn.", ALogCategories.Net);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (BackendApiException exception)
        {
            ALog.LogWarning(
                $"Init 恢复玩家会话失败, 进入 LogIn. Code={exception.Code}; Status={exception.StatusCode}",
                ALogCategories.Net);
        }
        catch (Exception exception)
        {
            ALog.LogWarning(
                $"Init 读取或恢复登录凭证异常, 进入 LogIn. Type={exception.GetType().Name}",
                ALogCategories.Net);
        }

        return AddressKeys.Scene.LogIn;
    }

    /// <summary>
    /// 登录状态通知驱动大厅流程; UI 通过流程事件更新忙碌状态与错误提示.
    /// </summary>
    static async UniTaskVoid EnterLobbyAsync()
    {
        if (s_enteringLobby) return;
        s_enteringLobby = true;
        EventCenter.Dispatch(GameEvent.LobbyEntering);
        try
        {
            await CheckContentAsync(SingletonManager.Instance.GetCancellationTokenOnDestroy());
            await GameConfigManager.Instance.InitializeAsync();
            await SceneLoader.LoadScene(AddressKeys.Scene.GameScene);
        }
        catch (Exception exception)
        {
            s_enteringLobby = false;
            ALog.LogError($"登录后进入大厅失败. Error={exception.Message}", ALogCategories.UI);
            EventCenter.Dispatch(GameEvent.LobbyEntryFailed, exception.Message);
            return;
        }
        s_enteringLobby = false;
        EventCenter.Dispatch(GameEvent.LobbyEntered);
    }
}
