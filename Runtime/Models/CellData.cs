using System;

namespace GSheetsCommander
{
    /// <summary>Value and location of a single cell.</summary>
    [Serializable]
    public sealed class CellData
    {
        public string sheet;
        public string cell;
        public object value;
    }
}
