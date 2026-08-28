using System;

namespace GSheetsCommander
{
    /// <summary>Envelope sent to a Google Apps Script endpoint.</summary>
    [Serializable]
    public sealed class GoogleSheetsRequest
    {
        public string action;
        public string requestId;
        public string apiKey;
        public object payload;
    }
}
