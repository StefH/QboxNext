using System;

namespace QboxNext.Qboxes.Parsing.Protocols
{
    /// <summary>
    /// Describes a factory for creating <see cref="ProtocolReader"/>s.
    /// </summary>
    public interface IProtocolReaderFactory
    {
        /// <summary>
        /// Creates a new protocol reader for specified <see cref="memory"/>.
        /// </summary>
        /// <param name="memory"></param>
        /// <returns></returns>
        IProtocolReader Create(ReadOnlyMemory<char> memory);
    }
}
