using Cysharp.Threading.Tasks;
using UnityEngine;
using AChen.Duel.Presentation;

/// <summary>
/// 业务场景入口: 加载该场景的 UISettings 创建 UIFrame, 并打开首个界面.
/// 会话恢复与全局流程订阅由 Init 场景的 SingletonManager 完成.
/// </summary>
public sealed class SceneEntry : MonoBehaviour
{
    public enum Kind
    {
        Login,
        Lobby,
        Battle
    }

    [SerializeField] Kind m_kind = Kind.Login;
    [SerializeField] BattleSceneController m_battle;

    UIFrame m_uiFrame;

    async UniTaskVoid Start()
    {
        string settingsAddress = m_kind == Kind.Battle ? AddressKeys.UISettings.BattleUISetting : m_kind == Kind.Lobby ? AddressKeys.UISettings.UISetting : AddressKeys.UISettings.LogInSetting;
        UISettings settings = await AddressableLoader.Instance.LoadUISettings(settingsAddress);
        if (settings == null)
        {
            SceneTransitionOverlay.Hide();
            ALog.LogError($"场景 UI 配置缺失. Kind={m_kind}; Address={settingsAddress}", ALogCategories.UI);
            return;
        }

        m_uiFrame = settings.CreateUIInstance();
        if (m_kind == Kind.Battle)
        {
            await m_battle.InitializeAsync(m_uiFrame);
        }
        else if (m_kind == Kind.Lobby)
        {
            m_uiFrame.ShowPanel(AddressKeys.Prefab.PreGameUIPanel);
            AChen.Duel.Client.DuelClientSession.Instance.OnLobbyReady(m_uiFrame);
        }
        else
        {
            m_uiFrame.OpenWindow(AddressKeys.Prefab.LogInWindow);
        }
#if (DEVELOPMENT_BUILD && UNITY_STANDALONE) || UNITY_EDITOR
        AChen.Duel.ClientTesting.DuelDevelopmentDriver.Attach(m_uiFrame);
#endif
    }
}
