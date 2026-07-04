using System.Collections.Generic;

namespace BE
{
    /// <summary>
    /// Catálogo hardcodeado de perfiles de usuario.
    /// Cada perfil se asocia a un rol numérico (campo USUARIOS.rol).
    ///
    /// Roles numéricos:
    ///   1 = Web master / Administrador / ABM / bitacora / alumnos / profesores / backup / restore / dv
    ///   2 = Recepcionista / ABM limitado
    ///   3 = Profesor / Entrenador
    ///   4 = Cliente / Docente / Alumno / Familiar
    /// </summary>
    public static class PerfilesSistema
    {
        public const int RolAdministrador = 1;
        public const int RolRecepcionista = 2;
        public const int RolEntrenador = 3;
        public const int RolCliente = 4;
        public const int RolWebMaster = 5;

        public const string WebMaster = "Web master";
        public const string Administrador = "Administrador";
        public const string ABM = "ABM";
        public const string Bitacora = "Bitacora";
        public const string Usuario = "usuario";
        public const string Alumnos = "alumnos";
        public const string Profesores = "profesores";
        public const string Backup = "back up";
        public const string Restore = "restore";
        public const string DV = "dv";
        public const string ClienteDocente = "Cliente/docente";

        /// <summary>
        /// Devuelve el nombre del perfil principal asociado a un rol numérico.
        /// </summary>
        public static string ObtenerNombrePerfilPrincipal(int rol)
        {
            switch (rol)
            {
                case RolWebMaster:
                    return WebMaster;
                case RolAdministrador:
                    return Administrador;
                case RolRecepcionista:
                    return ABM;
                case RolEntrenador:
                    return Profesores;
                case RolCliente:
                    return ClienteDocente;
                default:
                    return Usuario;
            }
        }

        /// <summary>
        /// Devuelve todos los perfiles asociados a un rol numérico.
        /// </summary>
        public static IReadOnlyList<string> ObtenerPerfiles(int rol)
        {
            switch (rol)
            {
                case RolWebMaster:
                    return new List<string>
                    {
                        WebMaster, Administrador, ABM, Bitacora,
                        Alumnos, Profesores, Backup, Restore, DV
                    }.AsReadOnly();

                case RolAdministrador:
                    return new List<string>
                    {
                        WebMaster, Administrador, ABM, Bitacora,
                        Alumnos, Profesores, Backup, Restore, DV
                    }.AsReadOnly();

                case RolRecepcionista:
                    return new List<string>
                    {
                        ABM, Bitacora, Alumnos, Profesores
                    }.AsReadOnly();

                case RolEntrenador:
                    return new List<string>
                    {
                        Profesores
                    }.AsReadOnly();

                case RolCliente:
                    return new List<string>
                    {
                        ClienteDocente
                    }.AsReadOnly();

                default:
                    return new List<string> { Usuario }.AsReadOnly();
            }
        }
    }
}
