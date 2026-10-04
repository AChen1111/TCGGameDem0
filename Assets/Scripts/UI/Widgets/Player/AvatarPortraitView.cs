using System;
using AChen.Networking;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

/// <summary>所有头像共用的遮罩与框层。引用由预制体绑定，异步回写只接受当前绑定。</summary>
public sealed class AvatarPortraitView : MonoBehaviour
{
    [SerializeField] Image m_ImgAvatar;
    [SerializeField] Image m_ImgMask;
    [SerializeField] Image m_ImgFrame;
    [SerializeField] AspectRatioFitter m_AvatarAspect;
    int m_version;

    public void SetPortrait(int avatarId, int frameId) => LoadAsync(avatarId, frameId, ++m_version).Forget();

    async UniTask LoadAsync(int avatarId, int frameId, int version)
    {
        var lifetime = destroyCancellationToken;
        m_ImgAvatar.enabled = m_ImgFrame.enabled = false;
        try
        {
            var store = GameConfigManager.Instance.Store;
            var frame = store.AvatarFrames[frameId];
            var sprites = await UniTask.WhenAll(new[] {
                AddressableLoader.Instance.LoadSprite(store.Avatars[avatarId].ResourceKey),
                AddressableLoader.Instance.LoadSprite(frame.ResourceKey),
                AddressableLoader.Instance.LoadSprite(frame.MaskResourceKey) });
            if (version != m_version || lifetime.IsCancellationRequested) return;
            Apply(sprites[0], sprites[1], sprites[2]);
        }
        catch (Exception e)
        {
            if (version == m_version && !lifetime.IsCancellationRequested)
                ALog.LogError($"头像组合加载失败: Avatar={avatarId}; Frame={frameId}; {e.Message}", ALogCategories.UI);
        }
    }

    public void Apply(Sprite avatar, Sprite frame, Sprite mask)
    {
        m_ImgMask.sprite = mask;
        m_ImgAvatar.sprite = avatar;
        m_AvatarAspect.aspectRatio = avatar.rect.width / avatar.rect.height;
        m_ImgFrame.sprite = frame;
        m_ImgAvatar.enabled = m_ImgFrame.enabled = true;
    }

    void OnDisable() => ++m_version;
}
