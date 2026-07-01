using System;
using System.Collections.Generic;
using BE;
using MPP;
using Servicios.Singleton;

namespace BLL
{
    /// <summary>
    /// Lógica de negocio para autorización basada en perfiles y permisos hardcodeados.
    /// Los perfiles se definen en BE.PerfilesSistema y no dependen de datos de BD.
    /// </summary>
    public class BLLRol
    {
        private MPPRol mppRol;
        private BLLEvento bllEvento;

        public BLLRol()
        {
            mppRol = new MPPRol();
            bllEvento = new BLLEvento();
        }

        public int ObtenerRol(string usuario)
        {
            try
            {
                return mppRol.ObtenerRol(usuario);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener el rol: " + ex.Message, ex);
            }
        }

        public void ActualizarRol(string usuario, int rol)
        {
            try
            {
                mppRol.ActualizarRol(usuario, rol);
                bllEvento.RegistrarCambioRol(usuario, rol);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al actualizar el rol: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Devuelve true si el rol numérico tiene acceso al permiso/módulo indicado.
        /// Basado en perfiles hardcodeados del sistema.
        /// </summary>
        public bool TieneAccesoAModulo(int rol, string modulo)
        {
            if (string.IsNullOrEmpty(modulo))
                return false;

            switch (modulo)
            {
                // Todos los usuarios autenticados ven el dashboard.
                case "Dashboard":
                    return true;

                // --- Solo Web master / Administrador ---
                case "VerificacionDV":
                case "Backup":
                case "Restore":
                case "RecalcularDV":
                case "EncriptarDatos":
                    return rol == PerfilesSistema.RolAdministrador;

                // --- Admin y Recepcionista ---
                case "GestionUsuarios":
                case "GestionEntrenadores":
                case "Bitacora":
                case "PreciosCuota":
                    return rol <= PerfilesSistema.RolRecepcionista;

                // --- Admin, Recepcionista y Cliente (solo sus datos) ---
                case "GestionAlumnos":
                    return rol <= PerfilesSistema.RolRecepcionista || rol == PerfilesSistema.RolCliente;

                // --- Todos excepto Entrenador ---
                case "ActividadesCalendario":
                case "Pagos":
                    return rol != PerfilesSistema.RolEntrenador;

                // --- Admin, Recepcionista y Entrenador ---
                case "GestionRutinas":
                    return rol <= PerfilesSistema.RolEntrenador;

                // --- Solo Cliente (perfil propio) ---
                case "Perfil":
                    return rol == PerfilesSistema.RolCliente;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Verifica si el usuario logueado actualmente tiene acceso al módulo indicado.
        /// </summary>
        public bool UsuarioActualTieneAcceso(string modulo)
        {
            var usuario = Singleton.Instancia?.Usuario;
            if (usuario == null)
                return false;

            return TieneAccesoAModulo(usuario.USUARIO_Rol, modulo);
        }

        /// <summary>
        /// Indica si el usuario logueado tiene el rol de administrador (Web master / Administrador).
        /// </summary>
        public bool UsuarioActualEsAdmin()
        {
            var usuario = Singleton.Instancia?.Usuario;
            return usuario != null && usuario.USUARIO_Rol == PerfilesSistema.RolAdministrador;
        }

        /// <summary>
        /// Indica si el usuario logueado es Recepcionista.
        /// </summary>
        public bool UsuarioActualEsRecepcionista()
        {
            var usuario = Singleton.Instancia?.Usuario;
            return usuario != null && usuario.USUARIO_Rol == PerfilesSistema.RolRecepcionista;
        }

        /// <summary>
        /// Indica si el usuario logueado es Entrenador/Profesor.
        /// </summary>
        public bool UsuarioActualEsEntrenador()
        {
            var usuario = Singleton.Instancia?.Usuario;
            return usuario != null && usuario.USUARIO_Rol == PerfilesSistema.RolEntrenador;
        }

        /// <summary>
        /// Indica si el usuario logueado es Cliente/Docente/Alumno/Familiar.
        /// </summary>
        public bool UsuarioActualEsCliente()
        {
            var usuario = Singleton.Instancia?.Usuario;
            return usuario != null && usuario.USUARIO_Rol == PerfilesSistema.RolCliente;
        }

        /// <summary>
        /// Devuelve la lista de perfiles hardcodeados del usuario logueado.
        /// </summary>
        public IReadOnlyList<string> ObtenerPerfilesUsuarioActual()
        {
            var usuario = Singleton.Instancia?.Usuario;
            if (usuario == null)
                return new List<string>().AsReadOnly();

            return PerfilesSistema.ObtenerPerfiles(usuario.USUARIO_Rol);
        }

        /// <summary>
        /// Devuelve el nombre del perfil principal del usuario logueado.
        /// </summary>
        public string ObtenerNombrePerfilUsuarioActual()
        {
            var usuario = Singleton.Instancia?.Usuario;
            if (usuario == null)
                return PerfilesSistema.Usuario;

            return PerfilesSistema.ObtenerNombrePerfilPrincipal(usuario.USUARIO_Rol);
        }
    }
}
