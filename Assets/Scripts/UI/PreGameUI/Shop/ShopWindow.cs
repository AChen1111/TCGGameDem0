using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

/// <summary>开窗参数.外部 OpenWindow(id, new ShopWindowProperties(list)).</summary>
public sealed class ShopWindowProperties : IWindowProperties
{
    public List<ShopCardItemData> CardPacks { get; }
    public int SelectedIndex { get; }

    public ShopWindowProperties(List<ShopCardItemData> cardPacks, int selectedIndex = -1)
    {
        CardPacks = cardPacks;
        SelectedIndex = selectedIndex;
    }
}

/// <summary>商城窗口.OnOpen 读取 Properties 填列表.卡包数据后续由新类提供.</summary>
public class ShopWindow : AWindowController<ShopWindowProperties>
{
    static readonly Color ChooseNormalColor = Color.white;
    static readonly Color ChooseHighlightColor = Color.yellow;

    [SerializeField] CardPackListController m_CardPackListController;
    [SerializeField] Button m_CloseButton;
    [SerializeField] Button[] m_ChooseButtons;

    int m_SelectedChooseIndex;

    protected override void OnOpen()
    {
        ApplyChooseHighlight(0);
        BindList();
    }

    protected override void OnResume()
    {
        ApplyChooseHighlight(m_SelectedChooseIndex);
        BindList();
    }

    protected override void AddListeners()
    {
        m_CloseButton.onClick.AddListener(OnCloseButtonClick);
        if (m_ChooseButtons == null) return;
        for (int i = 0; i < m_ChooseButtons.Length; i++)
        {
            int index = i;
            Button button = m_ChooseButtons[i];
            if (button == null) continue;
            button.transition = Selectable.Transition.None;
            button.onClick.AddListener(() => OnChooseButtonClick(index));
        }
    }

    protected override void RemoveListeners()
    {
        m_CloseButton.onClick.RemoveListener(OnCloseButtonClick);
        if (m_ChooseButtons == null) return;
        for (int i = 0; i < m_ChooseButtons.Length; i++)
        {
            if (m_ChooseButtons[i] != null)
            {
                m_ChooseButtons[i].onClick.RemoveAllListeners();
            }
        }
    }

    void OnCloseButtonClick()
    {
        UI_Close();
    }

    void OnChooseButtonClick(int index)
    {
        ApplyChooseHighlight(index);
        ALog.Log($"商城选项切换: Index={index}, Name={m_ChooseButtons[index].name}", ALogCategories.UI);
    }

    void ApplyChooseHighlight(int selectedIndex)
    {
        if (m_ChooseButtons == null || m_ChooseButtons.Length == 0) return;
        if (selectedIndex < 0 || selectedIndex >= m_ChooseButtons.Length)
        {
            selectedIndex = 0;
        }

        m_SelectedChooseIndex = selectedIndex;
        for (int i = 0; i < m_ChooseButtons.Length; i++)
        {
            Button button = m_ChooseButtons[i];
            if (button == null) continue;

            Image image = button.GetComponent<Image>();
            if (image == null)
            {
                ALog.LogWarning($"商城选项按钮缺少 Image: Index={i}, Name={button.name}", ALogCategories.UI);
                continue;
            }

            image.color = i == selectedIndex ? ChooseHighlightColor : ChooseNormalColor;
        }
    }

    void BindList()
    {
        m_CardPackListController.InitList(Properties.CardPacks, null, Properties.SelectedIndex).Forget();
    }
}
