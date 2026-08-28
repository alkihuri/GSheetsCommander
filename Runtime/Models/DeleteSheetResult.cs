using System;

namespace GSheetsCommander
{
    /// <summary>Result of deleting a sheet.</summary>
    [Serializable]
    public sealed class DeleteSheetResult
    {
        public string name;
        public bool deleted;
    }
}
