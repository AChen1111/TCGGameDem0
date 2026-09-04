using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>大厅头像/商城的本地假数据,正式数据接入后替换调用即可.</summary>
public static class PreGameUiFakeData
{
    const int AvatarCount = 14;
    const int CardPackCount = 11;

    public static async UniTask<List<AvatarItemData>> CreateAvatarsAsync()
    {
        var loads = new UniTask<Sprite>[AvatarCount];
        for (int i = 0; i < AvatarCount; i++)
        {
            loads[i] = AddressableLoader.Instance.LoadSprite(AddressKeys.GetAvatarAddress(i));
        }

        Sprite[] sprites = await UniTask.WhenAll(loads);
        var list = new List<AvatarItemData>(AvatarCount);
        for (int i = 0; i < AvatarCount; i++)
        {
            list.Add(new AvatarItemData
            {
                Id = i,
                Name = $"头像{i:D2}",
                Sprite = sprites[i],
                Owned = i % 2 == 0
            });
        }

        return list;
    }

    public static async UniTask<List<ShopCardItemData>> CreateCardPacksAsync()
    {
        var loads = new UniTask<Sprite>[CardPackCount];
        for (int i = 0; i < CardPackCount; i++)
        {
            loads[i] = AddressableLoader.Instance.LoadSprite($"c_{i:D2}");
        }

        Sprite[] sprites = await UniTask.WhenAll(loads);
        DateTimeOffset now = DateTimeOffset.UtcNow;
        var list = new List<ShopCardItemData>(CardPackCount);
        for (int i = 0; i < CardPackCount; i++)
        {
            DateTimeOffset? endsAt = i % 3 == 0
                ? (DateTimeOffset?)null
                : i % 3 == 1
                    ? now.AddHours(6)
                    : now.AddDays(3);
            list.Add(new ShopCardItemData(
                i + 1,
                $"卡包{i:D2}",
                sprites[i],
                100L * (i + 1),
                endsAt,
                i));
        }

        return list;
    }
}
