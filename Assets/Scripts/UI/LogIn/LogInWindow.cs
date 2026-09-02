using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LitMotion;
using Cysharp.Threading.Tasks;
using System;
using System.Text.RegularExpressions;
using AChen.Networking;

public class LogInWindow : AWindowController
{
    //两种登录状态: 登录和注册
    private enum AuthMode
    {
        Login,
        Register
    }

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
    //用于取消上一次输入反馈动画
    private MotionHandle m_nameInputMotion;
    private MotionHandle m_passwordInputMotion;
    private int m_nameInputVersion;
    private int m_passwordInputVersion;
    //记录输入框初始缩放，动画结束后恢复
    private Vector3 m_nameInputScale;
    private Vector3 m_passwordInputScale;

    protected override void AddListeners()
    {
        m_BtnOK.onClick.AddListener(OnBtnOKClick);
        m_BtnNo.onClick.AddListener(OnBtnNoClick);
        m_BtnRes.onClick.AddListener(OnBtnResClick);
        m_InpLogName.onValueChanged.AddListener(OnNameInputChanged);
        m_InpLogPassWord.onValueChanged.AddListener(OnPasswordInputChanged);
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
        string username = m_InpLogName.text.Trim();
        string password = m_InpLogPassWord.text;
        string validationMessage = ValidateInput(username, password);
        if (validationMessage != null)
        {
            ShowMessage(validationMessage);
            return;
        }

        m_CanvasGroup.interactable = false;
        try
        {
            if (m_authMode == AuthMode.Register)
            {
                await PlayerSession.Instance.RegisterAsync(
                    username,
                    password,
                    this.GetCancellationTokenOnDestroy());
                ALog.Log("账号注册成功并建立玩家会话.", ALogCategories.Net);
            }
            else
            {
                await PlayerSession.Instance.LoginAsync(
                    username,
                    password,
                    this.GetCancellationTokenOnDestroy());
                ALog.Log("账号登录成功并建立玩家会话.", ALogCategories.Net);
            }

            SceneTransitionOverlay.Show();
            try
            {
                await SceneLoader.LoadScene(AddressKeys.Scene.GameScene);
                return;
            }
            catch
            {
                SceneTransitionOverlay.Hide();
                throw;
            }
        }
        catch (BackendApiException exception)
        {
            SceneTransitionOverlay.Hide();
            ALog.LogError(
                $"账号认证失败. Mode={m_authMode}; Code={exception.Code}; Status={exception.StatusCode}",
                ALogCategories.Net);
            ShowMessage(GetAuthErrorMessage(exception.Code));
        }
        catch (OperationCanceledException)
        {
            SceneTransitionOverlay.Hide();
        }
        finally
        {
            if (m_CanvasGroup != null)
            {
                m_CanvasGroup.interactable = true;
            }
        }
    }

    private string ValidateInput(string username, string password)
    {
        if (!Regex.IsMatch(username, @"^[A-Za-z0-9_]{3,24}$"))
        {
            return "账号需为 3-24 位英文、数字或下划线";
        }

        if (password.Length is < 8 or > 128)
        {
            return "密码长度需为 8-128 位";
        }

        if (m_authMode == AuthMode.Register && StringValidator.IsWeakPassword(password))
        {
            return "密码过弱";
        }

        if (m_authMode == AuthMode.Register && password != m_InpLogPassWord_Again.text)
        {
            return "两次输入的密码不一致";
        }

        return null;
    }

    private void ShowMessage(string message)
    {
        m_UIFrame.OpenWindow(
            AddressKeys.Prefab.MessageWindow,
            new MessageWindowProperties(message, 2f));
    }

    private static string GetAuthErrorMessage(string code) => code switch
    {
        "ACCOUNT_EXISTS" => "该账号已被注册",
        "INVALID_CREDENTIALS" => "账号或密码错误",
        "VALIDATION_ERROR" => "账号或密码格式不正确",
        "NETWORK_ERROR" => "无法连接服务器，请检查网络",
        "RATE_LIMITED" => "操作过于频繁，请稍后再试",
        _ => "账号操作失败，请稍后再试"
    };

    protected override void RemoveListeners()
    {
        m_BtnNo.onClick.RemoveListener(OnBtnNoClick);
        m_BtnOK.onClick.RemoveListener(OnBtnOKClick);
        m_BtnRes.onClick.RemoveListener(OnBtnResClick);
        m_InpLogName.onValueChanged.RemoveListener(OnNameInputChanged);
        m_InpLogPassWord.onValueChanged.RemoveListener(OnPasswordInputChanged);
    }

    private void OnBtnNoClick()
    {
        //退出游戏
        Application.Quit();
    }


    protected override void Awake()
    {
        base.Awake();
        m_nameInputScale = m_InpLogName.transform.localScale;
        m_passwordInputScale = m_InpLogPassWord.transform.localScale;
        m_switchModeButtonText = m_BtnRes.GetComponentInChildren<TMP_Text>();
    }

    private void OnNameInputChanged(string _)
    {
        PlayNameInputFeedbackAsync(++m_nameInputVersion).Forget();
    }

    private async UniTaskVoid PlayNameInputFeedbackAsync(int version)
    {
        await UniTask.NextFrame();
        if (version != m_nameInputVersion)
        {
            return;
        }

        m_nameInputMotion.TryCancel();
        m_InpLogName.transform.localScale = m_nameInputScale;
        //输入时播放短促缩放反馈
        m_nameInputMotion = UITween.DoPunchScale(m_InpLogName.transform, 1.04f, 0.12f);
    }

    private void OnPasswordInputChanged(string _)
    {
        PlayPasswordInputFeedbackAsync(++m_passwordInputVersion).Forget();
    }

    private async UniTaskVoid PlayPasswordInputFeedbackAsync(int version)
    {
        await UniTask.NextFrame();
        if (version != m_passwordInputVersion)
        {
            return;
        }

        m_passwordInputMotion.TryCancel();
        m_InpLogPassWord.transform.localScale = m_passwordInputScale;
        //输入时播放短促缩放反馈
        m_passwordInputMotion = UITween.DoPunchScale(m_InpLogPassWord.transform, 1.04f, 0.12f);
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
        m_CanvasGroup.interactable = true;
    }



}
