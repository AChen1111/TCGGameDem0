using System.Collections.Generic;
using SuperScrollView;
using UnityEngine;
using UnityEngine.UI;

public class AvatarItemData
{
    public int Id { get; }
    public string Name { get; }
    public Sprite Sprite { get; }

    public AvatarItemData(int id, string name, Sprite sprite)
    {
        Id = id;
        Name = name;
        Sprite = sprite;
    }
}

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
        if (m_ImgMain != null)
        {
            m_ImgMain.sprite = data != null ? data.Sprite : null;
        }
    }

    void Awake()
    {
        if (m_BtnAll != null)
        {
            m_BtnAll.onClick.AddListener(OnClick);
        }
    }

    void OnDestroy()
    {
        if (m_BtnAll != null)
        {
            m_BtnAll.onClick.RemoveListener(OnClick);
        }
    }

    void OnClick()
    {
        m_List?.Select(m_Index);
    }
}

public class AvatarListController : MonoBehaviour
{
    [SerializeField] LoopListView2 m_LoopListView;
    [SerializeField] GameObject m_ItemPrefab;

    List<AvatarItemData> m_Data;
    bool m_Inited;
    int m_SelectedIndex = -1;

    public int SelectedIndex => m_SelectedIndex;

    public AvatarItemData Selected =>
        m_Data != null && m_SelectedIndex >= 0 && m_SelectedIndex < m_Data.Count
            ? m_Data[m_SelectedIndex]
            : null;

    public void InitList(List<AvatarItemData> avatars, int selectedIndex = -1)
    {
        m_Data = avatars;
        m_SelectedIndex = avatars != null && selectedIndex >= 0 && selectedIndex < avatars.Count
            ? selectedIndex
            : -1;

        if (m_LoopListView == null || m_ItemPrefab == null)
        {
            ALog.LogWarning("头像滑动列表未绑定 LoopListView2 或格子预制体", ALogCategories.UI);
            return;
        }

        if (m_LoopListView.GetItemPrefabConfData(m_ItemPrefab.name) == null)
        {
            m_LoopListView.ItemPrefabDataList.Add(new ItemPrefabConfData
            {
                mItemPrefab = m_ItemPrefab
            });
        }

        int count = m_Data != null ? m_Data.Count : 0;
        if (!m_Inited)
        {
            var scrollRect = m_LoopListView.GetComponent<ScrollRect>();
            if (scrollRect != null)
            {
                if (scrollRect.horizontalScrollbarVisibility == ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport)
                {
                    scrollRect.horizontalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
                }

                if (scrollRect.verticalScrollbarVisibility == ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport)
                {
                    scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
                }
            }

            m_LoopListView.InitListView(count, OnGetItemByIndex);
            m_Inited = true;
            return;
        }

        m_LoopListView.SetListItemCount(count, true);
        m_LoopListView.RefreshAllShownItem();
    }

    public void Select(int index)
    {
        if (m_Data == null || index < 0 || index >= m_Data.Count || m_SelectedIndex == index)
        {
            return;
        }

        m_SelectedIndex = index;
        m_LoopListView.RefreshAllShownItem();
    }

    LoopListViewItem2 OnGetItemByIndex(LoopListView2 listView, int index)
    {
        if (m_Data == null || index < 0 || index >= m_Data.Count)
        {
            return null;
        }

        LoopListViewItem2 item = listView.NewListViewItem(m_ItemPrefab.name);
        var avatarItem = item.GetComponent<AvatarItem>();
        avatarItem?.SetData(m_Data[index], index, this);
        return item;
    }
}
