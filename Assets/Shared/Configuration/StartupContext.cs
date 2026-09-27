using System;

namespace AChen.Configuration
{
    /// <summary>Validated startup results passed from the AOT loader to hot-update code.</summary>
    [Serializable]
    public sealed class StartupContext
    {
        public string BackendUrl;
        public string Target;
        public string ConfigHash;
        public string Channel;
        public string Platform;
        public string AppVersion;
        public string ReleaseId;
        public ConfigArtifact[] Configs;
        public string CatalogUrl;
        public string AddressablesBaseUrl;
        public DateTimeOffset ServerTime;
        public DateTimeOffset ServerTimeReceivedAt;
        public bool UseLocalAssets;
    }
}
