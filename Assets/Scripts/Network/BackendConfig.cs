using System;

namespace AChen.Networking
{
    public sealed class BackendConfig
    {
        public const string LocalDevelopmentUrl = "http://127.0.0.1:5080";

        public string BaseUrl { get; }
        public int TimeoutSeconds { get; }

        public BackendConfig(string baseUrl = LocalDevelopmentUrl, int timeoutSeconds = 10)
        {
            if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out Uri uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                throw new ArgumentException("Backend URL must be an absolute HTTP or HTTPS URL.", nameof(baseUrl));
            }

            if (timeoutSeconds < 1 || timeoutSeconds > 120)
            {
                throw new ArgumentOutOfRangeException(nameof(timeoutSeconds), "Timeout must be between 1 and 120 seconds.");
            }

            BaseUrl = baseUrl.TrimEnd('/');
            TimeoutSeconds = timeoutSeconds;
        }
    }
}
