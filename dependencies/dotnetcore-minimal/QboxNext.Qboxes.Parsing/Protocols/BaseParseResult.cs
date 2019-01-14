using System;
using System.Text;

namespace QboxNext.Qboxes.Parsing.Protocols
{
	public class BaseParseResult
	{
		public BaseParseResult()
		{
			this.Writer = new StringBuilder();
		}

		public int SequenceNr { get; set; }

		protected StringBuilder Writer { get; private set; }

		public byte ProtocolNr { get; set; }

		public byte[] GetBytes()
		{
			return Encoding.ASCII.GetBytes(Writer.ToString());
		}

		public static string HexStr(byte[] p)
		{
			var c = new char[p.Length * 2];
			byte b;

			for (int y = 0, x = 0; y < p.Length; ++y, ++x)
			{
				b = ((byte)(p[y] >> 4));
				c[x] = (char)(b > 9 ? b + 0x37 : b + 0x30);
				b = ((byte)(p[y] & 0xF));
				c[++x] = (char)(b > 9 ? b + 0x37 : b + 0x30);
			}
			return new string(c);
		}

		public void Write(byte[] p)
		{
			Writer.Append(HexStr(p));
		}

		public void Write(byte b)
		{
			Writer.Append(HexStr(new byte[] { b }));
		}

		public void Write(UInt16 value)
		{
			Writer.Append(HexStr(BinaryParser.InvertByteArray(BitConverter.GetBytes(value))));
		}

		public void Write(UInt32 value)
		{
			Writer.Append(HexStr(BinaryParser.InvertByteArray(BitConverter.GetBytes(value))));
		}

		public void Write(Int32 value)
		{
			Writer.Append(HexStr(BinaryParser.InvertByteArray(BitConverter.GetBytes(value))));
		}

		public void Write(Int16 value)
		{
			Writer.Append(HexStr(BinaryParser.InvertByteArray(BitConverter.GetBytes(value))));
		}

		public void Write(string value)
		{
			Writer.Append(value);
		}


		public override string ToString()
		{
			// Use GetMessage or GetMessageWithEnvelope instead.
			throw new InvalidOperationException();
		}


		/// <summary>
		/// Return message without envelope (STX/ETX).
		/// </summary>
		public string GetMessage()
		{
			return Writer.ToString();
		}

		/// <summary>
		/// Return message including envelope (STX/ETX).
		/// </summary>
		public string GetMessageWithEnvelope()
		{
			var result = string.Empty;
			if (Writer.Length > 0 && Writer[0] != '\x02')
				result += '\x02';

			result += Writer.ToString();

			if (Writer.Length > 0 && Writer[Writer.Length - 1] != '\x03')
				result += '\x03';

			return result;
		}
	}
}