using System;
using System.Buffers.Binary;
using Microsoft.Extensions.Logging;

namespace QboxNext.Qboxes.Parsing.Protocols
{
    /// <summary>
    /// A reader for Qbox protocol(s).
    /// </summary>
    public class ProtocolReader : IProtocolReader
    {
        private static readonly DateTime Epoch = new DateTime(2007, 1, 1);

        private readonly ILogger<ProtocolReader> _logger;
        private readonly ReadOnlyMemory<char> _data;
        private int _position;

        public ProtocolReader(ILogger<ProtocolReader> logger, ReadOnlyMemory<char> memory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _logger.LogTrace("{Source}", memory.ToString());
            _data = memory;
        }

        public ProtocolReader(ILogger<ProtocolReader> logger, string data)
            : this(logger, (data ?? "").AsMemory())
        {
        }

        /// <inheritdoc />
        public byte ReadByte()
        {
            return AdvanceRead(2, len => FromHex(_data.Span.Slice(_position, len))[0]);
        }

        /// <inheritdoc />
        public DateTime ReadDateTime()
        {
            int seconds = ReadInt32();
            return Epoch.AddSeconds(seconds);
        }

        /// <inheritdoc />
        public int ReadInt32()
        {
            return AdvanceRead(8, len => BinaryPrimitives.ReadInt32BigEndian(FromHex(_data.Span.Slice(_position, len))));
        }

        /// <inheritdoc />
        public short ReadInt16()
        {
            return AdvanceRead(4, len => BinaryPrimitives.ReadInt16BigEndian(FromHex(_data.Span.Slice(_position, len))));
        }

        /// <inheritdoc />
        public uint ReadUInt32()
        {
            return AdvanceRead(8, len => BinaryPrimitives.ReadUInt32BigEndian(FromHex(_data.Span.Slice(_position, len))));
        }

        /// <inheritdoc />
        public ushort ReadUInt16()
        {
            return AdvanceRead(4, len => BinaryPrimitives.ReadUInt16BigEndian(FromHex(_data.Span.Slice(_position, len))));
        }

        /// <inheritdoc />
        public string ReadEncapsulatedString(char character)
        {
            int count = 0;
            int nextDelimiterPos = -1;

            if (_data.Span[_position] == character)
            {
                // Find second encapsulating char, otherwise end of string.
                // ReSharper disable once ArrangeRedundantParentheses (for clarity!)
                ReadOnlySpan<char> span = _data.Span.Slice(_position + (++count));
                nextDelimiterPos = span.IndexOf(character);
                count += nextDelimiterPos == -1 ? span.Length : nextDelimiterPos + 1;
            }

            return AdvanceRead(count, len =>
                {
                    if (count == 0)
                    {
                        throw new ProtocolReaderException("Expected delimiter.", _data, _position);
                    }

                    return _data.Span.Slice(
                        // Skip first encapsulating char.
                        _position + 1,
                        // Subtract encapsulating chars.
                        (nextDelimiterPos >= 0 ? len - 1 : len) - 1).ToString();
                });
        }

        /// <inheritdoc />
        public string ReadToEnd()
        {
            return AdvanceRead(_data.Length - _position, len => _data.Span.Slice(_position, len).ToString());
        }

        /// <inheritdoc />
        public bool AtEndOfStream()
        {
            return _position >= _data.Length;
        }

        /// <summary>
        /// Ensures when reading <paramref name="byteCount"/> of data, the stream position is updated accordingly.
        /// </summary>
        private T AdvanceRead<T>(int byteCount, Func<int, T> readFunc)
        {
            try
            {
                if (_position + byteCount > _data.Length)
                {
                    throw new ProtocolReaderException($"Reached end of stream while attempting to read {byteCount} bytes of data.", _data, _position);
                }

                try
                {
                    return readFunc(byteCount);
                }
                catch (Exception ex) when (!(ex is ProtocolReaderException))
                {
                    throw new ProtocolReaderException($"Protocol read error occurred at position {_position}.", _data, _position, ex);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Protocol read error occurred at position {Position} in {ProtocolMessage}.", _position, _data.ToString());
                throw;
            }
            finally
            {
                _position = Math.Min(_data.Length, _position + byteCount);
            }
        }

        /// <summary>
        /// Converts the <paramref name="hex"/> span to a byte span.
        /// </summary>
        private static Span<byte> FromHex(ReadOnlySpan<char> hex)
        {
            int byteCount = hex.Length / 2;
            byte[] bytes = new byte[byteCount];
            for (int i = 0; i < byteCount; i++)
            {
                bytes[i] = Convert.ToByte(hex.Slice(i * 2, 2).ToString(), 16);
            }

            return bytes;
        }
    }
}