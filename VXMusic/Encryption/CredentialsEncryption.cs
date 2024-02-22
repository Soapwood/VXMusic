using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public class CredentialsEncryption
{
    private const string KeyFile = "encryption.key";

    public static void StoreCredentials(string username, string password, string passphrase)
    {
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = DeriveKeyFromPassphrase(passphrase);
            aesAlg.IV = GenerateRandomIV(aesAlg.BlockSize);//aesAlg.BlockSize == 128 ? aesAlg.Key : aesAlg.Key.Take(16).ToArray(); // IV should be the same size as the block size

            using (var encryptor = aesAlg.CreateEncryptor())
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(cryptoStream))
                        {
                            sw.Write($"{username}:{password}");
                        }
                    }

                    File.WriteAllBytes("credentials.dat", memoryStream.ToArray());
                }
            }
        }
    }


    private static byte[] GenerateRandomIV(int blockSize)
    {
        using (var rng = new RNGCryptoServiceProvider())
        {
            byte[] iv = new byte[blockSize / 8]; // Divide by 8 to convert bits to bytes
            rng.GetBytes(iv);
            return iv;
        }
    }

    public static string RetrieveCredentials(string passphrase)
    {
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = DeriveKeyFromPassphrase(passphrase);
            aesAlg.IV = aesAlg.Key; //aesAlg.BlockSize == 128 ? aesAlg.Key : aesAlg.Key.Take(16).ToArray();

            using (var decryptor = aesAlg.CreateDecryptor())
            {
                using (MemoryStream memoryStream = new MemoryStream(File.ReadAllBytes("credentials.dat")))
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader sr = new StreamReader(cryptoStream))
                        {
                            return sr.ReadToEnd();
                        }
                    }
                }
            }
        }
    }

    private static byte[] DeriveKeyFromPassphrase(string passphrase)
    {
        // Use a secure key derivation function (KDF) like PBKDF2
        using (Rfc2898DeriveBytes deriveBytes = new Rfc2898DeriveBytes(passphrase, Encoding.UTF8.GetBytes("SaltValue"), 10000))
        {
            return deriveBytes.GetBytes(32); // 256 bits key for AES-256
        }
    }
}

//class Program
//{
//    static void Main(string[] args)
//    {
//        string username = "example_user";
//        string password = "secret_password";
//        string passphrase = "user_provided_passphrase";

//        // Store credentials
//        SecureCredentialsManager.StoreCredentials(username, password, passphrase);

//        // Retrieve and decrypt credentials
//        string decryptedCredentials = SecureCredentialsManager.RetrieveCredentials(passphrase);
//        Console.WriteLine($"Decrypted Credentials: {decryptedCredentials}");
//    }
//}
