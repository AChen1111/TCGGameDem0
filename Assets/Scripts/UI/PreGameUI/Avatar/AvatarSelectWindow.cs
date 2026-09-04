using System;
using System.Collections.Generic;
using AChen.Networking;
using AChen.Player;
using Cysharp.Threading.Tasks;
using LitMotion;
using UnityEngine;
using UnityEngine.UI;

/// <summary>开窗参数.外部 OpenWindow(id, new AvatarSelectWindowProperties(list)).</summary>
public sealed class AvatarSelectWindowProperties : IWindowProperties
{
    public List<AvatarItemData> Avatars { get; }
    public int SelectedIndex { get; }

    public AvatarSelectWindowProperties(List<AvatarItemData> avatars, int selectedIndex = -1)
    {
        Avatars = avatars;
        SelectedIndex = selectedIndex;
    }
}

/// <summary>头像选择窗口.OnOpen 读取 Properties 填列表,确认按钮取当前选中项.</summary>
public class AvatarSelectWindow : AWindowController<AvatarSelectWindowProperties>
{
    [SerializeField] AvatarListController m_AvatarListController;
    [SerializeField] Button m_BtnConfirm;
    [SerializeField] Button m_CloseButton;
    [SerializeField] float m_ScrollToSelectedDuration = 0.25f;
    [SerializeField] Ease m_ScrollToSelectedEase = Ease.InOutCubic;

    protected override void AddListeners()
    {
        m_BtnConfirm.onClick.AddListener(OnConfirmClick);
        m_CloseButton.onClick.AddListener(OnCloseButtonClicked);
    }

    protected override void RemoveListeners()
    {
        m_BtnConfirm.onClick.RemoveListener(OnConfirmClick);
        m_CloseButton.onClick.RemoveListener(OnCloseButtonClicked);
    }

    protected override void OnOpen()
    {
        BindList();
    }

    protected override void OnResume()
    {
        BindList();
    }

    void BindList()
    {
        BindListAsync().Forget();
    }

    async UniTask BindListAsync()
    {
        await m_AvatarListController.InitList(Properties.Avatars, null, Properties.SelectedIndex);
        // 等列表首帧把可视行创建出来,再判断预选项是否在视口外.
        await UniTask.Yield();
        if (m_AvatarListController == null) return;
        m_AvatarListController.MoveToSelectedIfHidden(m_ScrollToSelectedDuration, m_ScrollToSelectedEase);
    }

    void OnCloseButtonClicked()
    {
        UI_Close();
    }

    void OnConfirmClick()
    {
        ConfirmAsync().Forget();
    }

    async UniTaskVoid ConfirmAsync()
    {
        int index = m_AvatarListController.SelectedIndex;
        List<AvatarItemData> avatars = Properties.Avatars;
        if (avatars == null || index < 0 || index >= avatars.Count)
        {
            ALog.LogWarning("确认头像失败: 未选中", ALogCategories.UI);
            return;
        }

        AvatarItemData selected = avatars[index];
        PlayerData player = PlayerSession.Instance.CurrentPlayer;
        if (player == null)
        {
            ALog.LogError("确认头像失败: 无玩家数据", ALogCategories.UI);
            ShowMessage("更换头像失败");
            return;
        }

        try
        {
            await PlayerSession.Instance.UpdatePlayerProfileAsync(
                player.Nickname,
                selected.Id,
                player.BackgroundId,
                player.Revision,
                this.GetCancellationTokenOnDestroy());
            ALog.Log($"确认头像成功: Id={selected.Id}, Name={selected.Name}", ALogCategories.UI);
            UI_Close();
        }
        catch (BackendApiException exception)
        {
            ALog.LogError(
                $"确认头像失败. Code={exception.Code}; Status={exception.StatusCode}",
                ALogCategories.UI);
            ShowMessage("更换头像失败");
        }
        catch (OperationCanceledException)
        {
        }
    }

    void ShowMessage(string message)
    {
        m_UIFrame.OpenWindow(
            AddressKeys.Prefab.MessageWindow,
            new MessageWindowProperties(message, 2f));
    }
}
