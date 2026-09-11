using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LitMotion;
using Cysharp.Threading.Tasks;
using System;
using AChen.Player;
using AChen.Events;

public class LogInWindow : AWindowController
{
    // --tag_start: 自动生成--
    [SerializeField] TMP_InputField m_InpLogName;
    [SerializeField] TMP_InputField m_InpLogPassWord;
    [SerializeField] TMP_InputField m_InpLogPassWord_Again;
    [SerializeField] Button m_BtnOK;
    [SerializeField] Button m_BtnNo;
    [SerializeField] Button m_BtnRes;
    // --tag_end: 自动生成--
    [SerializeField] private CanvasGroup m_CanvasGroup;
    private AuthMode m_authMode;
    private TMP_Text m_switchModeButtonText;
    InputFeedback m_nameInputFeedback;
    InputFeedback m_passwordInputFeedback;
    InputFeedback m_passwordAgainInputFeedback;

    protected override void AddListeners()
    {
        EventCenter.AddListener(GameEvent.LobbyEntering, OnLobbyEntering);
        EventCenter.AddListener(GameEvent.LobbyEntered, OnLobbyEntered);
        EventCenter.AddListener(GameEvent.LobbyEntryFailed, OnLobbyEntryFailed);
        m_BtnOK.onClick.AddListener(OnBtnOKClick);
        m_BtnNo.onClick.AddListener(OnBtnNoClick);
        m_BtnRes.onClick.AddListener(OnBtnResClick);
        m_InpLogName.onValueChanged.AddListener(m_nameInputFeedback.OnChanged);
        m_InpLogPassWord.onValueChanged.AddListener(m_passwordInputFeedback.OnChanged);
        m_InpLogPassWord_Again.onValueChanged.AddListener(m_passwordAgainInputFeedback.OnChanged);
    }


    private void OnBtnResClick()
    {
        SetAuthMode(m_authMode == AuthMode.Login ? AuthMode.Register : AuthMode.Login);
    }

    private void OnBtnOKClick()
    {
        AuthenticateAsync().Forget();
    }

    private async UniTaskVoid AuthenticateAsync()
    {
        if (m_authenticating || m_enteringLobby) return;
        string username = m_InpLogName.text.Trim();
        string password = m_InpLogPassWord.text;
        string validationMessage = AuthFlow.Validate(m_authMode, username, password, m_InpLogPassWord_Again.text);
        if (validationMessage != null)
        {
            ShowMessage(validationMessage);
            return;
        }

        m_authenticating = true;
        UpdateInteraction();
        try
        {
            AuthResult result = await AuthFlow.AuthenticateAsync(
                m_authMode,
                username,
                password,
                this.GetCancellationTokenOnDestroy());
            if (!result.Succeeded)
            {
                ShowMessage(result.ErrorMessage);
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception exception)
        {
            ALog.LogError("账号认证异常: " + exception.Message, ALogCategories.UI);
            ShowMessage("登录失败，请稍后重试");
        }
        finally
        {
            m_authenticating = false;
            UpdateInteraction();
        }
    }

    bool m_authenticating;
    bool m_enteringLobby;

    void OnLobbyEntering()
    {
        m_enteringLobby = true;
        UpdateInteraction();
    }

    void OnLobbyEntered()
    {
        m_enteringLobby = false;
        UpdateInteraction();
    }

    void OnLobbyEntryFailed(string message)
    {
        m_enteringLobby = false;
        UpdateInteraction();
        if (this != null && IsVisible) ShowMessage("进入大厅失败，请稍后再试");
    }

    void UpdateInteraction()
    {
        if (m_CanvasGroup != null) m_CanvasGroup.interactable = !m_authenticating && !m_enteringLobby;
    }

    protected override void RemoveListeners()
    {
        EventCenter.RemoveListener(GameEvent.LobbyEntering, OnLobbyEntering);
        EventCenter.RemoveListener(GameEvent.LobbyEntered, OnLobbyEntered);
        EventCenter.RemoveListener(GameEvent.LobbyEntryFailed, OnLobbyEntryFailed);
        m_BtnNo.onClick.RemoveListener(OnBtnNoClick);
        m_BtnOK.onClick.RemoveListener(OnBtnOKClick);
        m_BtnRes.onClick.RemoveListener(OnBtnResClick);
        m_InpLogName.onValueChanged.RemoveListener(m_nameInputFeedback.OnChanged);
        m_InpLogPassWord.onValueChanged.RemoveListener(m_passwordInputFeedback.OnChanged);
        m_InpLogPassWord_Again.onValueChanged.RemoveListener(m_passwordAgainInputFeedback.OnChanged);
    }

    private void OnBtnNoClick()
    {
        EventCenter.Dispatch(GameEvent.GameExitRequested);
    }


    protected override void Awake()
    {
        m_nameInputFeedback = new InputFeedback(m_InpLogName);
        m_passwordInputFeedback = new InputFeedback(m_InpLogPassWord);
        m_passwordAgainInputFeedback = new InputFeedback(m_InpLogPassWord_Again);
        m_switchModeButtonText = m_BtnRes.GetComponentInChildren<TMP_Text>();
        base.Awake();
    }

    protected override void OnOpen()
    {
        m_InpLogName.text = string.Empty;
        m_InpLogPassWord.text = string.Empty;
        m_InpLogPassWord_Again.text = string.Empty;
        SetAuthMode(AuthMode.Login);
        DoAnim().Forget();
    }

    private void SetAuthMode(AuthMode mode)
    {
        m_authMode = mode;
        bool isRegister = mode == AuthMode.Register;
        m_InpLogPassWord_Again.gameObject.SetActive(isRegister);
        m_switchModeButtonText.text = isRegister ? "返回登录" : "注册";

        if (!isRegister)
        {
            m_InpLogPassWord_Again.text = string.Empty;
        }
    }

    private async UniTaskVoid DoAnim()
    {
        m_CanvasGroup.interactable = false;
        var seq = LSequence.Create();
        seq.Append(UITween.DoFadeAnim(0, 1, 0.5f, m_CanvasGroup));
        if (SceneTransitionOverlay.TryFadeOut(0.5f, out MotionHandle overlayFade))
        {
            seq.Join(overlayFade);
        }

        await seq.Run().AddTo(this);
        SceneTransitionOverlay.Hide();
        UpdateInteraction();
    }

    sealed class InputFeedback
    {
        readonly TMP_InputField m_field;
        readonly Vector3 m_scale;
        MotionHandle m_motion;
        int m_version;

        public InputFeedback(TMP_InputField field)
        {
            m_field = field;
            m_scale = field.transform.localScale;
        }

        public void OnChanged(string _) => PlayAsync().Forget();

        async UniTaskVoid PlayAsync()
        {
            int version = ++m_version;
            await UniTask.NextFrame();
            if (version != m_version)
            {
                return;
            }

            m_motion.TryCancel();
            m_field.transform.localScale = m_scale;
            //输入时播放短促缩放反馈
            m_motion = UITween.DoPunchScale(m_field.transform, 1.04f, 0.12f);
        }
    }
}
