using System;
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
    [SerializeField] private GameObject m_heroSprite;
    private Image m_heroImage;

    protected override void Awake()
    {
        m_heroImage = m_heroSprite.GetComponent<Image>();
        base.Awake();
    }
    protected override void AddListeners()
    {
        m_BtnExit.onClick.AddListener(OnExitClick);
        m_BtnShop.onClick.AddListener(OnShopClick);
    }

    private void OnShopClick()
    {
        //todo:把这个字段写进父类
        var uiFrame = GetComponentInParent<UIFrame>();
        if (uiFrame != null)
        {
            uiFrame.OpenWindow(AddressKeys.Prefab.ShopWindows);
            // 或者 uiFrame.ShowScreen("ShopWindows");
        }
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
    private async UniTaskVoid DoStartAnimAsync()
    {
        m_CanvasGroup.interactable = false;
        m_heroSprite.SetActive(false);
        var seq = LSequence.Create();
        seq.Append(UITween.DoFadeAnim(0, 1, m_Duration, m_CanvasGroup));
        if (m_LeftLayOut != null) seq.Append(UITween.DoMoveAnim(m_LeftLayOut, UITween.MoveDirection.Right, m_Distance, m_Duration));
        if (m_RightLayOut != null) seq.Join(UITween.DoMoveAnim(m_RightLayOut, UITween.MoveDirection.Left, m_Distance, m_Duration));
        if (m_UpLayOut != null) seq.Join(UITween.DoMoveAnim(m_UpLayOut, UITween.MoveDirection.Down, m_Distance, m_Duration));
        if (m_DownLayOut != null) seq.Join(UITween.DoMoveAnim(m_DownLayOut, UITween.MoveDirection.Up, m_Distance, m_Duration));
        seq.Append(UITween.DoFadeAnim(0, 1, m_Duration, m_heroImage));
        await seq.Run().AddTo(this);
        m_CanvasGroup.interactable = true;
    }
}
