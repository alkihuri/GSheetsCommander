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

        /// <summary>The deployed Google Apps Script web-app URL.</summary>
        public string GoogleAppsScriptUrl => googleAppsScriptUrl;
        /// <summary>The API key expected by the backend.</summary>
        public string ApiKey => apiKey;
        /// <summary>HTTP request timeout in seconds.</summary>
        public int TimeoutSeconds => timeoutSeconds;
        /// <summary>Whether request and response metadata is logged.</summary>
        public bool EnableLogging => enableLogging;

        /// <summary>Configures this asset programmatically, including for test fixtures.</summary>
        public void Configure(string url, string key, int timeout = 10, bool logging = false)
        {
            googleAppsScriptUrl = url;
            apiKey = key;
            timeoutSeconds = timeout;
            enableLogging = logging;
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
