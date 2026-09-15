using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AChen.Networking
{
    public sealed class ContentUpdatePrompt : MonoBehaviour
    {
        static ContentUpdatePrompt s_instance;
        UniTaskCompletionSource m_retry;
        bool m_restart;
        Canvas m_canvas;
        TMP_Text m_message;
        TMP_Text m_action;
        Button m_button;

        void Awake()
        {
            if (s_instance != null && s_instance != this) { Destroy(gameObject); return; }
            s_instance = this;
            // UI 层级预置在 Init 场景, 跨场景保留且不在运行时创建控件.
            m_canvas = GetComponent<Canvas>();
            m_message = transform.Find("Panel/Message").GetComponent<TMP_Text>();
            m_button = transform.Find("Panel/Action").GetComponent<Button>();
            m_action = m_button.transform.Find("Label").GetComponent<TMP_Text>();
            m_button.onClick.AddListener(OnAction);
            m_canvas.enabled = false;
            DontDestroyOnLoad(gameObject);
        }

        static ContentUpdatePrompt Open()
        {
            if (s_instance == null)
                throw new InvalidOperationException("Init 场景缺少 ContentUpdatePrompt.");
            s_instance.m_canvas.enabled = true;
            s_instance.m_button.interactable = true;
            s_instance.m_button.Select();
            return s_instance;
        }

        public static void ShowRestart()
        {
            var prompt = Open();
            prompt.m_restart = true;
            prompt.RefreshText();
        }

        public static async UniTask WaitForRetryAsync(CancellationToken token)
        {
            var prompt = Open();
            prompt.m_retry = new UniTaskCompletionSource();
            prompt.RefreshText();
            try { await prompt.m_retry.Task.AttachExternalCancellation(token); }
            finally
            {
                if (prompt != null && !prompt.m_restart)
                {
                    prompt.m_canvas.enabled = false;
                    prompt.m_retry = null;
                }
            }
        }

        void RefreshText()
        {
            m_message.font = m_action.font = LocalizationService.CurrentFont;
            m_message.text = LocalizationService.GetText(m_restart ? "ui.content.restart_required" : "ui.content.check_failed");
            m_action.text = LocalizationService.GetText(m_restart ? "ui.content.exit" : "ui.content.retry");
        }

        void OnAction()
        {
            if (m_restart) Application.Quit();
            else
            {
                m_button.interactable = false;
                m_retry?.TrySetResult();
            }
        }

        void OnDestroy()
        {
            if (s_instance == this) s_instance = null;
            m_retry?.TrySetCanceled();
        }
    }
}
