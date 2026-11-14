using System;

namespace BLL
{
    /// <summary>
    /// Regla de negocio de permisos que no se cumple. Clave es la clave de traducción del
    /// mensaje (pantalla Permisos) y Args sus valores: textos, o Familia/Perfil/PermisoSimple
    /// para que la pantalla muestre el nombre traducido. Message queda en español para la bitácora.
    /// </summary>
    public class PermisoException : Exception
    {
        public string Clave { get; private set; }
        public object[] Args { get; private set; }

        public PermisoException(string clave, string mensaje, params object[] args)
            : base(mensaje)
        {
            Clave = clave;
            Args = args ?? new object[0];
        }
    }
}
