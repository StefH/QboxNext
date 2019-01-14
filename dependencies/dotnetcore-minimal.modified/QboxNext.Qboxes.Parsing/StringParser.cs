using System;
using System.Globalization;
using System.IO;
using System.Text;
using QboxNext.Qboxes.Parsing.Logging;

namespace QboxNext.Qboxes.Parsing
{
	public class StringParser
    {
        private static readonly ILog Log = LogProvider.GetLogger("StringParser");
        private readonly StringReader _reader;

        public StringParser(string source)
        {
            Log.TraceFormat("Enter: {source}", source);
            _reader = new StringReader(source);
            Log.Trace("Exit");
        }

        protected StringReader Reader
        {
            get { return _reader; }
        }

        public string ParseDelimitedText(char delimiter)
        {
            StringBuilder sb = new StringBuilder();
            int c = _reader.Read();
            if (c != delimiter && c != -1)
                throw new Exception("Delimiter expected");
            do
            {
                c = _reader.Read();

                if(c != delimiter && c != -1)
                    sb.Append((char)c);
            } while (c != delimiter && c != -1);
            
            return sb.ToString();
        }

        public byte ParseByte()
        {
            var buffer = HexEncoding.HexStringToByteArray(ReadCharacters(2));
            return buffer[0];
        }

        private string ReadCharacters(int count)
        {
            Log.TraceFormat("Enter - count: {count}", count);
            var buffer = new char[count];
            for (var i = 0; i < count; i++)
            {
				int ch = _reader.Read();
                buffer[i] = Convert.ToChar(ch);
            }
            var result = new string(buffer);
            Log.TraceFormat("Return - {result}", result);
            return result;
        }

        public DateTime ParseTime()
        {
            Log.Trace("Enter");
            var buffer = HexEncoding.HexStringToByteArray(ReadCharacters(8));
            Array.Reverse(buffer); // MSB -> LSB stuff
            var seconds = BitConverter.ToInt32(buffer, 0);
            var result = new DateTime(2007, 1, 1).AddSeconds(seconds);
            Log.TraceFormat("Return - {result}", result);
            return result;
        }

        public string ReadToEnd()
        {
            return _reader.ReadToEnd();
        }

        public int ParseInt32()
        {
            var buffer = HexEncoding.HexStringToByteArray(ReadCharacters(8));
            Array.Reverse(buffer); // LSB -> MSB stuff
            return BitConverter.ToInt32(buffer, 0);
        }

        public int ParseInt16()
        {
            var buffer = HexEncoding.HexStringToByteArray(ReadCharacters(4));
            Array.Reverse(buffer); // LSB -> MSB stuff
            return BitConverter.ToInt16(buffer, 0);
        }

        public ulong ParseUInt32()
        {
            var buffer = HexEncoding.HexStringToByteArray(ReadCharacters(8));
            Array.Reverse(buffer);
            return (ulong)BitConverter.ToUInt32(buffer, 0);
        }

        public int ParseUInt16()
        {
            var buffer = HexEncoding.HexStringToByteArray(ReadCharacters(4));
            Array.Reverse(buffer); // LSB -> MSB stuff
            return BitConverter.ToUInt16(buffer, 0);
        }

		/// <summary>
		/// Parse the counter value from a SmartMeter part.
		/// </summary>
		/// <param name="value">String containing one SmartMeter value, for example "1.8.1(00214.037*kWh) 1-0".
		/// Note that the last "1-0" is actually part of the next counter value, so it will not always be present.</param>
        /// <param name="counter">tbv format validatie</param>
		/// <returns>The value of the counter * 1000 if successful, otherwise MaxInt64.
		/// This means that for elecricity, the returned unit is Wh, for gas the unit is liter.</returns>
        public ulong ReadSmartMeterCounterValue(string value, int counter)
        {
            Log.TraceFormat("Enter - value: {value}", value);
			var result = UInt64.MaxValue;

            if (!string.IsNullOrEmpty(value))
			{
				var offset = value.LastIndexOf('(') + 1;
				var length = (value.IndexOf('*') == -1 ? value.LastIndexOf(')') : value.IndexOf('*')) - offset;
                var separator = value.IndexOf('.', offset);
				if (offset >= 0 && length > 0)
				{
                    var digits = (separator == -1 ? length : separator - offset);
                    var precision = (separator == -1 ? 0 : length - digits - 1);
                    // qplat-44, refactor: regularexpression? of een betere manier om hardcoded counters uit deze methode te halen
                    // qplat-116: waarden zonder decimalen punt niet meer accepteren
                    switch (counter)
                    {
                        case 181: 
                        case 182:
                        case 281:
                        case 282:
                            if (!((digits == 7 && precision == 1) || (digits == 6 && precision == 2) || (digits >= 5 && precision == 3)))
                                throw new InvalidFormatException(String.Format("Value {0} not in correct format for counter {1}, #######.# or ######.## or #####.### or ######.###", value.Substring(offset, length), counter));
                            break;
                        case 170:
                        case 270:
                            if (!((digits == 5 && precision == 1) || (digits == 4 && precision == 2) || (digits == 2 && precision == 3) || (digits == 5 && precision == 2)))
                                throw new InvalidFormatException(String.Format("Value {0} not in correct format for counter {1}, #####.# or ####.## or ##.### or #####.##", value.Substring(offset, length), counter));
                            break;
                        case 2421:
                        case 2420:
                            if (!((digits == 7 && precision == 1) || (digits == 6 && precision == 2) || (digits == 5 && precision == 3) || (digits == 6 && precision == 3)))
                                throw new InvalidFormatException(String.Format("Value {0} not in correct format for counter {1}, #######.# or ######.## or #####.###", value.Substring(offset, length), counter));
                            break;
                        default:
                            if (!(digits == 5 && precision == 3))
                                throw new InvalidFormatException(String.Format("Value {0} not in format {1}.{2}", value.Substring(offset, length), new String('#', 5), new String('#', 3)));
                            break;
                    }
					var partialResult = decimal.Parse(value.Substring(offset, length), CultureInfo.InvariantCulture);
					result = Convert.ToUInt64(partialResult * 1000m);
				}
			}

            Log.TraceFormat("Return - {result}", result);
            return result;
        }

        public ulong Read24bits(string value)
        {
            Log.TraceFormat("Enter - value: {value}", value);

            if (string.IsNullOrEmpty(value)) return UInt64.MaxValue;

            var buffer = HexEncoding.HexStringToByteArray(value + "0000000000"); // todo: fix this quick fix... array length not sufficient for ulong
            //Array.Reverse(buffer);
            return BitConverter.ToUInt64(buffer, 0);
        }

        public bool EndOfStream
        {
            get { return _reader.Peek() == -1; }
        }
    }
}