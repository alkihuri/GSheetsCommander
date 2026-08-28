using System;

namespace GSheetsCommander
{
    /// <summary>Values and location of a row.</summary>
    [Serializable]
    public sealed class RowData
    {
        public string sheet;
        public int row;
        public object[] values;
    }
}
