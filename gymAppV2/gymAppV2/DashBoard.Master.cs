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

            ConfigurarMenuSegunRol();
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
            liPagos.Visible = bllRol.UsuarioActualTieneAcceso(PermisosSistema.Pagos);
            liPerfil.Visible = bllRol.UsuarioActualTieneAcceso(PermisosSistema.Perfil);
            liVerificacionDV.Visible = bllRol.UsuarioActualTieneAcceso(PermisosSistema.VerificacionDV);
            liBackup.Visible = bllRol.UsuarioActualTieneAcceso(PermisosSistema.Backup);
            liRestore.Visible = bllRol.UsuarioActualTieneAcceso(PermisosSistema.Restore);
            liEncriptarDatos.Visible = bllRol.UsuarioActualTieneAcceso(PermisosSistema.EncriptarDatos);

            // El módulo de permisos no está implementado; se mantiene oculto hasta su desarrollo.
            liPermisos.Visible = false;
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
