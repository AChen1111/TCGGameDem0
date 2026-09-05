using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 大厅场景入口：创建 UIFrame 并显示大厅主界面。
/// </summary>
public class LobbySceneStart : MonoBehaviour
{
    UIFrame m_uiFrame;

    async UniTaskVoid Start()
    {
        UISettings ui = await AddressableLoader.Instance.LoadUISettings(AddressKeys.UISettings.UISetting);
        if (ui == null)
        {
            ALog.LogError("UISetting is null", ALogCategories.UI);
            return;
        }

        m_uiFrame = ui.CreateUIInstance();
        m_uiFrame.ShowPanel(AddressKeys.Prefab.PreGameUIPanel);
    }
}
