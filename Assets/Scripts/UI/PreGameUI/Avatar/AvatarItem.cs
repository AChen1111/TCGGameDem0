using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>一行一个头像.预制体挂 LoopListViewItem2 和本脚本,供 GridListController 复用.</summary>
public class AvatarItem : MonoBehaviour, IRowItem<AvatarItemData>
{
    [SerializeField] Button m_BtnAll; // 整行点击热区,回传当前行下标
    [SerializeField] Image m_ImgMain; // 头像图,由 SetRowData 赋 Sprite
    [SerializeField] Graphic m_Ring; // 已拥有时的环绕光效
    [SerializeField] Graphic m_Fog; // 未拥有时的黑色迷雾
    [SerializeField] Color m_OwnedColor = new Color(0.24f, 1f, 0.45f, 1f);
    [SerializeField] Color m_SelectedColor = new Color(1f, 0.84f, 0.16f, 1f);

    int m_Index;
    bool m_Owned;
    Action<int> m_OnSelected;

    public int RowCardCount => 1;

    public void SetRowData(int rowIndex, List<AvatarItemData> allData, int selectedIndex, Action<int> onSelected)
    {
        m_Index = rowIndex;
        m_OnSelected = onSelected;
        AvatarItemData data = allData[rowIndex];
        m_ImgMain.sprite = data.Sprite;
        m_Owned = data.Owned;

        // 切 GameObject 而非 Graphic.enabled: 后者被 Inspector 上的节点勾选覆盖后会静默失效.
        m_Ring.gameObject.SetActive(m_Owned);
        m_Fog.gameObject.SetActive(!m_Owned);

        // 光环颜色只靠顶点色区分绿/黄,两种状态共用同一份材质,避免列表复用时产生材质实例.
        if (m_Owned)
        {
            m_Ring.color = rowIndex == selectedIndex ? m_SelectedColor : m_OwnedColor;
        }
    }

    void Awake()
    {
        m_BtnAll.onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        // 未拥有不可选中,拦在回调之前,避免污染列表的选中下标.
        if (!m_Owned)
        {
            ALog.Log($"头像不可选: Index={m_Index}, 原因=未拥有", ALogCategories.UI);
            return;
        }

        m_OnSelected?.Invoke(m_Index);
    }
}
