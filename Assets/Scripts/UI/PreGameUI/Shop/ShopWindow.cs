using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

/// <summary>开窗参数.商品数据由品类自己加载,这里只指定初始页签.</summary>
public sealed class ShopWindowProperties : IWindowProperties
{
    public int InitialTabIndex { get; }

    public ShopWindowProperties(int initialTabIndex = 0)
    {
        InitialTabIndex = initialTabIndex;
    }
}

/// <summary>商城窗口.页签与品类一一对应,切换页签即用同一个列表重绑该品类的商品.</summary>
public class ShopWindow : AWindowController<ShopWindowProperties>
{
    [SerializeField] GridListController m_ListController;
    [SerializeField] Button m_CloseButton;
    [SerializeField] Button[] m_ChooseButtons;

    ShopCategory[] m_Categories;
    int m_SelectedChooseIndex;
    bool m_IsSwitching;

    protected override void Awake()
    {
        // 品类顺序与 m_ChooseButtons 一一对应,新增品类在这里加一项并在预制体上加一个页签按钮
        IShopDataSource dataSource = new FakeShopDataSource();
        m_Categories = new ShopCategory[]
        {
            new CardPackShopCategory(dataSource),
            new AvatarShopCategory(dataSource),
            new WallpaperShopCategory(dataSource),
        };
        base.Awake();
    }

    protected override void OnOpen()
    {
        SwitchCategory(Properties != null ? Properties.InitialTabIndex : 0);
    }

    protected override void OnResume()
    {
        SwitchCategory(m_SelectedChooseIndex);
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
        SwitchCategory(index);
    }

    void SwitchCategory(int index)
    {
        if (m_Categories == null || m_Categories.Length == 0) return;
        if (index < 0 || index >= m_Categories.Length)
        {
            index = 0;
        }

        // 数据加载是异步的,连点会让后发的绑定被先发的覆盖,这里直接丢弃切换中的点击
        if (m_IsSwitching) return;

        ApplyChooseHighlight(index);
        SwitchCategoryAsync(index).Forget();
    }

    async UniTaskVoid SwitchCategoryAsync(int index)
    {
        m_IsSwitching = true;
        ShopCategory category = m_Categories[index];
        try
        {
            await category.BindAsync(m_ListController).AttachExternalCancellation(this.GetCancellationTokenOnDestroy());
            ALog.Log($"商城切换品类成功: Index={index}, 品类={category.DisplayName}", ALogCategories.UI);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception exception)
        {
            ALog.LogError(
                $"商城切换品类失败: Index={index}, 品类={category.DisplayName}, 原因={exception.Message}",
                ALogCategories.UI);
        }
        finally
        {
            m_IsSwitching = false;
        }
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

            image.color = i == selectedIndex ? ShopItemColors.Selected : ShopItemColors.Normal;
        }
    }
}
