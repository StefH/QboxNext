using System;

namespace QboxNext.Qboxes.Parsing
{
    /// <summary>
    /// Describes a parser type and its maximum supported protocol version.
    /// </summary>
    public class ParserInfo
    {
        public int MaxProtocolVersion { get; set; }
        public Type Type { get; set; }
    }
}