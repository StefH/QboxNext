using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using NLog;
using QboxNext.Core.Utils;

namespace QboxNext.Core.Encryption
{
	/// <summary>
	/// Utility class for decrypting plain and encrypted Qbox messages.
	/// </summary>
	public static class QboxMessageDecrypter
	{
		private static readonly Logger Log = LogManager.GetLogger("QboxMessageDecrypter");

		/// <summary>
		/// Handles the decryption of the message if needed.
		/// We need this handled here because the encrypted messages is sent in a different encoding as opposed to the 
		/// unencrypted message. The encrypted message is sent in binary and the unencrypted message is sent UTF-8.
		/// So an encrypted message should be decrypted first or the conversion might remove somecharacters or even
		/// shorten the actual string.
		/// </summary>
		/// <param name="bytes">The actual message as a byte array</param>
		/// <returns>The unecrypted message as a string without STX/ETX</returns>
		public static string DecryptPlainOrEncryptedMessage(byte[] bytes)
		{
			Log.Debug(HexEncoding.ByteArrayToHexString(bytes));

			string message = string.Empty;
			bool isInputPlainText = true;

			try
			{
				// Assume that the message is encrypted (this will become the default). It this fails, it's plain.
				message = MiniRijndael.DecryptStringFromBytes(bytes);
				isInputPlainText = !IsPlainTextQboxMessage(message);
			}
			catch (ArgumentException)
			{
				// Ignore, handled below.
			}
			catch (CryptographicException)
			{
				// Ignore, handled below.
			}

			if (isInputPlainText)
			{
				message = Encoding.UTF8.GetString(bytes);
				Log.Debug("Encryption off");
			}
			else
			{
				Log.Debug("Encryption on");
			}

			Log.Debug(string.Format("Decrypted: {0}", message));
			//todo: check refactor this smell. The message is not correctly delimited by the chars.
			return message.WithoutStxEtxEnvelope().Trim();
		}


		/// <summary>
		/// Remove whitespace from message and discard everything from '\0' onward.
		/// </summary>
		private static string TrimMessage(string inMessage)
		{
			var trimmedMsg = inMessage;
			var len = trimmedMsg.Length;

			// The string may contain an end-of-string character, we only want to use everything
			// up to that character.
			var endOfStringPos = trimmedMsg.Length;
			for (int i = 0; i < len; ++i)
			{
				if (trimmedMsg[i] == ETX)
					endOfStringPos = i + 1;
			}
			if (endOfStringPos < len)
				trimmedMsg = trimmedMsg.Substring(0, endOfStringPos);

			return trimmedMsg;
		}


		/// <summary>
		/// Is the given message a plain-text Qbox message?
		/// </summary>
		public static bool IsPlainTextQboxMessage(string inMessage)
		{
			var trimmedMsg = TrimMessage(inMessage);
			var len = trimmedMsg.Length;

			// Every Qbox message starts with an STX character.
			if (trimmedMsg[0] != STX)
				return false;

			// And ends with an ETX character.
			if (len > 0 && trimmedMsg[len - 1] != ETX)
				return false;

			// The first two bytes should form a valid integer (the protocol version).
			Int32 protocol;
			if (!Int32.TryParse(trimmedMsg.Substring(1, 2), System.Globalization.NumberStyles.HexNumber, null, out protocol))
				return false;

			// The rest of the message is protocol dependent, so we can only check if it's all ASCII.
			// ASCII encoding replaces non-ascii with question marks, so we use UTF8 to see if multi-byte sequences are there
			return Encoding.UTF8.GetByteCount(trimmedMsg) == trimmedMsg.Length;
		}


		private const char STX = '\x02';
		private const char ETX = '\x03';
	}
}
