using UnityEngine;
using System;
using System.Text;
using System.Security.Cryptography;
using System.Linq;
using System.IO;

namespace YourCommonTools
{
	/******************************************
	 * 
	 * RJEncryptor
	 * 
	 * Encryption class that uses Rijndael algorithm
	 * 
	 */
	public static class RJEncryptor
	{
		// -------------------------------------------
		/* 
		 * DecryptString
		 */
		public static string DecryptString(string _textToDecrypt, bool _showLog, params string[] _keys)
		{
			string sEncryptedString = _textToDecrypt;

			var myRijndael = new RijndaelManaged()
			{
				Padding = PaddingMode.Zeros,
				Mode = CipherMode.CBC,
				KeySize = 256,
				BlockSize = 256
			};

			string[] elementsEncrypted = _textToDecrypt.Split('|');
			
			byte[] sEncrypted = Convert.FromBase64String(elementsEncrypted[0]);
			byte[] IV = Convert.FromBase64String(elementsEncrypted[1]);
			byte[] key = Encoding.ASCII.GetBytes(CommController.Instance.GetStringNameRandom());
			var decryptor = myRijndael.CreateDecryptor(key, IV);

			byte[] fromEncrypt = new byte[sEncrypted.Length];

			MemoryStream msDecrypt = new MemoryStream(sEncrypted);
			CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);

			csDecrypt.Read(fromEncrypt, 0, fromEncrypt.Length);

			string encrytpedText = Encoding.ASCII.GetString(fromEncrypt).Trim('\x0');
			if (_showLog)
			{
	#if UNITY_EDITOR
				Debug.Log("DecryptString::fromEncrypt.Length=" + fromEncrypt.Length + ";encrytpedText=" + encrytpedText.Length);
	#endif
			}
			return (encrytpedText);
		}

		// -------------------------------------------
		/* 
		 * Encrypt
		 */
		public static string EncryptString(string _textToEncrypt, bool _showLog, bool _addIVToResult, params string[] _iv)
		{
			string sToEncrypt = _textToEncrypt;

			var myRijndael = new RijndaelManaged()
			{
				Padding = PaddingMode.Zeros,
				Mode = CipherMode.CBC,
				KeySize = 256,
				BlockSize = 256
			};

			byte[] key = Encoding.ASCII.GetBytes(CommController.Instance.GetStringNameRandom());
			string ivGenerated = Utilities.RandomCodeIV(32);
			byte[] IV = Encoding.ASCII.GetBytes(ivGenerated);
			if (_iv.Length > 0)
			{
				IV = Convert.FromBase64String((string)_iv[0]);
			}        

			var encryptor = myRijndael.CreateEncryptor(key, IV);

			var msEncrypt = new MemoryStream();
			var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);

			var toEncrypt = Encoding.ASCII.GetBytes(sToEncrypt);

			csEncrypt.Write(toEncrypt, 0, toEncrypt.Length);
			csEncrypt.FlushFinalBlock();

			var encrypted = msEncrypt.ToArray();

			string encryptedResult = "";
			if (_addIVToResult)
			{
				encryptedResult = Convert.ToBase64String(encrypted) + "|" + Convert.ToBase64String(Encoding.UTF8.GetBytes(ivGenerated));
			}
			else
			{
				encryptedResult = Convert.ToBase64String(encrypted);
			}
			if (_showLog)
			{
	#if DEBUG_MODE_DISPLAY_LOG
				Debug.Log("Encrypt::ORIGINAL[" + _textToEncrypt + "]:encryptedResult[" + encryptedResult + "]::DESCRYPTED[" + DecryptString(encryptedResult, false) + "]");
	#endif
			}
			return (encryptedResult);
		}

		// -------------------------------------------
		/* 
		 * DecryptString
		 */
		public static string DecryptOldString(string _textToDecrypt, bool _showLog, int _previousDay, params string[] _keys)
		{
			string sEncryptedString = _textToDecrypt;

			var myRijndael = new RijndaelManaged()
			{
				Padding = PaddingMode.Zeros,
				Mode = CipherMode.CBC,
				KeySize = 256,
				BlockSize = 256
			};

			string[] elementsEncrypted = _textToDecrypt.Split('|');

			byte[] sEncrypted = Convert.FromBase64String(elementsEncrypted[0]);
			byte[] IV = Convert.FromBase64String(elementsEncrypted[1]);
			byte[] key = Encoding.ASCII.GetBytes(LanguageController.Instance.GetStringOldNameRandom(_previousDay));
			var decryptor = myRijndael.CreateDecryptor(key, IV);

			byte[] fromEncrypt = new byte[sEncrypted.Length];

			MemoryStream msDecrypt = new MemoryStream(sEncrypted);
			CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);

			csDecrypt.Read(fromEncrypt, 0, fromEncrypt.Length);

			string encrytpedText = Encoding.ASCII.GetString(fromEncrypt).Trim('\x0');
			if (_showLog)
			{
	#if UNITY_EDITOR
				Debug.Log("DecryptString::fromEncrypt.Length=" + fromEncrypt.Length + ";encrytpedText=" + encrytpedText.Length);
	#endif
			}
			return (encrytpedText);
		}

		// -------------------------------------------
		/* 
		 * Encrypt
		 */
		public static string EncryptOldString(string _textToEncrypt, bool _showLog, bool _addIVToResult, int _previousDayOfYear, params string[] _iv)
		{
			string sToEncrypt = _textToEncrypt;

			var myRijndael = new RijndaelManaged()
			{
				Padding = PaddingMode.Zeros,
				Mode = CipherMode.CBC,
				KeySize = 256,
				BlockSize = 256
			};

			byte[] key = Encoding.ASCII.GetBytes(LanguageController.Instance.GetStringOldNameRandom(_previousDayOfYear));
			string ivGenerated = Utilities.RandomCodeIV(32);
			byte[] IV = Encoding.ASCII.GetBytes(ivGenerated);
			if (_iv.Length > 0)
			{
				IV = Convert.FromBase64String((string)_iv[0]);
			}

			var encryptor = myRijndael.CreateEncryptor(key, IV);

			var msEncrypt = new MemoryStream();
			var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);

			var toEncrypt = Encoding.ASCII.GetBytes(sToEncrypt);

			csEncrypt.Write(toEncrypt, 0, toEncrypt.Length);
			csEncrypt.FlushFinalBlock();

			var encrypted = msEncrypt.ToArray();

			string encryptedResult = "";
			if (_addIVToResult)
			{
				encryptedResult = Convert.ToBase64String(encrypted) + "|" + Convert.ToBase64String(Encoding.UTF8.GetBytes(ivGenerated));
			}
			else
			{
				encryptedResult = Convert.ToBase64String(encrypted);
			}
			if (_showLog)
			{
	#if UNITY_EDITOR
				Debug.Log("Encrypt::ORIGINAL[" + _textToEncrypt + "]:encryptedResult[" + encryptedResult + "]::DESCRYPTED[" + DecryptString(encryptedResult, false) + "]");
	#endif
			}
			return (encryptedResult);
		}


		// -------------------------------------------
		/* 
		 * DecryptString
		 */
		public static string DecryptStringWithKey(string _textToDecrypt, string _key)
		{
			string sEncryptedString = _textToDecrypt;

			var myRijndael = new RijndaelManaged()
			{
				Padding = PaddingMode.Zeros,
				Mode = CipherMode.CBC,
				KeySize = 256,
				BlockSize = 256
			};

			string[] elementsEncrypted = _textToDecrypt.Split('|');

			byte[] sEncrypted = Convert.FromBase64String(elementsEncrypted[0]);
			byte[] IV = Convert.FromBase64String(elementsEncrypted[1]);
			byte[] key = Encoding.ASCII.GetBytes(_key);
			var decryptor = myRijndael.CreateDecryptor(key, IV);

			byte[] fromEncrypt = new byte[sEncrypted.Length];

			MemoryStream msDecrypt = new MemoryStream(sEncrypted);
			CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);

			csDecrypt.Read(fromEncrypt, 0, fromEncrypt.Length);

			string encrytpedText = Encoding.ASCII.GetString(fromEncrypt).Trim('\x0');
			return (encrytpedText);
		}

		// -------------------------------------------
		/* 
		 * Encrypt
		 */
		public static string EncryptStringWithKey(string _textToEncrypt, string _key)
		{
			string sToEncrypt = _textToEncrypt;

			var myRijndael = new RijndaelManaged()
			{
				Padding = PaddingMode.Zeros,
				Mode = CipherMode.CBC,
				KeySize = 256,
				BlockSize = 256
			};

			byte[] key = Encoding.ASCII.GetBytes(_key);
			string ivGenerated = Utilities.RandomCodeIV(32);
			byte[] IV = Encoding.ASCII.GetBytes(ivGenerated);
			var encryptor = myRijndael.CreateEncryptor(key, IV);

			var msEncrypt = new MemoryStream();
			var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);

			var toEncrypt = Encoding.ASCII.GetBytes(sToEncrypt);

			csEncrypt.Write(toEncrypt, 0, toEncrypt.Length);
			csEncrypt.FlushFinalBlock();

			var encrypted = msEncrypt.ToArray();

			string encryptedResult = "";
			encryptedResult = Convert.ToBase64String(encrypted) + "|" + Convert.ToBase64String(Encoding.UTF8.GetBytes(ivGenerated));
			return (encryptedResult);
		}


	}
}