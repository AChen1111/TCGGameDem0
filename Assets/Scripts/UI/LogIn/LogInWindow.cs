using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LitMotion;
using Cysharp.Threading.Tasks;
using System;
using AChen.Networking;

public class LogInWindow : AWindowController
{
    // --tag_start: 自动生成--
    [SerializeField] TMP_InputField m_InpLogName;
    [SerializeField] TMP_InputField m_InpLogPassWord;
    [SerializeField] Button m_BtnOK;
    [SerializeField] Button m_BtnNo;
    [SerializeField] Button m_BtnRes;
    // --tag_end: 自动生成--
    [SerializeField] private CanvasGroup m_CanvasGroup;
    private BackendConfig m_BackendConfig = new();
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


    /// <summary>
    /// 注册接口
    /// </summary>
    private void OnBtnResClick()
    {
         Application.OpenURL(m_BackendConfig.BaseUrl + "/register");
    }
    

    /// <summary>
    /// 登录接口
    /// </summary>
    private void OnBtnOKClick()
    {
        LoginAsync(m_InpLogName.text, m_InpLogPassWord.text).Forget();
    }

    private async UniTaskVoid LoginAsync(string logName, string logPassWord)
    {
        if (!StringValidator.IsEnglishOrNumber(logName) ||
            !StringValidator.IsEnglishOrNumber(logPassWord))
        {
            m_UIFrame.OpenWindow(
                AddressKeys.Prefab.MessageWindow,
                new MessageWindowProperties("账号和密码只能包含英文和数字", 2f));
            return;
        }

        m_BtnOK.interactable = false;
        try
        {
            await PlayerSession.Instance.LoginAsync(
                logName,
                logPassWord,
                this.GetCancellationTokenOnDestroy());
            await SceneLoader.LoadScene(AddressKeys.Scene.GameScene);
        }
        catch(BackendApiException ex)
        {
            ALog.LogError(ex.Message);
            m_UIFrame.OpenWindow(
                AddressKeys.Prefab.MessageWindow,
                new MessageWindowProperties(GetLoginErrorMessage(ex.Code), 2f));
        }
        finally
        {
            if (m_BtnOK != null)
            {
                m_BtnOK.interactable = true;
            }
        }
    }

    private static string GetLoginErrorMessage(string code) => code switch
    {
        "INVALID_CREDENTIALS" => "账号或密码错误",
        "VALIDATION_ERROR" => "账号或密码格式不正确",
        "NETWORK_ERROR" => "无法连接服务器，请检查网络",
        "RATE_LIMITED" => "操作过于频繁，请稍后再试",
        _ => "登录失败，请稍后再试"
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


    protected override void Awake() {
        base.Awake();
        m_nameInputScale = m_InpLogName.transform.localScale;
        m_passwordInputScale = m_InpLogPassWord.transform.localScale;
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
        DoAnim().Forget();
    }

    private async UniTaskVoid DoAnim()
    {
        //禁止用户输入
        m_CanvasGroup.interactable = false;
        //播放动画
        var seq = LSequence.Create();
        seq.Append(UITween.DoFadeAnim(0, 1, 0.5f, m_CanvasGroup));
        await seq.Run().AddTo(this);
        //允许用户输入
        m_CanvasGroup.interactable = true;
    }



}
