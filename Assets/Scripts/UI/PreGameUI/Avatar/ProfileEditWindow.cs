using System;
using System.Collections.Generic;
using System.Linq;
using AChen.Events;
using AChen.Networking;
using AChen.Player;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum ProfileEditTab { Name, Avatar, AvatarFrame }

public sealed class ProfileEditWindowProperties : IWindowProperties
{
    public ProfileEditTab Tab { get; }
    public ProfileEditWindowProperties(ProfileEditTab tab) { Tab = tab; }
}

/// <summary>三个独立草稿；确认只保存当前页，关闭放弃未提交内容。</summary>
public sealed class ProfileEditWindow : AWindowController<ProfileEditWindowProperties>
{
    // --tag_start: 自动生成--
    [SerializeField] Button m_BtnClose;
    [SerializeField] Button m_BtnName;
    [SerializeField] Button m_BtnAvatar;
    [SerializeField] Button m_BtnFrame;
    [SerializeField] TextMeshProUGUI m_TxtSelected;
    [SerializeField] GameObject m_GoNamePage;
    [SerializeField] TMP_InputField m_InpName;
    [SerializeField] GameObject m_GoCosmetics;
    [SerializeField] Button m_BtnConfirm;
    // --tag_end: 自动生成--
    [SerializeField] GridListController m_List;
    [SerializeField] AvatarPortraitView m_Preview;
    [SerializeField] Image[] m_TabImages;
    [SerializeField] TMP_Text[] m_TabLabels;
    [SerializeField] Sprite m_TabNormal;
    [SerializeField] Sprite m_TabSelected;
    ProfileEditTab m_tab;
    int m_avatarDraft, m_frameDraft;
    bool m_submitting;
    List<ProfileCosmeticData> m_items;

    protected override void AddListeners()
    {
        m_BtnName.onClick.AddListener(ShowName);
        m_BtnAvatar.onClick.AddListener(ShowAvatar);
        m_BtnFrame.onClick.AddListener(ShowFrame);
        m_BtnConfirm.onClick.AddListener(Confirm);
        m_BtnClose.onClick.AddListener(UI_Close);
    }
    protected override void RemoveListeners()
    {
        m_BtnName.onClick.RemoveListener(ShowName);
        m_BtnAvatar.onClick.RemoveListener(ShowAvatar);
        m_BtnFrame.onClick.RemoveListener(ShowFrame);
        m_BtnConfirm.onClick.RemoveListener(Confirm);
        m_BtnClose.onClick.RemoveListener(UI_Close);
    }
    protected override void OnOpen()
    {
        var player = PlayerSession.Instance.CurrentPlayer;
        m_InpName.SetTextWithoutNotify(player.Nickname);
        m_avatarDraft = player.AvatarId.Value;
        m_frameDraft = player.AvatarFrameId;
        m_submitting = false;
        EventCenter.AddListener(GameEvent.PlayerOwnedAvatarsChanged, InventoryChanged);
        EventCenter.AddListener(GameEvent.PlayerOwnedAvatarFramesChanged, InventoryChanged);
        EventCenter.AddListener(GameEvent.GameConfigChanged, ConfigChanged);
        Show(Properties.Tab);
    }
    protected override void OnResume() => Refresh();
    protected override void OnClose()
    {
        EventCenter.RemoveListener(GameEvent.PlayerOwnedAvatarsChanged, InventoryChanged);
        EventCenter.RemoveListener(GameEvent.PlayerOwnedAvatarFramesChanged, InventoryChanged);
        EventCenter.RemoveListener(GameEvent.GameConfigChanged, ConfigChanged);
    }
    void InventoryChanged(PlayerData _) => Refresh();
    void ConfigChanged(GameConfigSnapshot _, bool stale) => Refresh();
    void ShowName() => Show(ProfileEditTab.Name);
    void ShowAvatar() => Show(ProfileEditTab.Avatar);
    void ShowFrame() => Show(ProfileEditTab.AvatarFrame);
    void Show(ProfileEditTab tab)
    {
        m_tab = tab;
        m_GoNamePage.SetActive(tab == ProfileEditTab.Name);
        m_GoCosmetics.SetActive(tab != ProfileEditTab.Name);
        Button[] tabs = { m_BtnName, m_BtnAvatar, m_BtnFrame };
        for (int i = 0; i < m_TabImages.Length; i++)
        {
            // 当前页的底图由页签状态决定，避免 Selectable 用普通图覆盖绿色选中图。
            tabs[i].transition = i == (int)tab ? Selectable.Transition.None : Selectable.Transition.SpriteSwap;
            m_TabImages[i].sprite = i == (int)tab ? m_TabSelected : m_TabNormal;
            m_TabImages[i].overrideSprite = null;
            m_TabImages[i].color = i == (int)tab ? Color.white : new Color(.12f, .23f, .29f);
            m_TabLabels[i].color = i == (int)tab ? Color.black : Color.white;
        }
        Refresh();
    }
    void Refresh()
    {
        var player = PlayerSession.Instance.CurrentPlayer;
        var store = GameConfigManager.Instance.Store;
        m_BtnConfirm.interactable = !m_submitting;
        if (m_tab == ProfileEditTab.Name)
        {
            m_TxtSelected.text = "玩家名";
            m_Preview.SetPortrait(player.AvatarId.Value, player.AvatarFrameId);
            return;
        }
        bool avatars = m_tab == ProfileEditTab.Avatar;
        IEnumerable<CosmeticConfig> configs = avatars ? store.Avatars.Values.Cast<CosmeticConfig>() : store.AvatarFrames.Values;
        var owned = new HashSet<int>(avatars ? player.OwnedAvatarIds : player.OwnedAvatarFrameIds);
        m_items = configs.Where(x => x.IsEnabled && owned.Contains(x.Id)).OrderBy(x => x.SortOrder).ThenBy(x => x.Id)
            .Select(x => new ProfileCosmeticData(x.Id, avatars ? x.Id : player.AvatarId.Value,
                avatars ? player.AvatarFrameId : x.Id, x.Id == (avatars ? player.AvatarId.Value : player.AvatarFrameId))).ToList();
        int draft = avatars ? m_avatarDraft : m_frameDraft;
        int selected = m_items.FindIndex(x => x.Id == draft);
        m_List.InitList(AddressKeys.Prefab.ProfileCosmeticRow, m_items, Select, selected, ScreenToken).Forget();
        m_TxtSelected.text = avatars ? store.Avatars[draft].Name : store.AvatarFrames[draft].Name;
        m_Preview.SetPortrait(avatars ? draft : player.AvatarId.Value, avatars ? player.AvatarFrameId : draft);
    }
    void Select(int index)
    {
        if (m_tab == ProfileEditTab.Avatar) m_avatarDraft = m_items[index].Id;
        else m_frameDraft = m_items[index].Id;
        Refresh();
    }
    void Confirm() => SaveAsync().Forget();
    async UniTaskVoid SaveAsync()
    {
        if (m_submitting) return;
        m_submitting = true;
        m_BtnConfirm.interactable = false;
        m_BtnName.interactable = m_BtnAvatar.interactable = m_BtnFrame.interactable = false;
        var tab = m_tab;
        string name = m_InpName.text;
        int avatar = m_avatarDraft, frame = m_frameDraft;
        bool success = await RunGuardedAsync(async token =>
        {
            switch (tab)
            {
                case ProfileEditTab.Name: await PlayerSession.Instance.RenameAsync(name, token); break;
                case ProfileEditTab.Avatar: await PlayerSession.Instance.SetAvatarAsync(avatar, token); break;
                case ProfileEditTab.AvatarFrame: await PlayerSession.Instance.SetAvatarFrameAsync(frame, token); break;
            }
        }, "保存个人资料", "err.profile_save_failed");
        if (!IsOpened) return;
        m_submitting = false;
        m_BtnName.interactable = m_BtnAvatar.interactable = m_BtnFrame.interactable = true;
        if (success && tab == ProfileEditTab.Name) m_InpName.SetTextWithoutNotify(PlayerSession.Instance.CurrentPlayer.Nickname);
        Refresh();
    }
}
