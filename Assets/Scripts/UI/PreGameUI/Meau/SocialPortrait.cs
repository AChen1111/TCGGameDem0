using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

static class SocialPortrait
{
    public static async UniTaskVoid ApplyAsync(Image image, int? avatarId, int version, Func<int> current)
    {
        if (image == null)
        {
            return;
        }

        try
        {
            Sprite sprite = await AddressableLoader.Instance.LoadSprite(AddressKeys.GetAvatarAddress(avatarId ?? 0));
            if (image != null && version == current())
            {
                image.sprite = sprite;
            }
        }
        catch (Exception exception)
        {
            ALog.LogWarning($"加载社交头像失败. Avatar={avatarId}; Error={exception.Message}", ALogCategories.UI);
        }
    }
}
