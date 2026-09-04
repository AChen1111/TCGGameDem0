using System;
using System.Collections.Generic;
using AChen.Networking;
using AChen.Player;
using Cysharp.Threading.Tasks;
using LitMotion;
using Sirenix.OdinInspector;
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
    [SerializeField] Button m_BtnClose;
    [SerializeField] float m_ScrollToSelectedDuration = 0.25f;
    [SerializeField] Ease m_ScrollToSelectedEase = Ease.InOutCubic;

    protected override void AddListeners()
    {
        m_BtnConfirm.onClick.AddListener(OnConfirmClick);
        m_BtnClose.onClick.AddListener(OnCloseClick);
    }

    private void OnCloseClick()
    {
        UI_Close();
    }

    protected override void RemoveListeners()
    {
        m_BtnConfirm.onClick.RemoveListener(OnConfirmClick);
        m_BtnClose.onClick.RemoveListener(OnCloseClick);
    }

    protected override void OnOpen()
    {
        BindList();
    }

    protected override void OnResume()
    {
        BindList();
    }
    [Button("重试特效")]
    void BindList()
    {
        BindListAsync().Forget();
    }

    async UniTask BindListAsync()
    {
        List<AvatarItemData> avatars = Properties.Avatars;
        int selectedId = -1;
        if (avatars != null && Properties.SelectedIndex >= 0 && Properties.SelectedIndex < avatars.Count)
        {
            selectedId = avatars[Properties.SelectedIndex].Id;
        }

        SortOwnedThenId(avatars);
        int selected = selectedId < 0 || avatars == null
            ? -1
            : avatars.FindIndex(item => item.Id == selectedId);

        await m_AvatarListController.InitList(avatars, null, selected);
        // 等列表首帧把可视行创建出来,再判断预选项是否在视口外.
        await UniTask.Yield();
        if (m_AvatarListController == null) return;
        m_AvatarListController.MoveToSelectedIfHidden(m_ScrollToSelectedDuration, m_ScrollToSelectedEase);
    }

    // 已拥有在前,同组再按 Id
    static void SortOwnedThenId(List<AvatarItemData> avatars)
    {
        if (avatars == null || avatars.Count <= 1)
        {
            return;
        }

        avatars.Sort(static (a, b) =>
        {
            int owned = b.Owned.CompareTo(a.Owned);
            return owned != 0 ? owned : a.Id.CompareTo(b.Id);
        });
    }

    void OnConfirmClick()
    {
        SubmitAsync().Forget();
    }

    async UniTaskVoid SubmitAsync()
    {
        int index = m_AvatarListController.SelectedIndex;
        List<AvatarItemData> avatars = Properties.Avatars;
        if (avatars == null || index < 0 || index >= avatars.Count)
        {
            ALog.LogWarning("确认头像失败: 未选中", ALogCategories.UI);
            return;
        }

        AvatarItemData selected = avatars[index];
        // 列表点击已拦掉未拥有项,但预选下标由外部传入,这里兜一层避免确认到未拥有的头像.
        if (!selected.Owned)
        {
            ALog.LogWarning($"确认头像失败: Id={selected.Id}, 原因=未拥有", ALogCategories.UI);
            return;
        }

        PlayerData player = PlayerSession.HasInstance ? PlayerSession.Instance.CurrentPlayer : null;
        if (player == null)
        {
            ShowMessage("修改头像失败");
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
                $"修改头像失败. Code={exception.Code}; Status={exception.StatusCode}",
                ALogCategories.UI);
            ShowMessage(exception.Message);
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
