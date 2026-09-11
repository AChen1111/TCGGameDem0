using System;
using AChen.Networking;
using UnityEngine.SceneManagement;

namespace AChen.Events
{
    /// <summary>
    /// 玩家、配置、流程与场景事件目录. 业务只在这里声明 EventId, 不要在调用处写事件字符串.
    /// UI 框架内部事件见 UIEvent.
    /// </summary>
    public static class GameEvent
    {
        /// <summary>注册成功并建立本地会话时触发。参数：<c>AuthUser</c>、<c>PlayerData</c>。</summary>
        public static readonly EventId<AuthUser, PlayerData> PlayerRegistered = new EventId<AuthUser, PlayerData>("Player.Registered");

        /// <summary>账号密码验证成功并建立本地会话时触发。参数：<c>AuthUser</c>、<c>PlayerData</c>。</summary>
        public static readonly EventId<AuthUser, PlayerData> PlayerLoggedIn = new EventId<AuthUser, PlayerData>("Player.LoggedIn");

        /// <summary>登出或本地会话被清理后触发。参数：清理前的 <c>AuthUser</c>。</summary>
        public static readonly EventId<AuthUser> PlayerLoggedOut = new EventId<AuthUser>("Player.LoggedOut");

        // 字段级变更事件: 切换玩家时即使字段值相同也通知; null 表示清空会话显示.
        public static readonly EventId<long?> PlayerGoldChanged = new EventId<long?>("Player.GoldChanged");
        public static readonly EventId<Guid?, string> PlayerNicknameChanged = new EventId<Guid?, string>("Player.NicknameChanged");
        public static readonly EventId<int?> PlayerAvatarChanged = new EventId<int?>("Player.AvatarChanged");
        public static readonly EventId<int?> PlayerBackgroundChanged = new EventId<int?>("Player.BackgroundChanged");
        public static readonly EventId<PlayerData> PlayerOwnedAvatarsChanged = new EventId<PlayerData>("Player.OwnedAvatarsChanged");
        public static readonly EventId<PlayerData> PlayerOwnedWallpapersChanged = new EventId<PlayerData>("Player.OwnedWallpapersChanged");

        /// <summary>游戏配置快照被替换或过期状态变化后触发。参数：<c>GameConfigSnapshot</c>、<c>bool isStale</c>。</summary>
        public static readonly EventId<GameConfigSnapshot, bool> GameConfigChanged = new EventId<GameConfigSnapshot, bool>("GameConfig.Changed");

        public static readonly EventId LobbyEntering = new EventId("Game.LobbyEntering");
        public static readonly EventId LobbyEntered = new EventId("Game.LobbyEntered");
        public static readonly EventId<string> LobbyEntryFailed = new EventId<string>("Game.LobbyEntryFailed");
        public static readonly EventId GameExitRequested = new EventId("Game.ExitRequested");

        public static readonly EventId<string, LoadSceneMode> SceneLoadStarted = new EventId<string, LoadSceneMode>("Scene.LoadStarted");
        public static readonly EventId<string, Exception> SceneLoadFailed = new EventId<string, Exception>("Scene.LoadFailed");
    }
}
