using System;
using Servicios.Singleton;
using BLL;

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
                    Response.Redirect("~/LogIn/LogIn.aspx");
                }
            }

            ConfigurarMenuSegunRol();
        }

        /// <summary>
        /// Muestra u oculta las opciones del menú lateral según el rol del usuario logueado.
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
            liPagos.Visible = bllRol.UsuarioActualTieneAcceso("Pagos");
            liPerfil.Visible = bllRol.UsuarioActualTieneAcceso("Perfil");
            liVerificacionDV.Visible = bllRol.UsuarioActualTieneAcceso("VerificacionDV");

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

            // Registrar evento de logout antes de cerrar sesión
            try
            {
                var bllEvento = new BLLEvento();
                bllEvento.RegistrarLogout(usuarioNombre);
            }
            catch
            {
                // No impedir el logout si falla el log
            }

            Singleton.Instancia.LogOut();
            Response.Redirect("~/LogIn/LogIn.aspx", false);
            Context.ApplicationInstance.CompleteRequest();
        }
    }
}