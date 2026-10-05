using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using BE;
using MPP;
using Servicios.Singleton;

namespace BLL
{
    /// <summary>
    /// Lógica de negocio para autorización por rol.
    /// Qué permisos tiene cada rol se guarda en la tabla RolPermiso (pantalla Permisos).
    /// Si la tabla no existe o está vacía se usan los valores por defecto de PermisoPorDefecto.
    /// El WebMaster tiene siempre todos los permisos.
    /// </summary>
    public class BLLRol
    {
        private MPPRol mppRol;
        private BLLEvento bllEvento;

        private const string CACHE_MATRIZ = "BLLRol.MatrizPermisos";

        /// <summary>
        /// Roles cuyos permisos se pueden editar (el WebMaster siempre tiene todo).
        /// </summary>
        public static readonly int[] RolesEditables =
        {
            PerfilesSistema.RolAdministrador, PerfilesSistema.RolRecepcionista, PerfilesSistema.RolEntrenador,
            PerfilesSistema.RolCliente, PerfilesSistema.RolFamiliar
        };

        /// <summary>
        /// Permisos que el Administrador conserva siempre, para que nadie pueda dejar
        /// el sistema sin acceso a la pantalla de Permisos.
        /// </summary>
        public static readonly string[] PermisosFijosAdministrador =
        {
            PermisosSistema.Dashboard, PermisosSistema.GestionPermisos
        };

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
        /// Devuelve true si el rol numérico tiene acceso al permiso/módulo indicado,
        /// según la tabla RolPermiso (o los valores por defecto si la tabla no está cargada).
        /// </summary>
        public bool TieneAccesoAModulo(int rol, string modulo)
        {
            if (string.IsNullOrEmpty(modulo))
                return false;

            if (rol == PerfilesSistema.RolWebMaster)
                return true;

            Dictionary<int, HashSet<string>> matriz = ObtenerMatrizConfigurada();
            if (matriz == null)
                return PermisoPorDefecto(rol, modulo);

            return matriz.TryGetValue(rol, out HashSet<string> permisos) && permisos.Contains(modulo);
        }

        /// <summary>
        /// Permisos guardados en la base, cacheados unos minutos para no consultar en cada request.
        /// Devuelve null si la tabla no existe, está vacía o no se pudo leer (se usan los valores por defecto).
        /// </summary>
        private Dictionary<int, HashSet<string>> ObtenerMatrizConfigurada()
        {
            var cache = HttpRuntime.Cache;
            if (cache[CACHE_MATRIZ] is Dictionary<int, HashSet<string>> enCache)
                return enCache.Count > 0 ? enCache : null;

            Dictionary<int, HashSet<string>> matriz;
            try
            {
                matriz = mppRol.TablaRolPermisoExiste()
                    ? mppRol.ListarPermisosPorRol()
                    : new Dictionary<int, HashSet<string>>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine("[Permisos] No se pudo leer RolPermiso: " + ex.Message);
                return null;
            }

            // Un diccionario vacío también se cachea, para no consultar en cada request
            cache.Insert(CACHE_MATRIZ, matriz, null, DateTime.UtcNow.AddMinutes(10), Cache.NoSlidingExpiration);
            return matriz.Count > 0 ? matriz : null;
        }

        /// <summary>
        /// Matriz de permisos para mostrar en la pantalla de Permisos: la guardada en la base
        /// o, si todavía no hay nada guardado, la de los valores por defecto.
        /// </summary>
        public Dictionary<int, HashSet<string>> ObtenerMatrizPermisos()
        {
            try
            {
                Dictionary<int, HashSet<string>> guardada = mppRol.ListarPermisosPorRol();
                if (guardada.Count > 0)
                    return guardada;

                return RolesEditables.ToDictionary(
                    rol => rol,
                    rol => new HashSet<string>(PermisosSistema.Todos.Where(p => PermisoPorDefecto(rol, p)), StringComparer.OrdinalIgnoreCase));
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener los permisos: " + ex.Message, ex);
            }
        }

        public bool TablaPermisosDisponible()
        {
            return mppRol.TablaRolPermisoExiste();
        }

        /// <summary>
        /// Guarda los permisos de todos los roles editables. Ignora permisos desconocidos
        /// y fuerza los permisos fijos del Administrador.
        /// </summary>
        public void GuardarMatrizPermisos(Dictionary<int, HashSet<string>> matriz)
        {
            try
            {
                if (!mppRol.TablaRolPermisoExiste())
                    throw new Exception("La tabla RolPermiso no existe. Ejecutá scripts/old/crear-tabla-rol-permiso.sql.");

                foreach (int rol in RolesEditables)
                {
                    HashSet<string> permisos = matriz.TryGetValue(rol, out HashSet<string> p)
                        ? new HashSet<string>(p.Where(x => PermisosSistema.Todos.Contains(x)), StringComparer.OrdinalIgnoreCase)
                        : new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                    if (rol == PerfilesSistema.RolAdministrador)
                        permisos.UnionWith(PermisosFijosAdministrador);

                    mppRol.GuardarPermisosRol(rol, permisos);
                }

                HttpRuntime.Cache.Remove(CACHE_MATRIZ);

                string usuario = Singleton.Instancia?.Usuario?.USUARIO_Usuario;
                if (!string.IsNullOrEmpty(usuario))
                {
                    try { bllEvento.RegistrarConfiguracion(usuario, "Permisos por rol actualizados"); }
                    catch { /* No impedir la operación principal si falla el log */ }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al guardar los permisos: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Permisos por defecto de cada rol. Se usan para la carga inicial de RolPermiso
        /// y mientras la tabla no exista o esté vacía.
        /// </summary>
        public static bool PermisoPorDefecto(int rol, string modulo)
        {
            if (rol == PerfilesSistema.RolWebMaster)
                return true;

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
                case "GestionPermisos":
                case "GestionAulas":
                    return rol == PerfilesSistema.RolAdministrador;

                // --- Admin y Recepcionista ---
                case "GestionUsuarios":
                case "GestionEntrenadores":
                case "Bitacora":
                case "PreciosCuota":
                case "GestionActividades":
                    return rol <= PerfilesSistema.RolRecepcionista;

                // --- Admin, Recepcionista y Familiar (solo los alumnos a su cargo) ---
                // El Cliente (alumno) no entra a Alumnos: se anota a las actividades desde Actividades.
                case "GestionAlumnos":
                    return rol <= PerfilesSistema.RolRecepcionista || rol == PerfilesSistema.RolFamiliar;

                // --- Todos excepto Entrenador ---
                case "ActividadesCalendario":
                case "Pagos":
                    return rol != PerfilesSistema.RolEntrenador;

                // --- Admin, Recepcionista y Entrenador ---
                case "GestionRutinas":
                    return rol <= PerfilesSistema.RolEntrenador;

                // --- Cliente y Familiar (perfil propio) ---
                case "Perfil":
                    return rol == PerfilesSistema.RolCliente || rol == PerfilesSistema.RolFamiliar;

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
            return usuario != null && (
                usuario.USUARIO_Rol == PerfilesSistema.RolAdministrador ||
                usuario.USUARIO_Rol == PerfilesSistema.RolWebMaster
            );
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
        /// Indica si el usuario logueado es Familiar (madre/padre/tutor de uno o más alumnos).
        /// </summary>
        public bool UsuarioActualEsFamiliar()
        {
            var usuario = Singleton.Instancia?.Usuario;
            return usuario != null && usuario.USUARIO_Rol == PerfilesSistema.RolFamiliar;
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
