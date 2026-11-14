using BE;
using Servicios.Singleton;

namespace BLL
{
    /// <summary>
    /// Punto de entrada de la autorización para las pantallas (BasePage, menú, web service).
    /// Qué puede hacer cada usuario lo decide su perfil (patrón Composite, ver BLLPermiso).
    /// </summary>
    public class BLLRol
    {
        private BLLPermiso bllPermiso;

        public BLLRol()
        {
            bllPermiso = new BLLPermiso();
        }

        /// <summary>
        /// Verifica si el usuario logueado tiene el permiso indicado (constantes de PermisosSistema).
        /// </summary>
        public bool UsuarioActualTieneAcceso(string modulo)
        {
            return UsuarioTieneAcceso(Singleton.Instancia?.Usuario?.USUARIO_Usuario, modulo);
        }

        public bool UsuarioTieneAcceso(string usuario, string modulo)
        {
            return bllPermiso.UsuarioTieneAcceso(usuario, modulo);
        }

        /// <summary>
        /// Quien puede entrar con el sistema pausado por un error de integridad para repararlo
        /// (pantalla VerificacioDV): el que tiene el permiso de verificación de dígitos.
        /// </summary>
        public bool UsuarioActualPuedeRepararIntegridad()
        {
            return UsuarioActualTieneAcceso(PermisosSistema.VerificacionDV);
        }
    }
}
