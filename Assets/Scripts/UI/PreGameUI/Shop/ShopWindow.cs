using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AChen.Networking;
using Cysharp.Threading.Tasks;
using UnityEngine;

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

/// <summary>商城窗口.OnOpen 读取 Properties 填列表.</summary>
public class ShopWindow : AWindowController<ShopWindowProperties>
{
    [SerializeField] CardPackListController m_CardPackListController;
    readonly List<ShopCardItemData> m_CardPackList = new List<ShopCardItemData>();
    int? m_SelectedCardPackId;
    int m_RefreshGeneration;

    protected override void AddListeners()
    {
        GameConfigManager.Instance.Store.ConfigChanged += OnConfigChanged;
    }

    protected override void RemoveListeners()
    {
        if (GameConfigManager.HasInstance)
        {
            GameConfigManager.Instance.Store.ConfigChanged -= OnConfigChanged;
        }
    }

    protected override void OnOpen()
    {
        BindList(Properties.CardPacks, Properties.SelectedIndex);
    }

    protected override void OnResume()
    {
        RefreshFromConfigAsync(true, this.GetCancellationTokenOnDestroy()).Forget();
    }

    void OnConfigChanged(GameConfigSnapshot snapshot)
    {
        if (IsVisible)
        {
            RefreshFromConfigAsync(false, this.GetCancellationTokenOnDestroy()).Forget();
        }
    }

    void BindList(List<ShopCardItemData> cardPacks, int selectedIndex)
    {
        m_CardPackList.Clear();
        if (cardPacks != null)
        {
            m_CardPackList.AddRange(cardPacks);
        }

        int index = ResolveSelectedIndex(selectedIndex);
        m_CardPackListController.InitList(m_CardPackList, OnSelected, index).Forget();
    }

    int ResolveSelectedIndex(int selectedIndex)
    {
        if (m_SelectedCardPackId.HasValue)
        {
            int byId = m_CardPackList.FindIndex(value => value.Id == m_SelectedCardPackId.Value);
            if (byId >= 0)
            {
                return byId;
            }

            m_SelectedCardPackId = null;
        }

        if (selectedIndex >= 0 && selectedIndex < m_CardPackList.Count)
        {
            m_SelectedCardPackId = m_CardPackList[selectedIndex].Id;
            return selectedIndex;
        }

        return -1;
    }

    async UniTaskVoid RefreshFromConfigAsync(bool checkBackend, CancellationToken cancellationToken)
    {
        int generation = ++m_RefreshGeneration;
        try
        {
            List<ShopCardItemData> rebuilt = await LoadCardPacksAsync(checkBackend, cancellationToken);
            if (generation != m_RefreshGeneration || cancellationToken.IsCancellationRequested)
            {
                return;
            }

            BindList(rebuilt, -1);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception exception)
        {
            ALog.LogError("Shop configuration refresh failed: " + exception.Message, ALogCategories.UI);
        }
    }

    public static async UniTask<List<ShopCardItemData>> LoadCardPacksAsync(
        bool checkBackend,
        CancellationToken cancellationToken)
    {
        GameConfigManager manager = GameConfigManager.Instance;
        if (checkBackend)
        {
            await manager.EnsureFreshAsync(false, cancellationToken);
        }

        GameConfigStore store = manager.Store;
        CardPackConfig[] visible = store.Snapshot.CardPacks
            .Where(store.IsCardPackVisible)
            .OrderBy(value => value.SortOrder)
            .ThenBy(value => value.Id)
            .ToArray();
        var rebuilt = new List<ShopCardItemData>(visible.Length);
        for (int i = 0; i < visible.Length; i++)
        {
            CardPackConfig config = visible[i];
            Sprite sprite = null;
            try
            {
                sprite = await AddressableLoader.Instance.LoadSprite(config.CoverResourceKey);
            }
            catch (Exception exception)
            {
                ALog.LogWarning(
                    $"Card pack {config.Id} cover failed to load: {config.CoverResourceKey}, {exception.Message}",
                    ALogCategories.UI);
            }

            cancellationToken.ThrowIfCancellationRequested();
            rebuilt.Add(new ShopCardItemData(
                config.Id,
                config.Title,
                sprite,
                config.PriceGold,
                config.EndsAt,
                i));
        }

        return rebuilt;
    }

    void OnSelected(int index)
    {
        m_SelectedCardPackId = index >= 0 && index < m_CardPackList.Count
            ? m_CardPackList[index].Id
            : null;
    }
}
