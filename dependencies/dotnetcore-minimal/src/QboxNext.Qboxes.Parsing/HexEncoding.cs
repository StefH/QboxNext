using System;
using System.Linq;

namespace QboxNext.Qboxes.Parsing
{
    /// <summary>
    /// HexEncoding utilities.
    /// </summary>
    public static class HexEncoding
    {
        public static byte[] HexStringToByteArray(string hex)
        {
			if (hex == null)
				return null;

            var numberChars = hex.Length;
            if (numberChars % 2 != 0)
                throw new ArgumentOutOfRangeException("hex", "Invalid nr of Characters should be factor of 2");

            var bytes = new byte[numberChars / 2];
            for (var i = 0; i < numberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }


        public static int GetByteCount(string hexString)
        {
            // remove all none A-F, 0-9, characters
            var numHexChars = hexString.Count(IsHexDigit);

            // if odd number of characters, discard last character
            if (numHexChars % 2 != 0)
            {
                numHexChars--;
            }
            return numHexChars / 2; // 2 characters per byte
        }


        public static string ToString(byte[] bytes)
        {
            string hexString = "";
            for (int i = 0; i < bytes.Length; i++)
            {
                hexString += bytes[i].ToString("X2");
            }
            return hexString;
        }


        /// <summary>
        /// Determines if given string is in proper hexadecimal string format
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        public static bool InHexFormat(string hexString)
        {
            bool hexFormat = true;

            foreach (char digit in hexString)
            {
                if (!IsHexDigit(digit))
                {
                    hexFormat = false;
                    break;
                }
            }
            return hexFormat;
        }


        /// <summary>
        /// Returns true is c is a hexadecimal digit (A-F, a-f, 0-9)
        /// </summary>
        /// <param name="c">Character to test</param>
        /// <returns>true if hex digit, false if not</returns>
        public static bool IsHexDigit(Char c)
        {
            var numA = Convert.ToInt32('A');
            var num1 = Convert.ToInt32('0');
            c = Char.ToUpper(c);
            var numChar = Convert.ToInt32(c);
            if (numChar >= numA && numChar < (numA + 6))
                return true;
            return numChar >= num1 && numChar < (num1 + 10);
        }


        /// <summary>
        /// Converts a byte array to a Hex String representation
        /// </summary>
        /// <param name="inBytes">The byte array to convert</param>
        /// <returns>The string holding the byte array in hex string format</returns>
        public static string ByteArrayToHexString(byte[] inBytes)
        {
			int len = inBytes.Length;
			var c = new char[len * 2];
			byte b;

			for (int bx = 0, cx = 0; bx < len; ++bx, ++cx)
			{
				b = ((byte)(inBytes[bx] >> 4));
				c[cx] = (char)(b > 9 ? b + 0x37 + 0x20 : b + 0x30);

				b = ((byte)(inBytes[bx] & 0x0F));
				c[++cx] = (char)(b > 9 ? b + 0x37 + 0x20 : b + 0x30);
			}

			return new string(c);
        }


		/// <summary>
		/// Convert a byte array to a hex string, in reverse byte order.
		/// </summary>
		public static string ByteArrayToReverseHexString(byte[] inBytes)
		{
			int len = inBytes.Length;
			var c = new char[len * 2];
			byte b;

			for (int bx = 0, cx = 0; bx < len; ++bx, ++cx)
			{
				b = ((byte)(inBytes[len - 1 - bx] >> 4));
				c[cx] = (char)(b > 9 ? b + 0x37 + 0x20 : b + 0x30);

				b = ((byte)(inBytes[len - 1 - bx] & 0x0F));
				c[++cx] = (char)(b > 9 ? b + 0x37 + 0x20 : b + 0x30);
			}

			return new string(c);
		}
    }
}
