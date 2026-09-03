using UnityEngine;

namespace GSheetsCommander
{
    /// <summary>Configuration used by <see cref="GoogleSheetsClient"/>.</summary>
    [CreateAssetMenu(fileName = "GoogleSheetsConfig", menuName = "GSheetsCommander/Google Sheets Config")]
    public sealed class GoogleSheetsConfig : ScriptableObject
    {
        [Header("Google Sheets Config")]
        [SerializeField] private string googleAppsScriptUrl;
        [SerializeField] private string apiKey;
        [SerializeField, Min(1)] private int timeoutSeconds = 10;
        [SerializeField] private bool enableLogging;
        [SerializeField] private bool useGetRequests;

        /// <summary>The deployed Google Apps Script web-app URL.</summary>
        public string GoogleAppsScriptUrl => googleAppsScriptUrl;
        /// <summary>The API key expected by the backend.</summary>
        public string ApiKey => apiKey;
        /// <summary>HTTP request timeout in seconds.</summary>
        public int TimeoutSeconds => timeoutSeconds;
        /// <summary>Whether request and response metadata is logged.</summary>
        public bool EnableLogging => enableLogging;
        /// <summary>Whether requests should be sent as URL query parameters instead of a JSON POST body.</summary>
        public bool UseGetRequests => useGetRequests;

        /// <summary>Configures this asset programmatically, including for test fixtures.</summary>
        public void Configure(string url, string key, int timeout = 10, bool logging = false, bool useGetRequests = false)
        {
            googleAppsScriptUrl = url;
            apiKey = key;
            timeoutSeconds = timeout;
            enableLogging = logging;
            this.useGetRequests = useGetRequests;
        }

        /// <summary>Validates required configuration and throws a predictable exception when invalid.</summary>
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(googleAppsScriptUrl) || !System.Uri.TryCreate(googleAppsScriptUrl, System.UriKind.Absolute, out _))
                throw new GoogleSheetsException("CONFIGURATION_ERROR", "A valid Google Apps Script URL is required.");
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new GoogleSheetsException("CONFIGURATION_ERROR", "An API key is required.");
            if (timeoutSeconds < 1)
                throw new GoogleSheetsException("CONFIGURATION_ERROR", "Timeout Seconds must be at least 1.");
        }
    }
}
