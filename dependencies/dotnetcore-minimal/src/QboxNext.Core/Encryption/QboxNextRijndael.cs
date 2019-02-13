using System;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace QboxNext.Core.Encryption
{
	/// <summary>
	/// This class uses a symmetric key algorithm (Rijndael/AES) to encrypt and 
	/// decrypt data. As long as encryption and decryption routines use the same
	/// parameters to generate the keys, the keys are guaranteed to be the same.
	/// </summary>
	public class QboxNextRijndael
    {

        /// <summary>
        /// Transforms a Hexadecimal byte array from a string that holds hexadecimal characters
        /// </summary>
        /// <param name="text">the string to transformed</param>
        /// <returns>byte aray holding the hexadecimal bytes</returns>
        public static byte[] StringToHex(string text)
        {
            int tlen = text.Length;
            byte[] temp = new byte[tlen / 2];
            for (int i = 0; i < tlen / 2; i++)
            {
                temp[i] = byte.Parse(text.Substring((i * 2), 2), NumberStyles.HexNumber);
            }
            return temp;
        }


        /// <summary>
        /// Creates the Rijndael key.
        /// </summary>
        /// <param name="passPhrase">
        /// Passphrase from which a pseudo-random password will be derived. The
        /// derived password will be used to generate the encryption key.
        /// Passphrase can be any string. In this example we assume that this
        /// passphrase is an ASCII string.
        /// </param>
        /// <param name="saltValue">
        /// Salt value used along with passphrase to generate password. Salt can
        /// be any string. In this example we assume that salt is an ASCII string.
        /// </param>
        /// <param name="hashAlgorithm">
        /// Hash algorithm used to generate password. Allowed values are: "MD5" and
        /// "SHA1". SHA1 hashes are a bit slower, but more secure than MD5 hashes.
        /// </param>
        /// <param name="passwordIterations">
        /// Number of iterations used to generate password. One or two iterations
        /// should be enough.
        /// </param>
        /// <returns>
        /// Key.
        /// </returns>
        public static byte[] ConvertToKey(string passPhrase, string saltValue, string hashAlgorithm, int passwordIterations, int keySize)
        {
            // Let us assume that strings only contain ASCII codes.
            // If strings include Unicode characters, use Unicode, UTF7, or UTF8 
            // encoding.
            byte[] saltValueBytes = Encoding.ASCII.GetBytes(saltValue);

            // First, we must create a password, from which the key will be derived.
            // This password will be generated from the specified passphrase and 
            // salt value. The password will be created using the specified hash 
            // algorithm. Password creation can be done in several iterations.
            PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, saltValueBytes, hashAlgorithm, passwordIterations);

            // Return the key.
            return password.GetBytes(keySize / 8);
        }

        /// <summary>
        /// Encrypts specified plain data using Rijndael symmetric key algorithm
        /// and returns a base64-encoded result.
        /// </summary>
        /// <param name="plainData">
        /// Plain data value to be encrypted.
        /// </param>
        /// <param name="key">
        /// Key to encrypt the data with.
        /// </param>
        /// <param name="initVector">
        /// Initialization vector (or IV). This value is required to encrypt the
        /// first block of plaintext data. For RijndaelManaged class IV must be 
        /// exactly 16 ASCII characters long.
        /// </param>
        /// <param name="keySize">
        /// Size of encryption key in bits. Allowed values are: 128, 192, and 256. 
        /// Longer keys are more secure than shorter keys.
        /// </param>
        /// <returns>
        /// Encrypted value formatted as a base64-encoded string.
        /// </returns>
        public static byte[] Encrypt(byte[] plainData, byte[] key, byte[] initVector, int keySize)
        {

            // Set the init vector.
            byte[] initVectorBytes = initVector;

            // Create uninitialized Rijndael encryption object.
            var symmetricKey = new RijndaelManaged();
            
            // It is reasonable to set encryption mode to Cipher Block Chaining
            // (CBC). Use default options for other symmetric key parameters.
            // Rolf:  Within Qbox communication ECB is used.
            //symmetricKey.Mode = CipherMode.ECB;
            symmetricKey.Mode = CipherMode.ECB;
            symmetricKey.Padding = PaddingMode.None;

            // Generate encryptor from the existing key bytes and initialization 
            // vector. Key size will be defined based on the number of the key 
            // bytes.
            ICryptoTransform encryptor = symmetricKey.CreateEncryptor(key, initVectorBytes);

            // Define memory stream which will be used to hold encrypted data.
            var memoryStream = new MemoryStream();

            // Define cryptographic stream (always use Write mode for encryption).
            var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);

            // Start encrypting.
            cryptoStream.Write(plainData, 0, plainData.Length);

            // Finish encrypting.
            cryptoStream.FlushFinalBlock();

            // Convert our encrypted data from a memory stream into a byte array.
            byte[] cipherDataBytes = memoryStream.ToArray();

            // Close both streams.
            memoryStream.Close();
            cryptoStream.Close();

            // Return the encrypted data.
            return cipherDataBytes;
        }

        /// <summary>
        /// Decrypts specified cipher data using Rijndael symmetric key algorithm.
        /// </summary>
		/// <param name="cipherData">
        /// Base64-formatted cipher data value.
        /// </param>
        /// <param name="key">
        /// Key to decrypt the data with.
        /// </param>
        /// <param name="initVector">
        /// Initialization vector (or IV). This value is required to encrypt the
        /// first block of plaintext data. For RijndaelManaged class IV must be
        /// exactly 16 ASCII characters long.
        /// </param>
        /// <param name="keySize">
        /// Size of encryption key in bits. Allowed values are: 128, 192, and 256.
        /// Longer keys are more secure than shorter keys.
        /// </param>
        /// <returns>
        /// Decrypted string value.
        /// </returns>
        public static byte[] Decrypt(byte[] cipherData, byte[] key, byte[] initVector, int keySize)
        {

            // Set the init vector.
            byte[] initVectorBytes = initVector;

            // Create uninitialized Rijndael encryption object.
            var symmetricKey = new RijndaelManaged();

            // It is reasonable to set encryption mode to Cipher Block Chaining
            // (CBC). Use default options for other symmetric key parameters.
            // Within Qbox communication ECB is used.
            symmetricKey.Mode = CipherMode.ECB;

            symmetricKey.Padding = PaddingMode.None;

            // Generate decryptor from the existing key bytes and initialization 
            // vector. Key size will be defined based on the number of the key 
            // bytes.
            ICryptoTransform decryptor = symmetricKey.CreateDecryptor(key, initVectorBytes);

            // Define memory stream which will be used to hold encrypted data.
            var memoryStream = new MemoryStream(cipherData);

            // Define cryptographic stream (always use Read mode for encryption).
            var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);

            // Since at this point we don't know what the size of decrypted data
            // will be, allocate the buffer long enough to hold ciphertext;
            // plaintext is never longer than ciphertext.
            var plainDataBytes = new byte[cipherData.Length-1];

            // Start decrypting.
	        try
	        {
				int decryptedByteCount = cryptoStream.Read(plainDataBytes, 0, plainDataBytes.Length);
	        }
	        catch (CryptographicException)
	        {
				// Not a valid encrypted stream.
				return null;
	        }
			finally
	        {
				memoryStream.Close();
			}

			// Don't close this stream after the exception since that will throw another exception.
			cryptoStream.Close();
			
			// Return decrypted data.   
            return plainDataBytes;
        }

    }
}
//
// EOF
///////////////////////////////////////////////////////////////////////////////
