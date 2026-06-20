using System;

namespace SERVICIOS.Excepciones
{
    /// <summary>
    /// Excepción lanzada cuando un usuario intenta acceder a un módulo al que no tiene permisos.
    /// </summary>
    public class AccesoDenegadoException : Exception
    {
        /// <summary>
        /// Módulo al que se intentó acceder.
        /// </summary>
        public string Modulo { get; set; }

        public AccesoDenegadoException()
            : base("No tiene permisos para acceder a este módulo.")
        {
        }

        public AccesoDenegadoException(string modulo)
            : base($"No tiene permisos para acceder al módulo '{modulo}'.")
        {
            Modulo = modulo;
        }

        public AccesoDenegadoException(string modulo, Exception innerException)
            : base($"No tiene permisos para acceder al módulo '{modulo}'.", innerException)
        {
            Modulo = modulo;
        }
    }
}
