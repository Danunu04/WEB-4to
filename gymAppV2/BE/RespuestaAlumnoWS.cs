using System.Collections.Generic;

namespace BE
{
    /// <summary>
    /// Respuesta de las operaciones de alta, baja, modificación y asociación del web service de
    /// alumnos: código de estado y mensaje para que el cliente informe el resultado.
    ///
    /// Códigos: 200 OK, 400 datos inválidos o regla de negocio, 401 sin sesión, 403 sin permiso,
    /// 500 error interno. Clave y Argumentos permiten al cliente mostrar el mensaje traducido;
    /// Mensaje es el texto en español de respaldo.
    /// </summary>
    public class RespuestaAlumnoWS
    {
        public const int OK = 200;
        public const int DATOS_INVALIDOS = 400;
        public const int NO_AUTENTICADO = 401;
        public const int SIN_PERMISO = 403;
        public const int ERROR_INTERNO = 500;

        public int Codigo { get; set; }
        public string Mensaje { get; set; }
        public string Clave { get; set; }
        public List<string> Argumentos { get; set; } = new List<string>();

        public bool Exito
        {
            get { return Codigo == OK; }
            set { }
        }

        /// <summary>Alta: cuenta de Cliente creada para el alumno (null si ya tenía).</summary>
        public string UsuarioCreado { get; set; }

        /// <summary>Alta: contraseña generada por el sistema (null si se informó una).</summary>
        public string ContrasenaGenerada { get; set; }

        /// <summary>Baja: cuentas que quedaron desactivadas junto con el alumno.</summary>
        public List<string> CuentasDesactivadas { get; set; } = new List<string>();
    }
}
