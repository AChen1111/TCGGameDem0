using System;
using LitMotion;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

public enum CardShaderType
{
    None = 0,//普通
    Colorful = 1,//炫彩
    Mirror = 2,//镜碎
    Outline = 3,//描边
    Gold = 4,//金沙
}

public enum CardStatus
{
    None = 0,//没有显出
    CanFlip = 1,//可以被翻面
    ShowEnd = 3,//所有卡都被翻开
    CanInspect = 4,//展开后可点开详情
}

[Serializable]
public struct CardPickViewData
{
    public string cardId;
    public CardShaderType cardShaderType;
    public Texture cardTexture;
}

//抽卡界面的卡牌视图
public class CardPickView : MonoBehaviour
{

    [Header("显示变量")]
    [SerializeField] private CardShaderType _cardShaderType;
    [SerializeField] private MeshRenderer _meshFrontRenderer;
    [SerializeField] private MeshRenderer _meshBackRenderer;
    [SerializeField] private Material _matFront;
    [SerializeField] private Material _matBack;
    [SerializeField] private CardStatus _cardStatus = CardStatus.None;

    [Header("功能组件")]
    [SerializeField] private CardFlip _cardFlip;//点击翻面效果
    [SerializeField] private CardMouseTilt _cardMouseTilt;//鼠标倾斜效果

    [Header("Z轴移动")]
    [SerializeField, Min(0.01f)] private float _translateZDuration = 0.45f;
    [SerializeField] private Ease _translateZEase = Ease.InOutCubic;

    [Header("溶解")]
    [SerializeField, Min(0.01f)] private float _dissolveDuration = 0.6f;
    [SerializeField] private Ease _dissolveEase = Ease.InOutCubic;

    MotionHandle _translateZHandle;
    MotionHandle _dissolveHandle;
    Material _matEdge;
    CardPickController _controller;
    CardPickViewData _data;
    int _index;

    //todo:增加卡图替换功能
    //[SerializeField] private SpriteRenderer _spriteRenderer;//卡图

    static readonly int EffectId = Shader.PropertyToID("_Effect");
    static readonly int BaseMapId = Shader.PropertyToID("_BaseMap");
    static readonly int DissolveId = Shader.PropertyToID("_Dissolve");
    static readonly int GoldOutlineId = Shader.PropertyToID("_GoldOutline");
    static readonly string[] Keywords =
    {
        "_EFFECT_NONE",
        "_EFFECT_COLORFUL",
        "_EFFECT_MIRROR",
        "_EFFECT_OUTLINE",
        "_EFFECT_GOLD",
    };

    //应用材质效果. Gold 正面金沙+描边, 背面只描边无沙粒
    void ApplyEffect(CardShaderType type)
    {
        int index = (int)type;
        _matFront.SetFloat(EffectId, index);
        _matBack.SetFloat(EffectId, index);
        _matFront.SetFloat(GoldOutlineId, 1f);
        _matBack.SetFloat(GoldOutlineId, 0f);

        for (int i = 0; i < Keywords.Length; i++)
        {
            if (i == index)
            {
                _matFront.EnableKeyword(Keywords[i]);
                _matBack.EnableKeyword(Keywords[i]);
            }
            else
            {
                _matFront.DisableKeyword(Keywords[i]);
                _matBack.DisableKeyword(Keywords[i]);
            }
        }
    }

    [Button("切换效果")]
    public void CycleEffect()
    {
        int next = ((int)_cardShaderType + 1) % Keywords.Length;
        _cardShaderType = (CardShaderType)next;
        ApplyEffect(_cardShaderType);
        ALog.Log($"卡牌切换效果. Card={name}; Type={_cardShaderType}", ALogCategories.Default);
    }

    void ApplyDissolve(float value)
    {
        _matFront.SetFloat(DissolveId, value);
        _matBack.SetFloat(DissolveId, value);
        BindEdgeMaterial().SetFloat(DissolveId, value);
    }

    // 夹层材质从子物体 Edge 取, 不另挂序列化引用, 避免热更程序集新增字段后预制体对不上
    Material BindEdgeMaterial()
    {
        if (_matEdge != null)
        {
            return _matEdge;
        }

        var edgeRenderer = transform.Find("Edge").GetComponent<MeshRenderer>();
        _matEdge = new Material(edgeRenderer.sharedMaterial);
        edgeRenderer.material = _matEdge;
        return _matEdge;
    }



    public void Init(CardPickViewData cardPickViewData)
    {
        _cardShaderType = cardPickViewData.cardShaderType;
        _data = cardPickViewData;
        _matFront = new Material(_matFront);
        _matBack = new Material(_matBack);
        _meshFrontRenderer.material = _matFront;
        _meshBackRenderer.material = _matBack;
        BindEdgeMaterial();
        if (cardPickViewData.cardTexture != null)
        {
            _matFront.SetTexture(BaseMapId, cardPickViewData.cardTexture);
        }

        ApplyEffect(_cardShaderType);
        ApplyDissolve(0f);
        SwitchStatus(CardStatus.None);
    }

    public void SetIndex(int index)
    {
        _index = index;
    }

    public int Index => _index;

    public CardPickViewData Data => _data;


    //正面 = true, 背面 = false.
    public bool IsFlipped()
    {
        return _cardFlip.IsFlipped;
    }

    //翻面
    public void DoFlip()
    {
        _cardFlip.Flip();
    }

    public void SwitchStatus(CardStatus status)
    {
        _SwitchStatus(status);
    }

    //切换卡牌状态
    private void _SwitchStatus(CardStatus status)
    {
        _cardStatus = status;

        switch(_cardStatus)
        {
            //没有显出,不可以被翻面和倾斜
            case CardStatus.None:
                _cardFlip.SetShowBack(true);
                _cardMouseTilt.enabled = false;
                _cardFlip.enabled = false;
                break;
            case CardStatus.CanFlip:
                _cardMouseTilt.enabled = true;
                _cardFlip.enabled = true;
                break;
            //所有卡都被翻开时候的状态
            case CardStatus.ShowEnd:
                _cardFlip.SetShowBack(false);
                _cardMouseTilt.enabled = false;
                _cardFlip.enabled = false;
                break;
            case CardStatus.CanInspect:
                _cardFlip.SetShowBack(false);
                _cardMouseTilt.enabled = true;
                _cardFlip.enabled = false;
                break;
        }
       
    }

    
    void Awake()
    {
        _controller = GetComponentInParent<CardPickController>();
    }

    //检测卡牌是否被点击
    void Update()
    {
        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame)
        {
            return;
        }

        if (_cardStatus != CardStatus.CanFlip && _cardStatus != CardStatus.CanInspect)
        {
            return;
        }

        if (!TryGetPickRay(out Ray ray))
        {
            return;
        }

        if (Physics.Raycast(ray, out var hit) && hit.transform.IsChildOf(transform))
        {
            if (_cardStatus == CardStatus.CanInspect)
            {
                _controller?.NotifyInspect(_index);
                return;
            }

            DoFlip();
        }
    }

    bool TryGetPickRay(out Ray ray)
    {
        if (_controller == null)
        {
            _controller = GetComponentInParent<CardPickController>();
        }

        if (_controller == null)
        {
            ray = default;
            return false;
        }

        return _controller.TryGetPointerRay(out ray);
    }
    
    void OnDisable()
    {
        _translateZHandle.TryCancel();
        _dissolveHandle.TryCancel();
    }

    // 沿局部 Z 补帧移动到目标位置, XY 不变
    public void DoTranslateZ(int zValue, Action onComplete = null)
    {
        _translateZHandle.TryCancel();
        float from = transform.localPosition.z;
        float to = zValue;
        ALog.Log($"卡牌移动Z. Card={name}; From={from}; To={to}", ALogCategories.Default);
        var builder = LMotion.Create(from, to, _translateZDuration)
            .WithEase(_translateZEase);
        if (onComplete != null)
        {
            builder = builder.WithOnComplete(onComplete);
        }

        _translateZHandle = builder
            .Bind(z =>
            {
                var pos = transform.localPosition;
                pos.z = z;
                transform.localPosition = pos;
            })
            .AddTo(this);
    }

    // 正反面和夹层同时把 _Dissolve 补到目标值. 1 消失, 0 复原
    [Button("溶解")]
    public void DoDissolve(Action onComplete = null)
    {
        PlayDissolve(1f, onComplete);
    }

    [Button("复原")]
    public void RestoreDissolve(Action onComplete = null)
    {
        PlayDissolve(0f, onComplete);
    }

    void PlayDissolve(float to,Action onComplete)
    {
        _dissolveHandle.TryCancel();
        float from = _matFront.GetFloat(DissolveId);
        ALog.Log($"卡牌溶解. Card={name}; From={from}; To={to}", ALogCategories.Default);
        var builder = LMotion.Create(from, to, _dissolveDuration)
            .WithEase(_dissolveEase);
        if (onComplete != null)
        {
            builder = builder.WithOnComplete(onComplete);
        }

        _dissolveHandle = builder
            .Bind(ApplyDissolve)
            .AddTo(this);
    }
}
