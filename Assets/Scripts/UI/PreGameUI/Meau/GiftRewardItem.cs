using System;
using AChen.Networking;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>礼包奖励格: 金币图或卡图 + 右下角数量.</summary>
public class GiftRewardItem : MonoBehaviour
{
    [SerializeField] Image m_ImgGold;
    [SerializeField] RawImage m_ImgCard;
    [SerializeField] TMP_Text m_TxtCount;

    int m_Bind;

    public void SetGold(long count)
    {
        if (m_ImgGold != null)
        {
            m_ImgGold.gameObject.SetActive(true);
            ApplyNativeSize(m_ImgGold);
        }

        if (m_ImgCard != null) m_ImgCard.gameObject.SetActive(false);
        if (m_TxtCount != null) m_TxtCount.text = count.ToString();
    }

    public void SetCard(string cardId, long count)
    {
        if (m_ImgGold != null) m_ImgGold.gameObject.SetActive(false);
        if (m_ImgCard != null) m_ImgCard.gameObject.SetActive(true);
        if (m_TxtCount != null) m_TxtCount.text = count.ToString();
        int version = ++m_Bind;
        LoadArtAsync(cardId, version).Forget();
    }

    async UniTaskVoid LoadArtAsync(string cardId, int version)
    {
        try
        {
            CardArtwork texture = await CardPoolAddress.LoadCardArtworkAsync(ResolvePool(cardId), cardId);
            if (this == null || version != m_Bind || m_ImgCard == null)
            {
                return;
            }

            if (texture != null)
            {
                texture.ApplyTo(m_ImgCard);
                ApplyNativeSize(m_ImgCard);
            }
        }
        catch (Exception exception)
        {
            ALog.LogWarning($"礼品卡图加载失败. CardId={cardId}; Error={exception.Message}", ALogCategories.UI);
        }
    }

    static string ResolvePool(string cardId)
    {
        try
        {
            GachaPoolData all = LocalGameConfiguration.GetPool("CardAll");
            for (int i = 0; i < all.Cards.Count; i++)
            {
                if (all.Cards[i].CardId == cardId)
                {
                    return all.Cards[i].SourcePool;
                }
            }
        }
        catch (Exception)
        {
            // 配置未就绪时仍尝试默认卡包路径.
        }

        return "Card01";
    }

    // 先按贴图像素定尺寸, 超出奖励格时等比缩小以免撑破行高.
    static void ApplyNativeSize(Graphic graphic)
    {
        if (graphic is Image image)
        {
            image.SetNativeSize();
        }
        else if (graphic is RawImage raw)
        {
            raw.SetNativeSize();
        }

        RectTransform rt = graphic.rectTransform;
        RectTransform parent = rt.parent as RectTransform;
        if (parent == null)
        {
            return;
        }

        Vector2 size = rt.sizeDelta;
        if (size.x <= 0f || size.y <= 0f)
        {
            return;
        }

        float maxW = parent.rect.width;
        float maxH = parent.rect.height;
        if (maxW <= 0f || maxH <= 0f)
        {
            return;
        }

        float scale = Mathf.Min(1f, maxW / size.x, maxH / size.y);
        if (scale < 1f)
        {
            rt.sizeDelta = size * scale;
        }
    }
}
