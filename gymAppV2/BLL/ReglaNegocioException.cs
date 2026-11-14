using System;

namespace BLL
{
    /// <summary>
    /// Regla de negocio que no se cumple (fuera de permisos). Clave es la clave de traducción
    /// del mensaje y Args sus valores ya formateados. Message queda en español para la bitácora
    /// y como texto de respaldo si falta la traducción.
    /// </summary>
    public class ReglaNegocioException : Exception
    {
        public string Clave { get; private set; }
        public object[] Args { get; private set; }

        public ReglaNegocioException(string clave, string mensaje, params object[] args)
            : base(mensaje)
        {
            Clave = clave;
            Args = args ?? new object[0];
        }
    }
}
