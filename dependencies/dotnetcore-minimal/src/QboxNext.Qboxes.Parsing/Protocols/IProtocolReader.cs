using System;

namespace QboxNext.Qboxes.Parsing.Protocols
{
    /// <summary>
    /// Describes reader for Qbox protocol(s).
    /// </summary>
    public interface IProtocolReader
    {
        /// <summary>
        /// Reads the next byte.
        /// </summary>
        /// <returns>The next byte value.</returns>
        /// <exception cref="ProtocolReaderException">Thrown when a read error occurs.</exception>
        byte ReadByte();

        /// <summary>
        /// Reads the next date time.
        /// </summary>
        /// <returns>The next date time value.</returns>
        /// <exception cref="ProtocolReaderException">Thrown when a read error occurs.</exception>
        DateTime ReadDateTime();

        /// <summary>
        /// Reads the next int value.
        /// </summary>
        /// <returns>The next int value.</returns>
        /// <exception cref="ProtocolReaderException">Thrown when a read error occurs.</exception>
        int ReadInt32();

        /// <summary>
        /// Reads the next short value.
        /// </summary>
        /// <returns>The next short value.</returns>
        /// <exception cref="ProtocolReaderException">Thrown when a read error occurs.</exception>
        short ReadInt16();

        /// <summary>
        /// Reads the next uint value.
        /// </summary>
        /// <returns>The next uint value.</returns>
        /// <exception cref="ProtocolReaderException">Thrown when a read error occurs.</exception>
        uint ReadUInt32();

        /// <summary>
        /// Reads the next ushort value.
        /// </summary>
        /// <returns>The next ushort value.</returns>
        /// <exception cref="ProtocolReaderException">Thrown when a read error occurs.</exception>
        ushort ReadUInt16();

        /// <summary>
        /// Reads a string encapsulated by specified character.
        /// </summary>
        /// <param name="character">The character that is encapsulating a string.</param>
        /// <returns>The string between two <paramref name="character" /> characters.</returns>
        /// <exception cref="ProtocolReaderException">Thrown when a read error occurs.</exception>
        string ReadEncapsulatedString(char character);

        /// <summary>
        /// Reads text until the end of the stream.
        /// </summary>
        /// <returns>The remaining text on the stream.</returns>
        /// <exception cref="ProtocolReaderException">Thrown when a read error occurs.</exception>
        string ReadToEnd();

        /// <summary>
        /// Determines if the current position is at the end of the stream.
        /// </summary>
        /// <returns></returns>
        bool AtEndOfStream();
    }
}