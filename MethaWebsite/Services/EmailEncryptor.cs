using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace MethaWebsite.Services
{

    public class EmailEncryptor
    {
        // NOTE: Use a secure key & IV in real applications
        private static readonly byte[] Key = Encoding.UTF8.GetBytes("A1B2C3D4E5F6G7H8"); // 16 bytes for AES-128
        private static readonly byte[] IV = Encoding.UTF8.GetBytes("1H2G3F4E5D6C7B8A"); // 16 bytes IV

        public static string EncryptEmail(string email)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Key;
                aes.IV = IV;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using MemoryStream ms = new MemoryStream();
                using CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
                using (StreamWriter sw = new StreamWriter(cs))
                {
                    sw.Write(email);
                    sw.Flush();
                }
                return Convert.ToBase64String(ms.ToArray());
            }
        }

        public static string DecryptEmail(string encryptedEmail)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Key;
                aes.IV = IV;

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using MemoryStream ms = new MemoryStream(Convert.FromBase64String(encryptedEmail));
                using CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
                using StreamReader sr = new StreamReader(cs);

                return sr.ReadToEnd();
            }
        }
    }

}
