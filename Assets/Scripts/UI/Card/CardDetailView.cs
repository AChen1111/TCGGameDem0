using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public readonly struct CardDetailEntry
{
    public string CardId { get; }
    public string SourcePool { get; }
    public Texture Texture { get; }

    public CardDetailEntry(string cardId, string sourcePool, Texture texture)
    {
        CardId = cardId ?? string.Empty;
        SourcePool = sourcePool ?? string.Empty;
        Texture = texture;
    }
}

public class CardDetailView : MonoBehaviour
{
    [SerializeField] Button m_BtnDim;
    [SerializeField] RawImage m_RawCard;
    [SerializeField] Button m_BtnCard;
    [SerializeField] Image m_ImgNameBase;
    [SerializeField] TextMeshProUGUI m_TxtName;
    [SerializeField] Image m_ImgAttr;
    [SerializeField] GameObject m_GoLevelRow;
    [SerializeField] Image m_ImgLevel;
    [SerializeField] TextMeshProUGUI m_TxtLevel;
    [SerializeField] Image[] m_ImgTypes;
    [SerializeField] GameObject m_GoStatRow;
    [SerializeField] GameObject m_GoAtk;
    [SerializeField] TextMeshProUGUI m_TxtAtk;
    [SerializeField] GameObject m_GoDef;
    [SerializeField] Image m_ImgDef;
    [SerializeField] TextMeshProUGUI m_TxtDef;
    [SerializeField] TextMeshProUGUI m_TxtType;
    [SerializeField] TextMeshProUGUI m_TxtDesc;
    [SerializeField] Button m_BtnPrev;
    [SerializeField] Button m_BtnNext;
    [SerializeField] Sprite m_SpriteAttrLight;
    [SerializeField] Sprite m_SpriteAttrDark;
    [SerializeField] Sprite m_SpriteAttrFire;
    [SerializeField] Sprite m_SpriteAttrWater;
    [SerializeField] Sprite m_SpriteAttrWind;
    [SerializeField] Sprite m_SpriteAttrEarth;
    [SerializeField] Sprite m_SpriteAttrDivine;
    [SerializeField] Sprite m_SpriteLevel;
    [SerializeField] Sprite m_SpriteRank;
    [SerializeField] Sprite m_SpriteLink;
    [SerializeField] Sprite m_SpriteDef;
    [SerializeField] Sprite m_SpriteTuner;
    [SerializeField] Sprite m_SpriteEffect;
    [SerializeField] Sprite m_SpriteFusion;
    [SerializeField] Sprite m_SpriteSynchro;
    [SerializeField] Sprite m_SpriteXyz;
    [SerializeField] Sprite m_SpriteLinkType;
    [SerializeField] Sprite m_SpriteSpell;
    [SerializeField] Sprite m_SpriteTrap;
    [SerializeField] Sprite m_SpriteQuick;
    [SerializeField] Sprite m_SpriteContinuous;
    [SerializeField] Sprite m_SpriteEquip;
    [SerializeField] Sprite m_SpriteField;
    [SerializeField] Sprite m_SpriteRitual;
    [SerializeField] Sprite m_SpriteCounter;

    IReadOnlyList<CardDetailEntry> m_Cards;
    int m_Index = -1;
    Action m_OnClosed;
    Action<bool> m_OnVisibleChanged;
    Action<Texture> m_OnCardClicked;

    public bool IsShowing => gameObject.activeSelf;

    void Awake()
    {
        BindCardButton();
    }

    void BindCardButton()
    {
        if (m_BtnCard == null && m_RawCard != null)
        {
            m_BtnCard = m_RawCard.GetComponent<Button>();
        }
    }

    public void SetCallbacks(Action onClosed, Action<bool> onVisibleChanged, Action<Texture> onCardClicked = null)
    {
        m_OnClosed = onClosed;
        m_OnVisibleChanged = onVisibleChanged;
        m_OnCardClicked = onCardClicked;
    }

    void OnEnable()
    {
        LocalizationService.LanguageChanged += RefreshCurrent;
        if (m_BtnDim != null)
        {
            m_BtnDim.onClick.AddListener(Hide);
        }

        if (m_BtnPrev != null)
        {
            m_BtnPrev.onClick.AddListener(OnPrevClicked);
        }

        if (m_BtnNext != null)
        {
            m_BtnNext.onClick.AddListener(OnNextClicked);
        }

        if (m_BtnCard != null)
        {
            m_BtnCard.onClick.AddListener(OnCardClicked);
        }
    }

    void OnDisable()
    {
        LocalizationService.LanguageChanged -= RefreshCurrent;
        if (m_BtnDim != null)
        {
            m_BtnDim.onClick.RemoveListener(Hide);
        }

        if (m_BtnPrev != null)
        {
            m_BtnPrev.onClick.RemoveListener(OnPrevClicked);
        }

        if (m_BtnNext != null)
        {
            m_BtnNext.onClick.RemoveListener(OnNextClicked);
        }

        if (m_BtnCard != null)
        {
            m_BtnCard.onClick.RemoveListener(OnCardClicked);
        }
    }

    public void Show(IReadOnlyList<CardDetailEntry> cards, int index)
    {
        BindCardButton();
        if (cards == null || cards.Count == 0 || index < 0 || index >= cards.Count)
        {
            ALog.LogWarning("卡牌详情打开失败. 原因=列表为空或下标无效", ALogCategories.UI);
            return;
        }

        m_Cards = cards;
        bool wasHidden = !gameObject.activeSelf;
        transform.SetAsLastSibling();
        gameObject.SetActive(true);
        Bind(index, logOpen: wasHidden);
        if (wasHidden)
        {
            m_OnVisibleChanged?.Invoke(true);
        }
    }

    public bool TryShowRelative(int delta)
    {
        if (m_Cards == null || m_Index < 0)
        {
            return false;
        }

        int next = m_Index + delta;
        if (next < 0 || next >= m_Cards.Count)
        {
            return false;
        }

        Bind(next, logOpen: false);
        ALog.Log($"卡牌详情切换. CardId={m_Cards[next].CardId}; Index={next}; Delta={delta}", ALogCategories.UI);
        return true;
    }

    public void Hide()
    {
        // Window 托管时只出栈, 不能自己 SetActive(false)
        if (m_OnClosed != null)
        {
            m_OnClosed.Invoke();
            return;
        }

        if (!gameObject.activeSelf)
        {
            return;
        }

        string cardId = m_Index >= 0 && m_Cards != null && m_Index < m_Cards.Count
            ? m_Cards[m_Index].CardId
            : string.Empty;
        gameObject.SetActive(false);
        m_Cards = null;
        m_Index = -1;
        ALog.Log($"卡牌详情关闭. CardId={cardId}", ALogCategories.UI);
        m_OnVisibleChanged?.Invoke(false);
    }

    void OnPrevClicked() => TryShowRelative(-1);

    void OnNextClicked() => TryShowRelative(1);

    void OnCardClicked()
    {
        Texture texture = m_RawCard != null ? m_RawCard.texture : null;
        if (texture == null)
        {
            return;
        }

        m_OnCardClicked?.Invoke(texture);
    }

    void RefreshCurrent()
    {
        if (m_Cards != null && m_Index >= 0)
        {
            Bind(m_Index, logOpen: false);
        }
    }

    void Bind(int index, bool logOpen)
    {
        CardDetailEntry entry = m_Cards[index];
        m_Index = index;
        ApplyFont(m_TxtName);
        ApplyFont(m_TxtLevel);
        ApplyFont(m_TxtAtk);
        ApplyFont(m_TxtDef);
        ApplyFont(m_TxtType);
        ApplyFont(m_TxtDesc);

        bool hasTexture = entry.Texture != null;
        if (m_RawCard != null)
        {
            m_RawCard.texture = entry.Texture;
            m_RawCard.enabled = hasTexture;
            m_RawCard.color = Color.white;
        }

        if (!CardCatalog.TryGet(entry.CardId, out Table.CardRow row))
        {
            ALog.LogWarning($"卡牌详情缺表. CardId={entry.CardId}", ALogCategories.UI);
            BindNameAndDesc(entry.CardId);
            SetActive(m_GoLevelRow, false);
            SetActive(m_GoStatRow, false);
            BindAttr(CardKind.None, null);
            SetText(m_TxtType, "【" + entry.CardId + "】");
            RefreshArrows();
            ForceTexts();
            return;
        }

        BindNameAndDesc(entry.CardId);
        if (m_ImgNameBase != null)
        {
            m_ImgNameBase.color = FrameColor((CardFrame)row.Frame);
        }

        var kind = (CardKind)row.Kind;
        var frame = (CardFrame)row.Frame;
        BindAttr(kind, row);
        if (kind == CardKind.Monster)
        {
            BindMonsterRows(row, frame);
        }
        else
        {
            BindSpellTrapRow(kind, (CardSpellTrapType)row.SpellTrapType);
            SetActive(m_GoStatRow, false);
        }

        SetText(m_TxtType, CardCatalog.FormatTypeLine(row));
        RefreshArrows();
        ForceTexts();
        if (logOpen)
        {
            ALog.Log($"卡牌详情打开. CardId={entry.CardId}; Index={index}; Count={m_Cards.Count}", ALogCategories.UI);
        }
    }

    void BindNameAndDesc(string cardId)
    {
        BindLocalized(m_TxtName, "card." + cardId + ".name");
        BindLocalized(m_TxtDesc, "card." + cardId + ".desc");
    }

    static void BindLocalized(TextMeshProUGUI text, string key)
    {
        if (text == null)
        {
            return;
        }

        LocalizedText loc = text.Localized();
        if (loc != null)
        {
            loc.SetKey(key);
            return;
        }

        text.text = LocalizationService.GetText(key);
    }

    void BindAttr(CardKind kind, Table.CardRow row)
    {
        if (m_ImgAttr == null)
        {
            return;
        }

        Sprite sprite = null;
        if (row != null && kind == CardKind.Monster)
        {
            sprite = AttrSprite((CardAttribute)row.Attribute);
        }
        else if (row != null && kind == CardKind.Spell)
        {
            sprite = m_SpriteSpell;
        }
        else if (row != null && kind == CardKind.Trap)
        {
            sprite = m_SpriteTrap;
        }

        bool show = sprite != null;
        m_ImgAttr.gameObject.SetActive(show);
        if (show)
        {
            m_ImgAttr.sprite = sprite;
            m_ImgAttr.enabled = true;
        }
    }

    Sprite AttrSprite(CardAttribute attribute)
    {
        switch (attribute)
        {
            case CardAttribute.Light: return m_SpriteAttrLight;
            case CardAttribute.Dark: return m_SpriteAttrDark;
            case CardAttribute.Fire: return m_SpriteAttrFire;
            case CardAttribute.Water: return m_SpriteAttrWater;
            case CardAttribute.Wind: return m_SpriteAttrWind;
            case CardAttribute.Earth: return m_SpriteAttrEarth;
            case CardAttribute.Divine: return m_SpriteAttrDivine;
            default: return null;
        }
    }

    void BindMonsterRows(Table.CardRow row, CardFrame frame)
    {
        SetActive(m_GoLevelRow, true);
        SetActive(m_GoStatRow, true);
        SetActive(m_GoAtk, true);
        SetActive(m_GoDef, true);
        if (m_ImgLevel != null)
        {
            m_ImgLevel.gameObject.SetActive(true);
            m_ImgLevel.sprite = frame == CardFrame.Xyz ? m_SpriteRank
                : frame == CardFrame.Link ? m_SpriteLink
                : m_SpriteLevel;
            m_ImgLevel.enabled = m_ImgLevel.sprite != null;
        }

        SetText(m_TxtLevel, row.Level.ToString());
        BindTypeIcons(CollectMonsterIcons(row, frame));
        SetText(m_TxtAtk, CardCatalog.FormatStat(row.Atk));
        if (frame == CardFrame.Link)
        {
            if (m_ImgDef != null)
            {
                m_ImgDef.sprite = m_SpriteLink != null ? m_SpriteLink : m_ImgDef.sprite;
            }

            SetText(m_TxtDef, row.Level.ToString());
        }
        else
        {
            if (m_ImgDef != null && m_SpriteDef != null)
            {
                m_ImgDef.sprite = m_SpriteDef;
            }

            SetText(m_TxtDef, CardCatalog.FormatStat(row.Def));
        }
    }

    void BindSpellTrapRow(CardKind kind, CardSpellTrapType spellTrapType)
    {
        SetActive(m_GoLevelRow, true);
        SetText(m_TxtLevel, string.Empty);
        Sprite property = PropertySprite(kind, spellTrapType);
        if (m_ImgLevel != null)
        {
            bool show = property != null;
            m_ImgLevel.gameObject.SetActive(show);
            m_ImgLevel.sprite = property;
            m_ImgLevel.enabled = show;
        }

        BindTypeIcons(Array.Empty<Sprite>());
    }

    Sprite[] CollectMonsterIcons(Table.CardRow row, CardFrame frame)
    {
        // 星级旁只标协调/超量, 没有则空着. 种族只走类型文本行
        var list = new List<Sprite>(2);
        var flags = (CardFlags)row.Flags;
        if ((flags & CardFlags.Tuner) != 0)
        {
            AddIcon(list, m_SpriteTuner);
        }

        if (frame == CardFrame.Xyz)
        {
            AddIcon(list, m_SpriteXyz);
        }

        return list.ToArray();
    }

    static void AddIcon(List<Sprite> list, Sprite sprite)
    {
        if (sprite != null)
        {
            list.Add(sprite);
        }
    }

    Sprite PropertySprite(CardKind kind, CardSpellTrapType type)
    {
        if (type == CardSpellTrapType.None || type == CardSpellTrapType.Normal)
        {
            return null;
        }

        if (kind == CardKind.Trap)
        {
            return type == CardSpellTrapType.Counter ? m_SpriteCounter : m_SpriteContinuous;
        }

        return SpellSprite(type);
    }

    Sprite SpellSprite(CardSpellTrapType type)
    {
        switch (type)
        {
            case CardSpellTrapType.QuickPlay: return m_SpriteQuick;
            case CardSpellTrapType.Continuous: return m_SpriteContinuous;
            case CardSpellTrapType.Equip: return m_SpriteEquip;
            case CardSpellTrapType.Field: return m_SpriteField;
            case CardSpellTrapType.Ritual: return m_SpriteRitual;
            default: return null;
        }
    }

    void BindTypeIcons(Sprite[] sprites)
    {
        if (m_ImgTypes == null)
        {
            return;
        }

        for (int i = 0; i < m_ImgTypes.Length; i++)
        {
            Image image = m_ImgTypes[i];
            if (image == null)
            {
                continue;
            }

            bool show = sprites != null && i < sprites.Length && sprites[i] != null;
            image.gameObject.SetActive(show);
            if (show)
            {
                image.sprite = sprites[i];
            }
        }
    }

    void RefreshArrows()
    {
        bool hasList = m_Cards != null && m_Cards.Count > 1;
        if (m_BtnPrev != null)
        {
            m_BtnPrev.gameObject.SetActive(true);
            m_BtnPrev.interactable = hasList && m_Index > 0;
        }

        if (m_BtnNext != null)
        {
            m_BtnNext.gameObject.SetActive(true);
            m_BtnNext.interactable = hasList && m_Index >= 0 && m_Index < m_Cards.Count - 1;
        }
    }

    static Color FrameColor(CardFrame frame)
    {
        switch (frame)
        {
            case CardFrame.Normal: return new Color(0.82f, 0.68f, 0.22f, 1f);
            case CardFrame.Effect: return new Color(0.76f, 0.48f, 0.22f, 1f);
            case CardFrame.Ritual: return new Color(0.32f, 0.45f, 0.78f, 1f);
            case CardFrame.Fusion: return new Color(0.55f, 0.28f, 0.62f, 1f);
            case CardFrame.Synchro: return new Color(0.86f, 0.86f, 0.86f, 1f);
            case CardFrame.Xyz: return new Color(0.12f, 0.12f, 0.14f, 1f);
            case CardFrame.Link: return new Color(0.12f, 0.38f, 0.58f, 1f);
            case CardFrame.Spell: return new Color(0.12f, 0.52f, 0.32f, 1f);
            case CardFrame.Trap: return new Color(0.66f, 0.24f, 0.48f, 1f);
            default: return new Color(0.76f, 0.48f, 0.22f, 1f);
        }
    }

    static void SetActive(GameObject go, bool active)
    {
        if (go != null && go.activeSelf != active)
        {
            go.SetActive(active);
        }
    }

    static void SetText(TextMeshProUGUI text, string value)
    {
        if (text != null)
        {
            text.text = value ?? string.Empty;
        }
    }

    static void ApplyFont(TextMeshProUGUI text)
    {
        LocalizationService.ApplyPresentation(text);
    }

    void ForceTexts()
    {
        ForceMesh(m_TxtName);
        ForceMesh(m_TxtLevel);
        ForceMesh(m_TxtAtk);
        ForceMesh(m_TxtDef);
        ForceMesh(m_TxtType);
        ForceMesh(m_TxtDesc);
    }

    static void ForceMesh(TextMeshProUGUI text)
    {
        if (text != null)
        {
            text.ForceMeshUpdate();
        }
    }
}
