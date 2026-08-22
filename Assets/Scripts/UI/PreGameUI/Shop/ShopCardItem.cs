using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ShopCardItemData
{
    public int id;//卡包id
    public string title;//上方标题
    public Sprite mainSprite;//卡包贴图
    public string value;//卡包价格
    public string remainTime;//剩余时间
    public int index;//在列表中的索引
    public ShopCardItemData(int id, string title, Sprite mainSprite, string value, string remainTime, int index)
    {
        this.id = id;
        this.title = title;
        this.mainSprite = mainSprite;
        this.value = value;
        this.remainTime = remainTime;
        this.index = index;
    }
}

public class ShopCardItem : MonoBehaviour
{
    // --tag_start: 自动生成--
    [SerializeField] Button m_BtnAll;
    [SerializeField] TextMeshProUGUI m_TxtTitile;
    [SerializeField] Image m_ImgMain;
    [SerializeField] TextMeshProUGUI m_TxtValue;
    [SerializeField] TextMeshProUGUI m_TxtRemainTime;
    // --tag_end: 自动生成--
    private int m_Index;
    private Action<int> m_OnSelected;
    public void SetData(ShopCardItemData data,bool isSelected,Action<int> onSelected)
    {
        m_TxtTitile.text = data.title;
        m_ImgMain.sprite = data.mainSprite;
        m_TxtValue.text = data.value;
        m_TxtRemainTime.text = data.remainTime;
        m_Index = data.index;

        //todo:被选择效果
        if(isSelected)
        {
        }

        m_OnSelected = onSelected;
    }

    private void Awake() {
        m_BtnAll.onClick.AddListener(OnSelectedClick);
    }
    private void Destroy() {
        m_BtnAll.onClick.RemoveListener(OnSelectedClick);
    }
    private void OnSelectedClick() {
        m_OnSelected?.Invoke(m_Index);//通知上层点击了哪个
    }
}
