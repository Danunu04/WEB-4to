using System;
using System.Web.UI;
using BLL;
using Servicios.Singleton;

namespace gymAppV2
{
    /// <summary>
    /// Inicio público. Antes tenía el check-in por DNI (se sacó para la entrega final):
    /// ahora redirige al Dashboard si hay sesión, o al inicio de sesión si no.
    /// </summary>
    public partial class Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Redirigir(HaySesion() ? DestinoConSesion() : "~/LogIn/LogIn.aspx");
        }

        private static bool HaySesion()
        {
            try
            {
                var sesion = Singleton.Instancia;
                return sesion != null && sesion.IsLogged();
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Durante una pausa de integridad, quien no puede repararla va a la pantalla de bloqueo.
        /// </summary>
        private static string DestinoConSesion()
        {
            try
            {
                if (new BLLDigitoVerificador().ExisteErrorIntegridad() && !new BLLRol().UsuarioActualPuedeRepararIntegridad())
                    return "~/VerificacioDV/VerificacioDV.aspx";

                return "~/DashBoard/WebForm1.aspx";
            }
            catch
            {
                return "~/VerificacioDV/VerificacioDV.aspx";
            }
        }

        private void Redirigir(string url)
        {
            Response.Redirect(ResolveUrl(url), false);
            Context.ApplicationInstance.CompleteRequest();
        }
    }
}
