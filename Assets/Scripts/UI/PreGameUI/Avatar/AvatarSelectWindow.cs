using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>头像选择窗口.调用 Init 传入头像 List,点确认读取当前选中项.</summary>
public class AvatarSelectWindow : AWindowController
{
    [SerializeField] AvatarListController m_AvatarListController;
    [SerializeField] Button m_BtnConfirm;

    protected override void AddListeners()
    {
        m_BtnConfirm.onClick.AddListener(OnConfirmClick);
    }

    protected override void RemoveListeners()
    {
        m_BtnConfirm.onClick.RemoveListener(OnConfirmClick);
    }

    public void Init(List<AvatarItemData> avatars, int selectedIndex = -1)
    {
        m_AvatarListController.InitList(avatars, selectedIndex);
    }

    void OnConfirmClick()
    {
        AvatarItemData selected = m_AvatarListController.Selected;
        if (selected == null)
        {
            ALog.LogWarning("确认头像失败: 未选中", ALogCategories.UI);
            return;
        }

        ALog.Log($"确认头像: Id={selected.Id}, Name={selected.Name}", ALogCategories.UI);
    }
}
