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
    private AuthClient m_AuthClient = new();
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
            await m_AuthClient.LoginAsync(logName, logPassWord);
            m_UIFrame.OpenWindow(AddressKeys.Prefab.MessageWindow, new MessageWindowProperties("登录成功", 2f));
        }
        catch(BackendApiException ex)
        {
            ALog.LogError(ex.Message);
            m_UIFrame.OpenWindow(AddressKeys.Prefab.MessageWindow, new MessageWindowProperties(ex.Message, 2f));
        }
        finally
        {
            m_BtnOK.interactable = true;
        }
    }

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
