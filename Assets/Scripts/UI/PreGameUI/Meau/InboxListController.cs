using System.Collections.Generic;
using System.Threading;
using AChen.Networking;
using Cysharp.Threading.Tasks;
using SuperScrollView;
using UnityEngine;
using UnityEngine.UI;

public class InboxListController : MonoBehaviour
{
    [SerializeField] LoopListView2 loopListView;

    bool m_inited;
    int m_bindVersion;
    List<InboxItemData> m_items;
    string m_requestPrefabName;
    string m_giftPrefabName;

    public async UniTask BindAsync(List<InboxItemData> items, CancellationToken cancellationToken)
    {
        int version = ++m_bindVersion;
        GameObject requestPrefab = await AddressableLoader.Instance.LoadPrefab(AddressKeys.Prefab.FriendApplyRowPrefab)
            .AttachExternalCancellation(cancellationToken);
        GameObject giftPrefab = await AddressableLoader.Instance.LoadPrefab(AddressKeys.Prefab.GiftApplyRowPrefab)
            .AttachExternalCancellation(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        if (this == null || version != m_bindVersion)
        {
            throw new System.OperationCanceledException();
        }

        Register(requestPrefab);
        Register(giftPrefab);
        m_requestPrefabName = requestPrefab.name;
        m_giftPrefabName = giftPrefab.name;
        m_items = items;

        if (!m_inited)
        {
            var scrollRect = loopListView.GetComponent<ScrollRect>();
            if (scrollRect != null)
            {
                if (scrollRect.horizontalScrollbarVisibility == ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport)
                    scrollRect.horizontalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
                if (scrollRect.verticalScrollbarVisibility == ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport)
                    scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
            }

            loopListView.InitListView(items.Count, OnGetItem);
            m_inited = true;
            return;
        }

        loopListView.SetListItemCount(items.Count, true);
        loopListView.RefreshAllShownItem();
    }

    void Register(GameObject prefab)
    {
        if (loopListView.GetItemPrefabConfData(prefab.name) != null)
        {
            return;
        }

        RectTransform itemRt = prefab.GetComponent<RectTransform>();
        loopListView.AddItemPrefab(new ItemPrefabConfData
        {
            mItemPrefab = prefab,
            mStartPosOffset = itemRt.rect.width * itemRt.pivot.x
        });
    }

    LoopListViewItem2 OnGetItem(LoopListView2 listView, int index)
    {
        if (m_items == null || index < 0 || index >= m_items.Count)
        {
            return null;
        }

        InboxItemData data = m_items[index];
        string prefabName = data.IsGift ? m_giftPrefabName : m_requestPrefabName;
        LoopListViewItem2 item = listView.NewListViewItem(prefabName);
        LoopListRowFit.FitWidth(listView, item);
        if (data.IsGift)
        {
            item.GetComponent<GiftRewardRowItem>().SetRowData(index, m_items, -1, null);
        }
        else
        {
            item.GetComponent<GiftRequestRowItem>().SetRowData(index, m_items, -1, null);
        }

        return item;
    }

    void OnRectTransformDimensionsChange()
    {
        if (!m_inited || loopListView == null)
        {
            return;
        }

        loopListView.RefreshAllShownItem();
    }
}
