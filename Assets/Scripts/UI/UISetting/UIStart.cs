using Cysharp.Threading.Tasks;
using AChen.Networking;
using UnityEngine;

public class UIStart : MonoBehaviour
{
    UIFrame m_uiFrame;

    async UniTaskVoid Start()
    {
        // try
        // {
        //     await GameConfigManager.Instance.InitializeAsync(
        //         cancellationToken: this.GetCancellationTokenOnDestroy());
        // }
        // catch (System.Exception exception)
        // {
        //     ALog.LogError("游戏配置初始化失败，停止进入主界面：" + exception.Message, "Network");
        //     return;
        // }

        UISettings ui = await AddressableLoader.Instance.LoadUISettings(AddressKeys.UISettings.UISetting);
        if(ui == null)
        {
            ALog.LogError("UISetting is null");
            return;
        }
        m_uiFrame = ui.CreateUIInstance();
        m_uiFrame.ShowPanel(AddressKeys.Prefab.PreGameUIPanel);
    }
}
