using System;

namespace GSheetsCommander
{
    /// <summary>An error returned by GSheetsCommander or its backend.</summary>
    public sealed class GoogleSheetsException : Exception
    {
        /// <summary>The stable machine-readable error code.</summary>
        public string Code { get; }
        /// <summary>Optional backend-provided error details.</summary>
        public object Details { get; }

        public GoogleSheetsException(string code, string message, object details = null, Exception innerException = null)
            : base(message, innerException)
        {
            Code = string.IsNullOrWhiteSpace(code) ? "INTERNAL_ERROR" : code;
            Details = details;
        }
    }
}
