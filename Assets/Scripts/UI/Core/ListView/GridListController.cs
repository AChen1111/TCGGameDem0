using System;
using System.Collections.Generic;
using System.Threading;
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
    private string mCurrentPrefabName;
    int m_bindVersion;
    public int SelectedIndex => mSelectedIndex;

    // 热更里泛型 async 实例方法会丢 <>4__this,所以异步加载和泛型绑定拆开
    public UniTask InitList<TData>(
        string rowPrefabKey,
        List<TData> dataList,
        Action<int> onSelected = null,
        int selectedIndex = -1,
        CancellationToken cancellationToken = default)
    {
        int version = ++m_bindVersion;
        CancelMoveToSelected();
        return LoadRowPrefabAsync(rowPrefabKey, cancellationToken).ContinueWith(prefab =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (this == null || version != m_bindVersion)
            {
                throw new OperationCanceledException();
            }

            mOnSelectedCallback = onSelected;
            mSelectedIndex = selectedIndex >= 0 && selectedIndex < dataList.Count
                ? selectedIndex
                : -1;
            BindList(prefab, dataList);
        });
    }

    async UniTask<GameObject> LoadRowPrefabAsync(string rowPrefabKey, CancellationToken cancellationToken)
    {
        return await AddressableLoader.Instance.LoadPrefab(rowPrefabKey)
            .AttachExternalCancellation(cancellationToken);
    }

    void BindList<TData>(GameObject prefab, List<TData> dataList)
    {
        var rowItemComp = prefab.GetComponent<IRowItem<TData>>()
            ?? throw new InvalidOperationException($"列表预制体缺少对应行类型: Prefab={prefab.name}; Data={typeof(TData).Name}");
        int rowCardCount = rowItemComp.RowCardCount;
        if (rowCardCount <= 0) throw new InvalidOperationException($"列表每行数量必须大于 0: Prefab={prefab.name}");
        mRowCardCount = rowCardCount;

        if (loopListView.GetItemPrefabConfData(prefab.name) == null)
        {
            // 横向列表的 PosY 由 StartPosOffset 决定,直接用预制体上的值
            float startPosOffset = 0f;
            if (loopListView.ArrangeType is ListItemArrangeType.LeftToRight or ListItemArrangeType.RightToLeft)
            {
                startPosOffset = prefab.GetComponent<RectTransform>().anchoredPosition.y;
            }

            loopListView.AddItemPrefab(new ItemPrefabConfData
            {
                mItemPrefab = prefab,
                mStartPosOffset = startPosOffset
            });
        }

        string prefabName = prefab.name;
        int totalCount = dataList.Count;
        int rowCount = Mathf.CeilToInt((float)totalCount / rowCardCount);

        mOnGetItemHandler = (listView, rowIndex) =>
        {
            if (rowIndex < 0 || rowIndex >= Mathf.CeilToInt((float)dataList.Count / rowCardCount))
                return null;

            LoopListViewItem2 item = listView.NewListViewItem(prefabName);
            var row = item.GetComponent<IRowItem<TData>>();
            row.SetRowData(rowIndex, dataList, mSelectedIndex, OnCardSelected);
            return item;
        };

        bool prefabChanged = mCurrentPrefabName != prefabName;
        mCurrentPrefabName = prefabName;

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
            return;
        }

        if (prefabChanged)
        {
            // 换了行预制体,先清空让旧类型的行全部回收,否则复用池会把旧行留在视口里;
            // 置 0 后再设新数量会从头重建,滚动位置一并归零
            loopListView.SetListItemCount(0, false);
            loopListView.SetListItemCount(rowCount, false);
            return;
        }

        loopListView.SetListItemCount(rowCount, true);
        loopListView.RefreshAllShownItem();
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

    public void ClearList()
    {
        m_bindVersion++;
        CancelMoveToSelected();
        mOnSelectedCallback = null;
        mOnGetItemHandler = null;
        mSelectedIndex = -1;
        if (mIsInited) loopListView.SetListItemCount(0, false);
    }

    void CancelMoveToSelected()
    {
        m_MoveToSelectedHandle.TryCancel();
    }

    void OnDestroy()
    {
        m_bindVersion++;
        CancelMoveToSelected();
    }

    void OnDisable()
    {
        m_bindVersion++;
        CancelMoveToSelected();
    }

    private LoopListViewItem2 OnGetItemByIndex(LoopListView2 listView, int rowIndex)
    {
        return mOnGetItemHandler?.Invoke(listView, rowIndex);
    }

    private void OnCardSelected(int dataIndex)
    {
        bool changed = mSelectedIndex != dataIndex;
        mSelectedIndex = dataIndex;
        mOnSelectedCallback?.Invoke(dataIndex);
        if (changed)
        {
            loopListView.RefreshAllShownItem();
        }
    }
}
