using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public sealed class CardZoomWindowProperty : IWindowProperties
{
    public Texture Texture { get; }

    public CardZoomWindowProperty(Texture texture)
    {
        Texture = texture;
    }
}

public class CardZoomWindow : AWindowController<CardZoomWindowProperty>
{
    const float DimAlpha = 0.4f;
    const float ScaleDuration = 0.35f;
    const float HeightRatio = 0.92f;
    // 与抽卡舞台错开, 避免同层同位置被两台相机一起拍进 RT
    const float StageY = -1500f;
    // 左上-右下轴, 负角让右上角贴向相机 (相机在 -Z 看向原点)
    static readonly Vector3 BowAxis = new Vector3(1f, -1f, 0f).normalized;
    const float BowTowardCamera = -45f;

    [SerializeField] Image m_ImgDim;
    [SerializeField] RawImage m_RawCard;
    [SerializeField] GameObject m_StagePrefab;

    GameObject m_Stage;
    CardPickController m_Controller;
    Transform m_Pivot;
    CardPickView m_Card;
    CardMouseTilt m_Tilt;
    RenderTexture m_Rt;
    Vector3 m_RestScale = Vector3.one;
    bool m_Animating;
    bool m_Closing;

    protected override void OnOpen()
    {
        m_Closing = false;
        m_Animating = true;
        ResetDim(0f);
        if (!TrySpawnStage())
        {
            m_Animating = false;
            return;
        }

        PlayOpenAsync().Forget();
        bool hasTexture = Properties?.Texture != null;
        ALog.Log($"卡图放大打开. HasTexture={hasTexture}", ALogCategories.UI);
    }

    protected override void OnClose()
    {
        m_Animating = false;
        m_Closing = false;
        ReleaseStage();
        ALog.Log("卡图放大关闭", ALogCategories.UI);
    }

    protected override void OnDestroy()
    {
        ReleaseStage();
        base.OnDestroy();
    }

    void Update()
    {
        if (m_Animating || m_Closing || !IsOpened)
        {
            return;
        }

        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame)
        {
            return;
        }

        if (!TryHitCard())
        {
            return;
        }

        PlayCloseAsync().Forget();
    }

    void OnRectTransformDimensionsChange()
    {
        if (m_Controller == null)
        {
            return;
        }

        Camera pickCamera = m_Controller.PickCamera;
        if (pickCamera != null)
        {
            BindRenderTexture(pickCamera);
        }
    }

    bool TrySpawnStage()
    {
        ReleaseStage();
        if (m_StagePrefab == null)
        {
            ALog.LogError("卡图放大打开失败. 原因=未指定舞台预制体", ALogCategories.UI);
            return false;
        }

        m_Stage = Instantiate(m_StagePrefab);
        m_Stage.name = "CardZoomStage";
        m_Stage.transform.position = new Vector3(0f, StageY, 0f);
        m_Controller = m_Stage.GetComponentInChildren<CardPickController>(true);
        Camera pickCamera = m_Controller != null ? m_Controller.PickCamera : null;
        GameObject cardPrefab = m_Controller != null ? m_Controller.CardPrefab : null;
        if (m_Controller == null || pickCamera == null || cardPrefab == null || m_RawCard == null)
        {
            ALog.LogError("卡图放大打开失败. 原因=舞台或卡预制体缺失", ALogCategories.UI);
            ReleaseStage();
            return false;
        }

        var layout = m_Controller.GetComponent<GameObjectHorizontalLayout>();
        if (layout != null)
        {
            layout.enabled = false;
        }

        var pivotGo = new GameObject("CardZoomPivot");
        SetLayerRecursively(pivotGo, m_Controller.gameObject.layer);
        m_Pivot = pivotGo.transform;
        m_Pivot.SetParent(m_Controller.transform, false);
        m_Pivot.localPosition = Vector3.zero;
        m_Pivot.localRotation = Quaternion.identity;
        // 先放到最终大小再生成卡, 让 CardMouseTilt.Awake 记到正确的世界半宽
        m_RestScale = ComputeRestScale(pickCamera, 1.457627f);
        m_Pivot.localScale = m_RestScale;

        GameObject card = Instantiate(cardPrefab, m_Pivot);
        card.name = "ZoomCard";
        SetLayerRecursively(card, m_Controller.gameObject.layer);
        card.transform.localPosition = Vector3.zero;
        card.transform.localRotation = Quaternion.identity;
        card.transform.localScale = Vector3.one;

        m_Card = card.GetComponent<CardPickView>();
        CardFlip flip = card.GetComponent<CardFlip>();
        m_Tilt = card.GetComponent<CardMouseTilt>();
        if (m_Card == null)
        {
            ALog.LogError("卡图放大打开失败. 原因=卡视图缺失", ALogCategories.UI);
            ReleaseStage();
            return false;
        }

        Texture texture = Properties != null ? Properties.Texture : null;
        if (texture == null)
        {
            ALog.LogWarning("卡图放大打开失败. 原因=贴图缺失", ALogCategories.UI);
        }

        m_Card.Init(new CardPickViewData
        {
            cardId = string.Empty,
            cardShaderType = CardShaderType.None,
            cardTexture = texture
        });
        if (flip != null)
        {
            flip.SetShowBack(false);
            flip.enabled = false;
        }

        if (m_Tilt != null)
        {
            m_Tilt.enabled = false;
        }

        m_Pivot.localScale = Vector3.zero;
        BindRenderTexture(pickCamera);
        Camera uiCamera = m_UIFrame != null ? m_UIFrame.UICamera : null;
        m_Controller.BindPointer(new CardPickScreenPointer(m_RawCard, uiCamera, pickCamera));
        return true;
    }

    Vector3 ComputeRestScale(Camera pickCamera, float cardHeight)
    {
        if (cardHeight < 0.01f)
        {
            return Vector3.one;
        }

        float distance = Mathf.Abs(pickCamera.transform.position.z - m_Pivot.position.z);
        if (distance < 0.01f)
        {
            distance = 2f;
        }

        float viewHeight = 2f * distance * Mathf.Tan(pickCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        return Vector3.one * (viewHeight * HeightRatio / cardHeight);
    }

    void BindRenderTexture(Camera pickCamera)
    {
        int width = Mathf.Max(Screen.width, 1);
        int height = Mathf.Max(Screen.height, 1);
        if (m_Rt != null && m_Rt.IsCreated() && m_Rt.width == width && m_Rt.height == height)
        {
            return;
        }

        ReleaseRenderTexture(pickCamera);
        m_Rt = new RenderTexture(width, height, 16, RenderTextureFormat.ARGB32)
        {
            name = "CardZoomRT",
            antiAliasing = 1
        };
        m_Rt.Create();
        pickCamera.clearFlags = CameraClearFlags.SolidColor;
        pickCamera.backgroundColor = new Color(0f, 0f, 0f, 0f);
        pickCamera.allowHDR = false;
        pickCamera.allowMSAA = false;
        pickCamera.targetTexture = m_Rt;
        m_RawCard.texture = m_Rt;
        m_RawCard.color = Color.white;
        m_RawCard.raycastTarget = true;
    }

    void ReleaseRenderTexture(Camera pickCamera)
    {
        if (pickCamera != null)
        {
            pickCamera.targetTexture = null;
        }

        if (m_RawCard != null)
        {
            m_RawCard.texture = null;
        }

        if (m_Rt != null)
        {
            m_Rt.Release();
            Destroy(m_Rt);
            m_Rt = null;
        }
    }

    async UniTaskVoid PlayOpenAsync()
    {
        try
        {
            await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);
            if (m_Closing || this == null || !IsOpened || m_Pivot == null)
            {
                return;
            }

            UniTask fade = m_ImgDim != null
                ? UITween.RunAsync(UITween.DoFadeAnim(0f, DimAlpha, ScaleDuration, m_ImgDim), this)
                : UniTask.CompletedTask;
            UniTask scale = UITween.RunAsync(
                UITween.DoScaleAnim(Vector3.zero, m_RestScale, ScaleDuration, m_Pivot, Ease.OutCubic),
                this);
            await UniTask.WhenAll(fade, scale, PlayOpenBowAsync());
            if (m_Closing || this == null || !IsOpened)
            {
                return;
            }

            if (m_Tilt != null)
            {
                m_Tilt.enabled = true;
            }
        }
        catch (System.OperationCanceledException)
        {
        }

        m_Animating = false;
    }

    async UniTaskVoid PlayCloseAsync()
    {
        m_Closing = true;
        m_Animating = true;
        if (m_Tilt != null)
        {
            m_Tilt.enabled = false;
        }

        ALog.Log("卡图放大收回", ALogCategories.UI);
        try
        {
            Vector3 from = m_Pivot != null ? m_Pivot.localScale : m_RestScale;
            UniTask fade = m_ImgDim != null
                ? UITween.RunAsync(UITween.DoFadeAnim(m_ImgDim.color.a, 0f, ScaleDuration, m_ImgDim), this)
                : UniTask.CompletedTask;
            UniTask scale = m_Pivot != null
                ? UITween.RunAsync(
                    UITween.DoScaleAnim(from, Vector3.zero, ScaleDuration, m_Pivot, Ease.InCubic),
                    this)
                : UniTask.CompletedTask;
            await UniTask.WhenAll(fade, scale, PlayCloseBowAsync());
        }
        catch (System.OperationCanceledException)
        {
        }

        if (this == null || !IsOpened)
        {
            return;
        }

        UI_Close();
    }

    async UniTask PlayOpenBowAsync()
    {
        float half = ScaleDuration * 0.5f;
        await AnimateBowAsync(0f, BowTowardCamera, half);
        if (this == null || m_Closing || m_Pivot == null)
        {
            return;
        }

        await AnimateBowAsync(BowTowardCamera, 0f, half);
    }

    async UniTask PlayCloseBowAsync()
    {
        float opposite = -BowTowardCamera;
        float half = ScaleDuration * 0.5f;
        await AnimateBowAsync(0f, opposite, half);
        if (this == null || m_Pivot == null)
        {
            return;
        }

        await AnimateBowAsync(opposite, 0f, half);
    }

    async UniTask AnimateBowAsync(float fromDeg, float toDeg, float duration)
    {
        if (m_Pivot == null)
        {
            return;
        }

        m_Pivot.localRotation = Quaternion.AngleAxis(fromDeg, BowAxis);
        await LMotion.Create(fromDeg, toDeg, duration)
            .WithEase(Ease.OutCubic)
            .Bind(deg =>
            {
                if (m_Pivot != null)
                {
                    m_Pivot.localRotation = Quaternion.AngleAxis(deg, BowAxis);
                }
            })
            .AddTo(this);
    }

    bool TryHitCard()
    {
        if (m_Controller == null || m_Card == null)
        {
            return false;
        }

        if (!m_Controller.TryGetPointerRay(out Ray ray))
        {
            return false;
        }

        return Physics.Raycast(ray, out RaycastHit hit) && hit.transform.IsChildOf(m_Card.transform);
    }

    void ResetDim(float alpha)
    {
        if (m_ImgDim == null)
        {
            return;
        }

        Color color = m_ImgDim.color;
        color.a = alpha;
        m_ImgDim.color = color;
    }

    void ReleaseStage()
    {
        if (m_Tilt != null)
        {
            m_Tilt.enabled = false;
            m_Tilt = null;
        }

        m_Card = null;
        m_Pivot = null;
        if (m_Controller != null)
        {
            m_Controller.BindPointer(null);
            Camera pickCamera = m_Controller.PickCamera;
            ReleaseRenderTexture(pickCamera);
            m_Controller = null;
        }
        else
        {
            ReleaseRenderTexture(null);
        }

        if (m_Stage != null)
        {
            Destroy(m_Stage);
            m_Stage = null;
        }
    }

    static void SetLayerRecursively(GameObject root, int layer)
    {
        root.layer = layer;
        Transform transform = root.transform;
        for (int i = 0; i < transform.childCount; i++)
        {
            SetLayerRecursively(transform.GetChild(i).gameObject, layer);
        }
    }
}
