using System;
using System.Text.RegularExpressions;

namespace BLL
{
    /// <summary>
    /// Validación de formato de una tarjeta para el pago simulado (Pagos y Registro). Los datos
    /// no se guardan ni se envían a ningún lado. Devuelve la clave de traducción del error, o
    /// null si la tarjeta es válida. Como es una demo, acepta cualquier número de 16 dígitos
    /// (no controla el dígito verificador de las tarjetas reales).
    /// </summary>
    public static class ValidadorTarjeta
    {
        public const string ERROR_NUMERO = "pagos_err_tarjeta_numero";
        public const string ERROR_TITULAR = "pagos_err_tarjeta_titular";
        public const string ERROR_VENCE = "pagos_err_tarjeta_vence";
        public const string ERROR_CVV = "pagos_err_tarjeta_cvv";

        /// <summary>
        /// Lleva el número al formato único "0000 0000 0000 0000". Acepta 16 dígitos juntos o en
        /// grupos de 4 separados por espacios o por guiones (siempre el mismo separador).
        /// Devuelve null si no tiene ninguno de esos formatos.
        /// </summary>
        public static string Normalizar(string numero)
        {
            Match m = Regex.Match((numero ?? string.Empty).Trim(), @"^(\d{4})([ -]?)(\d{4})\2(\d{4})\2(\d{4})$");
            if (!m.Success)
                return null;

            return $"{m.Groups[1].Value} {m.Groups[3].Value} {m.Groups[4].Value} {m.Groups[5].Value}";
        }

        /// <summary>
        /// Lleva el vencimiento al formato "MM/AA". Acepta el mes con 1 o 2 dígitos y el año con 2 o 4,
        /// juntos o separados por "/", "-", "." o espacios (ej. "10/29", "10 29", "1029", "1/2029").
        /// Devuelve null si no se puede interpretar.
        /// </summary>
        public static string NormalizarVencimiento(string vence)
        {
            Match m = Regex.Match((vence ?? string.Empty).Trim(), @"^(\d{1,2})\s*[/\-. ]?\s*(\d{4}|\d{2})$");
            if (!m.Success)
                return null;

            int mes = int.Parse(m.Groups[1].Value);
            string anio = m.Groups[2].Value;
            if (mes < 1 || mes > 12 || (anio.Length == 4 && !anio.StartsWith("20")))
                return null;

            return $"{mes:00}/{anio.Substring(anio.Length - 2)}";
        }

        public static string Validar(string numero, string titular, string vence, string cvv, DateTime hoy)
        {
            if (Normalizar(numero) == null)
                return ERROR_NUMERO;

            if (string.IsNullOrWhiteSpace(titular))
                return ERROR_TITULAR;

            Match fecha = Regex.Match(NormalizarVencimiento(vence) ?? string.Empty, @"^(0[1-9]|1[0-2])/(\d{2})$");
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
    }
}
