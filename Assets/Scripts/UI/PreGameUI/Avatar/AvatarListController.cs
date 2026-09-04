using System.Collections.Generic;
using SuperScrollView;
using UnityEngine;
using UnityEngine.UI;

/// <summary>一条头像数据,由窗口 Init 传入的 List 提供.</summary>
public class AvatarItemData
{
    public int Id;
    public string Name;
    public Sprite Sprite;
}

/// <summary>滑动列表里的单个头像格子.预制体上挂此脚本,并绑按钮和图.</summary>
public class AvatarItem : MonoBehaviour
{
    [SerializeField] Button m_BtnAll;
    [SerializeField] Image m_ImgMain;

    int m_Index;
    AvatarListController m_List;

    public void SetData(AvatarItemData data, int index, AvatarListController list)
    {
        m_Index = index;
        m_List = list;
        m_ImgMain.sprite = data.Sprite;
    }

    void Awake()
    {
        m_BtnAll.onClick.AddListener(() => m_List.Select(m_Index));
    }
}

/// <summary>头像滑动列表.Inspector 绑定 LoopListView2 和格子预制体后,调用 InitList 传入数据.</summary>
public class AvatarListController : MonoBehaviour
{
    [SerializeField] LoopListView2 m_LoopListView;
    [SerializeField] GameObject m_ItemPrefab;

    List<AvatarItemData> m_Data;
    bool m_Inited;
    int m_SelectedIndex = -1;

    public AvatarItemData Selected =>
        m_Data != null && m_SelectedIndex >= 0 && m_SelectedIndex < m_Data.Count ? m_Data[m_SelectedIndex] : null;

    public void InitList(List<AvatarItemData> avatars, int selectedIndex = -1)
    {
        m_Data = avatars;
        m_SelectedIndex = selectedIndex;

        // LoopListView2 只认已登记的预制体名,首次要把格子预制体塞进池.
        if (m_LoopListView.GetItemPrefabConfData(m_ItemPrefab.name) == null)
        {
            m_LoopListView.ItemPrefabDataList.Add(new ItemPrefabConfData { mItemPrefab = m_ItemPrefab });
        }

        if (!m_Inited)
        {
            // SuperScrollView 不支持 AutoHideAndExpandViewport,否则 Init 会直接报错.
            var scrollRect = m_LoopListView.GetComponent<ScrollRect>();
            if (scrollRect.verticalScrollbarVisibility == ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport)
            {
                scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
            }

            m_LoopListView.InitListView(m_Data.Count, OnGetItemByIndex);
            m_Inited = true;
            return;
        }

        m_LoopListView.SetListItemCount(m_Data.Count, true);
        m_LoopListView.RefreshAllShownItem();
    }

    public void Select(int index)
    {
        m_SelectedIndex = index;
        m_LoopListView.RefreshAllShownItem();
    }

    // 可视区域内回收复用格子时由 LoopListView2 回调,按 index 填数据.
    LoopListViewItem2 OnGetItemByIndex(LoopListView2 listView, int index)
    {
        if (index < 0 || index >= m_Data.Count)
        {
            return null;
        }

        LoopListViewItem2 item = listView.NewListViewItem(m_ItemPrefab.name);
        item.GetComponent<AvatarItem>().SetData(m_Data[index], index, this);
        return item;
    }
}
