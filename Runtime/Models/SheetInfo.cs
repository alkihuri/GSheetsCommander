using System;

namespace GSheetsCommander
{
    /// <summary>Metadata for one sheet tab.</summary>
    [Serializable]
    public sealed class SheetInfo
    {
        public string name;
        public int sheetId;
        public int rowCount;
        public int columnCount;
    }
}
