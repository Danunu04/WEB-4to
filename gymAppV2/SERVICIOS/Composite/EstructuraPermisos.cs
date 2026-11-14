using System;
using System.Collections.Generic;
using System.Linq;

namespace SERVICIOS.Composite
{
    /// <summary>
    /// Fotografía de las tablas de permisos (qué contiene cada familia y cada perfil, y qué
    /// perfil tiene cada usuario) a partir de la cual se arman los árboles del Composite.
    ///
    /// Se carga entera de una vez (MPPPermiso.CargarEstructura) en lugar de consultar la base
    /// por cada nivel del árbol. También sirve para probar un cambio antes de guardarlo:
    /// se clona, se aplica el cambio en memoria y se vuelven a armar todos los árboles.
    /// </summary>
    public class EstructuraPermisos
    {
        public class DatosNodo
        {
            public string Nombre { get; set; }
            public string Codigo { get; set; }
        }

        /// <summary>idPermiso → clave del permiso (ej. "GestionAlumnos").</summary>
        public Dictionary<int, string> Permisos { get; private set; } = new Dictionary<int, string>();
        public Dictionary<int, DatosNodo> Familias { get; private set; } = new Dictionary<int, DatosNodo>();
        public Dictionary<int, DatosNodo> Perfiles { get; private set; } = new Dictionary<int, DatosNodo>();

        public Dictionary<int, List<int>> PermisosDeFamilia { get; private set; } = new Dictionary<int, List<int>>();
        public Dictionary<int, List<int>> FamiliasDeFamilia { get; private set; } = new Dictionary<int, List<int>>();
        public Dictionary<int, List<int>> PermisosDePerfil { get; private set; } = new Dictionary<int, List<int>>();
        public Dictionary<int, List<int>> FamiliasDePerfil { get; private set; } = new Dictionary<int, List<int>>();

        /// <summary>usr → idPerfil.</summary>
        public Dictionary<string, int> PerfilDeUsuario { get; private set; } = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        #region Armado de los árboles

        public PermisoSimple ConstruirPermiso(int idPermiso)
        {
            return Permisos.TryGetValue(idPermiso, out string clave) ? new PermisoSimple(idPermiso, clave) : null;
        }

        /// <summary>
        /// Arma la familia con todo su árbol. Lanza ComposicionInvalidaException si encuentra
        /// un ciclo o un permiso repetido.
        /// </summary>
        public Familia ConstruirFamilia(int idFamilia)
        {
            return ConstruirFamilia(idFamilia, new HashSet<int>());
        }

        private Familia ConstruirFamilia(int idFamilia, HashSet<int> camino)
        {
            if (!Familias.TryGetValue(idFamilia, out DatosNodo datos))
                return null;

            var familia = new Familia(idFamilia, datos.Nombre, datos.Codigo);
            camino.Add(idFamilia);

            foreach (int idPermiso in Hijos(PermisosDeFamilia, idFamilia))
            {
                PermisoSimple permiso = ConstruirPermiso(idPermiso);
                if (permiso != null)
                    familia.Agregar(permiso);
            }

            foreach (int idHija in Hijos(FamiliasDeFamilia, idFamilia))
            {
                if (camino.Contains(idHija))
                {
                    // Se corta acá: armar la hija seguiría el ciclo para siempre
                    DatosNodo datosHija = Familias[idHija];
                    throw new CicloFamiliaException(familia, new Familia(idHija, datosHija.Nombre, datosHija.Codigo));
                }

                Familia hija = ConstruirFamilia(idHija, camino);
                if (hija != null)
                    familia.Agregar(hija);
            }

            camino.Remove(idFamilia);
            return familia;
        }

        /// <summary>
        /// Arma el perfil con sus permisos sueltos y sus familias. Lanza ComposicionInvalidaException
        /// si algún permiso le llega por dos caminos.
        /// </summary>
        public Perfil ConstruirPerfil(int idPerfil)
        {
            if (!Perfiles.TryGetValue(idPerfil, out DatosNodo datos))
                return null;

            var perfil = new Perfil(idPerfil, datos.Nombre, datos.Codigo);

            foreach (int idPermiso in Hijos(PermisosDePerfil, idPerfil))
            {
                PermisoSimple permiso = ConstruirPermiso(idPermiso);
                if (permiso != null)
                    perfil.AgregarComponente(permiso);
            }

            foreach (int idFamilia in Hijos(FamiliasDePerfil, idPerfil))
            {
                Familia familia = ConstruirFamilia(idFamilia);
                if (familia != null)
                    perfil.AgregarComponente(familia);
            }

            return perfil;
        }

        /// <summary>
        /// Perfil del usuario, o null si no tiene ninguno asignado.
        /// </summary>
        public Perfil ConstruirPerfilDeUsuario(string usuario)
        {
            if (string.IsNullOrEmpty(usuario) || !PerfilDeUsuario.TryGetValue(usuario, out int idPerfil))
                return null;

            return ConstruirPerfil(idPerfil);
        }

        #endregion

        #region Consultas sobre la estructura

        public int? BuscarPerfilPorCodigo(string codigo)
        {
            foreach (var perfil in Perfiles)
            {
                if (string.Equals(perfil.Value.Codigo, codigo, StringComparison.OrdinalIgnoreCase))
                    return perfil.Key;
            }
            return null;
        }

        public int? BuscarPermisoPorClave(string clave)
        {
            foreach (var permiso in Permisos)
            {
                if (string.Equals(permiso.Value, clave, StringComparison.OrdinalIgnoreCase))
                    return permiso.Key;
            }
            return null;
        }

        /// <summary>Familias que tienen a esta como hija directa.</summary>
        public List<int> FamiliasPadre(int idFamilia)
        {
            return FamiliasDeFamilia.Where(f => f.Value.Contains(idFamilia)).Select(f => f.Key).ToList();
        }

        /// <summary>Perfiles que tienen a esta familia como componente directo.</summary>
        public List<int> PerfilesConFamilia(int idFamilia)
        {
            return FamiliasDePerfil.Where(p => p.Value.Contains(idFamilia)).Select(p => p.Key).ToList();
        }

        public List<string> UsuariosConPerfil(int idPerfil)
        {
            return PerfilDeUsuario.Where(u => u.Value == idPerfil).Select(u => u.Key).ToList();
        }

        #endregion

        #region Cambios en memoria (para validar antes de guardar)

        public EstructuraPermisos Clonar()
        {
            var copia = new EstructuraPermisos();

            foreach (var p in Permisos) copia.Permisos[p.Key] = p.Value;
            foreach (var f in Familias) copia.Familias[f.Key] = new DatosNodo { Nombre = f.Value.Nombre, Codigo = f.Value.Codigo };
            foreach (var p in Perfiles) copia.Perfiles[p.Key] = new DatosNodo { Nombre = p.Value.Nombre, Codigo = p.Value.Codigo };
            CopiarRelacion(PermisosDeFamilia, copia.PermisosDeFamilia);
            CopiarRelacion(FamiliasDeFamilia, copia.FamiliasDeFamilia);
            CopiarRelacion(PermisosDePerfil, copia.PermisosDePerfil);
            CopiarRelacion(FamiliasDePerfil, copia.FamiliasDePerfil);
            foreach (var u in PerfilDeUsuario) copia.PerfilDeUsuario[u.Key] = u.Value;

            return copia;
        }

        public void DefinirFamilia(int idFamilia, string nombre, string codigo, IEnumerable<int> permisos, IEnumerable<int> familias)
        {
            Familias[idFamilia] = new DatosNodo { Nombre = nombre, Codigo = codigo };
            PermisosDeFamilia[idFamilia] = permisos.ToList();
            FamiliasDeFamilia[idFamilia] = familias.ToList();
        }

        public void QuitarFamilia(int idFamilia)
        {
            Familias.Remove(idFamilia);
            PermisosDeFamilia.Remove(idFamilia);
            FamiliasDeFamilia.Remove(idFamilia);
        }

        public void DefinirPerfil(int idPerfil, string nombre, string codigo, IEnumerable<int> permisos, IEnumerable<int> familias)
        {
            Perfiles[idPerfil] = new DatosNodo { Nombre = nombre, Codigo = codigo };
            PermisosDePerfil[idPerfil] = permisos.ToList();
            FamiliasDePerfil[idPerfil] = familias.ToList();
        }

        public void QuitarPerfil(int idPerfil)
        {
            Perfiles.Remove(idPerfil);
            PermisosDePerfil.Remove(idPerfil);
            FamiliasDePerfil.Remove(idPerfil);
        }

        #endregion

        /// <summary>
        /// Agrega una fila de una tabla de relación (ej. FamiliaPermiso) a su diccionario.
        /// </summary>
        public static void AgregarRelacion(Dictionary<int, List<int>> relacion, int idPadre, int idHijo)
        {
            if (!relacion.TryGetValue(idPadre, out List<int> hijos))
            {
                hijos = new List<int>();
                relacion[idPadre] = hijos;
            }
            hijos.Add(idHijo);
        }

        private static IEnumerable<int> Hijos(Dictionary<int, List<int>> relacion, int idPadre)
        {
            return relacion.TryGetValue(idPadre, out List<int> hijos) ? hijos : Enumerable.Empty<int>();
        }

        private static void CopiarRelacion(Dictionary<int, List<int>> origen, Dictionary<int, List<int>> destino)
        {
            foreach (var r in origen)
                destino[r.Key] = new List<int>(r.Value);
        }
    }
}
