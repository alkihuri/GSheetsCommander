using System;

namespace GSheetsCommander
{
    /// <summary>Structured API error returned by the backend.</summary>
    [Serializable]
    public sealed class GoogleSheetsError
    {
        public string code;
        public string message;
        public object details;
    }
}
