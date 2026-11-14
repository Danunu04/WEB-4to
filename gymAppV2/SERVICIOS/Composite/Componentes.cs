using System.Collections.Generic;
using System.Linq;

namespace SERVICIOS.Composite
{
    /// <summary>
    /// Operaciones sobre componentes que comparten Familia y Perfil.
    /// </summary>
    public static class Componentes
    {
        /// <summary>
        /// Todos los permisos simples que hay debajo del componente (él mismo si es una hoja).
        /// </summary>
        public static List<PermisoSimple> Aplanar(IComponente componente)
        {
            var permisos = new List<PermisoSimple>();
            AgregarPermisos(componente, permisos);
            return permisos;
        }

        private static void AgregarPermisos(IComponente componente, List<PermisoSimple> permisos)
        {
            if (componente is PermisoSimple permiso)
            {
                permisos.Add(permiso);
                return;
            }

            foreach (IComponente hijo in componente.ObtenerHijos())
                AgregarPermisos(hijo, permisos);
        }

        /// <summary>
        /// Mismo tipo (permiso o familia) y mismo Id; si no tienen Id, la misma instancia.
        /// </summary>
        public static bool MismoComponente(IComponente a, IComponente b)
        {
            if (ReferenceEquals(a, b))
                return true;

            return a != null && b != null
                && a.GetType() == b.GetType()
                && a.Id != 0
                && a.Id == b.Id;
        }

        /// <summary>
        /// Regla de duplicados del Composite: el componente nuevo no puede estar ya agregado
        /// y ninguno de sus permisos puede llegar al contenedor por otro camino.
        /// </summary>
        internal static void ValidarAgregado(IEnumerable<IComponente> existentes, IComponente nuevo, object contenedor)
        {
            List<IComponente> lista = existentes.ToList();

            IComponente repetido = lista.FirstOrDefault(c => MismoComponente(c, nuevo));
            if (repetido != null)
                throw new ComponenteRepetidoException(nuevo, contenedor);

            var idsExistentes = new HashSet<int>(lista.SelectMany(Aplanar).Select(p => p.Id));
            PermisoSimple duplicado = Aplanar(nuevo).FirstOrDefault(p => idsExistentes.Contains(p.Id));
            if (duplicado != null)
                throw new PermisoDuplicadoException(duplicado, contenedor);

            // Un mismo permiso repetido dentro del componente nuevo también es un duplicado
            PermisoSimple repetidoInterno = Aplanar(nuevo).GroupBy(p => p.Id).Where(g => g.Count() > 1).Select(g => g.First()).FirstOrDefault();
            if (repetidoInterno != null)
                throw new PermisoDuplicadoException(repetidoInterno, contenedor);
        }
    }
}
