using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>一条头像数据,由窗口 Init 传入的 List 提供.</summary>
public class AvatarItemData
{
    public int Id;
    public string Name;
    public Sprite Sprite;
}

/// <summary>一行一个头像.预制体挂 LoopListViewItem2 和本脚本,供 GridListController 复用.</summary>
public class AvatarItem : MonoBehaviour, IRowItem<AvatarItemData>
{
    [SerializeField] Button m_BtnAll;
    [SerializeField] Image m_ImgMain;

    int m_Index;
    Action<int> m_OnSelected;

    public int RowCardCount => 1;

    public void SetRowData(int rowIndex, List<AvatarItemData> allData, int selectedIndex, Action<int> onSelected)
    {
        m_Index = rowIndex;
        m_OnSelected = onSelected;
        m_ImgMain.sprite = allData[rowIndex].Sprite;
    }

    void Awake()
    {
        m_BtnAll.onClick.AddListener(() => m_OnSelected?.Invoke(m_Index));
    }
}
