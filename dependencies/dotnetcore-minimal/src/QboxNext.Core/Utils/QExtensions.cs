using System;
using System.Linq;

namespace QboxNext.Core.Utils
{
    /// <summary>
    /// Class to hold the extension methods
    /// </summary>
    public static class QExtension
    {
        /// <summary>
        /// Extension method for the ushort class to easily reverse the order of bits
        /// </summary>
        /// <param name="inValue">the value that needs to be reversed</param>
        public static byte[] Reversed(this ushort inValue)
        {
            var bytes = BitConverter.GetBytes((inValue)).Reverse();
            return bytes.ToArray();
        }


		/// <summary>
		/// Extension method for removing STX and ETX from a string.
		/// </summary>
		public static string WithoutStxEtxEnvelope(this string inValue)
		{
			return inValue.Replace(STX, string.Empty).Replace(ETX, string.Empty);
		}


		private const string STX = "\x02";
		private const string ETX = "\x03";
    }
}
