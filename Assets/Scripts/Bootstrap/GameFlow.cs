using System;
using System.Threading;
using AChen.Events;
using AChen.Networking;
using AChen.Player;
using Cysharp.Threading.Tasks;

/// <summary>
/// 跨场景的游戏流程入口。
/// </summary>
public static class GameFlow
{
    static bool s_enteringLobby;

    public static void Initialize()
    {
        EventCenter.RemoveListener(GameEvent.PlayerLoggedIn, OnAuthenticated);
        EventCenter.RemoveListener(GameEvent.PlayerRegistered, OnAuthenticated);
        EventCenter.RemoveListener(GameEvent.GameExitRequested, OnExitRequested);
        EventCenter.AddListener(GameEvent.PlayerLoggedIn, OnAuthenticated);
        EventCenter.AddListener(GameEvent.PlayerRegistered, OnAuthenticated);
        EventCenter.AddListener(GameEvent.GameExitRequested, OnExitRequested);
        SceneTransitionOverlay.Initialize();
    }

    [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetState() => s_enteringLobby = false;

    static void OnExitRequested()
    {
        ALog.Log("收到退出请求, 结束游戏.", ALogCategories.UI);
        UnityEngine.Application.Quit();
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
