using System;

namespace GSheetsCommander
{
    /// <summary>Values and location of a cell range.</summary>
    [Serializable]
    public sealed class RangeData
    {
        public string sheet;
        public string range;
        public object[][] values;
    }
}
