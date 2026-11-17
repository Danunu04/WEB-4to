using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using BE;
using MPP;
using SERVICIOS.Composite;
using Servicios.Singleton;

namespace BLL
{
    /// <summary>
    /// Permisos con el patrón Composite: familias (grupos de permisos y de otras familias),
    /// perfiles (lo que se le asigna a cada usuario) y el control de acceso.
    ///
    /// Reglas:
    ///  - Un permiso no puede llegar dos veces a una familia ni a un perfil (ni directo ni por
    ///    otra familia) y una familia no puede contenerse a sí misma (ni indirectamente).
    ///  - Al cambiar una familia se revalidan todas las familias y perfiles que la contienen.
    ///  - No se elimina una familia que esté en uso ni un perfil que tenga usuarios.
    ///  - Los perfiles y familias del sistema (con código) no se renombran; los perfiles del sistema
    ///    no se eliminan. Solo un WebMaster asigna o quita el perfil WebMaster.
    ///    El WebMaster y el Administrador conservan siempre Dashboard y Permisos.
    ///  - Siempre tiene que quedar al menos un usuario activo con acceso a Permisos.
    /// </summary>
    public class BLLPermiso
    {
        private const string CACHE_KEY = "BLLPermiso.Permisos";

        // Id en memoria de una familia o perfil que todavía no se guardó
        private const int ID_NUEVO = -1;

        private const int LARGO_MAXIMO_NOMBRE = 100;

        private MPPPermiso mpp;
        private BLLEvento bllEvento;

        /// <summary>
        /// Lo que se guarda en caché: la estructura y, ya calculados, los permisos de cada perfil.
        /// </summary>
        private class PermisosCacheados
        {
            public EstructuraPermisos Estructura;
            public Dictionary<int, HashSet<string>> PermisosPorPerfil = new Dictionary<int, HashSet<string>>();
            public HashSet<int> PerfilesWebMaster = new HashSet<int>();
        }

        public BLLPermiso()
        {
            mpp = new MPPPermiso();
            bllEvento = new BLLEvento();
        }

        public bool TablasDisponibles()
        {
            return mpp.TablasExisten();
        }

        #region Control de acceso

        /// <summary>
        /// True si el perfil del usuario incluye el permiso. Ante cualquier
        /// error devuelve false: sin poder leer los permisos no se da acceso.
        /// </summary>
        public bool UsuarioTieneAcceso(string usuario, string permiso)
        {
            if (string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(permiso))
                return false;

            try
            {
                PermisosCacheados cache = ObtenerCache();
                if (!cache.Estructura.PerfilDeUsuario.TryGetValue(usuario, out int idPerfil))
                    return false;

                return cache.PermisosPorPerfil.TryGetValue(idPerfil, out HashSet<string> permisos) && permisos.Contains(permiso);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine("[Permisos] No se pudieron leer los permisos: " + ex.Message);
                return false;
            }
        }

        public bool UsuarioActualTieneAcceso(string permiso)
        {
            return UsuarioTieneAcceso(UsuarioActual(), permiso);
        }

        public bool UsuarioEsWebMaster(string usuario)
        {
            if (string.IsNullOrEmpty(usuario))
                return false;

            try
            {
                PermisosCacheados cache = ObtenerCache();
                return cache.Estructura.PerfilDeUsuario.TryGetValue(usuario, out int idPerfil)
                    && cache.PerfilesWebMaster.Contains(idPerfil);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine("[Permisos] No se pudieron leer los permisos: " + ex.Message);
                return false;
            }
        }

        public static void LimpiarCache()
        {
            HttpRuntime.Cache.Remove(CACHE_KEY);
        }

        private PermisosCacheados ObtenerCache()
        {
            if (HttpRuntime.Cache[CACHE_KEY] is PermisosCacheados enCache)
                return enCache;

            EstructuraPermisos estructura = mpp.CargarEstructura();
            var nuevo = new PermisosCacheados { Estructura = estructura };

            foreach (var perfil in estructura.Perfiles)
            {
                if (EsCodigoWebMaster(perfil.Value.Codigo))
                    nuevo.PerfilesWebMaster.Add(perfil.Key);

                try
                {
                    nuevo.PermisosPorPerfil[perfil.Key] = new HashSet<string>(
                        estructura.ConstruirPerfil(perfil.Key).ObtenerPermisosSimples().Select(p => p.Nombre),
                        StringComparer.OrdinalIgnoreCase);
                }
                catch (ComposicionInvalidaException ex)
                {
                    // Solo pasa si se tocaron las tablas por fuera de la aplicación: ese perfil queda sin acceso
                    System.Diagnostics.Trace.WriteLine($"[Permisos] El perfil {perfil.Value.Nombre} es inválido: {ex.Message}");
                    nuevo.PermisosPorPerfil[perfil.Key] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                }
            }

            HttpRuntime.Cache.Insert(CACHE_KEY, nuevo, null, DateTime.UtcNow.AddMinutes(10), Cache.NoSlidingExpiration);
            return nuevo;
        }

        #endregion

        #region Consultas para la pantalla

        /// <summary>
        /// Permisos del sistema, en el orden de PermisosSistema.Todos.
        /// </summary>
        public List<PermisoSimple> ListarPermisos()
        {
            List<string> orden = PermisosSistema.Todos.ToList();
            return ObtenerCache().Estructura.Permisos
                .Select(p => new PermisoSimple(p.Key, p.Value))
                .OrderBy(p => orden.IndexOf(p.Nombre) < 0 ? int.MaxValue : orden.IndexOf(p.Nombre))
                .ThenBy(p => p.Nombre)
                .ToList();
        }

        /// <summary>
        /// Familias con su árbol completo; primero las del sistema.
        /// </summary>
        public List<Familia> ListarFamilias()
        {
            EstructuraPermisos estructura = ObtenerCache().Estructura;
            return estructura.Familias.Keys
                .Select(id => ConstruirFamiliaSegura(estructura, id))
                .OrderBy(f => f.Codigo == null)
                .ThenBy(f => f.Id)
                .ToList();
        }

        /// <summary>
        /// Perfiles con su árbol completo; primero los del sistema.
        /// </summary>
        public List<Perfil> ListarPerfiles()
        {
            EstructuraPermisos estructura = ObtenerCache().Estructura;
            return estructura.Perfiles.Keys
                .Select(id => ConstruirPerfilSeguro(estructura, id))
                .OrderBy(p => p.Codigo == null)
                .ThenBy(p => p.Id)
                .ToList();
        }

        public Familia ObtenerFamilia(int idFamilia)
        {
            EstructuraPermisos estructura = ObtenerCache().Estructura;
            return estructura.Familias.ContainsKey(idFamilia) ? ConstruirFamiliaSegura(estructura, idFamilia) : null;
        }

        public Perfil ObtenerPerfil(int idPerfil)
        {
            EstructuraPermisos estructura = ObtenerCache().Estructura;
            return estructura.Perfiles.ContainsKey(idPerfil) ? ConstruirPerfilSeguro(estructura, idPerfil) : null;
        }

        /// <summary>
        /// usr → idPerfil de todos los usuarios que tienen perfil.
        /// </summary>
        public Dictionary<string, int> ObtenerPerfilesDeUsuarios()
        {
            return new Dictionary<string, int>(ObtenerCache().Estructura.PerfilDeUsuario, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Familias y perfiles que contienen directamente a la familia.
        /// </summary>
        public List<object> ObtenerUsosDeFamilia(int idFamilia)
        {
            return ObtenerUsosDeFamilia(ObtenerCache().Estructura, idFamilia);
        }

        /// <summary>
        /// Arma en memoria la familia que se está editando, agregando los componentes de a uno
        /// para que la pantalla avise apenas algo no se puede agregar. No guarda nada.
        /// </summary>
        public Familia ArmarFamilia(int idFamilia, string nombre, IList<int> permisos, IList<int> familias)
        {
            return ArmarFamilia(ObtenerCache().Estructura, idFamilia, nombre, null, permisos, familias);
        }

        /// <summary>
        /// Arma en memoria el perfil que se está editando. No guarda nada.
        /// </summary>
        public Perfil ArmarPerfil(int idPerfil, string nombre, IList<int> permisos, IList<int> familias)
        {
            return ArmarPerfil(ObtenerCache().Estructura, idPerfil, nombre, null, permisos, familias);
        }

        #endregion

        #region Familias

        /// <summary>
        /// Crea (idFamilia = 0) o modifica una familia. Devuelve su id.
        /// </summary>
        public int GuardarFamilia(int idFamilia, string nombre, IList<int> permisos, IList<int> familias)
        {
            ValidarQuePuedeGestionar();

            permisos = SinRepetidos(permisos);
            familias = SinRepetidos(familias);
            nombre = (nombre ?? string.Empty).Trim();

            EstructuraPermisos estructura = mpp.CargarEstructura();

            EstructuraPermisos.DatosNodo anterior = null;
            if (idFamilia != 0 && !estructura.Familias.TryGetValue(idFamilia, out anterior))
                throw new PermisoException("permisos_err_no_existe", "La familia ya no existe.");

            // Las familias del sistema se muestran traducidas por su código: no se renombran
            string codigo = anterior?.Codigo;
            if (codigo != null)
                nombre = anterior.Nombre;
            else
                ValidarNombre(nombre, estructura.Familias.Where(f => f.Key != idFamilia).Select(f => f.Value.Nombre), "permisos_err_nombre_repetido_familia");

            if (permisos.Count + familias.Count == 0)
                throw new PermisoException("permisos_err_familia_vacia", "La familia tiene que tener al menos un permiso o una familia.");

            ValidarQueExisten(estructura, permisos, familias);

            // Primero la familia sola (errores directos con el mensaje más claro), después todo lo que la contiene
            Familia familia = ArmarFamilia(estructura, idFamilia, nombre, codigo, permisos, familias);

            int idPrueba = idFamilia == 0 ? ID_NUEVO : idFamilia;
            EstructuraPermisos prueba = estructura.Clonar();
            prueba.DefinirFamilia(idPrueba, nombre, codigo, permisos, familias);
            ValidarEstructura(prueba, familia, estructura);

            int id;
            try
            {
                id = mpp.GuardarFamilia(idFamilia, nombre, codigo, familias.Count > 0, permisos, familias);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al guardar la familia: " + ex.Message, ex);
            }
            finally
            {
                LimpiarCache();
            }

            RegistrarEvento(BLLEvento.EVENTO_CONFIGURACION,
                $"{(idFamilia == 0 ? "Alta" : "Modificación")} de familia de permisos '{nombre}': {DescribirComponentes(familia.ObtenerHijos())}");
            return id;
        }

        public void EliminarFamilia(int idFamilia)
        {
            ValidarQuePuedeGestionar();

            EstructuraPermisos estructura = mpp.CargarEstructura();
            if (!estructura.Familias.TryGetValue(idFamilia, out EstructuraPermisos.DatosNodo datos))
                throw new PermisoException("permisos_err_no_existe", "La familia ya no existe.");

            Familia familia = new Familia(idFamilia, datos.Nombre, datos.Codigo);
            List<object> usos = ObtenerUsosDeFamilia(estructura, idFamilia);
            if (usos.Count > 0)
                throw new PermisoException("permisos_err_familia_en_uso",
                    $"La familia '{familia.Nombre}' se usa en: {string.Join(", ", usos)}. Quitala de ahí antes de eliminarla.",
                    familia, usos);

            try
            {
                mpp.EliminarFamilia(idFamilia);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al eliminar la familia: " + ex.Message, ex);
            }
            finally
            {
                LimpiarCache();
            }

            RegistrarEvento(BLLEvento.EVENTO_CONFIGURACION, $"Baja de familia de permisos '{familia.Nombre}'");
        }

        #endregion

        #region Perfiles

        /// <summary>
        /// Crea (idPerfil = 0) o modifica un perfil. Devuelve su id.
        /// </summary>
        public int GuardarPerfil(int idPerfil, string nombre, IList<int> permisos, IList<int> familias)
        {
            ValidarQuePuedeGestionar();

            permisos = SinRepetidos(permisos);
            familias = SinRepetidos(familias);
            nombre = (nombre ?? string.Empty).Trim();

            EstructuraPermisos estructura = mpp.CargarEstructura();

            EstructuraPermisos.DatosNodo anterior = null;
            if (idPerfil != 0 && !estructura.Perfiles.TryGetValue(idPerfil, out anterior))
                throw new PermisoException("permisos_err_no_existe", "El perfil ya no existe.");

            string codigo = anterior?.Codigo;
            if (codigo != null)
                nombre = anterior.Nombre;
            else
                ValidarNombre(nombre, estructura.Perfiles.Where(p => p.Key != idPerfil).Select(p => p.Value.Nombre), "permisos_err_nombre_repetido_perfil");

            ValidarQueExisten(estructura, permisos, familias);

            // WebMaster y Administrador conservan siempre Dashboard y Permisos: si al editarlos
            // quedan sin alguno, se les agrega solo (en vez de rechazar el cambio)
            if (ConservaPermisosFijos(codigo))
                permisos = CompletarPermisosFijos(estructura, idPerfil, nombre, codigo, permisos, familias);

            if (permisos.Count + familias.Count == 0)
                throw new PermisoException("permisos_err_perfil_vacio", "El perfil tiene que tener al menos un permiso o una familia.");

            Perfil perfil = ArmarPerfil(estructura, idPerfil, nombre, codigo, permisos, familias);

            EstructuraPermisos prueba = estructura.Clonar();
            prueba.DefinirPerfil(idPerfil == 0 ? ID_NUEVO : idPerfil, nombre, codigo, permisos, familias);
            ValidarEstructura(prueba, perfil, estructura);

            int id;
            try
            {
                id = mpp.GuardarPerfil(idPerfil, nombre, codigo, permisos, familias);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al guardar el perfil: " + ex.Message, ex);
            }
            finally
            {
                LimpiarCache();
            }

            RegistrarEvento(BLLEvento.EVENTO_CONFIGURACION,
                $"{(idPerfil == 0 ? "Alta" : "Modificación")} de perfil '{nombre}': {DescribirComponentes(perfil.ObtenerComponentes())}");
            return id;
        }

        public void EliminarPerfil(int idPerfil)
        {
            ValidarQuePuedeGestionar();

            EstructuraPermisos estructura = mpp.CargarEstructura();
            if (!estructura.Perfiles.TryGetValue(idPerfil, out EstructuraPermisos.DatosNodo datos))
                throw new PermisoException("permisos_err_no_existe", "El perfil ya no existe.");

            Perfil perfil = new Perfil(idPerfil, datos.Nombre, datos.Codigo);
            if (perfil.Codigo != null)
                throw new PermisoException("permisos_err_perfil_sistema_eliminar",
                    $"'{perfil.Nombre}' es un perfil del sistema: no se puede eliminar.", perfil);

            int cantidadUsuarios = estructura.UsuariosConPerfil(idPerfil).Count;
            if (cantidadUsuarios > 0)
                throw new PermisoException("permisos_err_perfil_con_usuarios",
                    $"El perfil '{perfil.Nombre}' lo tienen {cantidadUsuarios} usuario(s): asignales otro perfil antes de eliminarlo.",
                    perfil, cantidadUsuarios);

            try
            {
                mpp.EliminarPerfil(idPerfil);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al eliminar el perfil: " + ex.Message, ex);
            }
            finally
            {
                LimpiarCache();
            }

            RegistrarEvento(BLLEvento.EVENTO_CONFIGURACION, $"Baja de perfil '{perfil.Nombre}'");
        }

        #endregion

        #region Asignación de perfiles a usuarios

        public void AsignarPerfil(string usuario, int idPerfil)
        {
            ValidarQuePuedeGestionar();

            EstructuraPermisos estructura = mpp.CargarEstructura();
            if (!estructura.Perfiles.ContainsKey(idPerfil))
                throw new PermisoException("permisos_err_no_existe", "El perfil ya no existe.");

            bool tieneAnterior = estructura.PerfilDeUsuario.TryGetValue(usuario, out int idAnterior);
            if (tieneAnterior && idAnterior == idPerfil)
                return;

            ValidarQuienAsigna(estructura, idPerfil, tieneAnterior ? idAnterior : (int?)null);

            EstructuraPermisos prueba = estructura.Clonar();
            prueba.PerfilDeUsuario[usuario] = idPerfil;
            ValidarQueQuedeGestor(prueba, mpp.ListarUsuariosActivos());

            try
            {
                mpp.AsignarPerfil(usuario, idPerfil);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al asignar el perfil: " + ex.Message, ex);
            }
            finally
            {
                LimpiarCache();
            }

            string nombreAnterior = tieneAnterior && estructura.Perfiles.ContainsKey(idAnterior) ? estructura.Perfiles[idAnterior].Nombre : "sin perfil";
            RegistrarEvento(BLLEvento.EVENTO_CAMBIO_ROL,
                $"Perfil de {usuario}: '{nombreAnterior}' → '{estructura.Perfiles[idPerfil].Nombre}'");
        }

        /// <summary>
        /// Antes de crear una cuenta: el tipo de cuenta tiene que tener perfil por defecto y
        /// quien la crea tiene que poder darlo.
        /// </summary>
        public void ValidarPerfilPorDefecto(int rol)
        {
            EstructuraPermisos estructura = mpp.CargarEstructura();
            ValidarQuienAsigna(estructura, BuscarPerfilPorDefecto(estructura, rol), null);
        }

        /// <summary>
        /// Después de crear una cuenta: le asigna el perfil por defecto de su tipo.
        /// </summary>
        public void AsignarPerfilPorDefecto(string usuario, int rol)
        {
            EstructuraPermisos estructura = mpp.CargarEstructura();
            int idPerfil = BuscarPerfilPorDefecto(estructura, rol);
            ValidarQuienAsigna(estructura, idPerfil, null);

            try
            {
                mpp.AsignarPerfil(usuario, idPerfil);
            }
            finally
            {
                LimpiarCache();
            }

            RegistrarEvento(BLLEvento.EVENTO_CAMBIO_ROL, $"Perfil inicial de {usuario}: '{estructura.Perfiles[idPerfil].Nombre}'");
        }

        /// <summary>
        /// Antes de modificar una cuenta (Usuarios). Si cambia el tipo, la cuenta pasa al perfil
        /// por defecto del tipo nuevo; si se desactiva, tiene que quedar otro usuario con acceso a Permisos.
        /// </summary>
        public void ValidarCambioDeCuenta(string usuario, int rolAnterior, int rolNuevo, bool activoAnterior, bool activoNuevo)
        {
            bool cambiaRol = rolAnterior != rolNuevo;
            bool seDesactiva = activoAnterior && !activoNuevo;
            if (!cambiaRol && !seDesactiva)
                return;

            EstructuraPermisos estructura = mpp.CargarEstructura();
            EstructuraPermisos prueba = estructura.Clonar();

            if (cambiaRol)
            {
                int idNuevo = BuscarPerfilPorDefecto(estructura, rolNuevo);
                bool tieneAnterior = estructura.PerfilDeUsuario.TryGetValue(usuario, out int idAnterior);
                ValidarQuienAsigna(estructura, idNuevo, tieneAnterior ? idAnterior : (int?)null);
                prueba.PerfilDeUsuario[usuario] = idNuevo;
            }

            HashSet<string> activos = mpp.ListarUsuariosActivos();
            if (seDesactiva)
                activos.Remove(usuario);

            ValidarQueQuedeGestor(prueba, activos);
        }

        /// <summary>
        /// Después de modificar una cuenta: si se renombró, recalcula el dvh de su asignación
        /// (el usr lo cambia la FK en cascada); si cambió el tipo, le asigna el perfil por defecto.
        /// </summary>
        public void AplicarCambioDeCuenta(string usuarioOriginal, string usuarioNuevo, int rolAnterior, int rolNuevo)
        {
            try
            {
                if (!string.Equals(usuarioOriginal, usuarioNuevo, StringComparison.Ordinal))
                    mpp.RecalcularDvhUsuario(usuarioNuevo);

                if (rolAnterior != rolNuevo)
                {
                    EstructuraPermisos estructura = mpp.CargarEstructura();
                    int idPerfil = BuscarPerfilPorDefecto(estructura, rolNuevo);
                    mpp.AsignarPerfil(usuarioNuevo, idPerfil);
                    RegistrarEvento(BLLEvento.EVENTO_CAMBIO_ROL,
                        $"Perfil de {usuarioNuevo} por cambio de tipo de cuenta: '{estructura.Perfiles[idPerfil].Nombre}'");
                }
            }
            finally
            {
                LimpiarCache();
            }
        }

        /// <summary>
        /// Antes de desactivar una cuenta: tiene que quedar otro usuario activo con acceso a Permisos.
        /// </summary>
        public void ValidarDesactivacion(string usuario)
        {
            HashSet<string> activos = mpp.ListarUsuariosActivos();
            if (!activos.Remove(usuario))
                return;

            ValidarQueQuedeGestor(mpp.CargarEstructura(), activos);
        }

        #endregion

        #region Validaciones

        private void ValidarQuePuedeGestionar()
        {
            if (!UsuarioActualTieneAcceso(PermisosSistema.GestionPermisos))
                throw new PermisoException("permisos_err_sin_permiso", "No tenés permiso para gestionar permisos.");
        }

        private static void ValidarNombre(string nombre, IEnumerable<string> otrosNombres, string claveRepetido)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new PermisoException("permisos_err_nombre_obligatorio", "El nombre es obligatorio.");

            if (nombre.Length > LARGO_MAXIMO_NOMBRE)
                throw new PermisoException("permisos_err_nombre_largo",
                    $"El nombre no puede superar los {LARGO_MAXIMO_NOMBRE} caracteres.", LARGO_MAXIMO_NOMBRE);

            if (otrosNombres.Any(n => string.Equals(n, nombre, StringComparison.OrdinalIgnoreCase)))
                throw new PermisoException(claveRepetido, $"Ya existe '{nombre}'.", nombre);
        }

        private static void ValidarQueExisten(EstructuraPermisos estructura, IList<int> permisos, IList<int> familias)
        {
            if (permisos.Any(p => !estructura.Permisos.ContainsKey(p)) || familias.Any(f => !estructura.Familias.ContainsKey(f)))
                throw new PermisoException("permisos_err_componente_inexistente",
                    "Alguno de los permisos o familias elegidos ya no existe. Volvé a abrir el formulario.");
        }

        /// <summary>
        /// Arma todas las familias y perfiles de la estructura (así se detectan ciclos y permisos
        /// repetidos en cualquier lugar donde se use lo editado) y controla las reglas fijas.
        /// La regla de WebMaster y Administrador (conservar Dashboard y Permisos) se exige al perfil
        /// editado y a los que la cumplían antes del cambio: si otro perfil ya estaba sin esos
        /// permisos (ej. WebMaster vacío), eso no bloquea cambios que no lo tocan.
        /// </summary>
        private void ValidarEstructura(EstructuraPermisos estructura, object editado, EstructuraPermisos original)
        {
            foreach (int idFamilia in estructura.Familias.Keys)
                ConstruirValidando(() => estructura.ConstruirFamilia(idFamilia), editado);

            foreach (var datos in estructura.Perfiles)
            {
                Perfil perfil = ConstruirValidando(() => estructura.ConstruirPerfil(datos.Key), editado);

                bool conservaFijos = perfil.EsWebMaster
                    || string.Equals(perfil.Codigo, PerfilesSistema.CodigoAdministrador, StringComparison.OrdinalIgnoreCase);
                if (conservaFijos && !TienePermisosFijos(perfil)
                    && (EsLoEditado(perfil, editado) || TeniaPermisosFijos(original, datos.Key)))
                {
                    throw new PermisoException("permisos_err_admin_fijos",
                        $"El perfil '{perfil.Nombre}' tiene que conservar los permisos Dashboard y Permisos.", perfil);
                }
            }

            ValidarQueQuedeGestor(estructura, mpp.ListarUsuariosActivos());
        }

        private static bool ConservaPermisosFijos(string codigoPerfil)
        {
            return string.Equals(codigoPerfil, PerfilesSistema.CodigoWebMaster, StringComparison.OrdinalIgnoreCase)
                || string.Equals(codigoPerfil, PerfilesSistema.CodigoAdministrador, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Agrega como permisos sueltos Dashboard y Permisos si no le llegan al perfil por ningún
        /// camino (sueltos o dentro de una familia). Si ya los tiene, devuelve la lista sin cambios.
        /// </summary>
        private IList<int> CompletarPermisosFijos(EstructuraPermisos estructura, int idPerfil, string nombre, string codigo,
            IList<int> permisos, IList<int> familias)
        {
            Perfil armado = ArmarPerfil(estructura, idPerfil, nombre, codigo, permisos, familias);

            List<int> faltantes = new[] { PermisosSistema.Dashboard, PermisosSistema.GestionPermisos }
                .Where(clave => !armado.TienePermiso(clave))
                .Select(clave => estructura.Permisos.FirstOrDefault(p => string.Equals(p.Value, clave, StringComparison.OrdinalIgnoreCase)).Key)
                .Where(id => id != 0)
                .ToList();

            return faltantes.Count == 0 ? permisos : permisos.Concat(faltantes).ToList();
        }

        private static bool TienePermisosFijos(Perfil perfil)
        {
            return perfil.TienePermiso(PermisosSistema.Dashboard) && perfil.TienePermiso(PermisosSistema.GestionPermisos);
        }

        /// <summary>Si el perfil cumplía la regla antes del cambio (si no existía o no se puede armar, false).</summary>
        private static bool TeniaPermisosFijos(EstructuraPermisos original, int idPerfil)
        {
            if (original == null || !original.Perfiles.ContainsKey(idPerfil))
                return false;

            try
            {
                return TienePermisosFijos(original.ConstruirPerfil(idPerfil));
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Arma un árbol. Si el problema está en lo que se edita se informa tal cual; si está en
        /// otra familia o perfil que lo contiene, se aclara que es por el cambio.
        /// </summary>
        private static T ConstruirValidando<T>(Func<T> construir, object editado)
        {
            try
            {
                return construir();
            }
            catch (CicloFamiliaException)
            {
                throw;
            }
            catch (ComposicionInvalidaException ex) when (!EsLoEditado(ex.Contenedor, editado))
            {
                IComponente componente = ex is PermisoDuplicadoException duplicado
                    ? duplicado.Permiso
                    : ((ComponenteRepetidoException)ex).Componente;

                throw new PermisoException("permisos_err_conflicto",
                    $"Con este cambio, '{componente.Nombre}' quedaría repetido en '{ex.Contenedor}'.",
                    componente, ex.Contenedor);
            }
        }

        private static bool EsLoEditado(object contenedor, object editado)
        {
            if (contenedor is Familia familia && editado is Familia familiaEditada)
                return familia.Id == (familiaEditada.Id == 0 ? ID_NUEVO : familiaEditada.Id);

            if (contenedor is Perfil perfil && editado is Perfil perfilEditado)
                return perfil.Id == (perfilEditado.Id == 0 ? ID_NUEVO : perfilEditado.Id);

            return false;
        }

        /// <summary>
        /// Siempre tiene que quedar al menos un usuario activo cuyo perfil incluya Permisos.
        /// </summary>
        private static void ValidarQueQuedeGestor(EstructuraPermisos estructura, HashSet<string> usuariosActivos)
        {
            var perfilesGestores = new HashSet<int>();
            foreach (var datos in estructura.Perfiles)
            {
                try
                {
                    if (estructura.ConstruirPerfil(datos.Key).TienePermiso(PermisosSistema.GestionPermisos))
                    {
                        perfilesGestores.Add(datos.Key);
                    }
                }
                catch (ComposicionInvalidaException)
                {
                    // Un perfil inválido no da acceso
                }
            }

            bool quedaGestor = estructura.PerfilDeUsuario.Any(u => usuariosActivos.Contains(u.Key) && perfilesGestores.Contains(u.Value));
            if (!quedaGestor)
                throw new PermisoException("permisos_err_sin_gestor",
                    "Tiene que quedar al menos un usuario activo con acceso a Permisos.");
        }

        /// <summary>
        /// El perfil WebMaster solo lo da o lo quita un WebMaster. Quien no gestiona permisos
        /// (ej. al crear cuentas desde Usuarios) no puede dar un perfil que incluya Permisos.
        /// </summary>
        private void ValidarQuienAsigna(EstructuraPermisos estructura, int idPerfilNuevo, int? idPerfilAnterior)
        {
            string actor = UsuarioActual();
            bool tocaWebMaster = EsPerfilWebMaster(estructura, idPerfilNuevo)
                || (idPerfilAnterior.HasValue && EsPerfilWebMaster(estructura, idPerfilAnterior.Value));

            if (tocaWebMaster && !UsuarioEsWebMaster(actor))
                throw new PermisoException("permisos_err_webmaster_asignar",
                    "Solo un WebMaster puede asignar o quitar el perfil WebMaster.");

            if (!UsuarioTieneAcceso(actor, PermisosSistema.GestionPermisos))
            {
                Perfil perfil = ConstruirPerfilSeguro(estructura, idPerfilNuevo);
                if (perfil.TienePermiso(PermisosSistema.GestionPermisos))
                    throw new PermisoException("permisos_err_asignar_gestion",
                        $"Solo quien gestiona permisos puede dar el perfil '{perfil.Nombre}'.", perfil);
            }
        }

        #endregion

        #region Auxiliares

        private static Familia ArmarFamilia(EstructuraPermisos estructura, int idFamilia, string nombre, string codigo,
            IList<int> permisos, IList<int> familias)
        {
            var familia = new Familia(idFamilia == 0 ? ID_NUEVO : idFamilia, nombre, codigo);
            foreach (int idPermiso in permisos ?? new List<int>())
                familia.Agregar(estructura.ConstruirPermiso(idPermiso) ?? throw ComponenteInexistente());
            foreach (int idHija in familias ?? new List<int>())
                familia.Agregar(estructura.ConstruirFamilia(idHija) ?? throw ComponenteInexistente());
            return familia;
        }

        private static Perfil ArmarPerfil(EstructuraPermisos estructura, int idPerfil, string nombre, string codigo,
            IList<int> permisos, IList<int> familias)
        {
            var perfil = new Perfil(idPerfil == 0 ? ID_NUEVO : idPerfil, nombre, codigo);
            foreach (int idPermiso in permisos ?? new List<int>())
                perfil.AgregarComponente(estructura.ConstruirPermiso(idPermiso) ?? throw ComponenteInexistente());
            foreach (int idFamilia in familias ?? new List<int>())
                perfil.AgregarComponente(estructura.ConstruirFamilia(idFamilia) ?? throw ComponenteInexistente());
            return perfil;
        }

        private static PermisoException ComponenteInexistente()
        {
            return new PermisoException("permisos_err_componente_inexistente",
                "Alguno de los permisos o familias elegidos ya no existe. Volvé a abrir el formulario.");
        }

        /// <summary>
        /// Arma la familia; si las tablas quedaron inválidas (cambio por fuera de la app) devuelve
        /// la familia sin hijos para poder mostrarla y corregirla.
        /// </summary>
        private static Familia ConstruirFamiliaSegura(EstructuraPermisos estructura, int idFamilia)
        {
            try
            {
                return estructura.ConstruirFamilia(idFamilia);
            }
            catch (ComposicionInvalidaException)
            {
                EstructuraPermisos.DatosNodo datos = estructura.Familias[idFamilia];
                return new Familia(idFamilia, datos.Nombre, datos.Codigo);
            }
        }

        private static Perfil ConstruirPerfilSeguro(EstructuraPermisos estructura, int idPerfil)
        {
            try
            {
                return estructura.ConstruirPerfil(idPerfil);
            }
            catch (ComposicionInvalidaException)
            {
                EstructuraPermisos.DatosNodo datos = estructura.Perfiles[idPerfil];
                return new Perfil(idPerfil, datos.Nombre, datos.Codigo);
            }
        }

        private static List<object> ObtenerUsosDeFamilia(EstructuraPermisos estructura, int idFamilia)
        {
            var usos = new List<object>();
            foreach (int idPadre in estructura.FamiliasPadre(idFamilia))
            {
                EstructuraPermisos.DatosNodo datos = estructura.Familias[idPadre];
                usos.Add(new Familia(idPadre, datos.Nombre, datos.Codigo));
            }
            foreach (int idPerfil in estructura.PerfilesConFamilia(idFamilia))
            {
                EstructuraPermisos.DatosNodo datos = estructura.Perfiles[idPerfil];
                usos.Add(new Perfil(idPerfil, datos.Nombre, datos.Codigo));
            }
            return usos;
        }

        private static int BuscarPerfilPorDefecto(EstructuraPermisos estructura, int rol)
        {
            string codigo = PerfilesSistema.CodigoPerfilPorDefecto(rol);
            int? idPerfil = codigo != null ? estructura.BuscarPerfilPorCodigo(codigo) : null;
            if (!idPerfil.HasValue)
                throw new PermisoException("permisos_err_sin_perfil_defecto",
                    "No existe el perfil por defecto para este tipo de cuenta. Revisá la pantalla Permisos.");
            return idPerfil.Value;
        }

        private static bool EsPerfilWebMaster(EstructuraPermisos estructura, int idPerfil)
        {
            return estructura.Perfiles.TryGetValue(idPerfil, out EstructuraPermisos.DatosNodo datos) && EsCodigoWebMaster(datos.Codigo);
        }

        private static bool EsCodigoWebMaster(string codigo)
        {
            return string.Equals(codigo, Perfil.CodigoWebMaster, StringComparison.OrdinalIgnoreCase);
        }

        private static List<int> SinRepetidos(IList<int> ids)
        {
            return (ids ?? new List<int>()).Distinct().ToList();
        }

        private static string DescribirComponentes(IEnumerable<IComponente> componentes)
        {
            List<IComponente> lista = componentes.ToList();
            string permisos = string.Join(", ", lista.OfType<PermisoSimple>().Select(p => p.Nombre));
            string familias = string.Join(", ", lista.OfType<Familia>().Select(f => f.Nombre));
            return $"permisos [{permisos}], familias [{familias}]";
        }

        private static string UsuarioActual()
        {
            return Singleton.Instancia?.Usuario?.USUARIO_Usuario;
        }

        private void RegistrarEvento(string tipo, string accion)
        {
            try
            {
                string usuario = UsuarioActual();
                if (!string.IsNullOrEmpty(usuario))
                    bllEvento.RegistrarEvento(tipo, usuario, accion, 2, "Permisos");
            }
            catch
            {
                // No impedir la operación principal si falla el log
            }
        }

        #endregion
    }
}
