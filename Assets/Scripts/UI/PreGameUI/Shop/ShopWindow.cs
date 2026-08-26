using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AChen.Networking;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class ShopWindow : AWindowController
{
    [SerializeField] private CardPackListController m_CardPackListController;
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
        RefreshAsync(true, this.GetCancellationTokenOnDestroy()).Forget();
    }

    protected override void OnResume()
    {
        RefreshAsync(true, this.GetCancellationTokenOnDestroy()).Forget();
    }

    void OnConfigChanged(GameConfigSnapshot snapshot)
    {
        if (IsVisible)
        {
            RefreshAsync(false, this.GetCancellationTokenOnDestroy()).Forget();
        }
    }

    async UniTaskVoid RefreshAsync(bool checkBackend, CancellationToken cancellationToken)
    {
        int generation = ++m_RefreshGeneration;
        try
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
                        $"卡包 {config.Id} 封面加载失败：{config.CoverResourceKey}，{exception.Message}",
                        ALogCategories.UI);
                }

                if (generation != m_RefreshGeneration || cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                rebuilt.Add(new ShopCardItemData(
                    config.Id,
                    config.Title,
                    sprite,
                    config.PriceGold,
                    config.EndsAt,
                    i));
            }

            m_CardPackList.Clear();
            m_CardPackList.AddRange(rebuilt);
            int selectedIndex = m_SelectedCardPackId.HasValue
                ? m_CardPackList.FindIndex(value => value.Id == m_SelectedCardPackId.Value)
                : -1;
            if (selectedIndex < 0)
            {
                m_SelectedCardPackId = null;
            }

            await m_CardPackListController.InitList(m_CardPackList, OnSelected, selectedIndex);
        }
        catch (OperationCanceledException)
        {
            // Window is closing.
        }
        catch (Exception exception)
        {
            ALog.LogError("商店配置刷新失败：" + exception.Message, ALogCategories.UI);
        }
    }

    void OnSelected(int index)
    {
        m_SelectedCardPackId = index >= 0 && index < m_CardPackList.Count
            ? m_CardPackList[index].Id
            : null;
    }
}
