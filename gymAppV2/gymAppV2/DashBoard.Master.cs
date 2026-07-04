using System;
using System.Web;
using BE;
using Servicios.Singleton;
using BLL;
using System.Web.UI;

namespace gymAppV2
{
    public partial class DashBoardMaster : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (!Singleton.Instancia.IsLogged())
                {
                    RedirigirSeguro("~/LogIn/LogIn.aspx");
                    return;
                }
            }

            if (SistemaEnPausa() && !UsuarioActualEsAdmin())
            {
                ConfigurarMenuPausa();
                string paginaActual = Request.AppRelativeCurrentExecutionFilePath;
                if (!string.IsNullOrEmpty(paginaActual)
                    && !paginaActual.Replace("~/", "").Equals("VerificacioDV/VerificacioDV.aspx", StringComparison.OrdinalIgnoreCase))
                {
                    RedirigirSeguro("~/VerificacioDV/VerificacioDV.aspx");
                    return;
                }
            }
            else
            {
                ConfigurarMenuSegunRol();
            }
        }

        /// <summary>
        /// Indica si el sistema está pausado por un error de integridad de datos.
        /// </summary>
        private bool SistemaEnPausa()
        {
            try
            {
                var bllDV = new BLLDigitoVerificador();
                return bllDV.ExisteErrorIntegridad();
            }
            catch
            {
                // Si no se puede verificar, se asume pausa para no continuar con datos dudosos.
                return true;
            }
        }

        /// <summary>
        /// Indica si el usuario logueado actualmente es administrador.
        /// </summary>
        private bool UsuarioActualEsAdmin()
        {
            try
            {
                var bllRol = new BLLRol();
                return bllRol.UsuarioActualEsAdmin();
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Durante una pausa de integridad para usuarios no administradores,
        /// oculta todo el menú de navegación excepto la opción de cerrar sesión.
        /// </summary>
        private void ConfigurarMenuPausa()
        {
            ulMenuNavegacion.Visible = false;
            ulCambiarContra.Visible = false;
            divDivider.Visible = false;
        }

        /// <summary>
        /// Muestra u oculta las opciones del menú lateral según el perfil del usuario logueado.
        /// </summary>
        private void ConfigurarMenuSegunRol()
        {
            var bllRol = new BLLRol();

            liDashboard.Visible = bllRol.UsuarioActualTieneAcceso("Dashboard");
            liUsuarios.Visible = bllRol.UsuarioActualTieneAcceso("GestionUsuarios");
            liAlumnos.Visible = bllRol.UsuarioActualTieneAcceso("GestionAlumnos");
            liEntrenadores.Visible = bllRol.UsuarioActualTieneAcceso("GestionEntrenadores");
            liActividades.Visible = bllRol.UsuarioActualTieneAcceso("ActividadesCalendario");
            liRutinas.Visible = bllRol.UsuarioActualTieneAcceso("GestionRutinas");
            liBitacora.Visible = bllRol.UsuarioActualTieneAcceso("Bitacora");
            liRespaldo.Visible = bllRol.UsuarioActualTieneAcceso(PermisosSistema.Backup)
                             || bllRol.UsuarioActualTieneAcceso(PermisosSistema.Restore);
            liPagos.Visible = bllRol.UsuarioActualTieneAcceso(PermisosSistema.Pagos);
            liPerfil.Visible = bllRol.UsuarioActualTieneAcceso(PermisosSistema.Perfil);

        }

        protected void LnkLogout_Click(object sender, EventArgs e)
        {
            var usuario = Singleton.Instancia.Usuario;
            string usuarioNombre = usuario?.USUARIO_Usuario ?? "desconocido";

            // Invalidar la cookie de autenticación de forms antes de destruir la sesión.
            // El orden es importante: primero la cookie, luego la sesión.
            System.Web.Security.FormsAuthentication.SignOut();

            // Registrar evento de logout antes de cerrar sesión.
            try
            {
                var bllEvento = new BLLEvento();
                bllEvento.RegistrarLogout(usuarioNombre);
            }
            catch
            {
                // No impedir el logout si falla el log.
            }

            try
            {
                Singleton.Instancia.LogOut();
            }
            catch
            {
                // Si el logout falla, al menos la cookie de forms ya fue invalidada.
            }

            RedirigirSeguro("~/LogIn/LogIn.aspx");
        }

        /// <summary>
        /// Redirige de forma segura terminando la request correctamente.
        /// </summary>
        private void RedirigirSeguro(string url)
        {
            try
            {
                Response.Redirect(ResolveUrl(url), false);
                Context.ApplicationInstance.CompleteRequest();
            }
            catch (HttpException)
            {
                // Contexto no disponible.
            }
            catch (Exception)
            {
                // Ignorar errores menores de redirección.
            }
        }
    }
}
