using System;
using System.Globalization;
using System.IO;
using QboxNext.Qboxes.Parsing.Logging;

namespace QboxNext.Qboxes.Parsing
{
    public class BinaryParser
    {
        private static readonly ILog Log = LogProvider.GetCurrentClassLogger();

        #region private

        readonly BinaryReader _reader;

        #endregion

        #region public

        public long Length 
        { 
            get 
            { 
                return _reader.BaseStream.Length; 
            } 
        }

        public long Position
        {
            get
            {
                return _reader.BaseStream.Position;
            }
        }

        /// <summary>
        /// Inverts a byte array to enable reading in BigEndian when LittleEndian is in the array
        /// </summary>
        /// <param name="b">a byte array containing an integer to be read</param>
        /// <returns>returns an inverted byte array</returns>
        public static byte[] InvertByteArray(byte[] b)
        {
            var invert = new byte[b.Length];
            for (var i = 0; i < b.Length; i++)
                invert[i] = b[(b.Length - 1) - i];
            return invert;
        }

        public char ReadChar()
        {
            return _reader.ReadChar();
        }

        /// <summary>
        /// Returns the bytes found in the Reader from the current position to the "count" number of bytes
        /// in a new byte array.
        /// </summary>
        /// <param name="count">The number of bytes to read from the Reader</param>
        /// <returns>a new byte array with length of "count"</returns>
        public byte[] GetRealBytes(int count)
        {
            return _reader.ReadBytes(count);
        }

        public int GetSerialDataLength()
        {
            int length = this.ParseInt16(true) - 2;
            Log.Trace(string.Format("Serial data length: {0}", length));
            return length;
        }       

        public int StartByte()
        {
            Log.Trace(string.Format("StartByte() Position: {0}", _reader.BaseStream.Position));
            byte _byte = _reader.ReadByte();
            switch (_byte)
            {
                case 0xFF:
                    Log.Trace("Skip FF");
                    return StartByte();
                case 0x10:
                    Log.Trace("Skip <DLE>");
                    if (_reader.ReadByte() == 0x02)
                    {
                        Log.Trace("Return 02");
                        return 0x02;
                    }
                    else
                        return StartByte();
                default:
                    return StartByte();
            }
        }

        public byte[] GetBytes(int count)
        {
            Log.Trace(string.Format("GetBytes({0})", count));

            byte[] bytes = new byte[count];
            byte input;
            for (int i = 0; i < count; i++)
            {
                input = _reader.ReadByte();
                if (SkipDLE && input == 0x10) // step over <dle> and spaces
                {
                    Log.Trace("Skip <DLE>");
                    input = _reader.ReadByte();
                }
                bytes[i] = input;
            }
            for (int i = 0; i < count; i++)
            {
                Log.Trace(bytes[i].ToString("X2"));
            }
            return bytes;
        }        

        public Int16 ParseInt16(bool invert)
        {
            Log.Trace("ParseInt16");
            byte[] bytes = GetRealBytes(2);            
            Int16 value = BitConverter.ToInt16(invert ? InvertByteArray(bytes) : bytes, 0);
            Log.Debug("ParseInt16: value=" + value.ToString(CultureInfo.InvariantCulture));
            return value; 
        }

        public UInt16 ParseUInt16(bool invert)
        {
            Log.Trace("ParseUInt16");
            byte[] bytes = GetRealBytes(2);
            UInt16 value = BitConverter.ToUInt16(invert ? InvertByteArray(bytes) : bytes, 0);
            Log.Debug("ParseUInt16: value=" + value.ToString(CultureInfo.InvariantCulture));
            return value;
        }

        public UInt16 ParseMSBUInt()
        {
            Log.Trace("ParseMSBUInt");

            byte[] bytes = new byte[2];
            bytes[1] = GetRealBytes(1)[0];
            bytes[0] = GetRealBytes(1)[0];

            UInt16 value = BitConverter.ToUInt16(bytes, 0);
            Log.Debug("ParseMSBUInt: value=" + value.ToString(CultureInfo.InvariantCulture));
            return value; 

        }

        public UInt16 ParseUInt16()
        {
            Log.Trace("ParseUInt16()");
            byte[] bytes = GetRealBytes(2);
            UInt16 value = BitConverter.ToUInt16(bytes, 0);
            Log.Debug("ParseUInt16: value=" + value.ToString(CultureInfo.InvariantCulture));
            return value; 
        }

        public byte ParseByte()
        {
            Log.Trace("ParseByte()");

            byte[] bytes = GetBytes(1);
            return bytes[0];
        }

        public DateTime ParseTimeInSeconds()
        {
            Log.Trace("ParseTime()");

            int seconds = this.ParseInt32(true, true);
            var value = ConvertToDateTime(seconds);
            Log.Debug("ParseTime: value=" + value.ToString(CultureInfo.InvariantCulture));
            return value; 

        }

        public static DateTime ConvertToDateTime(int seconds)
        {
            var start = new DateTime(2007, 1, 1);
            var value = start.AddSeconds(seconds);
            if (value < start)
            {
                Log.Warn("Time cannot be smaller then epoch!");
                value = start;
            }
            return value;
        }

        /// <summary>
        /// Reads 4 bytes from the stream, inverts them and creates an integer.
        /// The integer is added to the epoch start in minutes
        /// </summary>
        /// <returns>Date time from epoch in minutes</returns>
        public DateTime ParseTimeInMinutes()
        {
            Log.Trace("ParseTime()");

            var minutes = this.ParseInt32(true);
            return TimeFromMinutes(minutes);

        }

        public static DateTime TimeFromMinutes(int minutes)
        {
            Log.Trace("Enter");

            var start = new DateTime(2007, 1, 1);

            var value = start.AddMinutes(minutes);
            if (value < start)
            {
                Log.Warn("Time cannot be smaller then epoch!");
                value = start;
            }
            Log.Debug("ParseTime: value=" + value.ToString(CultureInfo.InvariantCulture));
            Log.Trace("Return");
            return value;
        }

        #endregion

        #region constructors
        public BinaryParser(Stream stream)
            : base()
        {
            _reader = new BinaryReader(stream);
        }
        #endregion

        public int ParseInt32()
        {
            return this.ParseInt32(false);
        }

        public int ParseInt32(bool invert, bool escaped = false)
        {
            byte[] bytes = escaped ? GetBytes(4) : GetRealBytes(4);
            Int32 value = BitConverter.ToInt32(invert ? InvertByteArray(bytes) : bytes, 0);
            return value;
        }

        public int PeekChar()
        {
            return _reader.PeekChar();
        }

        public UInt32 ParseUInt32(bool invert)
        {
            Log.Trace("ParseUInt32");
            byte[] bytes = GetRealBytes(4);
            var value = BitConverter.ToUInt32(invert ? InvertByteArray(bytes) : bytes, 0);
            Log.Debug("ParseUInt32: value=" + value.ToString(CultureInfo.InvariantCulture));
            return value;
        }

        public bool SkipDLE { get; set; }
    }
}
