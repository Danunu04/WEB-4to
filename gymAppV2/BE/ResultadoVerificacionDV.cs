using System;

namespace BE
{
    /// <summary>
    /// Representa el resultado de verificar integridad de una fila/campo de una tabla.
    /// </summary>
    public class ResultadoVerificacionDV
    {
        /// <summary>
        /// Nombre de la tabla verificada.
        /// </summary>
        public string NombreTabla { get; set; }

        /// <summary>
        /// Clave primaria de la fila afectada (puede ser compuesta o varias columnas separadas por '|').
        /// </summary>
        public string ClaveFila { get; set; }

        /// <summary>
        /// Nombre del campo/columna sospechoso. Vacío si el error es a nivel de fila completa.
        /// </summary>
        public string Campo { get; set; }

        /// <summary>
        /// Indica si la fila/campo es válido.
        /// </summary>
        public bool EsValido { get; set; }

        /// <summary>
        /// Descripción del estado o error detectado.
        /// </summary>
        public string Mensaje { get; set; }

        /// <summary>
        /// DVH almacenado en la fila (si aplica).
        /// </summary>
        public string DVHAlmacenado { get; set; }

        /// <summary>
        /// DVH recalculado a partir de los valores actuales (si aplica).
        /// </summary>
        public string DVHCalculado { get; set; }

        /// <summary>
        /// DVV almacenado en la fila (si aplica).
        /// </summary>
        public string DVVAlmacenado { get; set; }

        /// <summary>
        /// DVV recalculado a partir de los valores actuales (si aplica).
        /// </summary>
        public string DVVCalculado { get; set; }

        public ResultadoVerificacionDV()
        {
        }

        public ResultadoVerificacionDV(string nombreTabla, string claveFila, string campo, bool esValido, string mensaje)
        {
            NombreTabla = nombreTabla;
            ClaveFila = claveFila;
            Campo = campo;
            EsValido = esValido;
            Mensaje = mensaje;
        }
    }
}
