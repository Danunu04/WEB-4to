using System;
using System.Web.UI;
using BLL;
using Servicios.Singleton;
using System.Web;

namespace gymAppV2
{
    /// <summary>
    /// Página base para todas las pantallas protegidas del sistema.
    /// Centraliza la verificación de login, autorización por rol y utilidades de UI.
    /// </summary>
    public class BasePage : System.Web.UI.Page
    {
        protected BLLRol BllRol { get; private set; }
        protected BLLDigitoVerificador BllDV { get; private set; }

        /// <summary>
        /// Nombre de la página de verificación de integridad para evitar redirecciones en bucle.
        /// </summary>
        private const string PAGINA_VERIFICACION_DV = "VerificacioDV/VerificacioDV.aspx";

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            BllRol = new BLLRol();
            BllDV = new BLLDigitoVerificador();

            if (!Singleton.Instancia.IsLogged())
            {
                Response.Redirect("~/LogIn/LogIn.aspx");
                return;
            }

            VerificarIntegridadSiAplica();
        }

        /// <summary>
        /// Si existe un error de integridad y el usuario no es administrador,
        /// redirige a la página de verificación para pausar el sistema.
        /// La página de verificación misma se excluye para evitar bucles.
        /// </summary>
        private void VerificarIntegridadSiAplica()
        {
            string paginaActual = Request.AppRelativeCurrentExecutionFilePath;
            if (!string.IsNullOrEmpty(paginaActual) && paginaActual.Replace("~/", "").Equals(PAGINA_VERIFICACION_DV, StringComparison.OrdinalIgnoreCase))
                return;

            try
            {
                if (BllDV.ExisteErrorIntegridad() && !BllDV.UsuarioActualEsAdmin())
                {
                    Response.Redirect("~/VerificacioDV/VerificacioDV.aspx");
                }
            }
            catch
            {
                // Si la verificación falla (por ejemplo, tabla no existe aún),
                // no bloqueamos al usuario hasta que un admin confirme el error.
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
