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
    [SerializeField] bool m_listThumbnail;
    [SerializeField] AvatarCompositeGraphic m_composite;
    int m_version;

    public void SetPortrait(int avatarId, int frameId) => LoadAsync(avatarId, frameId, ++m_version).Forget();

    async UniTask LoadAsync(int avatarId, int frameId, int version)
    {
        var lifetime = destroyCancellationToken;
        if(m_listThumbnail)m_composite.enabled=false;
        else m_ImgAvatar.enabled = m_ImgFrame.enabled = false;
        try
        {
            var store = GameConfigManager.Instance.Store;
            var frame = store.AvatarFrames[frameId];
            if(m_listThumbnail)
            {
                var thumbnails=await UniTask.WhenAll(new[]{
                    AddressableLoader.Instance.LoadAtlasSprite(AddressKeys.Atlas.PortraitThumbnails,"thumb_"+store.Avatars[avatarId].ResourceKey),
                    AddressableLoader.Instance.LoadAtlasSprite(AddressKeys.Atlas.PortraitThumbnails,"thumb_"+frame.ResourceKey),
                    AddressableLoader.Instance.LoadAtlasSprite(AddressKeys.Atlas.PortraitThumbnails,"thumb_"+frame.MaskResourceKey)});
                if(version!=m_version || lifetime.IsCancellationRequested)return;
                Apply(thumbnails[0],thumbnails[1],thumbnails[2]);
                return;
            }
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
        if(m_listThumbnail)
        {m_composite.SetPortrait(avatar,frame,mask);m_composite.enabled=true;return;}
        m_ImgMask.sprite = mask;
        m_ImgAvatar.sprite = avatar;
        m_AvatarAspect.aspectRatio = avatar.rect.width / avatar.rect.height;
        m_ImgFrame.sprite = frame;
        m_ImgAvatar.enabled = m_ImgFrame.enabled = true;
    }

    void OnDisable() => ++m_version;
}
