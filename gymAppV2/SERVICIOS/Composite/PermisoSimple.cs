using System;
using System.Collections.Generic;

namespace SERVICIOS.Composite
{
    /// <summary>
    /// Hoja del Composite: un permiso del sistema (uno por pantalla, ver BE.PermisosSistema).
    /// No puede contener otros componentes.
    /// </summary>
    public class PermisoSimple : IComponente
    {
        public int Id { get; private set; }
        public string Nombre { get; private set; }

        public PermisoSimple(int id, string nombre)
        {
            Id = id;
            Nombre = nombre;
        }

        public void Agregar(IComponente componente)
        {
            throw new InvalidOperationException("No se puede agregar un componente a un permiso simple.");
        }

        public void Quitar(IComponente componente)
        {
            throw new InvalidOperationException("No se puede quitar un componente de un permiso simple.");
        }

        public List<IComponente> ObtenerHijos()
        {
            return new List<IComponente>();
        }

        public override string ToString()
        {
            return Nombre;
        }
    }
}
