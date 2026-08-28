using System;

namespace GSheetsCommander
{
    /// <summary>Health-check data supplied by the backend.</summary>
    [Serializable]
    public sealed class HealthData
    {
        public bool healthy;
        public string status;
        public string message;
    }
}
