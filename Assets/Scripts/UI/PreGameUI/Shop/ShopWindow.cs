using System;
using System.Threading;
using AChen.Events;
using AChen.Networking;
using AChen.Player;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

/// <summary>商城窗口. 页签与品类一一对应, 切换页签即用同一个列表重绑该品类的商品.</summary>
public class ShopWindow : AWindowController
{
    static readonly Color s_tabNormalColor = Color.white;
    static readonly Color s_tabSelectedColor = Color.yellow;

    [SerializeField] GridListController m_ListController;
    [SerializeField] Button m_CloseButton;
    [SerializeField] Button[] m_ChooseButtons;

    ShopCategory[] m_Categories;
    int m_SelectedChooseIndex;
    bool m_IsSwitching;
    bool m_IsPurchasing;
    bool m_IsShown;
    bool m_RefreshPending;
    bool m_AreItemsCurrent;
    CancellationTokenSource m_RefreshCancellation;
    GameConfigSnapshot m_ConfigSnapshot;

    protected override void Awake()
    {
        // 品类顺序与 m_ChooseButtons 一一对应, 新增品类在这里加一项并在预制体上加一个页签按钮.
        m_Categories = new ShopCategory[]
        {
            new CardPackShopCategory(),
            new CosmeticShopCategory("头像", ShopCatalogTypes.Avatar, AddressKeys.Prefab.AvatarShopItemRowPrefab),
            new CosmeticShopCategory("壁纸", ShopCatalogTypes.Wallpaper, AddressKeys.Prefab.WallpaperShopItemRowPrefab),
        };
        m_ConfigSnapshot = GameConfigManager.HasInstance ? GameConfigManager.Instance.Store.Snapshot : null;
        base.Awake();
    }

    protected override void OnOpen()
    {
        m_IsPurchasing = false;
        m_IsShown = true;
        SwitchCategory(0);
    }

    protected override void OnResume()
    {
        m_IsShown = true;
        SwitchCategory(m_SelectedChooseIndex);
    }

    protected override void OnHide()
    {
        PauseRefresh();
    }

    protected override void OnClose()
    {
        PauseRefresh();
    }

    void PauseRefresh()
    {
        m_IsShown = false;
        m_RefreshCancellation?.Cancel();
    }

    protected override void AddListeners()
    {
        EventCenter.AddListener(GameEvent.PlayerOwnedAvatarsChanged, OnOwnedAvatarsChanged);
        EventCenter.AddListener(GameEvent.PlayerOwnedWallpapersChanged, OnOwnedWallpapersChanged);
        EventCenter.AddListener(GameEvent.GameConfigChanged, OnConfigChanged);
        m_CloseButton.onClick.AddListener(UI_Close);
        for (int i = 0; i < m_ChooseButtons.Length; i++)
        {
            int index = i;
            Button button = m_ChooseButtons[i];
            button.transition = Selectable.Transition.None;
            button.onClick.AddListener(() => SwitchCategory(index));
        }
    }

    protected override void RemoveListeners()
    {
        EventCenter.RemoveListener(GameEvent.PlayerOwnedAvatarsChanged, OnOwnedAvatarsChanged);
        EventCenter.RemoveListener(GameEvent.PlayerOwnedWallpapersChanged, OnOwnedWallpapersChanged);
        EventCenter.RemoveListener(GameEvent.GameConfigChanged, OnConfigChanged);
        PauseRefresh();
        m_CloseButton.onClick.RemoveListener(UI_Close);
        for (int i = 0; i < m_ChooseButtons.Length; i++)
        {
            m_ChooseButtons[i].onClick.RemoveAllListeners();
        }
    }

    void OnOwnedAvatarsChanged(PlayerData player) => OnOwnedItemsChanged(ShopCatalogTypes.Avatar, player);

    void OnOwnedWallpapersChanged(PlayerData player) => OnOwnedItemsChanged(ShopCatalogTypes.Wallpaper, player);

    void OnOwnedItemsChanged(string catalogType, PlayerData player)
    {
        if (player == null || m_Categories[m_SelectedChooseIndex].CatalogType == catalogType) RequestRefresh();
    }

    void OnConfigChanged(GameConfigSnapshot snapshot, bool isStale)
    {
        // 只有过期状态变化时仍是同一份快照, 无需重新加载商品图片.
        if (ReferenceEquals(m_ConfigSnapshot, snapshot)) return;

        m_ConfigSnapshot = snapshot;
        RequestRefresh();
    }

    void SwitchCategory(int index)
    {
        m_SelectedChooseIndex = index;
        ApplyChooseHighlight(index);
        RequestRefresh();
    }

    void RequestRefresh()
    {
        m_RefreshPending = true;
        m_AreItemsCurrent = false;
        m_RefreshCancellation?.Cancel();
        TryRefresh();
    }

    void TryRefresh()
    {
        if (!m_IsShown || !m_RefreshPending || m_IsSwitching || m_IsPurchasing) return;

        RefreshCurrentCategoryAsync().Forget();
    }

    async UniTaskVoid RefreshCurrentCategoryAsync()
    {
        m_IsSwitching = true;
        try
        {
            // 购买结果的事件早于购买 await 返回, 保留 pending 后由购买结束继续刷新.
            while (m_IsShown && m_RefreshPending && !m_IsPurchasing)
            {
                m_RefreshPending = false;
                int index = m_SelectedChooseIndex;
                ShopCategory category = m_Categories[index];
                using (var cancellation = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy()))
                {
                    m_RefreshCancellation = cancellation;
                    try
                    {
                        if (!PlayerSession.HasInstance || PlayerSession.Instance.CurrentPlayer == null)
                        {
                            m_ListController.ClearList();
                            continue;
                        }
                        await category.BindAsync(m_ListController, OnShopItemClicked, cancellation.Token);
                        cancellation.Token.ThrowIfCancellationRequested();
                        m_AreItemsCurrent = true;
                        ALog.Log($"商城刷新品类成功: Index={index}, 品类={category.DisplayName}", ALogCategories.UI);
                    }
                    catch (OperationCanceledException)
                    {
                    }
                    catch (Exception exception)
                    {
                        ALog.LogError(
                            $"商城刷新品类失败: Index={index}, 品类={category.DisplayName}, 原因={exception.Message}",
                            ALogCategories.UI);
                    }
                    finally
                    {
                        m_RefreshCancellation = null;
                    }
                }
            }
        }
        finally
        {
            m_IsSwitching = false;
        }
    }

    void ApplyChooseHighlight(int selectedIndex)
    {
        for (int i = 0; i < m_ChooseButtons.Length; i++)
        {
            Image image = m_ChooseButtons[i].GetComponent<Image>();
            image.color = i == selectedIndex ? s_tabSelectedColor : s_tabNormalColor;
        }
    }

    void OnShopItemClicked(int index)
    {
        if (m_IsSwitching || m_IsPurchasing || !m_AreItemsCurrent) return;
        ShopCategory category = m_Categories[m_SelectedChooseIndex];
        if (!category.TryGetPurchaseTarget(index, out ShopPurchaseTarget target))
        {
            return;
        }

        if (target.Owned)
        {
            ShowMessage("已拥有");
            return;
        }

        ALog.Log(
            $"商城购买确认: 品类={target.CatalogType}, Id={target.Id}, Name={target.Name}, 价格={target.PriceGold}",
            ALogCategories.UI);
        RequestOpenWindow(
            AddressKeys.Prefab.ChooseWindow,
            new ChooseWindowProperties(
                $"确认花费 {target.PriceGold} 金币购买{target.Name}?",
                () => PurchaseAsync(target).Forget(),
                null));
    }

    async UniTaskVoid PurchaseAsync(ShopPurchaseTarget target)
    {
        if (m_IsPurchasing || !IsOpened) return;

        PlayerData player = PlayerSession.HasInstance ? PlayerSession.Instance.CurrentPlayer : null;
        if (player == null)
        {
            ShowMessage("购买失败");
            return;
        }

        if (player.Gold < target.PriceGold)
        {
            ALog.LogWarning(
                $"商城购买中止: 品类={target.CatalogType}, Id={target.Id}, Name={target.Name}, 价格={target.PriceGold}, 余额={player.Gold}, 原因=金币不足",
                ALogCategories.UI);
            ShowMessage("金币不足");
            return;
        }

        m_IsPurchasing = true;
        await RunGuardedAsync(
            token => PlayerSession.Instance.PurchaseShopItemAsync(target.CatalogType, target.Id, token),
            $"商城购买 {target.CatalogType}/{target.Id}",
            "购买失败，请稍后重试");
        if (this == null || !IsOpened) return;

        m_IsPurchasing = false;
        TryRefresh();
    }
}
