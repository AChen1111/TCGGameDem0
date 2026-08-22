using System.Collections.Generic;
using UnityEngine;
using SuperScrollView;

public class CardPackListController : MonoBehaviour
{
    [SerializeField] private LoopListView2 loopListView;
    private const int RowCardCount = 3;
    private const string RowPrefabName = "CardPackRowPrefab";

    private List<ShopCardItemData> mCardPackList = new List<ShopCardItemData>();
    private int mSelectedIndex = -1;

    public void InitList(List<ShopCardItemData> dataList)
    {
        mCardPackList = dataList;
        int rowCount = Mathf.CeilToInt((float)mCardPackList.Count / RowCardCount);

        // 初始化 LoopListView2
        loopListView.InitListView(rowCount, OnGetItemByIndex);
    }

    private LoopListViewItem2 OnGetItemByIndex(LoopListView2 listView, int rowIndex)
    {
        if (rowIndex < 0) return null;

        int rowCount = Mathf.CeilToInt((float)mCardPackList.Count / RowCardCount);
        if (rowIndex >= rowCount) return null;

        LoopListViewItem2 item = listView.NewListViewItem(RowPrefabName);
        CardPackRowItem rowItem = item.GetComponent<CardPackRowItem>();

        rowItem.SetRowData(rowIndex, mCardPackList, mSelectedIndex, OnCardSelected);
        return item;
    }

    private void OnCardSelected(int dataIndex)
    {
        if (mSelectedIndex == dataIndex) return;
        mSelectedIndex = dataIndex;
        // 刷新当前可见视图中的所有 Item，更新选中外框
        loopListView.RefreshAllShownItem();
    }
}
