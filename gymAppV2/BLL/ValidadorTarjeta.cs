using System;
using System.Text.RegularExpressions;

namespace BLL
{
    /// <summary>
    /// Validación de formato de una tarjeta para el pago simulado (Pagos y Registro). Los datos
    /// no se guardan ni se envían a ningún lado. Devuelve la clave de traducción del error, o
    /// null si la tarjeta es válida.
    /// </summary>
    public static class ValidadorTarjeta
    {
        public const string ERROR_NUMERO = "pagos_err_tarjeta_numero";
        public const string ERROR_TITULAR = "pagos_err_tarjeta_titular";
        public const string ERROR_VENCE = "pagos_err_tarjeta_vence";
        public const string ERROR_CVV = "pagos_err_tarjeta_cvv";

        public static string Validar(string numero, string titular, string vence, string cvv, DateTime hoy)
        {
            string digitos = Regex.Replace(numero ?? string.Empty, @"[\s-]", string.Empty);
            if (!Regex.IsMatch(digitos, @"^\d{13,19}$") || !PasaLuhn(digitos))
                return ERROR_NUMERO;

            if (string.IsNullOrWhiteSpace(titular))
                return ERROR_TITULAR;

            Match fecha = Regex.Match((vence ?? string.Empty).Trim(), @"^(0[1-9]|1[0-2])/(\d{2})$");
            if (!fecha.Success)
                return ERROR_VENCE;

            var finDeMes = new DateTime(2000 + int.Parse(fecha.Groups[2].Value), int.Parse(fecha.Groups[1].Value), 1).AddMonths(1);
            if (finDeMes <= hoy.Date)
                return ERROR_VENCE;

            if (!Regex.IsMatch((cvv ?? string.Empty).Trim(), @"^\d{3,4}$"))
                return ERROR_CVV;

            return null;
        }

        /// <summary>Texto en español de cada error, de respaldo si falta la traducción.</summary>
        public static string MensajeRespaldo(string clave)
        {
            switch (clave)
            {
                case ERROR_NUMERO: return "El número de tarjeta no es válido.";
                case ERROR_TITULAR: return "Ingresá el nombre del titular.";
                case ERROR_VENCE: return "El vencimiento de la tarjeta no es válido o ya pasó.";
                case ERROR_CVV: return "El código de seguridad no es válido.";
                default: return null;
            }
        }

        private static bool PasaLuhn(string numero)
        {
            int suma = 0;
            bool duplicar = false;
            for (int i = numero.Length - 1; i >= 0; i--)
            {
                int digito = numero[i] - '0';
                if (duplicar)
                {
                    digito *= 2;
                    if (digito > 9) digito -= 9;
                }
                suma += digito;
                duplicar = !duplicar;
            }
            return suma % 10 == 0;
        }
    }
}
