using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class CardPickWindowProperty : IWindowProperties
{
    public IReadOnlyList<CardPickViewData> Cards { get; }

    public CardPickWindowProperty(IReadOnlyList<CardPickViewData> cards)
    {
        Cards = cards ?? System.Array.Empty<CardPickViewData>();
    }
}

public class CardPickWindow : AWindowController<CardPickWindowProperty>
{
    // --tag_start: 自动生成--
    [SerializeField] Button m_BtnNext;
    [SerializeField] RawImage m_RawCardPick;
    // --tag_end: 自动生成--
    [SerializeField] GameObject _stagePrefab;

    GameObject _stage;
    CardPickController _controller;
    RenderTexture _rt;

    protected override void AddListeners()
    {
        if (m_BtnNext != null)
        {
            m_BtnNext.onClick.AddListener(OnNextClicked);
        }
    }

    protected override void RemoveListeners()
    {
        if (m_BtnNext != null)
        {
            m_BtnNext.onClick.RemoveListener(OnNextClicked);
        }
    }

    protected override void OnOpen()
    {
        if (!TrySpawnStage())
        {
            return;
        }

        if (m_BtnNext != null)
        {
            m_BtnNext.gameObject.SetActive(true);
        }

        _controller.Play(Properties.Cards, UI_Close);
        ALog.Log($"抽卡窗口展示. Count={Properties.Cards.Count}", ALogCategories.UI);
    }

    protected override void OnClose()
    {
        ReleaseStage();
    }

    protected override void OnDestroy()
    {
        ReleaseStage();
        base.OnDestroy();
    }

    void OnNextClicked()
    {
        _controller?.Advance();
    }

    void OnNextVisible(bool visible)
    {
        if (m_BtnNext != null)
        {
            m_BtnNext.gameObject.SetActive(visible);
        }
    }

    bool TrySpawnStage()
    {
        ReleaseStage();
        if (_stagePrefab == null)
        {
            ALog.LogError("抽卡窗口打开失败. 原因=未指定舞台预制体", ALogCategories.UI);
            return false;
        }

        _stage = Instantiate(_stagePrefab);
        _stage.name = "CardPickStage";
        _stage.transform.position = new Vector3(0f, -500f, 0f);
        _controller = _stage.GetComponentInChildren<CardPickController>(true);
        Camera pickCamera = _controller != null ? _controller.PickCamera : null;
        if (_controller == null || pickCamera == null || m_RawCardPick == null)
        {
            ALog.LogError("抽卡窗口打开失败. 原因=舞台或RawImage缺失", ALogCategories.UI);
            ReleaseStage();
            return false;
        }

        BindRenderTexture(pickCamera);
        Camera uiCamera = m_UIFrame != null ? m_UIFrame.UICamera : null;
        _controller.BindPointer(new CardPickScreenPointer(m_RawCardPick, uiCamera, pickCamera));
        _controller.NextVisibleChanged += OnNextVisible;
        return true;
    }

    void BindRenderTexture(Camera pickCamera)
    {
        int width = Mathf.Max(Screen.width, 1);
        int height = Mathf.Max(Screen.height, 1);
        _rt = new RenderTexture(width, height, 16, RenderTextureFormat.ARGB32)
        {
            name = "CardPickRT",
            antiAliasing = 1
        };
        _rt.Create();
        pickCamera.clearFlags = CameraClearFlags.SolidColor;
        pickCamera.backgroundColor = new Color(0f, 0f, 0f, 0f);
        pickCamera.allowHDR = false;
        pickCamera.allowMSAA = false;
        pickCamera.targetTexture = _rt;
        m_RawCardPick.texture = _rt;
        ALog.Log($"抽卡RT已绑定. Size={width}x{height}", ALogCategories.UI);
    }

    void ReleaseStage()
    {
        if (_controller != null)
        {
            _controller.NextVisibleChanged -= OnNextVisible;
            _controller.BindPointer(null);
            Camera pickCamera = _controller.PickCamera;
            if (pickCamera != null)
            {
                pickCamera.targetTexture = null;
            }

            _controller.Clear();
            _controller = null;
        }

        if (_rt != null)
        {
            if (m_RawCardPick != null)
            {
                m_RawCardPick.texture = null;
            }

            _rt.Release();
            Destroy(_rt);
            _rt = null;
        }

        if (_stage != null)
        {
            Destroy(_stage);
            _stage = null;
        }
    }
}
