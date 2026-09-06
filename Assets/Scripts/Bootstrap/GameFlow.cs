using System;
using System.Threading;
using AChen.Networking;
using AChen.Player;
using Cysharp.Threading.Tasks;

/// <summary>
/// 跨场景的游戏流程入口。
/// </summary>
public static class GameFlow
{
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
    /// 显示遮挡层并切换到大厅场景。失败时收回遮挡层并抛出，由调用方决定提示方式。
    /// </summary>
    public static async UniTask EnterLobbyAsync()
    {
        try
        {
            SceneTransitionOverlay.Show();
            await GameConfigManager.Instance.InitializeAsync();
            await SceneLoader.LoadScene(AddressKeys.Scene.GameScene);
        }
        catch
        {
            SceneTransitionOverlay.Hide();
            throw;
        }
    }
}
