using System;
using System.Configuration;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SERVICIOS
{
    /// <summary>
    /// Gestiona operaciones criptográficas: hashing de contraseñas y encriptación reversible AES.
    /// La clave maestra AES se lee de la configuración de la aplicación (Web.config/App.config).
    /// Cada valor encriptado usa un IV (vector de inicialización) aleatorio de 128 bits que se
    /// almacena junto con el ciphertext. Esto evita que dos textos iguales generen el mismo
    /// resultado encriptado y es más seguro que un IV fijo global.
    /// El IV fijo anterior (AesIV) se conserva como fallback para desencriptar datos legacy.
    /// </summary>
    public class CriptoManager
    {
        private readonly string _aesKey;
        private readonly string _aesIVLegacy;

        public CriptoManager()
        {
            _aesKey = ConfigurationManager.AppSettings["AesKey"];
            _aesIVLegacy = ConfigurationManager.AppSettings["AesIV"];

            if (string.IsNullOrEmpty(_aesKey))
            {
                throw new ConfigurationErrorsException("La clave AES (AesKey) debe estar configurada en appSettings.");
            }
        }

        /// <summary>
        /// Genera el hash SHA-256 de un texto.
        /// </summary>
        public string GenerarHashSHA256(string texto)
        {
            try
            {
                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(texto));
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < hash.Length; i++) sb.AppendFormat("{0:x2}", hash[i]);
                    return sb.ToString();
                }
            }
            catch (ArgumentNullException ex)
            {
                throw new Exception("El texto a hashear no puede ser nulo: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al generar hash SHA256: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Encripta un texto plano usando AES-256 con un IV aleatorio y devuelve Base64.
        /// El resultado tiene el formato: Base64(version 1 byte + IV de 16 bytes + ciphertext).
        /// La versión permite distinguir inequívocamente el formato nuevo del legacy.
        /// </summary>
        public string EncriptarAES256(string textoPlano)
        {
            try
            {
                using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
                {
                    aesAlg.Key = GenerarClave(_aesKey);
                    aesAlg.GenerateIV();

                    ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                    using (MemoryStream msEncrypt = new MemoryStream())
                    {
                        // Formato: [1 byte versión = 0x01] + [IV 16 bytes] + [ciphertext]
                        msEncrypt.WriteByte(0x01);
                        msEncrypt.Write(aesAlg.IV, 0, aesAlg.IV.Length);

                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                            {
                                swEncrypt.Write(textoPlano);
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

        /// <summary>
        /// Desencripta un texto en Base64 generado por <see cref="EncriptarAES256"/>.
        /// Primero intenta el formato nuevo (byte de versión + IV aleatorio prefijado). Si falla,
        /// intenta desencriptar con el formato legacy (IV fijo) para mantener compatibilidad con
        /// datos encriptados por versiones anteriores.
        /// </summary>
        public string DesencriptarAES256(string textoEncriptado)
        {
            try
            {
                string resultado;
                if (IntentarDesencriptarFormatoNuevo(textoEncriptado, out resultado))
                    return resultado;

                if (IntentarDesencriptarFormatoLegacy(textoEncriptado, out resultado))
                    return resultado;

                throw new CryptographicException("El valor no pudo ser desencriptado con ninguno de los formatos soportados.");
            }
            catch (CryptographicException)
            {
                throw;
            }
            catch (FormatException ex)
            {
                throw new Exception("El formato del texto encriptado no es válido (no es Base64 correcto): " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al desencriptar con AES256: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Determina si un valor está encriptado con el formato nuevo (byte de versión + IV aleatorio prefijado).
        /// </summary>
        public bool EsFormatoNuevo(string textoEncriptado)
        {
            string dummy;
            return IntentarDesencriptarFormatoNuevo(textoEncriptado, out dummy);
        }

        /// <summary>
        /// Determina si un valor está encriptado con el formato legacy (IV fijo).
        /// </summary>
        public bool EsFormatoLegacy(string textoEncriptado)
        {
            string dummy;
            return IntentarDesencriptarFormatoLegacy(textoEncriptado, out dummy);
        }

        /// <summary>
        /// Intenta desencriptar con el formato nuevo: Base64(1 byte versión + IV de 16 bytes + ciphertext).
        /// </summary>
        private bool IntentarDesencriptarFormatoNuevo(string textoEncriptado, out string textoPlano)
        {
            textoPlano = null;

            if (string.IsNullOrWhiteSpace(textoEncriptado))
                return false;

            try
            {
                byte[] fullCipher = Convert.FromBase64String(textoEncriptado);
                if (fullCipher.Length <= 17 || fullCipher[0] != 0x01)
                    return false;

                byte[] iv = new byte[16];
                Array.Copy(fullCipher, 1, iv, 0, 16);

                byte[] cipher = new byte[fullCipher.Length - 17];
                Array.Copy(fullCipher, 17, cipher, 0, cipher.Length);

                using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
                {
                    aesAlg.Key = GenerarClave(_aesKey);
                    aesAlg.IV = iv;

                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                    using (MemoryStream msDecrypt = new MemoryStream(cipher))
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    {
                        textoPlano = srDecrypt.ReadToEnd();
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Intenta desencriptar con el formato legacy: IV fijo derivado de la cadena original.
        /// </summary>
        private bool IntentarDesencriptarFormatoLegacy(string textoEncriptado, out string textoPlano)
        {
            textoPlano = null;

            if (string.IsNullOrWhiteSpace(textoEncriptado))
                return false;

            try
            {
                byte[] cipherBytes = Convert.FromBase64String(textoEncriptado);

                using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
                {
                    aesAlg.Key = GenerarClave(_aesKey);
                    aesAlg.IV = GenerarIVLegacy(_aesIVLegacy);

                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                    using (MemoryStream msDecrypt = new MemoryStream(cipherBytes))
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    {
                        textoPlano = srDecrypt.ReadToEnd();
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Deriva una clave AES de 256 bits a partir de la clave maestra usando SHA-256.
        /// </summary>
        private static byte[] GenerarClave(string key)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(Encoding.UTF8.GetBytes(key));
            }
        }

        /// <summary>
        /// Deriva el IV legacy de 128 bits a partir de la cadena original.
        /// </summary>
        private static byte[] GenerarIVLegacy(string iv)
        {
            byte[] ivBytes = Encoding.UTF8.GetBytes(iv);
            Array.Resize(ref ivBytes, 16);
            return ivBytes;
        }
    }
}
