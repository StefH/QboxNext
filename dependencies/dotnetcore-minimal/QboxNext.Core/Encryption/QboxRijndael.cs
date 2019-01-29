using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using QboxNext.Logging;

namespace QboxNext.Core.Encryption
{
    public class QboxRijndael
    {
        protected static ILogger Logger = QboxNextLogProvider.CreateLogger("QboxRijndael");

        protected byte[] Key;
        protected byte[] Iv;
        private byte[] _encrypted;
        private byte[] _decrypted;

        public QboxRijndael()
        {
            Key = QboxNextRijndael.StringToHex("00010203050607080A0B0C0D0F10111214151617191A1B1C1E1F202123242526");
            Iv = QboxNextRijndael.StringToHex("00000000000000000000000000000000");
        }

        /// <summary>
        /// Encrypts the given byte array using Rijndael / AES.
        /// The array is aligned on 16 bytes. 
        /// </summary>
        /// <param name="aBytes">The array of bytes to be encrypted</param>
        /// <returns>Byte array that holds the encryped version of the aBytes parameter</returns>
        public byte[] Encrypt(byte[] aBytes)
        {
            Logger.LogTrace("Enter");
            // Reset the lenght of the string to exact blocks of 16 bytes
            var length = aBytes.Length;
            var add = 0;
            if (length % 16 != 0)
                add = 16 - (length % 16);
            var bytes = new byte[add + length];
            Buffer.BlockCopy(aBytes, 0, bytes, 0, length);
            for (var i = 0; i < add; i++)
            {
                bytes[length + i] = 32;
            }

            Logger.LogDebug("Number of bytes added: " + add);

            _encrypted = QboxNextRijndael.Encrypt(bytes, Key, Iv, 256);

            Logger.LogTrace("Return");
            return _encrypted;
        }

        /// <summary>
        /// Encrypts the given string using RijnDael / AES.
        /// The text is padded to a length of multiple of 16 bytes.
        /// </summary>
        /// <param name="aText">The string the must be encrypted</param>
        /// <returns>an array of bytes holding the encrypted version of the string aText</returns>
        public byte[] Encrypt(string aText)
        {
            // Reset the lenght of the string to exact blocks of 16 bytes
            int length = aText.Length;
            int add = 0;
            if (length % 16 != 0)
                add = 16 - (length % 16);

            Logger.LogDebug("Number of bytes to add: " + add);

            string text = aText.PadRight(add + length, ' ');
            _decrypted = Encoding.UTF8.GetBytes(text);
            _encrypted = QboxNextRijndael.Encrypt(_decrypted, Key, Iv, 256);
            return _encrypted;
        }

        public string Decrypt(Stream encryptedStream)
        {
            if (encryptedStream == null || encryptedStream.Length == 0)
                throw new ArgumentNullException("Encrypted stream cannot be null or 0 bytes");

            _encrypted = new BinaryReader(encryptedStream).ReadBytes(
                Convert.ToInt32(encryptedStream.Length));

            return Decrypt(_encrypted);
        }

        public byte[] DecryptBytes(Stream encryptedStream)
        {
            if (encryptedStream == null || encryptedStream.Length == 0)
                throw new ArgumentNullException("Encrypted stream cannot be null or 0 bytes");

            _encrypted = new BinaryReader(encryptedStream).ReadBytes(
                Convert.ToInt32(encryptedStream.Length));

            return DecryptBytes(_encrypted);
        }

        public byte[] DecryptBytes(byte[] encryptedBytes)
        {
            return QboxNextRijndael.Decrypt(encryptedBytes, Key, Iv, 256);
        }

        public string Decrypt(byte[] encryptedBytes)
        {
            _decrypted = QboxNextRijndael.Decrypt(encryptedBytes, Key, Iv, 256);
            if (_decrypted == null)
                return null;
            return Encoding.UTF8.GetString(_decrypted).Trim();
        }
    }

    public class MiniRijndael
    {
        public static byte[] Key = new byte[]
            {
                0x18, 0x34, 0x15, 0x64,
                0x89, 0x05, 0x46, 0x67,
                0x43, 0x97, 0x84, 0x53,
                0x34, 0x78, 0x35, 0x48,
                0x87, 0x63, 0x68, 0x35,
                0x15, 0x46, 0x23, 0x57,
                0x76, 0x23, 0x45, 0x64,
                0x43, 0x87, 0x65, 0x43
            };

        public static byte[] Iv = new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        /// <summary>
        /// Encrypts the given string using AES Rijndael.
        /// The key and the InitVector are set through the static fields of the class.
        /// The padding is done using spaces and they are automatically added to the plainText before the encryption is started.
        /// </summary>
        /// <param name="plainText">The string to encrypt</param>
        /// <returns></returns>
        public static byte[] EncryptStringToBytes(string plainText)
        {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");

            // Create an Rijndael object
            // with the specified key and IV.
            using (var rijAlg = Rijndael.Create())
            {
                rijAlg.Mode = CipherMode.ECB;
                rijAlg.Padding = PaddingMode.None;
                var blockSizeDiversion = 16 - (plainText.Length % 16);
                if (blockSizeDiversion != 16)
                {
                    plainText = plainText.PadRight(plainText.Length + blockSizeDiversion, ' ');
                }

                // Create a decrytor to perform the stream transform.
                var encryptor = rijAlg.CreateEncryptor(MiniRijndael.Key, MiniRijndael.Iv);

                // Create the streams used for encryption.
                using (var msEncrypt = new MemoryStream())
                using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                using (var swEncrypt = new StreamWriter(csEncrypt))
                {

                    //Write all data to the stream.
                    swEncrypt.Write(plainText);
                    swEncrypt.Close();
                    return msEncrypt.ToArray();
                }
            }
        }

        public static string DecryptStringFromBytes(byte[] cipherText)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");

            // Declare the string used to hold
            // the decrypted text.
            string plaintext;

            // Create an Rijndael object
            // with the specified key and IV.
            using (var rijAlg = Rijndael.Create())
            {
                rijAlg.Mode = CipherMode.ECB;
                rijAlg.Padding = PaddingMode.None;

                // Create a decrytor to perform the stream transform.
                var decryptor = rijAlg.CreateDecryptor(MiniRijndael.Key, MiniRijndael.Iv);

                // Create the streams used for decryption.
                using (var msDecrypt = new MemoryStream(cipherText))
                using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                using (var srDecrypt = new StreamReader(csDecrypt))
                {
                    // Read the decrypted bytes from the decrypting stream
                    // and place them in a string.
                    plaintext = srDecrypt.ReadToEnd();
                }

            }

            return plaintext;

        }
    }
}