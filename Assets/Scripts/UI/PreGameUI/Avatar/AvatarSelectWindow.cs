using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

/// <summary>头像选择窗口.Init 传入 List,确认按钮读取列表当前选中项.</summary>
public class AvatarSelectWindow : AWindowController
{
    [SerializeField] AvatarListController m_AvatarListController;
    [SerializeField] Button m_BtnConfirm;

    List<AvatarItemData> m_Avatars;

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
        m_Avatars = avatars;
        m_AvatarListController.InitList(avatars, null, selectedIndex).Forget();
    }

    void OnConfirmClick()
    {
        int index = m_AvatarListController.SelectedIndex;
        if (m_Avatars == null || index < 0 || index >= m_Avatars.Count)
        {
            ALog.LogWarning("确认头像失败: 未选中", ALogCategories.UI);
            return;
        }

        AvatarItemData selected = m_Avatars[index];
        ALog.Log($"确认头像: Id={selected.Id}, Name={selected.Name}", ALogCategories.UI);
    }
}
