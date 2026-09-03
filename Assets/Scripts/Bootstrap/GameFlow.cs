using Cysharp.Threading.Tasks;

/// <summary>
/// 跨场景的游戏流程入口。
/// </summary>
public static class GameFlow
{
    /// <summary>
    /// 显示遮挡层并切换到大厅场景。失败时收回遮挡层并抛出，由调用方决定提示方式。
    /// </summary>
    public static async UniTask EnterLobbyAsync()
    {
        SceneTransitionOverlay.Show();
        try
        {
            await SceneLoader.LoadScene(AddressKeys.Scene.GameScene);
        }
        catch
        {
            SceneTransitionOverlay.Hide();
            throw;
        }
    }
}
