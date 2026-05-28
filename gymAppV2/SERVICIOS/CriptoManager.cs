using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SERVICIOS
{
    public class CriptoManager
    {

        string key = "12345678901234567";
        string iv = "1234567890123";
        public string _686DPGetSHA256(string ste)
        {
            try
            {
                SHA256 sha256 = SHA256.Create();
                ASCIIEncoding encoding = new ASCIIEncoding();
                byte[] stream = null;
                StringBuilder sb = new StringBuilder();
                stream = sha256.ComputeHash(encoding.GetBytes(ste));
                for (int i = 0; i < stream.Length; i++) sb.AppendFormat("{0:x2}", stream[i]);
                return sb.ToString();
            }
            catch (ArgumentNullException ex)
            {
                throw new Exception("El texto a encriptar no puede ser nulo: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al generar hash SHA256: " + ex.Message, ex);
            }
        }
        public string _686DPGetAES256(string plainText)
        {
            try
            {
                using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
                {
                    aesAlg.Key = _686DPGenerateKey(key);
                    aesAlg.IV = _686DPGenerateIV(iv);

                    ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                    using (MemoryStream msEncrypt = new MemoryStream())
                    {
                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                            {
                                swEncrypt.Write(plainText);
                            }
                        }
                        return Convert.ToBase64String(msEncrypt.ToArray());
                    }
                }
            }
            catch (ArgumentNullException ex)
            {
                throw new Exception("El texto a encriptar no puede ser nulo: " + ex.Message, ex);
            }
            catch (CryptographicException ex)
            {
                throw new Exception("Error criptográfico al encriptar con AES: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al encriptar con AES256: " + ex.Message, ex);
            }
        }

        private static byte[] _686DPGenerateKey(string key)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(Encoding.UTF8.GetBytes(key));
            }
        }

        private static byte[] _686DPGenerateIV(string iv)
        {
            byte[] ivBytes = Encoding.UTF8.GetBytes(iv);
            Array.Resize(ref ivBytes, 16);
            return ivBytes;
        }

        public object _686DPGetAESDecrypt(string dniAES)
        {
            try
            {
                using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
                {
                    aesAlg.Key = _686DPGenerateKey(key);
                    aesAlg.IV = _686DPGenerateIV(iv);

                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                    byte[] cipherBytes = Convert.FromBase64String(dniAES);
                    using (MemoryStream msDecrypt = new MemoryStream(cipherBytes))
                    {
                        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                            {
                                return srDecrypt.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch (ArgumentNullException ex)
            {
                throw new Exception("El texto encriptado no puede ser nulo: " + ex.Message, ex);
            }
            catch (FormatException ex)
            {
                throw new Exception("El formato del texto encriptado no es válido (no es Base64 correcto): " + ex.Message, ex);
            }
            catch (CryptographicException ex)
            {
                throw new Exception("Error criptográfico al desencriptar con AES: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al desencriptar con AES256: " + ex.Message, ex);
            }
        }
    }
}
