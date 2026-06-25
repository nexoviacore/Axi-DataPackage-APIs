using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;

namespace AxiDataPackages.Services.Helpers
{
    public class EncryptDecrypt
    {
        private readonly string _key = "AxiDataPackageAxpertExportImport";
        private readonly string _iv = "AxiDataPackages1";
        private readonly ILogger<EncryptDecrypt> _logger;

        public EncryptDecrypt(ILogger<EncryptDecrypt> logger)
        {
            _logger = logger;

            _logger.LogInformation("Encryption helper initialized successfully.");
        }

        public string EncryptImplementation(string plainText)
        {
            try
            {
                using Aes aes = Aes.Create();

                aes.KeySize = 256;
                aes.BlockSize = 128;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                aes.Key = Encoding.UTF8.GetBytes(_key);
                aes.IV = Encoding.UTF8.GetBytes(_iv);

                using ICryptoTransform encryptor = aes.CreateEncryptor();

                byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                byte[] encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

                _logger.LogInformation("Data encrypted successfully.");

                return Convert.ToBase64String(encryptedBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while encrypting data.");
                throw;
            }
        }

        public string DecryptImplementation(string cipherText)
        {
            try
            {
                using Aes aes = Aes.Create();

                aes.KeySize = 256;
                aes.BlockSize = 128;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                aes.Key = Encoding.UTF8.GetBytes(_key);
                aes.IV = Encoding.UTF8.GetBytes(_iv);

                using ICryptoTransform decryptor = aes.CreateDecryptor();

                byte[] cipherBytes = Convert.FromBase64String(cipherText);
                byte[] decryptedBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

                _logger.LogInformation("Data decrypted successfully.");

                return Encoding.UTF8.GetString(decryptedBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while decrypting data.");
                throw;
            }
        }
    }
}