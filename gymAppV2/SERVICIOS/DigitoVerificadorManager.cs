using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace SERVICIOS
{
    /// <summary>
    /// Calcula y verifica los dígitos verificadores horizontal (DVH) y vertical (DVV).
    ///
    /// DVH: hash de todos los campos de una fila concatenados ("super string").
    /// DVV: hash de cada campo individual ("por columna") concatenado y vuelto a hashear.
    ///
    /// El helper no tiene dependencias de base de datos; recibe un diccionario de valores
    /// y devuelve los hashes. Quien llama al método es responsable de armar el diccionario
    /// con los valores que se quieren verificar (texto plano o ciphertext, según decisión
    /// de negocio).
    /// </summary>
    public class DigitoVerificadorManager
    {
        private readonly CriptoManager _criptoManager;

        public DigitoVerificadorManager()
        {
            _criptoManager = new CriptoManager();
        }

        /// <summary>
        /// Calcula el Dígito Verificador Horizontal (DVH) de una fila.
        /// Concatena todos los valores del diccionario (excepto claves que contengan "dvv" o "dvh")
        /// usando '|' como separador, y devuelve SHA-256 en hexadecimal.
        /// </summary>
        public string CalcularDVH(Dictionary<string, object> valores)
        {
            if (valores == null || valores.Count == 0)
                throw new ArgumentException("El diccionario de valores no puede ser nulo o vacío.", nameof(valores));

            StringBuilder superString = new StringBuilder();
            bool primero = true;

            foreach (var kvp in valores)
            {
                if (EsColumnaDigitoVerificador(kvp.Key))
                    continue;

                if (!primero)
                    superString.Append("|");

                superString.Append(NormalizarValor(kvp.Value));
                primero = false;
            }

            return _criptoManager.GenerarHashSHA256(superString.ToString());
        }

        /// <summary>
        /// Verifica que el DVH almacenado coincida con el calculado a partir de los valores.
        /// </summary>
        public bool VerificarDVH(string dvhAlmacenado, Dictionary<string, object> valores)
        {
            if (string.IsNullOrEmpty(dvhAlmacenado))
                return false;

            return dvhAlmacenado.Equals(CalcularDVH(valores), StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Normaliza un valor para la concatenación de DVH/DVV.
        /// - null o DBNull → "NULL"
        /// - bool → "1" / "0"
        /// - DateTime → "yyyy-MM-dd HH:mm:ss"
        /// - decimal/double/float → formato invariante sin separador de miles
        /// - string → tal cual (string.Empty para vacío)
        /// </summary>
        public string NormalizarValor(object valor)
        {
            if (valor == null || valor == DBNull.Value)
                return "NULL";

            // Nullable value types se unbox al tipo subyacente.
            Type tipo = Nullable.GetUnderlyingType(valor.GetType()) ?? valor.GetType();

            if (tipo == typeof(bool))
                return (bool)valor ? "1" : "0";

            if (tipo == typeof(DateTime))
            {
                DateTime fecha = (DateTime)valor;
                DateTime truncada = new DateTime(fecha.Year, fecha.Month, fecha.Day, fecha.Hour, fecha.Minute, fecha.Second);
                return truncada.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            }

            if (tipo == typeof(decimal))
                return ((decimal)valor).ToString(CultureInfo.InvariantCulture);

            if (tipo == typeof(float))
                return ((float)valor).ToString(CultureInfo.InvariantCulture);

            if (tipo == typeof(double))
                return ((double)valor).ToString(CultureInfo.InvariantCulture);

            if (tipo == typeof(int))
                return ((int)valor).ToString(CultureInfo.InvariantCulture);

            if (tipo == typeof(long))
                return ((long)valor).ToString(CultureInfo.InvariantCulture);

            if (tipo == typeof(string))
                return (string)valor ?? string.Empty;

            return valor.ToString() ?? string.Empty;
        }

        /// <summary>
        /// Indica si una columna es el campo dvv o dvh, que deben excluirse del cálculo.
        /// </summary>
        private bool EsColumnaDigitoVerificador(string nombreColumna)
        {
            if (string.IsNullOrEmpty(nombreColumna))
                return false;

            return nombreColumna.Equals("dvv", StringComparison.OrdinalIgnoreCase)
                || nombreColumna.Equals("dvh", StringComparison.OrdinalIgnoreCase);
        }
    }
}
