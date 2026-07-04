namespace BE
{
    /// <summary>
    /// Constantes compartidas de seguridad para toda la aplicación.
    /// Centraliza umbrales, longitudes y flags de seguridad para evitar magic numbers dispersos.
    /// </summary>
    public static class ConstantesSeguridad
    {
        /// <summary>
        /// Cantidad máxima de intentos fallidos de login permitidos antes de bloquear la cuenta.
        /// </summary>
        public const int MAX_INTENTOS_LOGIN = 3;

        /// <summary>
        /// Longitud mínima requerida para las contraseñas de usuario.
        /// </summary>
        public const int CONTRASENA_MIN_LENGTH = 6;

        /// <summary>
        /// Longitud máxima permitida para el campo contraseña.
        /// </summary>
        public const int CONTRASENA_MAX_LENGTH = 128;

        /// <summary>
        /// Longitud máxima permitida para el campo usuario.
        /// </summary>
        public const int USUARIO_MAX_LENGTH = 50;

        /// <summary>
        /// Cantidad máxima de contraseñas históricas a conservar por usuario.
        /// </summary>
        public const int MAX_HISTORIAL_CONTRASENAS = 10;
    }
}
