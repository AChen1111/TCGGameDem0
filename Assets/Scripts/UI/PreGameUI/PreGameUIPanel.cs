using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class PreGameUIPanel : APanelController
{
    // --tag_start: 自动生成--
    [SerializeField] Button m_BtnPlay;
    [SerializeField] Button m_BtnDeck;
    [SerializeField] Button m_BtnShop;
    [SerializeField] Button m_BtnExit;
    [SerializeField] Button m_BtnGift;
    [SerializeField] Button m_BtnFriend;
    [SerializeField] Button m_BtnMail;
    [SerializeField] Button m_BtnSetting;
    // --tag_end: 自动生成--

    [SerializeField] private RectTransform m_LeftLayOut;
    [SerializeField] private RectTransform m_RightLayOut;
    [SerializeField] private RectTransform m_UpLayOut;
    [SerializeField] private RectTransform m_DownLayOut;
    [SerializeField] private CanvasGroup m_CanvasGroup;

    [SerializeField] private float m_Duration = 1f;
    [SerializeField] private float m_Distance = 1500f;
    [SerializeField] private GameObject m_wallpaper;
    [SerializeField] private GameObject m_heroSprite;
    private Image m_heroImage;
    private Image m_wallpaperImage;

    public string key1,key2;
    protected override void Awake()
    {
        m_heroImage = m_heroSprite.GetComponent<Image>();
        m_wallpaperImage = m_wallpaper.GetComponent<Image>();
        base.Awake();
    }
    protected override void AddListeners()
    {
        m_BtnExit.onClick.AddListener(OnExitClick);
        m_BtnShop.onClick.AddListener(OnShopClick);
    }

    private void OnShopClick()
    {
        m_UIFrame.OpenWindow(AddressKeys.Prefab.ShopWindows);
    }

    private void OnExitClick()
    {
        Application.Quit();
    }


    protected override void OnOpen()
    {
        DoStartAnimAsync().Forget();
    }

    [Button("开始动画")]
    private async UniTask DoStartAnimAsync()
    {   
        //禁止输入
        m_CanvasGroup.interactable = false;
        m_wallpaper.SetActive(false);
        m_heroSprite.SetActive(false);
        m_CanvasGroup.alpha = 0;
        //加载资源
        await LoadWallpaperAndSpriteAsync(AddressKeys.Sprite.w_10_Down,AddressKeys.Sprite.w_10_Sprite);

        var seq = LSequence.Create();
        seq.Append(UITween.DoFadeAnim(0, 1, m_Duration, m_CanvasGroup));

        //step1 布局向中心移动
        if (m_LeftLayOut != null) seq.Append(UITween.DoMoveAnim(m_LeftLayOut, UITween.MoveDirection.Right, m_Distance, m_Duration));
        if (m_RightLayOut != null) seq.Join(UITween.DoMoveAnim(m_RightLayOut, UITween.MoveDirection.Left, m_Distance, m_Duration));
        if (m_UpLayOut != null) seq.Join(UITween.DoMoveAnim(m_UpLayOut, UITween.MoveDirection.Down, m_Distance, m_Duration));
        if (m_DownLayOut != null) seq.Join(UITween.DoMoveAnim(m_DownLayOut, UITween.MoveDirection.Up, m_Distance, m_Duration));
        

        //step2 加载背景
        seq.Append(UITween.DoFadeAnim(0, 1, m_Duration, m_wallpaperImage));
        m_heroSprite.SetActive(true);
        //step3 加载英雄
        seq.Append(UITween.DoVerticalReveal(m_heroImage, m_Duration));

        await seq.Run().AddTo(this);
        m_CanvasGroup.interactable = true;
    }

    private async UniTask LoadWallpaperAndSpriteAsync(string wallpaper, string sprite)
    {
        var wallpaperSprite = AddressableLoader.Instance.LoadSprite(wallpaper);
        var spriteSprite = AddressableLoader.Instance.LoadSprite(sprite);
        m_heroImage.sprite = await spriteSprite;
        m_wallpaperImage.sprite = await wallpaperSprite;
        m_heroImage.SetNativeSize();
        m_wallpaperImage.SetNativeSize();
    }
}