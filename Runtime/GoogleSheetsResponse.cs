using System;

namespace GSheetsCommander
{
    /// <summary>Envelope returned by a Google Apps Script endpoint.</summary>
    [Serializable]
    public sealed class GoogleSheetsResponse<T>
    {
        public bool success;
        public string requestId;
        public T data;
        public GoogleSheetsError error;
    }
}
