using System;
using System.Collections.Generic;
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
    const int DrawCount = 5;
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
        if (category.TryGetDrawTarget(index, out ShopDrawTarget drawTarget))
        {
            if (!CardPoolAddress.IsKnownDrawPool(drawTarget.PoolKey))
            {
                ShowMessage("该卡包未配置卡池");
                return;
            }

            ALog.Log(
                $"商城抽卡确认: Id={drawTarget.Id}; Title={drawTarget.Title}; Pool={drawTarget.PoolKey}; Count={DrawCount}",
                ALogCategories.UI);
            RequestOpenWindow(
                AddressKeys.Prefab.ChooseWindow,
                new ChooseWindowProperties(
                    $"确认抽取{drawTarget.Title}？",
                    () => DrawPackAsync(drawTarget).Forget(),
                    null));
            return;
        }

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

    async UniTaskVoid DrawPackAsync(ShopDrawTarget target)
    {
        if (m_IsPurchasing || !IsOpened) return;

        m_IsPurchasing = true;
        ALog.Log($"提交抽卡. Id={target.Id}; Title={target.Title}; Pool={target.PoolKey}; Count={DrawCount}", ALogCategories.Net);
        try
        {
            if (!PlayerSession.HasInstance || !PlayerSession.Instance.IsAuthenticated)
            {
                throw new BackendApiException(401, "INVALID_ACCESS_TOKEN", "登录状态已失效，请重新登录");
            }

            CardDrawResponse response = await PlayerSession.Instance.DrawCardsAsync(target.PoolKey, DrawCount);
            ALog.Log(
                $"抽卡成功. Pool={target.PoolKey}; Count={response.Results.Count}; Revision={response.Player.Revision}",
                ALogCategories.Net);
            List<CardPickViewData> cards = await LoadDrawCardsAsync(target.PoolKey, response.Results);
            if (this == null || !IsOpened)
            {
                return;
            }

            if (cards == null)
            {
                ShowMessage("卡图加载失败，请稍后重试");
                return;
            }

            RequestOpenWindow(AddressKeys.Prefab.CardPickWindow, new CardPickWindowProperty(cards));
        }
        catch (BackendApiException exception)
        {
            ALog.LogWarning(
                $"抽卡失败. Pool={target.PoolKey}; Count={DrawCount}; Code={exception.Code}; Status={exception.StatusCode}",
                ALogCategories.Net);
            ShowMessage(string.IsNullOrEmpty(exception.Message) ? "抽卡失败，请稍后重试" : exception.Message);
        }
        catch (Exception exception)
        {
            ALog.LogError($"抽卡异常. Pool={target.PoolKey}; 原因={exception.Message}", ALogCategories.Net);
            ShowMessage("抽卡失败，请稍后重试");
        }
        finally
        {
            if (this != null)
            {
                m_IsPurchasing = false;
            }
        }
    }

    async UniTask<List<CardPickViewData>> LoadDrawCardsAsync(string requestPoolKey, IReadOnlyList<CardDrawResult> results)
    {
        SceneTransitionOverlay.Show();
        try
        {
            var tasks = new UniTask<CardPickViewData>[results.Count];
            for (int i = 0; i < results.Count; i++)
            {
                tasks[i] = LoadDrawCardAsync(requestPoolKey, results[i]);
            }

            CardPickViewData[] cards = await UniTask.WhenAll(tasks);
            for (int i = 0; i < cards.Length; i++)
            {
                if (cards[i].cardTexture == null)
                {
                    ALog.LogWarning($"抽卡卡图缺失. CardId={results[i].CardId}; SourcePool={results[i].SourcePool}", ALogCategories.UI);
                    return null;
                }
            }

            return new List<CardPickViewData>(cards);
        }
        finally
        {
            SceneTransitionOverlay.Hide();
        }
    }

    static async UniTask<CardPickViewData> LoadDrawCardAsync(string requestPoolKey, CardDrawResult result)
    {
        string poolKey = string.IsNullOrEmpty(result.SourcePool) ? requestPoolKey : result.SourcePool;
        Texture texture = await CardPoolAddress.LoadCardTextureAsync(poolKey, result.CardId);
        return new CardPickViewData
        {
            cardId = result.CardId,
            cardShaderType = CardPickController.ToShaderType(result.Rarity),
            cardTexture = texture
        };
    }
}
