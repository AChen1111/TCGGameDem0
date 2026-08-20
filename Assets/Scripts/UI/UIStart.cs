using Cysharp.Threading.Tasks;
using UnityEngine;

public class UIStart : MonoBehaviour
{
    UIFrame m_uiFrame;

    async UniTaskVoid Start()
    {
        UISettings ui = await AddressableLoader.Instance.LoadUISettings(AddressKeys.UISettings.PreGameSceneUI);
        m_uiFrame = ui.CreateUIInstance();
        m_uiFrame.ShowPanel(AddressKeys.Prefab.PreGameUIPanel);
    }
}
