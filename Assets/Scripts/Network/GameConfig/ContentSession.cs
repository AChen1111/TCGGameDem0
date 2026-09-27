namespace AChen.Configuration
{
    public static class ContentSession
    {
        public static void Bind(StartupContext context)
        {
            if (context == null) throw new System.ArgumentNullException(nameof(context));
            BackendUrl = context.BackendUrl; Target = context.Target;
            ConfigHash = context.ConfigHash; Channel = context.Channel;
            Platform = context.Platform; AppVersion = context.AppVersion;
            ReleaseId = context.ReleaseId; Configs = context.Configs;
            CatalogUrl = context.CatalogUrl; ServerTime = context.ServerTime;
            ServerTimeReceivedAt = context.ServerTimeReceivedAt;
            UseLocalAssets = context.UseLocalAssets; RestartRequired = false;
        }
        public static string BackendUrl;
        public static string Target;
        public static string ConfigHash;
        public static string Channel;
        public static string Platform;
        public static string AppVersion;
        public static string ReleaseId;
        public static ConfigArtifact[] Configs;
        public static string CatalogUrl;
        public static System.DateTimeOffset ServerTime;
        public static System.DateTimeOffset ServerTimeReceivedAt;
        public static bool RestartRequired;
        /// <summary>Editor 默认走工程内资源与配置, 不拉远程内容包.</summary>
        public static bool UseLocalAssets;
    }
}
