using System.Collections.Generic;

namespace SERVICIOS.Composite
{
    /// <summary>
    /// Componente del patrón Composite de permisos. Lo implementan la hoja (PermisoSimple)
    /// y el compuesto (Familia), así un perfil o una familia trata igual a un permiso suelto
    /// que a un grupo de permisos.
    /// </summary>
    public interface IComponente
    {
        /// <summary>
        /// Id en su tabla (Permiso o Familia). 0 si todavía no se guardó.
        /// </summary>
        int Id { get; }

        /// <summary>
        /// En un permiso es su clave (ej. "GestionAlumnos"); en una familia, su nombre.
        /// </summary>
        string Nombre { get; }

        void Agregar(IComponente componente);

        void Quitar(IComponente componente);

        List<IComponente> ObtenerHijos();
    }
}
