using System;

namespace SERVICIOS.Composite
{
    /// <summary>
    /// Se lanza cuando agregar un componente a una familia o a un perfil rompería las reglas
    /// del Composite. Contenedor es la Familia o el Perfil al que se intentaba agregar.
    /// </summary>
    public abstract class ComposicionInvalidaException : Exception
    {
        public object Contenedor { get; private set; }

        protected ComposicionInvalidaException(string mensaje, object contenedor)
            : base(mensaje)
        {
            Contenedor = contenedor;
        }
    }

    /// <summary>
    /// El permiso ya llega al contenedor por otro camino (directo o dentro de otra familia).
    /// </summary>
    public class PermisoDuplicadoException : ComposicionInvalidaException
    {
        public PermisoSimple Permiso { get; private set; }

        public PermisoDuplicadoException(PermisoSimple permiso, object contenedor)
            : base($"El permiso '{permiso.Nombre}' ya está incluido en '{contenedor}'.", contenedor)
        {
            Permiso = permiso;
        }
    }

    /// <summary>
    /// El mismo permiso o la misma familia ya está agregado directamente al contenedor.
    /// </summary>
    public class ComponenteRepetidoException : ComposicionInvalidaException
    {
        public IComponente Componente { get; private set; }

        public ComponenteRepetidoException(IComponente componente, object contenedor)
            : base($"'{componente.Nombre}' ya está agregado en '{contenedor}'.", contenedor)
        {
            Componente = componente;
        }
    }

    /// <summary>
    /// Agregar la familia formaría un ciclo: una familia no puede contenerse a sí misma,
    /// ni directa ni indirectamente.
    /// </summary>
    public class CicloFamiliaException : ComposicionInvalidaException
    {
        public Familia Contenida { get; private set; }

        public CicloFamiliaException(Familia contenedor, Familia contenida)
            : base($"'{contenida.Nombre}' no puede ir dentro de '{contenedor.Nombre}' porque '{contenida.Nombre}' ya contiene a '{contenedor.Nombre}'.", contenedor)
        {
            Contenida = contenida;
        }
    }
}
