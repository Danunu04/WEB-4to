namespace BE
{
    /// <summary>
    /// Tipos de cuenta (USUARIOS.rol) y perfiles que crea el sistema.
    ///
    /// El rol numérico ya NO define permisos: dice qué tipo de cuenta es y, con eso, qué datos
    /// ve el usuario (un Cliente ve sus clases, un Familiar las de sus hijos, el personal las de
    /// todos) y qué registro se crea en el alta (ALUMNOS, ENTRENADORES).
    /// Qué pantallas puede usar lo decide su perfil (patrón Composite, pantalla Permisos).
    /// </summary>
    public static class PerfilesSistema
    {
        public const int RolAdministrador = 1;
        public const int RolRecepcionista = 2;
        public const int RolEntrenador = 3;
        public const int RolCliente = 4;
        public const int RolWebMaster = 5;
        public const int RolFamiliar = 6;

        // --- Códigos de los perfiles que crea scripts/crear-permisos-composite.sql ---
        public const string CodigoWebMaster = "WEBMASTER";
        public const string CodigoAdministrador = "ADMIN";
        public const string CodigoRecepcionista = "RECEPCION";
        public const string CodigoEntrenador = "ENTRENADOR";
        public const string CodigoCliente = "CLIENTE";
        public const string CodigoFamiliar = "FAMILIAR";

        /// <summary>
        /// Código del perfil que recibe una cuenta nueva según su tipo. Después se puede
        /// cambiar desde la pantalla Permisos.
        /// </summary>
        public static string CodigoPerfilPorDefecto(int rol)
        {
            switch (rol)
            {
                case RolWebMaster: return CodigoWebMaster;
                case RolAdministrador: return CodigoAdministrador;
                case RolRecepcionista: return CodigoRecepcionista;
                case RolEntrenador: return CodigoEntrenador;
                case RolCliente: return CodigoCliente;
                case RolFamiliar: return CodigoFamiliar;
                default: return null;
            }
        }
    }
}
