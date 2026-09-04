using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using LitMotion;
using SuperScrollView;
using UnityEngine;
using UnityEngine.UI;

public class GridListController : MonoBehaviour
{
    [SerializeField] private LoopListView2 loopListView;

    private bool mIsInited;
    private int mSelectedIndex = -1;
    private int mRowCardCount = 1;
    private Action<int> mOnSelectedCallback;
    private Func<LoopListView2, int, LoopListViewItem2> mOnGetItemHandler;
    private MotionHandle m_MoveToSelectedHandle;
    protected virtual string key { get; set; }
    public int SelectedIndex => mSelectedIndex;

    public async UniTask InitList<TData>(
        List<TData> dataList,
        Action<int> onSelected = null,
        int selectedIndex = -1)
    {
        CancelMoveToSelected();
        mOnSelectedCallback = onSelected;
        mSelectedIndex = selectedIndex >= 0 && dataList != null && selectedIndex < dataList.Count
            ? selectedIndex
            : -1;

        GameObject prefab = await AddressableLoader.Instance.LoadPrefab(key);
        var rowItemComp = prefab.GetComponent<IRowItem<TData>>();
        int rowCardCount = rowItemComp != null ? rowItemComp.RowCardCount : 1;
        if (rowCardCount <= 0) rowCardCount = 1;
        mRowCardCount = rowCardCount;

        if (loopListView.GetItemPrefabConfData(prefab.name) == null)
        {
            loopListView.ItemPrefabDataList.Add(new ItemPrefabConfData
            {
                mItemPrefab = prefab
            });
        }

        string prefabName = prefab.name;
        int totalCount = dataList != null ? dataList.Count : 0;
        int rowCount = Mathf.CeilToInt((float)totalCount / rowCardCount);

        mOnGetItemHandler = (listView, rowIndex) =>
        {
            if (rowIndex < 0 || rowIndex >= Mathf.CeilToInt((float)(dataList != null ? dataList.Count : 0) / rowCardCount))
                return null;

            LoopListViewItem2 item = listView.NewListViewItem(prefabName);
            ButtonClickTween.EnsureOn(item.transform);
            var row = item.GetComponent<IRowItem<TData>>();
            row?.SetRowData(rowIndex, dataList, mSelectedIndex, OnCardSelected);
            return item;
        };

        if (!mIsInited)
        {
            var scrollRect = loopListView.GetComponent<ScrollRect>();
            if (scrollRect != null)
            {
                if (scrollRect.horizontalScrollbarVisibility == ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport)
                    scrollRect.horizontalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
                if (scrollRect.verticalScrollbarVisibility == ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport)
                    scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
            }

            loopListView.InitListView(rowCount, OnGetItemByIndex);
            mIsInited = true;
        }
        else
        {
            loopListView.SetListItemCount(rowCount, true);
            loopListView.RefreshAllShownItem();
        }
    }

    /// <summary>选中项可能在首屏外,仅当对应行未显示时滚过去. duration 为秒,ease 用 LitMotion.Ease,默认 InOutCubic.</summary>
    public void MoveToSelectedIfHidden(float duration = 0, Ease ease = Ease.InOutCubic)
    {
        CancelMoveToSelected();
        if (!mIsInited || mSelectedIndex < 0) return;
        int selectedRow = mSelectedIndex / mRowCardCount;
        if (loopListView.GetShownItemByItemIndex(selectedRow) != null) return;

        if (duration <= 0f)
        {
            loopListView.MovePanelToItemIndexImmediately(selectedRow, 0);
            return;
        }

        // SuperScrollView 自带 duration 是线性插值,这里用 LitMotion 驱动行下标才能配 Ease.
        float from = loopListView.GetFirstShownFloatItemIndexInViewPort();
        LoopListView2 list = loopListView;
        m_MoveToSelectedHandle = LMotion.Create(from, (float)selectedRow, duration)
            .WithEase(ease)
            .Bind(index =>
            {
                if (list == null) return;
                int itemIndex = Mathf.Max(0, Mathf.FloorToInt(index));
                list.MovePanelToItemIndexImmediately(itemIndex, 0);
            })
            .AddTo(this);
    }

    void CancelMoveToSelected()
    {
        m_MoveToSelectedHandle.TryCancel();
    }

    void OnDestroy()
    {
        CancelMoveToSelected();
    }

    private LoopListViewItem2 OnGetItemByIndex(LoopListView2 listView, int rowIndex)
    {
        return mOnGetItemHandler?.Invoke(listView, rowIndex);
    }

    private void OnCardSelected(int dataIndex)
    {
        if (mSelectedIndex == dataIndex) return;
        mSelectedIndex = dataIndex;
        mOnSelectedCallback?.Invoke(dataIndex);
        loopListView.RefreshAllShownItem();
    }
}
