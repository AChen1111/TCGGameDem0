using System;
using System.Collections.Generic;
using System.Threading;
using AChen.Networking;
using AChen.Player;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class CardPreviewWindowProperty : IWindowProperties
{
    public ShopDrawTarget Target { get; }
    public Action OnConfirm { get; }
    public Action OnCancel { get; }

    public CardPreviewWindowProperty(ShopDrawTarget target, Action onConfirm, Action onCancel)
    {
        Target = target;
        OnConfirm = onConfirm;
        OnCancel = onCancel;
    }
}

public class CardPreviewWindow : AWindowController<CardPreviewWindowProperty>
{
    // --tag_start: 自动生成--
    [SerializeField] TextMeshProUGUI m_TxtMessage;
    [SerializeField] Button m_BtnNo;
    [SerializeField] Button m_BtnOk;
    // --tag_end: 自动生成--
    [SerializeField] GridListController m_ListController;

    List<CardInUIData> m_Cards;
    bool m_Ready;
    int m_LoadVersion;

    protected override void AddListeners()
    {
        m_BtnOk.onClick.AddListener(OnBtnOkClicked);
        m_BtnNo.onClick.AddListener(OnBtnNoClicked);
    }

    protected override void RemoveListeners()
    {
        m_BtnOk.onClick.RemoveListener(OnBtnOkClicked);
        m_BtnNo.onClick.RemoveListener(OnBtnNoClicked);
    }

    protected override void OnOpen()
    {
        m_Ready = false;
        m_Cards = null;
        if (m_BtnOk != null)
        {
            m_BtnOk.interactable = false;
        }

        ShopDrawTarget target = Properties.Target;
        m_TxtMessage.Localized().SetKey("ui.shop.confirm_draw", new Dictionary<string, object>
        {
            ["gold"] = target.PriceGold,
            ["title"] = new LocalizedMessage("shop.pack." + target.Id.ToString("D2"))
        });
        ALog.Log(
            $"卡包预览打开. Id={target.Id}; Title={target.Title}; Pool={target.PoolKey}; PriceGold={target.PriceGold}",
            ALogCategories.UI);
        LoadPoolAsync(target).Forget();
    }

    protected override void OnClose()
    {
        m_LoadVersion++;
        m_Ready = false;
        m_Cards = null;
        m_ListController?.ClearList();
    }

    void OnBtnOkClicked()
    {
        if (!m_Ready)
        {
            return;
        }

        ShopDrawTarget target = Properties.Target;
        ALog.Log(
            $"卡包预览确认. Id={target.Id}; Title={target.Title}; Pool={target.PoolKey}; PriceGold={target.PriceGold}",
            ALogCategories.UI);
        UI_Close();
        Properties.OnConfirm?.Invoke();
    }

    void OnBtnNoClicked()
    {
        ShopDrawTarget target = Properties.Target;
        ALog.Log(
            $"卡包预览取消. Id={target.Id}; Title={target.Title}; Pool={target.PoolKey}",
            ALogCategories.UI);
        UI_Close();
        Properties.OnCancel?.Invoke();
    }

    void OnCardClicked(int index)
    {
        if (m_Cards == null || index < 0 || index >= m_Cards.Count)
        {
            return;
        }

        CardInUIData card = m_Cards[index];
        ALog.Log($"卡包预览点卡. CardId={card.CardId}; SourcePool={card.SourcePool}", ALogCategories.UI);
    }

    async UniTaskVoid LoadPoolAsync(ShopDrawTarget target)
    {
        int version = ++m_LoadVersion;
        CancellationToken token = ScreenToken;
        try
        {
            if (!PlayerSession.HasInstance || !PlayerSession.Instance.IsAuthenticated)
            {
                throw new BackendApiException(401, "INVALID_ACCESS_TOKEN", "登录状态已失效，请重新登录");
            }

            GachaPoolData pool = await PlayerSession.Instance.GetGachaPoolAsync(target.PoolKey, token);
            token.ThrowIfCancellationRequested();
            if (version != m_LoadVersion || this == null || !IsOpened)
            {
                return;
            }

            List<CardInUIData> cards = await LoadCardImagesAsync(pool, token);
            token.ThrowIfCancellationRequested();
            if (version != m_LoadVersion || this == null || !IsOpened)
            {
                return;
            }

            if (cards.Count == 0)
            {
                ALog.LogWarning($"卡包预览无卡图. Pool={target.PoolKey}", ALogCategories.UI);
                ShowMessage("err.card_art_load_failed");
                return;
            }

            m_Cards = cards;
            await m_ListController.InitList(AddressKeys.Prefab.CardPreviewRowPrefab, cards, OnCardClicked, cancellationToken: token);
            if (version != m_LoadVersion || this == null || !IsOpened)
            {
                return;
            }

            m_Ready = true;
            if (m_BtnOk != null)
            {
                m_BtnOk.interactable = true;
            }

            ALog.Log($"卡包预览加载成功. Pool={target.PoolKey}; Count={cards.Count}", ALogCategories.UI);
        }
        catch (OperationCanceledException)
        {
        }
        catch (BackendApiException exception)
        {
            if (version != m_LoadVersion || this == null || !IsOpened)
            {
                return;
            }

            ALog.LogWarning(
                $"卡包预览加载失败. Pool={target.PoolKey}; Code={exception.Code}; Status={exception.StatusCode}",
                ALogCategories.Net);
            ShowMessage(exception.UserMessage);
        }
        catch (Exception exception)
        {
            if (version != m_LoadVersion || this == null || !IsOpened)
            {
                return;
            }

            ALog.LogError($"卡包预览异常. Pool={target.PoolKey}; 原因={exception.Message}", ALogCategories.UI);
            ShowMessage("err.pool_load_failed");
        }
    }

    static async UniTask<List<CardInUIData>> LoadCardImagesAsync(GachaPoolData pool, CancellationToken token)
    {
        var tasks = new UniTask<CardInUIData>[pool.Cards.Count];
        for (int i = 0; i < pool.Cards.Count; i++)
        {
            tasks[i] = LoadCardImageAsync(pool.Cards[i], i);
        }

        CardInUIData[] loaded = await UniTask.WhenAll(tasks).AttachExternalCancellation(token);
        var cards = new List<CardInUIData>(loaded.Length);
        for (int i = 0; i < loaded.Length; i++)
        {
            CardInUIData card = loaded[i];
            if (card.Texture == null)
            {
                continue;
            }

            cards.Add(new CardInUIData(card.CardId, card.SourcePool, card.Texture, cards.Count));
        }

        return cards;
    }

    static async UniTask<CardInUIData> LoadCardImageAsync(GachaPoolCard card, int index)
    {
        Texture texture = await CardPoolAddress.LoadCardTextureAsync(card.SourcePool, card.CardId);
        if (texture == null)
        {
            ALog.LogWarning($"卡包预览卡图缺失. CardId={card.CardId}; SourcePool={card.SourcePool}", ALogCategories.UI);
        }

        return new CardInUIData(card.CardId, card.SourcePool, texture, index);
    }
}
