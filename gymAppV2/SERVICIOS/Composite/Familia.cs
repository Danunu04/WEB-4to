using System.Collections.Generic;
using System.Linq;

namespace SERVICIOS.Composite
{
    /// <summary>
    /// Compuesto del Composite: agrupa permisos simples y otras familias.
    /// Al agregar valida que no se formen ciclos y que ningún permiso quede repetido.
    /// </summary>
    public class Familia : IComponente
    {
        private readonly List<IComponente> _hijos = new List<IComponente>();

        public int Id { get; private set; }
        public string Nombre { get; set; }

        /// <summary>
        /// Código de las familias que crea el sistema (se usa para traducir el nombre).
        /// Null en las familias creadas desde la pantalla.
        /// </summary>
        public string Codigo { get; set; }

        public Familia(int id, string nombre, string codigo = null)
        {
            Id = id;
            Nombre = nombre;
            Codigo = codigo;
        }

        /// <summary>
        /// True si contiene al menos otra familia (columna Familia.profundidad).
        /// </summary>
        public bool Profundidad
        {
            get { return _hijos.Any(h => h is Familia); }
        }

        public void Agregar(IComponente componente)
        {
            if (componente is Familia familia
                && (ReferenceEquals(familia, this)
                    || (Id != 0 && (familia.Id == Id || familia.ContieneFamilia(Id)))))
            {
                throw new CicloFamiliaException(this, familia);
            }

            Componentes.ValidarAgregado(_hijos, componente, this);
            _hijos.Add(componente);
        }

        public void Quitar(IComponente componente)
        {
            _hijos.RemoveAll(h => Componentes.MismoComponente(h, componente));
        }

        /// <summary>
        /// Copia de la lista de hijos directos: modificarla no cambia la familia.
        /// </summary>
        public List<IComponente> ObtenerHijos()
        {
            return new List<IComponente>(_hijos);
        }

        /// <summary>
        /// Permisos simples de toda la familia, incluidos los de sus subfamilias.
        /// </summary>
        public List<PermisoSimple> ObtenerPermisosSimples()
        {
            return Componentes.Aplanar(this);
        }

        /// <summary>
        /// True si la familia indicada está debajo de esta, en cualquier nivel.
        /// </summary>
        public bool ContieneFamilia(int idFamilia)
        {
            foreach (Familia hija in _hijos.OfType<Familia>())
            {
                if (hija.Id == idFamilia || hija.ContieneFamilia(idFamilia))
                    return true;
            }
            return false;
        }

        public override string ToString()
        {
            return Nombre;
        }
    }
}
