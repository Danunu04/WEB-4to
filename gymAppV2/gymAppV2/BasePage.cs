using System;
using System.Web.UI;
using BLL;
using Servicios.Singleton;

namespace gymAppV2
{
    /// <summary>
    /// Página base para todas las pantallas protegidas del sistema.
    /// Centraliza la verificación de login, autorización por rol y utilidades de UI.
    /// </summary>
    public class BasePage : System.Web.UI.Page
    {
        protected BLLRol BllRol { get; private set; }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            BllRol = new BLLRol();

            if (!Singleton.Instancia.IsLogged())
            {
                Response.Redirect("~/LogIn/LogIn.aspx");
            }
        }

        /// <summary>
        /// Verifica que el usuario logueado tenga acceso al módulo indicado.
        /// Si no tiene permisos, redirige a la página de acceso denegado.
        /// </summary>
        protected void VerificarAcceso(string modulo)
        {
            if (!BllRol.UsuarioActualTieneAcceso(modulo))
            {
                Response.Redirect("~/AccesoDenegado.aspx");
            }
        }

        /// <summary>
        /// Muestra un toast global usando la función showToast definida en DashBoard.Master.
        /// </summary>
        protected void MostrarToast(string mensaje, string tipo = "info")
        {
            ScriptManager.RegisterStartupScript(
                this,
                GetType(),
                Guid.NewGuid().ToString(),
                $"if(window.showToast) window.showToast('{System.Security.SecurityElement.Escape(mensaje)}','{tipo}');",
                true);
        }
    }
}
