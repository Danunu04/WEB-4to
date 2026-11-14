using System;
using System.Collections.Generic;
using System.Linq;

namespace SERVICIOS.Composite
{
    /// <summary>
    /// Perfil de usuario: el conjunto de permisos simples y familias que se le asigna a una
    /// cuenta (cada usuario tiene un solo perfil). No es parte del Composite, lo usa.
    /// </summary>
    public class Perfil
    {
        /// <summary>
        /// Código del perfil WebMaster. Sus permisos se editan como los de cualquier perfil;
        /// lo especial es que solo otro WebMaster lo asigna o lo quita.
        /// </summary>
        public const string CodigoWebMaster = "WEBMASTER";

        private readonly List<IComponente> _componentes = new List<IComponente>();

        public int Id { get; private set; }
        public string Nombre { get; set; }

        /// <summary>
        /// Código de los perfiles que crea el sistema (WEBMASTER, ADMIN, RECEPCION...).
        /// Null en los perfiles creados desde la pantalla.
        /// </summary>
        public string Codigo { get; set; }

        public Perfil(int id, string nombre, string codigo = null)
        {
            Id = id;
            Nombre = nombre;
            Codigo = codigo;
        }

        public bool EsWebMaster
        {
            get { return string.Equals(Codigo, CodigoWebMaster, StringComparison.OrdinalIgnoreCase); }
        }

        /// <summary>
        /// Agrega un permiso simple o una familia. Falla si el componente ya está o si alguno
        /// de sus permisos ya llega al perfil por otro camino.
        /// </summary>
        public void AgregarComponente(IComponente componente)
        {
            Componentes.ValidarAgregado(_componentes, componente, this);
            _componentes.Add(componente);
        }

        public void QuitarComponente(IComponente componente)
        {
            _componentes.RemoveAll(c => Componentes.MismoComponente(c, componente));
        }

        /// <summary>
        /// Componentes directos del perfil (copia).
        /// </summary>
        public List<IComponente> ObtenerComponentes()
        {
            return new List<IComponente>(_componentes);
        }

        /// <summary>
        /// Todos los permisos simples del perfil, recorriendo sus familias.
        /// </summary>
        public List<PermisoSimple> ObtenerPermisosSimples()
        {
            return _componentes.SelectMany(Componentes.Aplanar).ToList();
        }

        public bool TienePermiso(string nombrePermiso)
        {
            return ObtenerPermisosSimples().Any(p => string.Equals(p.Nombre, nombrePermiso, StringComparison.OrdinalIgnoreCase));
        }

        public override string ToString()
        {
            return Nombre;
        }
    }
}
