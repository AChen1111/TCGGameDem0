using System.Collections.Generic;
using AChen.Events;
using AChen.Networking;

namespace AChen.Player
{
    /// <summary>
    /// 比较前后两份玩家资料, 只对发生变化的字段派发事件.
    /// 切换玩家(Id 不同)或清空会话(current 为 null)时所有字段事件都会重放, 让界面整体刷新.
    /// </summary>
    public static class PlayerChangePublisher
    {
        public static void Publish(PlayerData previous, PlayerData current)
        {
            bool identityChanged = previous?.Id != current?.Id;
            bool avatarsChanged = identityChanged || !SameIds(previous?.OwnedAvatarIds, current?.OwnedAvatarIds);
            bool wallpapersChanged = identityChanged || !SameIds(previous?.OwnedBackgroundIds, current?.OwnedBackgroundIds);
            bool cardsChanged = identityChanged || !SameCards(previous?.OwnedCards, current?.OwnedCards);

            if (identityChanged || previous?.Gold != current?.Gold)
                EventCenter.Dispatch(GameEvent.PlayerGoldChanged, current?.Gold);
            if (identityChanged || previous?.Nickname != current?.Nickname)
                EventCenter.Dispatch(GameEvent.PlayerNicknameChanged, current?.Id, current?.Nickname);
            if (identityChanged || previous?.AvatarId != current?.AvatarId)
                EventCenter.Dispatch(GameEvent.PlayerAvatarChanged, current?.AvatarId);
            if (identityChanged || previous?.BackgroundId != current?.BackgroundId)
                EventCenter.Dispatch(GameEvent.PlayerBackgroundChanged, current?.BackgroundId);
            if (avatarsChanged)
                EventCenter.Dispatch(GameEvent.PlayerOwnedAvatarsChanged, current);
            if (wallpapersChanged)
                EventCenter.Dispatch(GameEvent.PlayerOwnedWallpapersChanged, current);
            if (cardsChanged)
                EventCenter.Dispatch(GameEvent.PlayerOwnedCardsChanged, current);
        }

        static bool SameIds(IReadOnlyList<int> previous, IReadOnlyList<int> current) =>
            ReferenceEquals(previous, current) ||
            (previous != null && current != null && new HashSet<int>(previous).SetEquals(current));

        static bool SameCards(IReadOnlyList<OwnedCardData> previous, IReadOnlyList<OwnedCardData> current)
        {
            if (ReferenceEquals(previous, current))
            {
                return true;
            }

            if (previous == null || current == null || previous.Count != current.Count)
            {
                return false;
            }

            for (int i = 0; i < previous.Count; i++)
            {
                OwnedCardData left = previous[i];
                OwnedCardData right = current[i];
                if (left.CardId != right.CardId || left.Rarity != right.Rarity || left.Count != right.Count)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
