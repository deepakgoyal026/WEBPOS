using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace EazyPOS.Common
{
    public class EncryptDecrypt_v1
    {
        private string SecretKey = "M@!N@h!B@t@ung@@";
        public string DecryptStringFromBytes_Aes(string ValuetoDecrypt)
        {
            byte[] key = Encoding.UTF8.GetBytes(SecretKey);
            byte[] cipherText = Convert.FromBase64String(ValuetoDecrypt);
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.Mode = CipherMode.ECB;
                aesAlg.Padding = PaddingMode.PKCS7;
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                using (var msDecrypt = new MemoryStream(cipherText))
                {
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (var srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }
        public string EncryptStringFromPlainText_Aes(string ValuetoEncrypt)
        {
            byte[] key = Encoding.UTF8.GetBytes(SecretKey);
            byte[] plainBytes = Encoding.UTF8.GetBytes(ValuetoEncrypt);
            byte[] encryptedBytes = null;
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.Mode = CipherMode.ECB;
                aes.Padding = PaddingMode.PKCS7;
                using (ICryptoTransform encryptor = aes.CreateEncryptor())
                {
                    encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
                }
            }
            string cipherText = Convert.ToBase64String(encryptedBytes);
            return cipherText;
        }
        
    }
}
