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
    [SerializeField] private GameObject m_heroSprite;
    private Image m_heroImage;
    protected override void Awake()
    { 
        m_heroImage = m_heroSprite.GetComponent<Image>();
        base.Awake();   
    }
    protected override void AddListeners()
    {
        
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
        //先播放一个渐入的效果
        seq.Append(DoFadeAnim(0, 1, m_Duration));
        if (m_LeftLayOut != null) seq.Append(DoMoveAnim(m_LeftLayOut, MoveDirection.Right, m_Distance, m_Duration));
        if (m_RightLayOut != null) seq.Join(DoMoveAnim(m_RightLayOut, MoveDirection.Left, m_Distance, m_Duration));
        if (m_UpLayOut != null) seq.Join(DoMoveAnim(m_UpLayOut, MoveDirection.Down, m_Distance, m_Duration));
        if (m_DownLayOut != null) seq.Join(DoMoveAnim(m_DownLayOut, MoveDirection.Up, m_Distance, m_Duration));
        seq.Append(DoFadeAnim(0, 1, m_Duration,m_heroImage));
        await seq.Run().AddTo(this);
        m_CanvasGroup.interactable = true;
    }
    //渐入动画
    private MotionHandle DoFadeAnim(float from, float to, float duration)
    {
        m_CanvasGroup.alpha = from;
        return LMotion.Create(from, to, duration)
            .WithEase(Ease.OutCubic)
            .BindToAlpha(m_CanvasGroup);
    }
    //Image的渐入渐出动画
    private MotionHandle DoFadeAnim(float from, float to, float duration,Image target)
    {
        if(target.gameObject.activeSelf == false)
        {
            target.gameObject.SetActive(true);
        }
        target.color = new Color(target.color.r, target.color.g, target.color.b, from);
        return LMotion.Create(from, to, duration)
            .WithEase(Ease.OutCubic)
            .BindToColorA(target);
    }
    enum MoveDirection
    {
        Left,
        Right,
        Up,
        Down
    }

    //移动动画
    private MotionHandle DoMoveAnim(RectTransform target, MoveDirection direction, float distance, float duration)
    {
        var origin = target.anchoredPosition;
        var opposite = direction switch
        {
            MoveDirection.Left => Vector2.right,
            MoveDirection.Right => Vector2.left,
            MoveDirection.Up => Vector2.down,
            MoveDirection.Down => Vector2.up,
            _ => Vector2.zero
        };
        var from = origin + opposite * distance;
        target.anchoredPosition = from;
        return LMotion.Create(from, origin, duration)
            .WithEase(Ease.OutCubic)
            .BindToAnchoredPosition(target);
}
}
