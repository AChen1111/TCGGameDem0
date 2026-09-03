using System;
using AChen.Networking;
using AChen.Player;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChangeNameWindow : AWindowController, IPlayerDataView
{
    // --tag_start: 自动生成--
    [SerializeField] TMP_InputField m_InpName;
    [SerializeField] Button m_BtnOk;
    [SerializeField] Button m_BtnNo;
    // --tag_end: 自动生成--

    protected override void AddListeners()
    {
        m_BtnOk.onClick.AddListener(OnBtnOkClicked);
        m_BtnNo.onClick.AddListener(OnBtnNoClicked);
    }

    protected override void RemoveListeners()
    {
        m_BtnOk.onClick.RemoveListener(OnBtnOkClicked);
        m_BtnNo.onClick.RemoveListener(OnBtnNoClicked);
    }

    protected override void OnOpen()
    {
        PlayerDataViews.Bind(this);
    }

    protected override void OnClose()
    {
        PlayerDataViews.Unbind(this);
    }

    public void OnPlayerDataChanged(PlayerData data)
    {
        m_InpName.text = data != null ? data.Nickname : string.Empty;
    }

    private void OnBtnOkClicked()
    {
        SubmitAsync().Forget();
    }

    private async UniTaskVoid SubmitAsync()
    {
        string nickname = m_InpName.text.Trim();
        if (nickname.Length is < 2 or > 24)
        {
            ShowMessage("昵称需为 2-24 个字符");
            return;
        }

        PlayerData player = PlayerSession.Instance.CurrentPlayer;
        if (player == null)
        {
            ShowMessage("修改昵称失败");
            return;
        }

        try
        {
            await PlayerSession.Instance.UpdatePlayerProfileAsync(
                nickname,
                player.AvatarId,
                player.BackgroundId,
                player.Revision,
                this.GetCancellationTokenOnDestroy());
            ALog.Log($"修改昵称成功. Nickname={nickname}", ALogCategories.UI);
            UI_Close();
        }
        catch (BackendApiException exception)
        {
            ALog.LogError(
                $"修改昵称失败. Code={exception.Code}; Status={exception.StatusCode}",
                ALogCategories.UI);
            ShowMessage("修改昵称失败");
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void OnBtnNoClicked()
    {
        UI_Close();
    }

    void ShowMessage(string message)
    {
        m_UIFrame.OpenWindow(
            AddressKeys.Prefab.MessageWindow,
            new MessageWindowProperties(message, 2f));
    }
}
