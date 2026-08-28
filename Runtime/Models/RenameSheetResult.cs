using System;

namespace GSheetsCommander
{
    /// <summary>Result of renaming a sheet.</summary>
    [Serializable]
    public sealed class RenameSheetResult
    {
        public string previousName;
        public string newName;
    }
}
