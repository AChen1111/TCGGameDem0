using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AvatarSelectWindow : AWindowController
{
    [SerializeField] AvatarListController m_AvatarListController;
    [SerializeField] Button m_BtnConfirm;

    protected override void AddListeners()
    {
        if (m_BtnConfirm != null)
        {
            m_BtnConfirm.onClick.AddListener(OnConfirmClick);
        }
    }

    protected override void RemoveListeners()
    {
        if (m_BtnConfirm != null)
        {
            m_BtnConfirm.onClick.RemoveListener(OnConfirmClick);
        }
    }

    public void Init(List<AvatarItemData> avatars, int selectedIndex = -1)
    {
        if (m_AvatarListController == null)
        {
            ALog.LogWarning("头像选择窗口未绑定滑动列表", ALogCategories.UI);
            return;
        }

        m_AvatarListController.InitList(avatars, selectedIndex);
    }

    void OnConfirmClick()
    {
        AvatarItemData selected = m_AvatarListController != null ? m_AvatarListController.Selected : null;
        if (selected == null)
        {
            ALog.LogWarning("确认更换头像失败: 未选中头像", ALogCategories.UI);
            return;
        }

        ALog.Log($"确认头像: Id={selected.Id}, Name={selected.Name}", ALogCategories.UI);
    }
}
