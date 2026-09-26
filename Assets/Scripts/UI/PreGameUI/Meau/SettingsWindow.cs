using System;
using AChen.Events;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsWindow : AWindowController
{
    // --tag_start: 自动生成--
    [SerializeField] Button m_BtnChangeLanguage;
    [SerializeField] Button m_BtnExit;
    [SerializeField] Button m_BtnClose;
    [SerializeField] Slider m_SldBGM;
    [SerializeField] Slider m_SldSFX;
    // --tag_end: 自动生成--

    bool m_isBusy;

    protected override void AddListeners()
    {
        m_BtnClose.onClick.AddListener(UI_Close);
        m_BtnChangeLanguage.onClick.AddListener(OnChangeLanguageClick);
        m_BtnExit.onClick.AddListener(OnExitClick);
    }

    protected override void RemoveListeners()
    {
        m_BtnClose.onClick.RemoveListener(UI_Close);
        m_BtnChangeLanguage.onClick.RemoveListener(OnChangeLanguageClick);
        m_BtnExit.onClick.RemoveListener(OnExitClick);
    }

    protected override void OnOpen()
    {
        m_isBusy = false;
        RefreshLanguageButton();
    }

    void RefreshLanguageButton()
    {
        string key = LocalizationService.CurrentLanguage == GameLanguage.English
            ? "ui.settings.language_zh"
            : "ui.settings.language_en";
        m_BtnChangeLanguage.GetComponentInChildren<TMP_Text>(true).Localized().SetKey(key);
    }

    void OnChangeLanguageClick()
    {
        if (m_isBusy || !IsOpened) return;
        Confirm("ui.settings.confirm_language", () => SwitchLanguageAsync().Forget());
    }

    async UniTaskVoid SwitchLanguageAsync()
    {
        if (m_isBusy || !IsOpened) return;

        GameLanguage next = LocalizationService.CurrentLanguage == GameLanguage.English
            ? GameLanguage.SimplifiedChinese
            : GameLanguage.English;
        m_isBusy = true;
        ALog.Log($"设置窗切换语言并重装大厅. Next={next}", ALogCategories.UI);
        SceneTransitionOverlay.Show();
        LocalizationService.SetLanguage(next);
        try
        {
            await SceneLoader.ReloadScene(AddressKeys.Scene.GameScene);
        }
        catch (Exception exception)
        {
            // 语言已写入偏好, 按钮改为显示新的下一语言.
            ALog.LogError($"设置窗切换语言后重装场景失败. Next={next}; Error={exception.Message}", ALogCategories.UI);
            SceneTransitionOverlay.Hide();
            if (this != null)
            {
                m_isBusy = false;
                RefreshLanguageButton();
            }
        }
    }

    void OnExitClick()
    {
        if (m_isBusy || !IsOpened) return;
        Confirm("ui.settings.confirm_logout", Logout);
    }

    void Logout()
    {
        if (m_isBusy || !IsOpened) return;
        ALog.Log("设置窗请求登出回登录", ALogCategories.UI);
        EventCenter.Dispatch(GameEvent.LogoutRequested);
    }
}
